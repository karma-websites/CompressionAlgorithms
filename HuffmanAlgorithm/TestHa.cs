using System.Diagnostics;

namespace HuffmanAlgorithm;

internal class TestHa
{
    private static void Main()
    {
        Console.WriteLine("Archiver: HUFFMAN");
        Console.WriteLine("Author: Popov Maxim");
        Console.WriteLine("Group: 2302\n\n");

        while (true)
        {
            try
            {
                PrintMenu();
                if (!SwitchMenu()) break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n\n");
            }
        }
    }

    private static bool SwitchMenu()
    {
        string dataFileName;
        string folderName;
        string extensionFile = ".huff";
        string compressFileName;
        string decompressFile;

        string mode = Console.ReadLine() ?? "0";
        Console.WriteLine("\n");

        switch (mode)
        {
            case "0": return false;

            case "1":
                Console.Write("Specify the path to the source file: ");
                dataFileName = Console.ReadLine() ?? "0";

                folderName = dataFileName[..(dataFileName.IndexOf('/') + 1)];
                compressFileName = folderName + "compress_" + dataFileName[folderName.Length..] + extensionFile;
                Console.WriteLine();

                CompressFile(dataFileName, compressFileName);

                break;

            case "2":
                Console.Write("Specify the path to the compressed file: ");
                compressFileName = Console.ReadLine() ?? "0";

                if (compressFileName.EndsWith(extensionFile))
                {
                    folderName = compressFileName[..(compressFileName.IndexOf('/') + 1)];
                    decompressFile = folderName + "de" + compressFileName[folderName.Length..][..^extensionFile.Length];
                    Console.WriteLine();

                    DecompressFile(compressFileName, decompressFile);
                }
                else
                {
                    Console.WriteLine($"The file extension does not match {extensionFile}\n");
                }

                break;

            case "3":
                Console.Write("Specify the path to the source file: ");
                dataFileName = Console.ReadLine() ?? "0";

                Console.Write("Specify the path to the decompressed file: ");
                decompressFile = Console.ReadLine() ?? "0";

                bool compareResult = WorkFile.Compare(dataFileName, decompressFile);
                Console.WriteLine($"The result of comparing the {dataFileName} and {decompressFile} files: {compareResult}\n");

                break;

            case "4":
                Console.Write("Specify the path to the source file: ");
                dataFileName = Console.ReadLine() ?? "0";

                byte[] data = WorkFile.ReadBytes(dataFileName);
                var canonCodes = Huffman.CreateCanonCodes(data);

                Huffman.PrintCanonCodes(canonCodes);

                break;

            default:
                Console.WriteLine("The selected function is not in the program\n");
                break;
        }
        Console.WriteLine();

        return true;
    }

    private static void PrintMenu()
    {
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine("Menu");
        Console.WriteLine("0. Exit");
        Console.WriteLine("1. Compress the file");
        Console.WriteLine("2. Decompress the file");
        Console.WriteLine("3. Compare files");
        Console.WriteLine("4. Get canonical Huffman codes");
        Console.Write("Select a function: ");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void CompressFile(string dataFileName, string archFileName)
    {
        Console.WriteLine($"Compression of the file {dataFileName}...");

        Stopwatch stopwatch = new();
        stopwatch.Start();
        byte[] data = WorkFile.ReadBytes(dataFileName);
        byte[] arch = Huffman.Compress(data, out double averageLength);
        WorkFile.WriteBytes(archFileName, arch);
        stopwatch.Stop();

        Console.WriteLine($"Compressed file {archFileName} was received\n");

        Huffman.PrintFreqTable(arch);
        Console.WriteLine($"Compression time: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"Source size:   {data.Length} byte");
        Console.WriteLine($"Compressed size: {arch.Length} byte");
        float compressPercent = (data.Length - (float)arch.Length) / data.Length * 100;
        Console.WriteLine($"Compression percentage: {compressPercent:f} %");
        StatisticsFile statistics = new(dataFileName);
        Console.WriteLine($"The entropy of the source file: {statistics.Entropy():f} bit");
        Console.WriteLine($"Average length of a code symbol in a compressed file: {averageLength:f} bit\n");
    }

    private static void DecompressFile(string archFileName, string dataFileName)
    {
        Console.WriteLine($"Decompression of the file {archFileName}...");

        Stopwatch stopwatch = new();
        stopwatch.Start();
        byte[] arch = WorkFile.ReadBytes(archFileName);
        byte[] data = Huffman.Decompress(arch);
        WorkFile.WriteBytes(dataFileName, data);
        stopwatch.Stop();

        Console.WriteLine($"Decompressed file {dataFileName} was received");
        Console.WriteLine($"Decompression time: {stopwatch.ElapsedMilliseconds} ms\n");
    }
}
