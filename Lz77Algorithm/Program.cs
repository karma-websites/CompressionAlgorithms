namespace Lz77Algorithm;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection.Emit;

class LZ77
{
    public const int WindowSize = 4095; // 15;
    public const int BufferSize = 15; // 5;
    public static int OffsetBits = (int)Math.Ceiling(Math.Log2(WindowSize + 1));
    public static int LengthBits = (int)Math.Ceiling(Math.Log2(BufferSize + 1));
    public static int PairBits = OffsetBits + LengthBits;

    public struct Node(int offset, int length, byte next = 0)
    {
        public int Offset = offset;
        public int Length = length;
        public byte Symbol = next;

        public override readonly string ToString()
        {
            return $"({Offset} {Length} {Symbol})";
        }
    }

    public static void Main()
    {
        Console.WriteLine("Тест 1");

        byte[] input = File.ReadAllBytes("text/enwik7");
        Console.WriteLine(input.Length);

        /*(byte[] arch, byte emptyBits) = Encode(input);
        Console.WriteLine(arch.Length);
        File.WriteAllBytes("text/compress_enwik7", arch);*/

        byte[] arch = File.ReadAllBytes("text/compress_enwik7");
        byte[] decodeInput = Decode(arch, 0);
        File.WriteAllBytes("text/decompress_text.txt", decodeInput);
        Console.WriteLine(decodeInput.Length);

        bool flag = WorkFile.Compare("text/text.txt", "text/decompress_text.txt");
        Console.WriteLine(flag);

        /*List<Node> list = [];
        list.Add(new Node(32, 2));
        *//*list.Add(new Node(0, 0, 'b'));
        list.Add(new Node(0, 0, 'a'));
        list.Add(new Node(0, 0, 'c'));
        list.Add(new Node(4, 3));
        list.Add(new Node(6, 5));
        list.Add(new Node(0, 0, 'c'));*//*

        (byte[] data, byte emptyBits) = PairsToBytes(list);

        foreach (byte elem in data)
        {
            Console.WriteLine(elem);
        }

        BytesToPairs(data, emptyBits);*/
    }

    public static (byte[], byte) Encode(byte[] data)
    { 
        List<Node> encodedList = [];

        int pos = 0;

        Stopwatch stopwatch = new();
        stopwatch.Start();

        while (pos < data.Length)
        {
            FindMatch(data, pos, out int offset, out int length);

            if (offset == 0 && length == 0 || length * 8 <= PairBits)
            {
                encodedList.Add(new Node(0, 0, data[pos]));
                pos += 1;
            }
            else
            {
                encodedList.Add(new Node(offset, length));
                pos += length;
            }
        }

        stopwatch.Stop();
        Console.WriteLine(stopwatch.ElapsedMilliseconds);

        stopwatch.Restart();
        var v = PairsToBytes2(encodedList);
        stopwatch.Stop();
        Console.WriteLine(stopwatch.ElapsedMilliseconds);

        return (v, 1);
    }

    public static (byte[], byte) PairsToBytes(List<Node> listPairs)
    {
        List<bool> listBits = [];

        foreach (Node elem in listPairs)
        {
            if (elem.Offset == 0 && elem.Length == 0) // Pair: (0, 0, symbol)
            {
                listBits.Add(false);
                for (int i = 0, bit = (int)Math.Pow(2, 7); i < 8; i++, bit >>= 1)
                {
                    listBits.Add((elem.Symbol & bit) != 0);
                }
            }
            else // Pair: (number1, number2, void)
            {
                listBits.Add(true);
                for (int i = 0, bit = (int)Math.Pow(2, OffsetBits - 1); i < OffsetBits; i++, bit >>= 1)
                {
                    listBits.Add((elem.Offset & bit) != 0);
                }
                for (int i = 0, bit = (int)Math.Pow(2, LengthBits - 1); i < LengthBits; i++, bit >>= 1)
                {
                    listBits.Add((elem.Length & bit) != 0);
                }
            }
        }

        byte emptyBits = (byte)(8 - listBits.Count % 8);

        return (BitsToBytes(listBits), emptyBits);
    }

    public static byte[] PairsToBytes2(List<Node> listPairs)
    {
        List<byte> listBytes = [];
        foreach (Node elem in listPairs)
        {
            if (elem.Offset == 0 && elem.Length == 0)
            {
                listBytes.Add(elem.Symbol);
            }
            else
            {
                byte[] pair = new byte[(PairBits + 7) / 8];
                for (int j = 0, offsetBits = OffsetBits; j < OffsetBits; j++, offsetBits--)
                {
                    int byteIndex = j / 8;
                    int bitIndex = j % 8;

                    byte bit = (byte)(elem.Offset & (1 << (offsetBits - 1)));

                    if (bit != 0)
                        pair[byteIndex] |= (byte)(1 << (7 - bitIndex));  
                }
                for (int j = OffsetBits, lengthBits = LengthBits; j < PairBits; j++, lengthBits--)
                {
                    int byteIndex = j / 8;
                    int bitIndex = j % 8;

                    byte bit = (byte)(elem.Length & (1 << (lengthBits - 1)));

                    if (bit != 0)
                        pair[byteIndex] |= (byte)(1 << (7 - bitIndex));
                }
                listBytes.AddRange(pair);
            }
        }

        return [..CreateFlagTable(listPairs), .. listBytes];
    }

    public static byte[] CreateFlagTable(List<Node> listPairs)
    {
        int flagTableSize = listPairs.Count;
        int numBytes = (flagTableSize + 7) / 8;
        byte[] bytes = BitConverter.GetBytes(flagTableSize);

        Array.Resize(ref bytes, sizeof(int) + numBytes);

        int i = sizeof(int);
        byte mask = (byte)Math.Pow(2, 7);
        byte sum = 0;
        foreach (Node elem in listPairs)
        {
            if (elem.Offset != 0 && elem.Length != 0) sum |= mask;

            if (mask > 0) mask >>= 1;
            else
            {
                bytes[i++] = sum;
                sum = 0;
                mask = (byte)Math.Pow(2, 7);
            }
        }

        return bytes;
    }

    public static List<Node> BytesToPairs(byte[] data, byte emptyBits)
    {
        List<Node> listPairs = [];
        bool[] bits = [.. BytesToBits(data)];

        int i = 0;
        while (i < bits.Length - emptyBits)
        {
            byte symbol = 0;
            int offset = 0;
            int length = 0;

            if (bits[i] == false)
            {
                i++;
                for (byte j = 0, bit = (byte)Math.Pow(2, 7); j < 8; j++, bit >>= 1)
                {
                    if (bits[i + j] == true) symbol |= bit;
                }
                i += 8;
                listPairs.Add(new Node(0, 0, symbol));
            }
            else
            {
                i++;
                for (int j = 0, bit = (int)Math.Pow(2, OffsetBits - 1); j < OffsetBits; j++, bit >>= 1)
                {
                    if (bits[i + j] == true) offset |= bit;
                }
                i += OffsetBits;
                for (int j = 0, bit = (int)Math.Pow(2, LengthBits - 1); j < LengthBits; j++, bit >>= 1)
                {
                    if (bits[i + j] == true) length |= bit;
                }
                i += LengthBits;
                listPairs.Add(new Node(offset, length));
            }
        }

        return listPairs;
    }

    public static List<Node> BytesToPairs2(byte[] data)
    {
        List<Node> listPairs = [];
        bool[] bits = [.. BytesToBits(data)];
        int flagTableSize = BitConverter.ToInt32(data, 0);
        int j = flagTableSize + sizeof(int) * 8;

        for (int i = 0; i < flagTableSize; i++)
        {
            if (bits[i + sizeof(int) * 8] == false)
            {
                byte symbol = 0;
                for (int k = 0; k < 8; k++)
                {
                    if (bits[j + k] == true)
                        symbol |= (byte)(1 << (7 - k));
                }
                j += 8;
                listPairs.Add(new Node(0, 0, symbol));
            }
            else
            {
                int offset = 0;
                int length = 0;
                for (int k = 0; k < OffsetBits; k++)
                {
                    if (bits[j + k] == true)
                        offset |= (byte)(1 << (OffsetBits - 1 - k));
                }
                j += OffsetBits;
                for (int k = 0; k < LengthBits; k++)
                {
                    if (bits[j + k] == true)
                        length |= (byte)(1 << (LengthBits - 1 - k));
                }
                j += LengthBits;
                listPairs.Add(new Node(offset, length));
            }
        }

        return listPairs;
    }

    public static byte[] BitsToBytes(List<bool> listBits)
    {
        int numBytes = (listBits.Count + 7) / 8;
        byte[] bytes = new byte[numBytes];

        int i = 0;
        foreach (var bit in listBits)
        {
            int byteIndex = i / 8;
            int bitIndex = i % 8;

            if (listBits[i++])
            {
                bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));
            }
        }

        return bytes;
    }

    public static List<bool> BytesToBits(byte[] bytes)
    {
        List<bool> listBits = [];

        for (int i = 0; i < bytes.Length; i++)
        {
            for (int j = 0, bit = (int)Math.Pow(2, 7); j < 8; j++, bit >>= 1)
            {
                listBits.Add((bytes[i] & bit) != 0);
            }
        }

        return listBits;
    }

    public static void FindMatch(byte[] data, int pos, out int offset, out int length)
    {
        offset = 0;
        length = 0;
        
        int bestMatchDistance = -1;
        int bestMatchLength = -1;

        int startWindow = Math.Max(pos - WindowSize, 0);
        int endBuffer = Math.Min(pos + BufferSize, data.Length) + 1;

        for (int j = pos + 1; j < endBuffer; j++)
        {
            int indexBuffer = IndexOf1(startWindow, pos, j, data);

            if (indexBuffer != -1)
            {
                bestMatchDistance = pos - startWindow - indexBuffer;
                bestMatchLength = j - pos;
            }
            else break;
        }

        if (bestMatchDistance > 0 && bestMatchLength > 0)
        {
            offset = bestMatchDistance;
            length = bestMatchLength;
        }
    }

    public static int IndexOf2(int startWindow, int pos, int endBuffer, byte[] data)
    {

        return -1;
    }

    public static int IndexOf1(int startWindow, int pos, int endBuffer, byte[] data)
    {
        int windowLength = pos - startWindow;
        int bufferLength = endBuffer - pos;

        if (windowLength < bufferLength) return -1;

        uint p = 31;
        uint windowHash = 0;
        uint bufferHash = 0;
        uint maxPower = 1;

        for (int i = 0; i < bufferLength; i++)
        {
            windowHash = windowHash * p + data[startWindow + i];
            bufferHash = bufferHash * p + data[pos + i];
            maxPower *= p;
        }

        maxPower /= p;

        for (int i = 0; i < windowLength - bufferLength; i++)
        {
            if (windowHash == bufferHash)
            {
                if (Equals(startWindow, pos, endBuffer, data, i)) return i;
            }

            windowHash -= data[startWindow + i] * maxPower;
            windowHash = windowHash * p + data[startWindow + bufferLength + i];
        }

        return -1;
    }

    public static bool Equals(int startWindow, int pos, int endBuffer, byte[] data, int index)
    {
        int bufferLength = endBuffer - pos;
        for (int i = 0; i < bufferLength; i++)
        {
            if (data[startWindow + index + i] != data[pos + i]) return false;
        }

        return true;
    }

    public static byte[] Decode(byte[] encodedData, byte emptyBits)
    {
        List<Node> encodedList = BytesToPairs2(encodedData);
        List<byte> result = [];

        foreach (var elem in encodedList)
        {
            if (elem.Offset == 0 && elem.Length == 0)
            {
                result.Add(elem.Symbol);
            }
            else
            {
                int startIndex = result.Count - elem.Offset;
                if (elem.Offset >= elem.Length)
                {
                    result.AddRange(result[startIndex..(startIndex + elem.Length)]);
                }
                else
                {
                    int i = elem.Length;
                    while (i > 0)
                    {
                        int length = Math.Min(i, elem.Offset);
                        result.AddRange(result[startIndex..(startIndex + length)]);
                        i -= elem.Offset;
                    }
                }
            }
        }

        return [.. result];
    }

    /*static void Main()
    {
        *//*string inputFileName = "text/text.txt";
        string compressFileName = "text/compress_text.txt";
        string dedcompressFileName = "text/decompress_text.txt";*//*

        string inputFileName = "text/enwik7";
        string compressFileName = "text/compress_enwik7.txt";
        string decompressFileName = "text/decompress_enwik7.txt";

        Lzss.Encode(inputFileName, compressFileName);
        Lzss.Decode(compressFileName, decompressFileName);

        WorkFile.Compare(inputFileName, decompressFileName);
    }*/
}
