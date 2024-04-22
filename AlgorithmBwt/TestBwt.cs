using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace BwtAlgorithm;

internal class TestBwt
{
    private static void Main()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        string[] testInputs = ["абракадабра", "bananaaa", "ABACABA", "ABRAKADABRA"];

        byte[] inputText = "ABRAKADABRA"u8.ToArray();

        string[] testFiles = ["text/text.txt", "images/house.bmp", "text/enwik7.txt",
            "images/gray.bmp", "images/color.bmp"];

        string[] testBwtFiles = ["text/text.bwt", "images/bwt_house.bmp", "text/bwt_enwik7.bwt", 
            "images/bwt_gray.bwt", "images/bwt_color.bwt"];

        string[] testInverseBwtFiles = ["text/inverse_bwt_text.txt","images/inverse_bwt_house.bmp", "text/inverse_bwt_enwik7.txt", 
            "images/inverse_bwt_gray.bmp", "images/inverse_bwt_color.bmp"];

        //Test1(testInputs[0]);
        //Test2(testInputs[0]);
        //Test3(inputText);

        /*for (int i = 2; i < 3; i++)
        {
            Test4(testFiles[i], testBwtFiles[i], testInverseBwtFiles[i]);
        }*/
    }


    private static void PrintString(string inputString)
    {
        Console.WriteLine(new string('=', 90));
        Console.WriteLine(inputString);
        Console.WriteLine(new string('=', 90) + "\n\n");
    }


    private static void Test1(string inputText)
    {
        PrintString("Testing the direct and reverse BWT conversion \nfor string with intermediate stages output");
        
        Console.WriteLine($"Original string:\n{inputText}\n\n");

        string bwt = BwtString.DirectVer1WithPrint(inputText);

        BwtString.InverseVer1WithPrint(bwt);

        Console.WriteLine("\n");
    }


    private static void Test2(string inputText)
    {
        PrintString("Testing the direct and reverse BWT conversion for string");

        Console.WriteLine($"Original string:\n{inputText}\n");

        string bwt = BwtString.DirectVer1(inputText);
        Console.WriteLine($"Result string BWT version 1:\n{bwt}\n");

        string original = BwtString.InverseVer1(bwt);
        Console.WriteLine($"Result inverse string BWT version 1:\n{original}\n");

        string bwt2 = BwtString.DirectVer2(inputText);
        Console.WriteLine($"Result string BWT version 2:\n{bwt}\n");

        string original2 = BwtString.InverseVer2(bwt2);
        Console.WriteLine($"Result inverse string BWT version 2:\n{original2}");

        Console.WriteLine("\n");
    }


    private static void Test3(byte[] inputText)
    {
        PrintString("Testing the direct and reverse BWT conversion for byte array");

        Console.WriteLine("Original byte array:");
        BwtByte.PrintByteArray(inputText);
        Console.WriteLine();

        (byte[] encodeText, ushort number) = BwtByte.DirectVer1(inputText);
        Console.WriteLine($"Result byte BWT version 1:");
        BwtByte.PrintByteArray(encodeText);
        Console.WriteLine();

        byte[] decodeText = BwtByte.InverseVer2(encodeText, number);
        Console.WriteLine($"Result inverse byte BWT version 2:");
        BwtByte.PrintByteArray(decodeText);
        Console.WriteLine();

        (byte[] encodeText2, ushort number2) = BwtByte.DirectVer2(inputText);
        Console.WriteLine($"Result byte BWT version 2:");
        BwtByte.PrintByteArray(encodeText2);
        Console.WriteLine();

        byte[] decodeText2 = BwtByte.InverseVer2(encodeText2, number2);
        Console.WriteLine($"Result inverse byte BWT version 2:");
        BwtByte.PrintByteArray(decodeText2);
        Console.WriteLine("\n");
    }


    private static void Test4(string pathInFile, string pathBwtFile, string pathInverseBwtFile)
    {
        try
        {
            PrintString("Testing direct and reverse BWT conversion based on byte arrays for files");

            Console.WriteLine($"The name of the input file: {pathInFile}");
            Console.WriteLine($"The name of the bwt file: {pathBwtFile}");
            Console.WriteLine($"The name of the inverse bwt file: {pathInverseBwtFile}\n");

            Console.Write("Efficiency factor for the RLE algorithm without using the BWT algorithm: ");
            using FileStream inputFile = new(pathInFile, FileMode.Open, FileAccess.Read);
            byte[] inputData = new byte[inputFile.Length];
            inputFile.Read(inputData);
            inputFile.Close();
            Console.WriteLine($"{BwtByte.EfficiencyFactor(inputData):N6}\n");

            Console.WriteLine($"Result - the result of comparing files {pathInFile} and {pathInverseBwtFile}\n");

            Console.WriteLine("\tBlock size\tCoefficient\tTime in ms\tResult");
            Console.WriteLine(new string('=', 90));

            int blockSize = 1000;
            int numBlocks = 5;
            int addNumber = 1000;

            for (int i = 0; i < numBlocks; i++)
            {
                Stopwatch stopwatch = new();

                BwtByte.BlockSize = blockSize;

                stopwatch.Start();
                BwtByte.DirectData(pathInFile, pathBwtFile);
                stopwatch.Stop();

                using FileStream bwtFile = new(pathBwtFile, FileMode.Open, FileAccess.Read);
                byte[] bwtData = new byte[bwtFile.Length];
                bwtFile.Read(bwtData);
                bwtFile.Close();

                double efficiency = BwtByte.EfficiencyFactor(bwtData);
                long time = stopwatch.ElapsedMilliseconds;

                BwtByte.InverseData(pathBwtFile, pathInverseBwtFile);

                bool compareFiles = CompareFiles(pathInFile, pathInverseBwtFile);

                Console.WriteLine($"{i + 1})\t{blockSize}\t\t{efficiency:N6}\t{time}\t\t{compareFiles}");

                blockSize += addNumber;
            }

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static bool CompareFiles(string filePath1, string filePath2)
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }
}
