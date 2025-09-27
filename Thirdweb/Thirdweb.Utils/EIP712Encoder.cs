using System.Collections;
using System.Numerics;
using System.Text;
using Nethereum.ABI;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Util;

namespace Thirdweb;

public class EIP712Encoder
{
    public static EIP712Encoder Current { get; } = new EIP712Encoder();

    private readonly ABIEncode _abiEncode = new();
    private readonly ParametersEncoder _parametersEncoder = new();

    public byte[] EncodeTypedData<T, TDomain>(T message, TypedData<TDomain> typedData)
    {
        typedData.Message = MemberValueFactory.CreateFromMessage(message);
        typedData.EnsureDomainRawValuesAreInitialised();
        return this.EncodeTypedDataRaw(typedData);
    }

    public byte[] EncodeTypedData<T, TDomain>(T data, TDomain domain, string primaryTypeName)
    {
        var typedData = this.GenerateTypedData(data, domain, primaryTypeName);

        return this.EncodeTypedData(typedData);
    }

    public byte[] EncodeTypedData(string json)
    {
        var typedDataRaw = TypedDataRawJsonConversion.DeserialiseJsonToRawTypedData(json);
        return this.EncodeTypedDataRaw(typedDataRaw);
    }

    public byte[] EncodeTypedData<DomainType>(string json, string messageKeySelector = "message")
    {
        var typedDataRaw = TypedDataRawJsonConversion.DeserialiseJsonToRawTypedData<DomainType>(json, messageKeySelector);
        return this.EncodeTypedDataRaw(typedDataRaw);
    }

    public byte[] EncodeAndHashTypedData<T, TDomain>(T message, TypedData<TDomain> typedData)
    {
        var encodedData = this.EncodeTypedData(message, typedData);
        return Sha3Keccack.Current.CalculateHash(encodedData);
    }

    public byte[] EncodeAndHashTypedData<TDomain>(TypedData<TDomain> typedData)
    {
        var encodedData = this.EncodeTypedData(typedData);
        return Sha3Keccack.Current.CalculateHash(encodedData);
    }

    public byte[] EncodeTypedData<TDomain>(TypedData<TDomain> typedData)
    {
        typedData.EnsureDomainRawValuesAreInitialised();
        return this.EncodeTypedDataRaw(typedData);
    }

    public byte[] EncodeTypedDataRaw(TypedDataRaw typedData)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        writer.Write("1901".HexToBytes());
        writer.Write(this.HashStruct(typedData.Types, "EIP712Domain", typedData.DomainRawValues));
        writer.Write(this.HashStruct(typedData.Types, typedData.PrimaryType, typedData.Message));

        writer.Flush();
        var result = memoryStream.ToArray();
        return result;
    }

    public byte[] HashDomainSeparator<TDomain>(TypedData<TDomain> typedData)
    {
        typedData.EnsureDomainRawValuesAreInitialised();
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        writer.Write(this.HashStruct(typedData.Types, "EIP712Domain", typedData.DomainRawValues));
        writer.Flush();
        var result = memoryStream.ToArray();
        return result;
    }

    public byte[] HashStruct<T>(T message, string primaryType, params Type[] types)
    {
        var memberDescriptions = MemberDescriptionFactory.GetTypesMemberDescription(types);
        var memberValue = MemberValueFactory.CreateFromMessage(message);
        return this.HashStruct(memberDescriptions, primaryType, memberValue);
    }

    public string GetEncodedType(string primaryType, params Type[] types)
    {
        var memberDescriptions = MemberDescriptionFactory.GetTypesMemberDescription(types);
        return EncodeType(memberDescriptions, primaryType);
    }

    public string GetEncodedTypeDomainSeparator<TDomain>(TypedData<TDomain> typedData)
    {
        typedData.EnsureDomainRawValuesAreInitialised();
        return EncodeType(typedData.Types, "EIP712Domain");
    }

    private byte[] HashStruct(IDictionary<string, MemberDescription[]> types, string primaryType, IEnumerable<MemberValue> message)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        var encodedType = EncodeType(types, primaryType);
        var typeHash = Sha3Keccack.Current.CalculateHash(Encoding.UTF8.GetBytes(encodedType));
        writer.Write(typeHash);

        this.EncodeData(writer, types, message);

        writer.Flush();
        return Sha3Keccack.Current.CalculateHash(memoryStream.ToArray());
    }

    private static string EncodeType(IDictionary<string, MemberDescription[]> types, string typeName)
    {
        var encodedTypes = EncodeTypes(types, typeName);
        var encodedPrimaryType = encodedTypes.Single(x => x.Key == typeName);
        var encodedReferenceTypes = encodedTypes.Where(x => x.Key != typeName).OrderBy(x => x.Key).Select(x => x.Value);
        var fullyEncodedType = encodedPrimaryType.Value + string.Join(string.Empty, encodedReferenceTypes.ToArray());

        return fullyEncodedType;
    }

    private static List<KeyValuePair<string, string>> EncodeTypes(IDictionary<string, MemberDescription[]> types, string currentTypeName, HashSet<string> visited = null)
    {
        visited ??= new HashSet<string>();

        if (visited.Contains(currentTypeName))
        {
            return new List<KeyValuePair<string, string>>();
        }

        _ = visited.Add(currentTypeName);

        var currentTypeMembers = types[currentTypeName];
        var currentTypeMembersEncoded = currentTypeMembers.Select(x => x.Type + " " + x.Name);
        var result = new List<KeyValuePair<string, string>> { new(currentTypeName, currentTypeName + "(" + string.Join(",", currentTypeMembersEncoded.ToArray()) + ")") };

        foreach (var member in currentTypeMembers)
        {
            var referencedType = ConvertToElementType(member.Type);
            if (Utils.IsReferenceType(referencedType) && !visited.Contains(referencedType))
            {
                result.AddRange(EncodeTypes(types, referencedType, visited));
            }
        }

        return result;
    }

    private static string ConvertToElementType(string type)
    {
        if (type.Contains('['))
        {
            return type[..type.IndexOf('[')];
        }
        return type;
    }

    private void EncodeData(BinaryWriter writer, IDictionary<string, MemberDescription[]> types, IEnumerable<MemberValue> memberValues)
    {
        foreach (var memberValue in memberValues)
        {
            switch (memberValue.TypeName)
            {
                case var refType when Utils.IsReferenceType(refType):
                {
                    writer.Write(this.HashStruct(types, memberValue.TypeName, (IEnumerable<MemberValue>)memberValue.Value));
                    break;
                }
                case "string":
                {
                    var value = Encoding.UTF8.GetBytes((string)memberValue.Value);
                    var abiValueEncoded = Sha3Keccack.Current.CalculateHash(value);
                    writer.Write(abiValueEncoded);
                    break;
                }
                case "bytes":
                {
                    byte[] value;
                    if (memberValue.Value is string v)
                    {
                        value = v.HexToBytes();
                    }
                    else
                    {
                        value = (byte[])memberValue.Value;
                    }
                    var abiValueEncoded = Sha3Keccack.Current.CalculateHash(value);
                    writer.Write(abiValueEncoded);
                    break;
                }
                default:
                {
                    if (memberValue.TypeName.Contains('['))
                    {
                        var items = (IList)memberValue.Value;
                        var itemsMemberValues = new List<MemberValue>();
                        foreach (var item in items)
                        {
                            itemsMemberValues.Add(new MemberValue() { TypeName = memberValue.TypeName[..memberValue.TypeName.LastIndexOf('[')], Value = item });
                        }
                        using (var memoryStream = new MemoryStream())
                        using (var writerItem = new BinaryWriter(memoryStream))
                        {
                            this.EncodeData(writerItem, types, itemsMemberValues);
                            writerItem.Flush();
                            writer.Write(Sha3Keccack.Current.CalculateHash(memoryStream.ToArray()));
                        }
                    }
                    else if (memberValue.TypeName.StartsWith("int") || memberValue.TypeName.StartsWith("uint"))
                    {
                        object value;
                        if (memberValue.Value is string)
                        {
                            BigInteger parsedOutput;
                            if (BigInteger.TryParse((string)memberValue.Value, out parsedOutput))
                            {
                                value = parsedOutput;
                            }
                            else
                            {
                                value = memberValue.Value;
                            }
                        }
                        else
                        {
                            value = memberValue.Value;
                        }
                        var abiValue = new ABIValue(memberValue.TypeName, value);
                        var abiValueEncoded = this._abiEncode.GetABIEncoded(abiValue);
                        writer.Write(abiValueEncoded);
                    }
                    else
                    {
                        var abiValue = new ABIValue(memberValue.TypeName, memberValue.Value);
                        var abiValueEncoded = this._abiEncode.GetABIEncoded(abiValue);
                        writer.Write(abiValueEncoded);
                    }
                    break;
                }
            }
        }
    }

    public TypedData<TDomain> GenerateTypedData<T, TDomain>(T data, TDomain domain, string primaryTypeName)
    {
        var parameters = this._parametersEncoder.GetParameterAttributeValues(typeof(T), data).OrderBy(x => x.ParameterAttribute.Order);

        var typeMembers = new List<MemberDescription>();
        var typeValues = new List<MemberValue>();
        foreach (var parameterAttributeValue in parameters)
        {
            typeMembers.Add(new MemberDescription { Type = parameterAttributeValue.ParameterAttribute.Type, Name = parameterAttributeValue.ParameterAttribute.Name });

            typeValues.Add(new MemberValue { TypeName = parameterAttributeValue.ParameterAttribute.Type, Value = parameterAttributeValue.Value });
        }

        var result = new TypedData<TDomain>
        {
            PrimaryType = primaryTypeName,
            Types = new Dictionary<string, MemberDescription[]>
            {
                [primaryTypeName] = typeMembers.ToArray(),
                ["EIP712Domain"] = MemberDescriptionFactory.GetTypesMemberDescription(typeof(TDomain))["EIP712Domain"],
            },
            Message = typeValues.ToArray(),
            Domain = domain,
        };

        return result;
    }
}
