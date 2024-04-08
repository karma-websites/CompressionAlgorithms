namespace SuffixArrayAlgorithm;

internal class SuffixArray
{
    private static int CompareSuffix(int xSuffix, int ySuffix, string inputData)
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

    public static int[] Compute(string inputData)
    {
        int lengthData = inputData.Length;

        int[] suffixArray = Enumerable.Range(0, lengthData).ToArray();

        Array.Sort(suffixArray, (x, y) => CompareSuffix(x, y, inputData));

        return suffixArray;
    }

    public static void ShowSuffixes(int[] suffixArray, string inputData)
    {
        Console.WriteLine("Suffixes:");
        for (int i = 0;i < suffixArray.Length;i++)
        {
            Console.WriteLine($"{suffixArray[i], 2} {inputData[suffixArray[i]..]}");
        }
        Console.WriteLine();
    }

    public static string GetTypes(string inputData)
    {
        int lengthData = inputData.Length;
        char[] typesArray = new char[lengthData];
        char type = '*';

        for (int i = 0; i < lengthData - 1; i++)
        {
            if (CompareSuffix(i, i + 1, inputData) < 0)
            {
                if (type == 'L') typesArray[i] = '*';
                else typesArray[i] = 'S';
            }
            else typesArray[i] = 'L';

            type = typesArray[i];
        }

        typesArray[lengthData - 1] = 'S';

        return new string(typesArray);
    }

    public static string GetTypes2(string inputData)
    {
        int lengthData = inputData.Length;
        char[] typesArray = new char[lengthData];

        for (int i = lengthData - 2; i >= 0; i--)
        {
            if (inputData[i] < inputData[i + 1])
                typesArray[i] = 'S';
            else if (inputData[i] > inputData[i + 1])
                typesArray[i] = 'L';
            else
                typesArray[i] = typesArray[i + 1];
        }

        for (int i = 0; i < lengthData - 1; i++)
        {
            if (typesArray[i] == 'L' && typesArray[i + 1] == 'S')
                typesArray[i + 1] = '*';
        }

        typesArray[lengthData - 1] = 'S';

        return new string(typesArray);
    }
}
