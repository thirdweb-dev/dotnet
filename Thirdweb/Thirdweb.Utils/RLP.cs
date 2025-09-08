namespace Thirdweb;

public class RLP
{
    /// <summary>
    ///     Reason for threshold according to Vitalik Buterin:
    ///     - 56 bytes maximizes the benefit of both options
    ///     - if we went with 60 then we would have only had 4 slots for long strings
    ///     so RLP would not have been able to store objects above 4gb
    ///     - if we went with 48 then RLP would be fine for 2^128 space, but that's way too much
    ///     - so 56 and 2^64 space seems like the right place to put the cutoff
    ///     - also, that's where Bitcoin's varint does the cutof
    /// </summary>
    private const int SIZE_THRESHOLD = 56;

    /* RLP encoding rules are defined as follows:
     * For a single byte whose value is in the [0x00, 0x7f] range, that byte is
     * its own RLP encoding.
     */

    /// <summary>
    ///     [0x80]
    ///     If a string is 0-55 bytes long, the RLP encoding consists of a single
    ///     byte with value 0x80 plus the length of the string followed by the
    ///     string. The range of the first byte is thus [0x80, 0xb7].
    /// </summary>
    private const byte OFFSET_SHORT_ITEM = 0x80;

    /// <summary>
    ///     [0xb7]
    ///     If a string is more than 55 bytes long, the RLP encoding consists of a
    ///     single byte with value 0xb7 plus the length of the length of the string
    ///     in binary form, followed by the length of the string, followed by the
    ///     string. For example, a length-1024 string would be encoded as
    ///     \xb9\x04\x00 followed by the string. The range of the first byte is thus
    ///     [0xb8, 0xbf].
    /// </summary>
    private const byte OFFSET_LONG_ITEM = 0xb7;

    /// <summary>
    ///     [0xc0]
    ///     If the total payload of a list (i.e. the combined length of all its
    ///     items) is 0-55 bytes long, the RLP encoding consists of a single byte
    ///     with value 0xc0 plus the length of the list followed by the concatenation
    ///     of the RLP encodings of the items. The range of the first byte is thus
    ///     [0xc0, 0xf7].
    /// </summary>
    public const byte OFFSET_SHORT_LIST = 0xc0;

    /// <summary>
    ///     [0xf7]
    ///     If the total payload of a list is more than 55 bytes long, the RLP
    ///     encoding consists of a single byte with value 0xf7 plus the length of the
    ///     length of the list in binary form, followed by the length of the list,
    ///     followed by the concatenation of the RLP encodings of the items. The
    ///     range of the first byte is thus [0xf8, 0xff].
    /// </summary>
    private const byte OFFSET_LONG_LIST = 0xf7;

    public static readonly byte[] EMPTY_BYTE_ARRAY = Array.Empty<byte>();
    public static readonly byte[] ZERO_BYTE_ARRAY = { 0 };

    public static int ByteArrayToInt(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return BitConverter.ToInt32(bytes, 0);
    }

    public static byte[] EncodeByte(byte singleByte)
    {
        if (singleByte == 0)
        {
            return new[] { OFFSET_SHORT_ITEM };
        }

        if (singleByte <= 0x7F)
        {
            return new[] { singleByte };
        }

        return new[] { (byte)(OFFSET_SHORT_ITEM + 1), singleByte };
    }

    public static byte[] EncodeElement(byte[] srcData)
    {
        // null or empty
        if (srcData == null || srcData.Length == 0)
        {
            return new[] { OFFSET_SHORT_ITEM };
        }

        // single zero
        if (srcData.Length == 1 && srcData[0] == 0)
        {
            return srcData;
        }

        if (srcData.Length == 1 && srcData[0] < 0x80)
        {
            return srcData;
        }

        if (srcData.Length < SIZE_THRESHOLD)
        {
            // length = 8X
            var length = (byte)(OFFSET_SHORT_ITEM + srcData.Length);
            var data = new byte[srcData.Length + 1];
            Array.Copy(srcData, 0, data, 1, srcData.Length);
            data[0] = length;

            return data;
        }
        else
        {
            // length of length = BX
            // prefix = [BX, [length]]
            var tmpLength = srcData.Length;
            byte byteNum = 0;
            while (tmpLength != 0)
            {
                ++byteNum;
                tmpLength >>= 8;
            }
            var lenBytes = new byte[byteNum];
            for (var i = 0; i < byteNum; ++i)
            {
                lenBytes[byteNum - 1 - i] = (byte)(srcData.Length >> (8 * i));
            }
            // first byte = F7 + bytes.length
            var data = new byte[srcData.Length + 1 + byteNum];
            Array.Copy(srcData, 0, data, 1 + byteNum, srcData.Length);
            data[0] = (byte)(OFFSET_LONG_ITEM + byteNum);
            Array.Copy(lenBytes, 0, data, 1, lenBytes.Length);

            return data;
        }
    }

    public static byte[] EncodeDataItemsAsElementOrListAndCombineAsList(byte[][] dataItems, int[] indexOfListDataItems = null)
    {
        if (indexOfListDataItems == null)
        {
            return EncodeList(dataItems.Select(EncodeElement).ToArray());
        }

        var encodedData = new List<byte[]>();

        for (var i = 0; i < dataItems.Length; i++)
        {
            if (indexOfListDataItems.Contains(i))
            {
                var item = dataItems[i];
                encodedData.Add(EncodeList(item));
            }
            else
            {
                encodedData.Add(EncodeElement(dataItems[i]));
            }
        }

        return EncodeList(encodedData.ToArray());
    }

    public static byte[] EncodeList(params byte[][] items)
    {
        if (items == null || (items.Length == 1 && items[0] == null))
        {
            return new[] { OFFSET_SHORT_LIST };
        }

        var totalLength = 0;
        for (var i = 0; i < items.Length; i++)
        {
            totalLength += items[i].Length;
        }

        byte[] data;

        int copyPos;

        if (totalLength < SIZE_THRESHOLD)
        {
            var dataLength = 1 + totalLength;
            data = new byte[dataLength];

            //single byte length
            data[0] = (byte)(OFFSET_SHORT_LIST + totalLength);
            copyPos = 1;
        }
        else
        {
            // length of length = BX
            // prefix = [BX, [length]]
            var tmpLength = totalLength;
            byte byteNum = 0;

            while (tmpLength != 0)
            {
                ++byteNum;
                tmpLength >>= 8;
            }

            tmpLength = totalLength;

            var lenBytes = new byte[byteNum];
            for (var i = 0; i < byteNum; ++i)
            {
                lenBytes[byteNum - 1 - i] = (byte)(tmpLength >> (8 * i));
            }
            // first byte = F7 + bytes.length
            data = new byte[1 + lenBytes.Length + totalLength];

            data[0] = (byte)(OFFSET_LONG_LIST + byteNum);
            Array.Copy(lenBytes, 0, data, 1, lenBytes.Length);

            copyPos = lenBytes.Length + 1;
        }

        //Combine all elements
        foreach (var item in items)
        {
            Array.Copy(item, 0, data, copyPos, item.Length);
            copyPos += item.Length;
        }
        return data;
    }
}
