using System.IO.Compression;

namespace Test;

public static class FileCompressionModeExample
{
    private const string Message = "Lorem ipsum dolor sit amet";
    private const string OriginalFileName = "enwik7";
    private const string CompressedFileName = "compressed_enwik7.dfl";
    private const string DecompressedFileName = "decompressed_enwik7";

    private static void Main()
    {
        CompressFile();
        DecompressFile();
        PrintResults();
    }

    private static void CreateFileToCompress() => File.WriteAllText(OriginalFileName, Message);

    private static void CompressFile()
    {
        using FileStream originalFileStream = File.Open(OriginalFileName, FileMode.Open);
        using FileStream compressedFileStream = File.Create(CompressedFileName);
        using var compressor = new DeflateStream(compressedFileStream, CompressionMode.Compress);
        originalFileStream.CopyTo(compressor);
    }

    private static void DecompressFile()
    {
        using FileStream compressedFileStream = File.Open(CompressedFileName, FileMode.Open);
        using FileStream outputFileStream = File.Create(DecompressedFileName);
        using var decompressor = new DeflateStream(compressedFileStream, CompressionMode.Decompress);
        decompressor.CopyTo(outputFileStream);
    }

    private static void PrintResults()
    {
        long originalSize = new FileInfo(OriginalFileName).Length;
        long compressedSize = new FileInfo(CompressedFileName).Length;
        long decompressedSize = new FileInfo(DecompressedFileName).Length;

        Console.WriteLine($"The original file '{OriginalFileName}' is {originalSize} bytes.");
        Console.WriteLine($"The compressed file '{CompressedFileName}' is {compressedSize} bytes.");
        Console.WriteLine($"The decompressed file '{DecompressedFileName}' is {decompressedSize} bytes.");
    }

    private static void DeleteFiles()
    {
        File.Delete(OriginalFileName);
        File.Delete(CompressedFileName);
        File.Delete(DecompressedFileName);
    }
}
