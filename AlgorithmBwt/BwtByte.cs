namespace BwtAlgorithm;

internal class BwtByte
{
    public static int BlockSize { get; set; } = 5000;


    public static double EfficiencyFactor(byte[] inputData)
    {
        int totalSequenceLength = 0;
        int sequenceLength = 1;
        int numberSequences = 0;
        int textLength = inputData.Length;

        for (int i = 0; i < textLength - 1; i++)
        {
            if (inputData[i] == inputData[i + 1])
            {
                sequenceLength++;
            }
            else
            {
                if (sequenceLength > 2)
                {
                    numberSequences++;
                    totalSequenceLength += sequenceLength;
                }
                sequenceLength = 1;
            }
        }

        if (sequenceLength > 2)
        {
            numberSequences++;
            totalSequenceLength += sequenceLength;
        }

        int lengthNumbers = (int)Math.Ceiling((double)textLength / BlockSize) * sizeof(ushort);

        return (double)(totalSequenceLength - 3 * numberSequences) / (textLength + lengthNumbers);
    }


    private static int CompareByteArrays(byte[] x, byte[] y)
    {
        for (int i = 0; i < x.Length; i++)
        {
            if (!x[i].Equals(y[i])) return x[i].CompareTo(y[i]);
        }

        return 0;
    }

    public static (byte[], ushort) DirectVer1(byte[] inputData)
    {
        int lengthData = inputData.Length;
        byte[][] rotations = new byte[lengthData][];

        for (int i = 0; i < lengthData; i++)
        {
            rotations[i] = new byte[lengthData];
            Array.Copy(inputData, i, rotations[i], 0, lengthData - i);
            Array.Copy(inputData, 0, rotations[i], lengthData - i, i);
        }

        Array.Sort(rotations, CompareByteArrays);

        byte[] result = new byte[lengthData];
        ushort number = 0;
        for (ushort i = 0; i < lengthData; i++)
        {
            result[i] = rotations[i][lengthData - 1];
            if (rotations[i].SequenceEqual(inputData)) number = i;
        }

        return (result, number);
    }


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

    public static (byte[], ushort) DirectVer2(byte[] inputData)
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

    public static byte[] InverseVer2(byte[] bwt, ushort number)
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

    public static void DirectData(string pathInFile, string pathOutFile)
    {
        try
        {
            using FileStream inputFile = new(pathInFile, FileMode.Open, FileAccess.Read);
            using FileStream outputFile = new(pathOutFile, FileMode.Create, FileAccess.Write);

            long fileSize = inputFile.Length;
            long position = 0;

            while (position < fileSize)
            {
                byte[] buffer = new byte[BlockSize];
                int bytesRead = inputFile.Read(buffer);

                if (buffer.Length != bytesRead)
                {
                    buffer = buffer.Take(bytesRead).ToArray();
                }

                (byte[] encodeData, ushort number) = DirectVer2(buffer);

                byte[] numberBytes = BitConverter.GetBytes(number);
                outputFile.Write(numberBytes);

                outputFile.Write(encodeData);
               
                position += bytesRead;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }


    public static void InverseData(string pathInFile, string pathOutFile)
    {
        try
        {
            if (BlockSize > ushort.MaxValue)
                throw new Exception("The maximum block size (ushort.MaxValue) for the BWT algorithm has been exceeded");
            if (BlockSize < 0)
                throw new Exception("Invalid block size for the BWT algorithm");

            using FileStream inputFile = new(pathInFile, FileMode.Open, FileAccess.Read);
            using FileStream outputFile = new(pathOutFile, FileMode.Create, FileAccess.Write);

            long fileSize = inputFile.Length;
            long position = 0;

            while (position < fileSize)
            {
                byte[] numberBytes = new byte[sizeof(ushort)];
                inputFile.Read(numberBytes);
                ushort number = BitConverter.ToUInt16(numberBytes);

                byte[] buffer = new byte[BlockSize];
                int bytesRead = inputFile.Read(buffer);

                if (buffer.Length != bytesRead)
                {
                    buffer = buffer.Take(bytesRead).ToArray();
                }

                outputFile.Write(InverseVer2(buffer, number));

                position += bytesRead + sizeof(ushort);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
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
