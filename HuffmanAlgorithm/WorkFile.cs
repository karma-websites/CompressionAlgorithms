namespace HuffmanAlgorithm;

internal class WorkFile
{
    public static byte[] ReadBytes(string inputFileName)
    {
        using FileStream inputFile = new(inputFileName, FileMode.Open, FileAccess.Read);
        byte[] inputData = new byte[inputFile.Length];
        inputFile.Read(inputData);

        return inputData;
    }

    public static void WriteBytes(string archFileName, byte[] arch)
    {
        using FileStream archFile = new(archFileName, FileMode.Create, FileAccess.Write);
        archFile.Write(arch);
    }

    public static bool Compare(string filePath1, string filePath2)
    {
        using FileStream file1 = new(filePath1, FileMode.Open, FileAccess.Read);
        using FileStream file2 = new(filePath2, FileMode.Open, FileAccess.Read);

        if (!file1.Length.Equals(file2.Length))
        {
            Console.WriteLine($"Size file 1: {file1.Length}");
            Console.WriteLine($"Size file 2: {file2.Length}");
            return false;
        }

        for (int i = 0; i < file1.Length; i++)
        {
            if (!file1.ReadByte().Equals(file2.ReadByte()))
            {
                Console.WriteLine($"Position of a pair of different bytes: {file2.Position}");
                return false;
            }
        }

        return true;
    }
}
