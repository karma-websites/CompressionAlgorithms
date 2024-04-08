namespace Test;

internal class Program
{
    static void Main(string[] args)
    {
        /*double number = 1.0/17179869184.0;
        byte[] data = BitConverter.GetBytes(number);
        Array.ForEach(data, x =>  Console.Write(x + " "));*/

        /*byte[] data = [1, 2, 3, 4, 5, 6, 7, 8];
        byte[] buffer = new byte[10];
        Array.Copy(data, 0, buffer, 3, 5);
        Array.ForEach(buffer, x => Console.Write(x + " "));*/

        string extensionFile = ".huff";
        string compressFileName = "text/compress_enwik7.txt.huff";
        string folderName = compressFileName[..(compressFileName.IndexOf('/') + 1)];
        string decompressFile = folderName + "de" + compressFileName[folderName.Length..][..^extensionFile.Length];
    }
}
