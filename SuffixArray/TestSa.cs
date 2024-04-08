namespace SuffixArrayAlgorithm;

internal class TestSa
{
    static void Main()
    {
        string inputString = "абракадааааббрра";
        //string inputString = "banana$";

        Console.WriteLine($"Input data:\n{inputString}");
        for (int i = 0; i < inputString.Length; i++)
        {
            Console.Write($"{inputString[i], 3}");
        }
        Console.WriteLine();
        for (int i = 0; i < inputString.Length; i++)
        {
            Console.Write($"{i, 3}");
        }
        Console.WriteLine("\n");


        int[] suffixArray = SuffixArray.Compute(inputString);
        
        SuffixArray.ShowSuffixes(suffixArray, inputString);

        Console.WriteLine("Suffix array:");
        Array.ForEach(suffixArray, x => Console.Write(x + " "));
        Console.WriteLine("\n");

        string suffixTypes = SuffixArray.GetTypes(inputString);
        Console.WriteLine($"Types of suffixes version 1:\n{suffixTypes}\n{inputString}");

        Console.WriteLine();

        suffixTypes = SuffixArray.GetTypes2(inputString);
        Console.WriteLine($"Types of suffixes version 2:\n{suffixTypes}\n{inputString}");
    }
}
