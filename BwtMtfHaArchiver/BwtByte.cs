namespace BwtMtfHaArchiver;

internal class BwtByte
{
    public static int BlockSize { get; set; } = ushort.MaxValue;

    private static int CompareCyclicShifts(int x, int y, byte[] inputData)
    {
        int lengthData = inputData.Length;
        for (int i = 0; i < lengthData; i++)
        {
            if (!inputData[x].Equals(inputData[y]))
            {
                return inputData[x].CompareTo(inputData[y]);
            }
            x = ++x % lengthData;
            y = ++y % lengthData;
        }

        return 0;
    }

    public static (byte[], ushort) Direct(byte[] inputData)
    {
        int lengthData = inputData.Length;

        int[] cyclicShifts = Enumerable.Range(0, lengthData).ToArray();

        Array.Sort(cyclicShifts, (x, y) => CompareCyclicShifts(x, y, inputData));

        byte[] result = new byte[lengthData];
        ushort number = 0;
        for (ushort i = 0; i < lengthData; i++)
        {
            result[i] = inputData[(cyclicShifts[i] + lengthData - 1) % lengthData];
            if (cyclicShifts[i] == 0) number = i;
        }

        return (result, number);
    }

    private struct Pairs
    {
        public int Index;
        public byte Element;
    }

    private static int ComparePairs(Pairs x, Pairs y)
    {
        if (x.Element.Equals(y.Element))
        {
            return x.Index.CompareTo(y.Index);
        }
        else
        {
            return x.Element.CompareTo(y.Element);
        }
    }

    public static byte[] Inverse(byte[] bwt, ushort number)
    {
        int lengthBwt = bwt.Length;

        Pairs[] pairs = new Pairs[lengthBwt];
        for (int i = 0; i < lengthBwt; i++)
        {
            pairs[i].Index = i;
            pairs[i].Element = bwt[i];
        }

        Array.Sort(pairs, ComparePairs);

        int position = number;
        byte[] result = new byte[lengthBwt];
        for (int i = 0; i < lengthBwt; i++)
        {
            position = pairs[position].Index;
            result[i] = bwt[position];
        }

        return result;
    }

    // Converting a data file

    public static byte[] DirectData(byte[] data)
    {
        if (BlockSize > ushort.MaxValue)
            throw new Exception("The maximum block size (ushort.MaxValue) for the BWT algorithm has been exceeded");
        if (BlockSize < 0)
            throw new Exception("Invalid block size for the BWT algorithm");

        int dataLength = data.Length;
        int dataPosition = 0;
        List<byte> transformData = [];

        while (dataPosition < dataLength)
        {
            byte[] buffer = new byte[BlockSize];
            if (dataLength - dataPosition < BlockSize)
            {
                Array.Resize(ref buffer, dataLength - dataPosition);
            }
            Array.Copy(data, dataPosition, buffer, 0, buffer.Length);

            (byte[] encodeData, ushort number) = Direct(buffer);

            byte[] numberBytes = BitConverter.GetBytes(number);
            transformData.AddRange(numberBytes);

            transformData.AddRange(encodeData);

            dataPosition += BlockSize;
        }

        return [.. transformData];
    }

    public static byte[] InverseData(byte[] data)
    {
        int dataLength = data.Length;
        int dataPosition = 0;
        List<byte> decodeData = [];

        while (dataPosition < dataLength)
        {
            byte[] numberBytes = new byte[sizeof(ushort)];
            Array.Copy(data, dataPosition, numberBytes, 0, numberBytes.Length);
            ushort number = BitConverter.ToUInt16(numberBytes);

            dataPosition += sizeof(ushort);

            byte[] buffer = new byte[BlockSize];
            if (dataLength - dataPosition < BlockSize)
            {
                Array.Resize(ref buffer, dataLength - dataPosition);
            }
            Array.Copy(data, dataPosition, buffer, 0, buffer.Length);

            decodeData.AddRange(Inverse(buffer, number));

            dataPosition += BlockSize;
        }

        return [.. decodeData];
    }

    public static void PrintByteArray(byte[] input, Func<byte, object>? format = null)
    {
        foreach (byte item in input)
        {
            Console.Write(format != null ? format(item) : item + " ");
        }
        Console.WriteLine();
    }
}
