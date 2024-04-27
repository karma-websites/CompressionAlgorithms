namespace Lz77Algorithm;

internal class Lz77
{
    const int WindowSize = 4095; // 4095
    const int BufferSize = 15;   // 15
    static readonly int OffsetBits = (int)Math.Ceiling(Math.Log2(WindowSize + 1));
    static readonly int LengthBits = (int)Math.Ceiling(Math.Log2(BufferSize + 1));
    static readonly int PairBits = OffsetBits + LengthBits;

    private readonly struct Node(int offset, int length, byte next = 0)
    {
        public int Offset { get; } = offset;
        public int Length { get; } = length;
        public byte Symbol { get; } = next;

        public override readonly string ToString()
        {
            return $"({Offset} {Length} {Symbol})";
        }
    }

    public static byte[] Compress(byte[] data)
    {
        List<Node> encodedList = [];
        int pos = 0;

        while (pos < data.Length)
        {
            FindMatch(data, pos, out int offset, out int length);

            if (offset == 0 && length == 0 || length * 8 <= PairBits)
            {
                encodedList.Add(new Node(0, 0, data[pos]));
                pos += 1;
            }
            else
            {
                encodedList.Add(new Node(offset, length - 1));
                pos += length;
            }
        }

        return PairsToBytes2(encodedList);
    }

    public static byte[] Decompress(byte[] encodedData)
    {
        List<Node> encodedList = BytesToPairs2(encodedData);
        List<byte> result = [];

        foreach (var elem in encodedList)
        {
            if (elem.Offset == 0 && elem.Length == 0)
            {
                result.Add(elem.Symbol);
            }
            else
            {
                int startIndex = result.Count - elem.Offset;
                if (elem.Offset >= elem.Length)
                {
                    result.AddRange(result[startIndex..(startIndex + elem.Length + 1)]);
                }
                else
                {
                    int i = elem.Length + 1;
                    while (i > 0)
                    {
                        int length = Math.Min(i, elem.Offset);
                        result.AddRange(result[startIndex..(startIndex + length)]);
                        i -= elem.Offset;
                    }
                }
            }
        }

        return [.. result];
    }

    private static void FindMatch(byte[] data, int pos, out int offset, out int length)
    {
        offset = 0;
        length = 0;

        int bestOffset = -1;
        int bestLength = -1;

        int startWindow = Math.Max(pos - WindowSize, 0);
        int indexWindow = 0;
        int endBuffer = Math.Min(pos + BufferSize + 1, data.Length) + 1;

        for (int j = pos + 3; j < endBuffer; j++)
        {
            indexWindow = IndexOf(startWindow + indexWindow, pos, j, data);

            if (indexWindow != -1)
            {
                bestOffset = pos - startWindow - indexWindow;
                bestLength = j - pos;
            }
            else break;
        }

        if (bestOffset > 0 && bestLength > 0)
        {
            offset = bestOffset;
            length = bestLength;
        }
    }

    private static int IndexOf(int startWindow, int pos, int endBuffer, byte[] data)
    {
        int windowLength = pos - startWindow;
        int bufferLength = endBuffer - pos;

        if (windowLength < bufferLength) return -1;

        ulong p = 3;
        ulong windowHash = 0;
        ulong bufferHash = 0;
        ulong maxPower = 1;

        for (int i = 0; i < bufferLength; i++)
        {
            windowHash = windowHash * p + data[startWindow + i];
            bufferHash = bufferHash * p + data[pos + i];
            if (i != bufferLength - 1) maxPower *= p;
        }

        for (int i = 0; i < windowLength - bufferLength; i++)
        {
            if (windowHash == bufferHash)
            {
                if (Equals(startWindow, pos, endBuffer, data, i)) return i;
            }

            windowHash -= data[startWindow + i] * maxPower;
            windowHash = windowHash * p + data[startWindow + bufferLength + i];
        }

        return -1;
    }

    private static bool Equals(int startWindow, int pos, int endBuffer, byte[] data, int index)
    {
        int bufferLength = endBuffer - pos;
        for (int i = 0; i < bufferLength; i++)
        {
            if (data[startWindow + index + i] != data[pos + i]) return false;
        }

        return true;
    }

    private static List<Node> BytesToPairs2(byte[] data)
    {
        byte emptyBits = data[0];
        List<Node> listPairs = [];
        bool[] bits = [.. BytesToBits(data)];

        int i = 8;
        while (i < bits.Length - emptyBits)
        {
            byte symbol = 0;
            int offset = 0;
            int length = 0;

            if (bits[i] == false)
            {
                i++;
                for (byte j = 0, bit = (byte)Math.Pow(2, 7); j < 8; j++, bit >>= 1)
                {
                    if (bits[i + j] == true) symbol |= bit;
                }
                i += 8;
                listPairs.Add(new Node(0, 0, symbol));
            }
            else
            {
                i++;
                for (int j = 0, bit = (int)Math.Pow(2, OffsetBits - 1); j < OffsetBits; j++, bit >>= 1)
                {
                    if (bits[i + j] == true) offset |= bit;
                }
                i += OffsetBits;
                for (int j = 0, bit = (int)Math.Pow(2, LengthBits - 1); j < LengthBits; j++, bit >>= 1)
                {
                    if (bits[i + j] == true) length |= bit;
                }
                i += LengthBits;
                listPairs.Add(new Node(offset, length));
            }
        }

        return listPairs;
    }

    private static byte[] PairsToBytes2(List<Node> listPairs)
    {
        List<bool> listBits = [];

        foreach (Node elem in listPairs)
        {
            if (elem.Offset == 0 && elem.Length == 0) // Pair: (0, 0, symbol)
            {
                listBits.Add(false);
                for (int i = 0, bit = (int)Math.Pow(2, 7); i < 8; i++, bit >>= 1)
                {
                    listBits.Add((elem.Symbol & bit) != 0);
                }
            }
            else // Pair: (number1, number2, void)
            {
                listBits.Add(true);
                for (int i = 0, bit = (int)Math.Pow(2, OffsetBits - 1); i < OffsetBits; i++, bit >>= 1)
                {
                    listBits.Add((elem.Offset & bit) != 0);
                }
                for (int i = 0, bit = (int)Math.Pow(2, LengthBits - 1); i < LengthBits; i++, bit >>= 1)
                {
                    listBits.Add((elem.Length & bit) != 0);
                }
            }
        }

        byte emptyBits = (byte)(8 - listBits.Count % 8);

        return BitsToBytes(listBits, emptyBits);
    }

    public static byte[] BitsToBytes(List<bool> listBits, byte emptyBits)
    {
        int numBytes = (listBits.Count + 7) / 8;
        byte[] bytes = new byte[numBytes + 1];
        bytes[0] = emptyBits;

        int i = 0;
        foreach (var bit in listBits)
        {
            int byteIndex = i / 8;
            int bitIndex = i % 8;

            if (listBits[i++])
            {
                bytes[byteIndex + 1] |= (byte)(1 << (7 - bitIndex));
            }
        }

        return bytes;
    }

    private static List<bool> BytesToBits(byte[] bytes)
    {
        List<bool> listBits = [];

        for (int i = 0; i < bytes.Length; i++)
        {
            for (int j = 0, bit = (int)Math.Pow(2, 7); j < 8; j++, bit >>= 1)
            {
                listBits.Add((bytes[i] & bit) != 0);
            }
        }

        return listBits;
    }


    // Неиспользуемый код 
    private static byte[] PairsToBytes(List<Node> listPairs)
    {
        List<byte> listBytes = [];

        foreach (Node elem in listPairs)
        {
            if (elem.Offset == 0 && elem.Length == 0)
                listBytes.Add(elem.Symbol);
            else
            {
                byte[] pair = new byte[(PairBits + 7) / 8];
                for (int j = 0, offsetBits = OffsetBits; j < OffsetBits; j++, offsetBits--)
                {
                    int byteIndex = j / 8;
                    int bitIndex = j % 8;
                    int mask = elem.Offset & (1 << (offsetBits - 1));

                    if (mask != 0)
                        pair[byteIndex] |= (byte)(1 << (7 - bitIndex));
                }
                for (int j = OffsetBits, lengthBits = LengthBits; j < PairBits; j++, lengthBits--)
                {
                    int byteIndex = j / 8;
                    int bitIndex = j % 8;
                    int mask = elem.Length & (1 << (lengthBits - 1));

                    if (mask != 0)
                        pair[byteIndex] |= (byte)(1 << (7 - bitIndex));
                }
                listBytes.AddRange(pair);
            }
        }

        return [.. CreateFlagTable(listPairs), .. listBytes];
    }

    private static byte[] CreateFlagTable(List<Node> listPairs)
    {
        int flagTableSize = listPairs.Count;
        int numBytes = (flagTableSize + 7) / 8;
        byte[] bytes = BitConverter.GetBytes(flagTableSize);

        Array.Resize(ref bytes, sizeof(int) + numBytes);

        int i = sizeof(int);
        byte mask = (byte)Math.Pow(2, 7);
        byte sum = 0;

        foreach (Node elem in listPairs)
        {
            if (elem.Offset != 0 && elem.Length != 0) sum |= mask;

            if (mask > 1) mask >>= 1;
            else
            {
                bytes[i++] = sum;
                sum = 0;
                mask = (byte)Math.Pow(2, 7);
            }
        }

        if (mask != (byte)Math.Pow(2, 7))
            bytes[i] = sum;

        return bytes;
    }

    private static List<Node> BytesToPairs(byte[] data)
    {
        List<Node> listPairs = [];
        bool[] bits = [.. BytesToBits(data)];
        int flagTableSize = BitConverter.ToInt32(data, 0);
        int j = ((flagTableSize + 7) / 8 + sizeof(int)) * 8;

        for (int i = 0; i < flagTableSize; i++)
        {
            if (bits[i + sizeof(int) * 8] == false)
            {
                byte symbol = 0;

                for (int k = 0; k < 8; k++)
                {
                    if (bits[j + k] == true)
                        symbol |= (byte)(1 << (7 - k));
                }
                j += 8;

                listPairs.Add(new Node(0, 0, symbol));
            }
            else
            {
                int offset = 0;
                int length = 0;

                for (int k = 0; k < OffsetBits; k++)
                {
                    if (bits[j + k] == true)
                        offset |= 1 << (OffsetBits - 1 - k);
                }
                j += OffsetBits;

                for (int k = 0; k < LengthBits; k++)
                {
                    if (bits[j + k] == true)
                        length |= 1 << (LengthBits - 1 - k);
                }
                j += LengthBits;

                if (PairBits % 8 != 0)
                    j += 8 - PairBits % 8;

                listPairs.Add(new Node(offset, length));
            }
        }

        return listPairs;
    }

}
