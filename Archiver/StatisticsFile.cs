namespace SuperArchiver;

internal class StatisticsFile
{
    readonly FileStream inputFile;
    readonly SortedDictionary<byte, int> dict;

    public byte[] Alphabet => dict.Keys.ToArray();

    public int[] Freqs => dict.Values.ToArray();

    public StatisticsFile(string dataFileName)
    {
        inputFile = new(dataFileName, FileMode.Open, FileAccess.Read);
        dict = CreateDictionary();
    }

    public void Print()
    {
        double[] probability = Probability();

        Console.WriteLine("\nStatistics of data in a file\n");
        Console.WriteLine("byte\tsymbol\tfrequency\tprobability");
        Console.WriteLine(new string('=', 45));
        int i = 0;
        foreach (var pair in dict)
        {
            if (pair.Key < 20)
                Console.WriteLine($"{pair.Key,3}\tspec\t{pair.Value}\t\t{probability[i++]:f4}");
            else
                Console.WriteLine($"{pair.Key,3}\t{(char)pair.Key}\t{pair.Value}\t\t{probability[i++]:f4}");
        }

        Console.WriteLine($"\nFile entropy: {Entropy():f}");
        Console.WriteLine($"\nFile size: {inputFile.Length}");
    }

    public double Entropy()
    {
        double[] probabilities = Probability();

        double entropy = 0;
        foreach (double prob in probabilities)
        {
            if (prob > 0)
                entropy -= prob * Math.Log2(prob);
        }
        return entropy;
    }

    private SortedDictionary<byte, int> CreateDictionary()
    {
        SortedDictionary<byte, int> dict = [];

        for (int i = 0; i < inputFile.Length; i++)
        {
            byte b = (byte)inputFile.ReadByte();
            if (dict.TryGetValue(b, out int value)) dict[b] = ++value;
            else dict.Add(b, 1);
        }

        return dict;
    }

    public double[] Probability()
    {
        double[] probabilities = new double[inputFile.Length];

        int i = 0;
        foreach (var letter in dict)
        {
            probabilities[i++] = (double)letter.Value / inputFile.Length;
        }

        return probabilities;
    }
}
