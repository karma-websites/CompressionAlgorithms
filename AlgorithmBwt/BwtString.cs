namespace BwtAlgorithm;

internal class BwtString
{
    private const char CHAR_ETX = char.MinValue;

    public static int BlockSize { get; set; } = 5000;


    public static double EfficiencyFactor(string inputData)
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


    public static string DirectVer1(string inputData)
    {
        if (inputData.Any(text => text == CHAR_ETX))
        {
            throw new ArgumentException("Input can't contain ETX");
        }

        inputData += CHAR_ETX;

        int lengthData = inputData.Length;
        string[] rotations = new string[lengthData];

        for (int i = 0; i < lengthData; i++)
        {
            rotations[i] = string.Concat(inputData.AsSpan(i), inputData.AsSpan(0, i));
        }

        Array.Sort(rotations, StringComparer.Ordinal);

        char[] bwt = new char[lengthData];
        for (int i = 0; i < lengthData; i++)
        {
            bwt[i] = rotations[i][lengthData - 1];
        }

        return new string(bwt);
    }


    private static int CompareSuffixArray(int xSuffix, int ySuffix, string inputData)
    {
        int lengthData = inputData.Length;
        int lengthX = lengthData - xSuffix;
        int lengthY = lengthData - ySuffix;

        for (int i = 0; i < Math.Min(lengthX, lengthY); i++)
        {
            if (!inputData[xSuffix].Equals(inputData[ySuffix]))
            {
                return inputData[xSuffix].CompareTo(inputData[ySuffix]);
            }
            xSuffix++;
            ySuffix++;
        }

        return lengthX.CompareTo(lengthY);
    }

    private static int[] ComputeSuffixArray(string inputData)
    {
        int lengthData = inputData.Length;

        int[] suffixArray = Enumerable.Range(0, lengthData).ToArray();

        Array.Sort(suffixArray, (x, y) => CompareSuffixArray(x, y, inputData));

        return suffixArray;
    }

    public static string DirectVer2(string inputData)
    {
        if (inputData.Any(text => text == CHAR_ETX))
        {
            throw new ArgumentException("Input can't contain ETX");
        }

        inputData += CHAR_ETX;

        int[] suffixArr = ComputeSuffixArray(inputData);

        int lengthData = suffixArr.Length;
        char[] bwt = new char[lengthData];

        for (short i = 0; i < lengthData; i++)
        {
            bwt[i] = inputData[(suffixArr[i] - 1 + lengthData) % lengthData];
        }

        return new string(bwt);
    }


    public static string InverseVer1(string bwt)
    {
        int lengthBwt = bwt.Length;
        string[] table = new string[lengthBwt];

        for (int i = 0; i < lengthBwt; i++)
        {
            for (int j = 0; j < lengthBwt; j++)
            {
                table[j] = bwt[j] + table[j];
            }

            Array.Sort(table, StringComparer.Ordinal);
        }

        return table[0].TrimStart(CHAR_ETX);
    }


    private struct Pairs
    {
        public int Index;
        public char Element;
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

    public static string InverseVer2(string bwt)
    {
        int lengthBwt = bwt.Length;

        Pairs[] pairs = new Pairs[lengthBwt];
        for (int i = 0; i < lengthBwt; i++)
        {
            pairs[i].Index = i;
            pairs[i].Element = bwt[i];
        }

        Array.Sort(pairs, ComparePairs);

        int position = 0;
        char[] result = new char[lengthBwt];
        for (int i = 0; i < lengthBwt; i++)
        {
            position = pairs[position].Index;
            result[i] = bwt[position];
        }

        return new string(result).TrimStart(CHAR_ETX);
    }


    // Algorithm methods with actions output to the console

    public static string DirectVer1WithPrint(string inputData)
    {
        Console.WriteLine("Algorithm BWT\n");

        if (inputData.Any(text => text == '#'))
        {
            throw new ArgumentException("Input can't contain ETX");
        }

        inputData += '#';

        int lengthData = inputData.Length;
        string[] rotations = new string[lengthData];

        Console.WriteLine("Stage 1 \nCreating a cyclic shift matrix:\n");
        for (int i = 0; i < lengthData; i++)
        {
            rotations[i] = string.Concat(inputData.AsSpan(i), inputData.AsSpan(0, i));
            Console.WriteLine($"{inputData.AsSpan(i)}  {inputData.AsSpan(0, i)}");
        }

        Console.WriteLine("\nStage 2 \nSorting strings:\n");
        Array.Sort(rotations, StringComparer.Ordinal);

        foreach (string rotation in rotations)
        {
            Console.WriteLine(rotation);
        }
        Console.WriteLine("\nStage 3");

        char[] bwt = new char[lengthData];
        for (int i = 0; i < lengthData; i++)
        {
            bwt[i] = rotations[i][lengthData - 1];
        }
        string result = new(bwt);

        Console.WriteLine($"Result BWT version 1:\n{result}");

        return result;
    }


    public static string InverseVer1WithPrint(string bwt)
    {
        Console.WriteLine("\n\nInverse algorithm BWT\n");

        int lengthBwt = bwt.Length;
        string[] table = new string[lengthBwt];

        for (int i = 0; i < lengthBwt; i++)
        {
            Console.WriteLine($"Adding {i + 1}");
            for (int j = 0; j < lengthBwt; j++)
            {
                table[j] = bwt[j] + table[j];
                Console.WriteLine($"{table[j]} ");
            }

            Console.WriteLine($"\nSorting {i + 1}");
            Array.Sort(table, StringComparer.Ordinal);

            for (int j = 0; j < lengthBwt; j++)
            {
                Console.WriteLine($"{table[j]} ");
            }
            Console.WriteLine();
        }

        string result = table[0].TrimStart('#');
        Console.WriteLine($"Result inverse BWT version 1:\n{result}");

        return result;
    }


    // Converting a data file

    public static void DirectData(string pathInFile, string pathOutFile)
    {
        try
        {
            using StreamReader inputFile = new(pathInFile); //  Encoding.GetEncoding("windows-1251")
            using StreamWriter outputFile = new(pathOutFile, false);

            while (inputFile.Peek() != -1)
            {
                char[] buffer = new char[BlockSize];
                int charsRead = inputFile.Read(buffer);

                if (BlockSize != charsRead)
                {
                    buffer = buffer.Take(charsRead).ToArray();
                }

                outputFile.Write(DirectVer1(new string(buffer)));
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
            using StreamReader inputFile = new(pathInFile);
            using StreamWriter outputFile = new(pathOutFile, false);

            while (inputFile.Peek() != -1)
            {
                char[] buffer = new char[BlockSize + 1];
                int charsRead = inputFile.Read(buffer);

                if (BlockSize != charsRead)
                {
                    buffer = buffer.Take(charsRead).ToArray();
                }

                outputFile.Write(InverseVer1(new string(buffer)));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}