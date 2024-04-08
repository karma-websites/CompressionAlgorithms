using System.Diagnostics;

namespace BwtMtfHaArchiver;

internal class Program
{
    private static void Main()
    {
        Console.WriteLine("Archiver: RLE + BWT + MTF + HUFFMAN");
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
        string extensionFile = ".rbmh";
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

            default:
                Console.WriteLine("The selected function is not in the program\n");
                break;
        }
        Console.WriteLine();

        return true;
    }

    private static void PrintMenu()
    {
        Console.WriteLine("Menu");
        Console.WriteLine("0. Exit");
        Console.WriteLine("1. Compress the file");
        Console.WriteLine("2. Decompress the file");
        Console.WriteLine("3. Compare files");
        Console.Write("Select a function: ");
    }

    private static void CompressFile(string dataFileName, string archFileName)
    {
        Console.WriteLine($"Compression of the file {dataFileName}...\n");

        Stopwatch stopwatch = new();
        Stopwatch totalStopwatch = new();

        totalStopwatch.Start();

        stopwatch.Start();
        byte[] data = WorkFile.ReadBytes(dataFileName);
        Console.WriteLine($"{"The size of the input data to be read: ",-50} {data.Length} byte. \tWorking time: {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        byte[] rleData = Rle.Compress(data);
        stopwatch.Stop();
        Console.WriteLine($"{"RLE size data: ",-50} {rleData.Length} byte. \tWorking time: {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        byte[] bwtData = BwtByte.DirectData(rleData);
        stopwatch.Stop();
        Console.WriteLine($"{"BWT size data: ",-50} {bwtData.Length} byte. \tWorking time: {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        byte[] mtfData = Mtf.Encode(bwtData);
        stopwatch.Stop();
        Console.WriteLine($"{"MTF size data: ",-50} {mtfData.Length} byte. \tWorking time: {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        byte[] arch = Huffman.Compress(mtfData, out double averageLength);
        stopwatch.Stop();
        Console.WriteLine($"{"HUFFMAN size data: ",-50} {arch.Length} byte. \tWorking time: {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        WorkFile.WriteBytes(archFileName, arch);
        stopwatch.Stop();
        Console.WriteLine($"{"The size of the compressed data to be recorded: ",-50} {arch.Length} byte. \tWorking time: {stopwatch.ElapsedMilliseconds} ms");

        totalStopwatch.Stop();

        Console.WriteLine($"\nCompressed file {archFileName} was received\n");

        Console.WriteLine($"Total compression time: {totalStopwatch.ElapsedMilliseconds} ms");
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
        Stopwatch totalStopwatch = new();

        totalStopwatch.Start();

        stopwatch.Start();
        byte[] arch = WorkFile.ReadBytes(archFileName);
        Console.WriteLine($"{$"Data size to read from file: ",-50} {arch.Length} byte. \tWorking time: {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        byte[] mtfData = Huffman.Decompress(arch);
        Console.WriteLine($"{"{Inverse HUFFMAN size data: ",-50} {mtfData.Length} byte. \tWorking time: {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        byte[] bwtData = Mtf.Decode(mtfData);
        Console.WriteLine($"{$"Inverse Mtf size data: ",-50} {bwtData.Length} byte. \tWorking time: {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        byte[] rleData = BwtByte.InverseData(bwtData);
        Console.WriteLine($"{$"Inverse BWT size data: ",-50} {rleData.Length} byte. \tWorking time: {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        byte[] data = Rle.Decompress(rleData);
        Console.WriteLine($"{$"Inverse RLE size data: ",-50} {data.Length} byte. \tWorking time: {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        WorkFile.WriteBytes(dataFileName, data);
        stopwatch.Stop();
        Console.WriteLine($"{$"Size of compressed data to be written to file: ",-50} {data.Length} byte. \tWorking time: {stopwatch.ElapsedMilliseconds} ms");

        totalStopwatch.Stop();

        Console.WriteLine($"Decompressed file {dataFileName} was received");
        Console.WriteLine($"Decompression time: {totalStopwatch.ElapsedMilliseconds} ms\n");
    }
}
