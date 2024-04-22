using System.Text;

namespace MtfAlgorithm;

internal class TestMtf
{
    private static void Main()
    {
        Test1();
        //Test2();
        //Test3();
    }

    private static void Test1()
    {
        Console.WriteLine("Testing the MTF algorithm\n");

        string[] testInputs = ["bbbbbcccccdddddaaaaa", "bananaaa", "hiphophiphop", "ABC$BAAA", "abracadabra"];
        foreach (string s in testInputs)
        {
            byte[] input = Encoding.UTF8.GetBytes(s);
            Console.Write("Input string: ");
            Mtf.PrintByteArray(input, b => (char)b);

            byte[] encoding = Mtf.Encode(input);
            Console.Write("Encoded string: ");
            Mtf.PrintByteArray(encoding);

            byte[] decoding = Mtf.Decode(encoding);
            Console.Write("Decoded string: ");
            Mtf.PrintByteArray(decoding, b => (char)b);

            Console.WriteLine();
        }  
    }

    private static void Test2()
    {
        using FileStream inputFile = new("enwik7.txt", FileMode.Open, FileAccess.Read);
        byte[] text = new byte[inputFile.Length];
        inputFile.Read(text);
        inputFile.Close();

        byte[] encodeArr = Mtf.Encode(text);

        using FileStream encodeFile = new("encodeEnwik7.txt", FileMode.Create, FileAccess.Write);
        encodeFile.Write(encodeArr);
        encodeFile.Close();

        using FileStream encodeFile2 = new("encodeEnwik7.txt", FileMode.Open, FileAccess.Read);
        byte[] text2 = new byte[encodeFile2.Length];
        encodeFile2.Read(text2);
        encodeFile2.Close();

        byte[] decodeArr = Mtf.Decode(text2);

        using FileStream decodeFile = new("decodeEnwik7.txt", FileMode.Create, FileAccess.Write);
        decodeFile.Write(decodeArr);
        decodeFile.Close();

        Console.WriteLine(CompareFiles("enwik7.txt", "decodeEnwik7.txt"));
    }

    private static void Test3()
    {
        using FileStream inputFile = new("text.txt", FileMode.Open, FileAccess.Read);
        byte[] text = new byte[inputFile.Length];
        inputFile.Read(text);
        inputFile.Close();

        byte[] encodeArr = Mtf.Encode(text, "text.txt");

        using FileStream encodeFile = new("encodeText.txt", FileMode.Create, FileAccess.Write);
        encodeFile.Write(encodeArr);
        encodeFile.Close();


        using FileStream encodeFile2 = new("encodeText.txt", FileMode.Open, FileAccess.Read);
        byte[] text2 = new byte[encodeFile2.Length];
        encodeFile2.Read(text2);
        encodeFile2.Close();

        byte[] decodeArr = Mtf.Decode(text2, "text.txt");

        using FileStream decodeFile = new("decodeText.txt", FileMode.Create, FileAccess.Write);
        decodeFile.Write(decodeArr);
        decodeFile.Close();

        Console.WriteLine(CompareFiles("text.txt", "decodeText.txt"));
    }

    private static bool CompareFiles(string filePath1, string filePath2)
    {
        try
        {
            using FileStream file1 = new(filePath1, FileMode.Open, FileAccess.Read);
            using FileStream file2 = new(filePath2, FileMode.Open, FileAccess.Read);

            if (file1.Length != file2.Length)
            {
                Console.WriteLine($"Size file 1: {file1.Length}");
                Console.WriteLine($"Size file 2: {file2.Length}");
                return false;
            }

            for (int i = 0; i < file1.Length; i++)
            {
                if (file1.ReadByte() != file2.ReadByte())
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
