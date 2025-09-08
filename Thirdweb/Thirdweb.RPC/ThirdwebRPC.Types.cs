using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thirdweb.RPC;

[JsonObject]
public class RpcError
{
    [JsonConstructor]
    private RpcError() { }

    [JsonProperty("code")]
    public int Code { get; private set; }

    [JsonProperty("message")]
    public string Message { get; private set; }

    [JsonProperty("data")]
    public JToken Data { get; private set; }
}

[JsonObject]
public class RpcResponseMessage
{
    [JsonProperty("id")]
    public object Id { get; private set; }

    [JsonProperty("jsonrpc")]
    public string JsonRpcVersion { get; private set; }

    [JsonProperty("result")]
    public JToken Result { get; private set; }

    [JsonProperty("error")]
    public RpcError Error { get; protected set; }

    [JsonIgnore]
    public bool HasError => this.Error != null;
}

public class RpcResponse<T>
{
    [JsonProperty("jsonrpc")]
    public string Jsonrpc { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("result")]
    public T Result { get; set; }

    [JsonProperty("error")]
    public RpcError Error { get; set; }
}

public class RpcRequest
{
    [JsonProperty("jsonrpc")]
    public string Jsonrpc { get; set; } = "2.0";

    [JsonProperty("method")]
    public string Method { get; set; }

    [JsonProperty("params")]
    public object[] Params { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }
}

[JsonObject]
public class RpcRequestMessage
{
    [JsonConstructor]
    private RpcRequestMessage() { }

    public RpcRequestMessage(object id, string method, params object[] parameterList)
    {
        this.Id = id;
        this.JsonRpcVersion = "2.0";
        this.Method = method;
        this.RawParameters = parameterList;
    }

    [JsonProperty("id")]
    public object Id { get; set; }

    [JsonProperty("jsonrpc")]
    public string JsonRpcVersion { get; private set; }

    [JsonProperty("method")]
    public string Method { get; private set; }

    [JsonProperty("params")]
    [JsonConverter(typeof(RpcParameterJsonConverter))]
    public object RawParameters { get; private set; }
}

public class RpcParameterJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.StartObject:
                try
                {
                    var jObject = JObject.Load(reader);
                    return jObject.ToObject<Dictionary<string, object>>();
                }
                catch (Exception)
                {
                    throw new Exception("Request parameters can only be an associative array, list or null.");
                }
            case JsonToken.StartArray:
                return JArray.Load(reader).ToObject<object[]>(serializer);
            case JsonToken.Null:
            case JsonToken.None:
            case JsonToken.StartConstructor:
            case JsonToken.PropertyName:
            case JsonToken.Comment:
            case JsonToken.Raw:
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.String:
            case JsonToken.Boolean:
            case JsonToken.Undefined:
            case JsonToken.EndObject:
            case JsonToken.EndArray:
            case JsonToken.EndConstructor:
            case JsonToken.Date:
            case JsonToken.Bytes:
            default:
                throw new Exception("Request parameters can only be an associative array, list or null.");
        }
    }

    public override bool CanConvert(Type objectType)
    {
        return true;
    }
}
