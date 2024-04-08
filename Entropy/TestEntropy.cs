namespace Entropy;

internal class TestEntropy
{
    private static void Main()
    {
        double[] probabilities = [0.4, 0.2, 0.2, 0.2];
        double entropy = CalculateEntropy(probabilities);
        Console.WriteLine("Entropy: " + entropy);
    }
    
    private static double CalculateEntropy(double[] probabilities)
    {
        double entropy = 0;
        foreach (double prob in probabilities)
        {
            if (prob > 0)
                entropy -= prob * Math.Log2(prob);
        }
        return entropy;
    }
}
