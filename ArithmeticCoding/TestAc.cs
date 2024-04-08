namespace ArithmeticCoding;

internal class TestAc
{
    private static void Main()
    {
        Test1("text/data.txt");
        //Test1("images/color.bmp");
        //Test2("text/data.txt", "text/encode_data.txt");
    }

    private static void PrintDict(Dictionary<byte, int> dict)
    {
        Console.WriteLine("Dictionary: ");
        int j = 0;
        foreach (var item in dict)
        {
            Console.WriteLine($"{j}. {(char)item.Key} - {item.Value}");
            j++;
        }
        Console.WriteLine();
    }

    private static bool CompareArrays(byte[] inputData, byte[] decodeData)
    {
        for (int i = 0; i < inputData.Length; i++)
        {
            if (inputData[i] != decodeData[i])
            {
                Console.WriteLine("Arrays differ on an element with an index: " + i);
                return false;
            }
        }

        return true;
    }

    private static void Test1(string inputFilePath)
    {
        try
        {
            using FileStream inputFile = new(inputFilePath, FileMode.Open, FileAccess.Read);
            
            byte[] inputData = new byte[inputFile.Length];
            inputFile.Read(inputData);

            Dictionary<byte, int> dict = Arithmetic.CreateDictionary(inputData);
            PrintDict(dict);
            var codes = Arithmetic.Encode(dict, inputData);

            Console.WriteLine(codes.Count * 16 + dict.Count);

            byte[] decodeData = Arithmetic.Decode(dict, codes, inputData.Length);

            if (CompareArrays(inputData, decodeData) == true)
                Console.WriteLine("Arrays inputData and decodeData are equal");

            /*byte[] data = BitConverter.GetBytes(codes[0]);
            Array.ForEach(data, x => Console.Write(x + " "));
            Console.WriteLine();*/

            /*Console.WriteLine("\nDecode data:");
            Array.ForEach(decodeData, x => Console.Write((char)x));
            Console.WriteLine();*/
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void Test2(string inputFilePath, string encodeFilePath)
    {
        EncodeFile(inputFilePath, encodeFilePath);
    }

    private static void EncodeFile(string inputFilePath, string encodeFilePath)
    {
        try
        {
            using FileStream inputFile = new(inputFilePath, FileMode.Open, FileAccess.Read);
            using FileStream encodeFile = new(encodeFilePath, FileMode.Create, FileAccess.Write);

            byte[] inputData = new byte[inputFile.Length];
            inputFile.Read(inputData);

            Dictionary<byte, int> dict = Arithmetic.CreateDictionary(inputData);
            PrintDict(dict);
            var codes = Arithmetic.Encode(dict, inputData);

            for (int i = 0; i < codes.Count; i++)
            {
                byte[] decimalBytes = decimal.GetBits(codes[i].Item1).SelectMany(BitConverter.GetBytes).ToArray();
                encodeFile.Write(decimalBytes);
                encodeFile.WriteByte(codes[i].Item2);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void DecodeFile(string encodeFilePath, string decodeFilePath)
    {
        try
        {
            using FileStream encodeFile = new(encodeFilePath, FileMode.Open, FileAccess.Read);
            using FileStream decodeFile = new(decodeFilePath, FileMode.Create, FileAccess.Write);

           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
