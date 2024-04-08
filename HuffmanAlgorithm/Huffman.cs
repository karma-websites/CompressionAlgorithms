namespace HuffmanAlgorithm;

internal class Huffman
{
    public static void PrintFreqTable(byte[] arch)
    {
        Console.WriteLine("Frequency table:");
        if (arch[0] == 0)
        {
            for (int i = 0; i < byte.MaxValue + 1; i++) Console.Write(arch[i + 1] + " ");
            Console.WriteLine("\n");
        }
        else
        {
            int countBytes = 1;
            for (int i = 1; i < arch[0] * 2 + 1; i += 2)
            {
                Console.WriteLine($"{countBytes++}. {arch[i]} {arch[i + 1]}");
            }
            Console.WriteLine();
        }
    }

    public static byte[] Decompress(byte[] arch)
    {
        byte[] data = [];
        byte[] freqs = new byte[byte.MaxValue + 1];
        byte modeFreqsTable = arch[0];
        int dataLength, startIndexData;

        if (modeFreqsTable == 0)
        {
            Array.Copy(arch, 1, freqs, 0, freqs.Length);

            dataLength = BitConverter.ToInt32(arch, freqs.Length + 1);
            startIndexData = freqs.Length + 1 + sizeof(int);
        }
        else
        {
            for (int i = 1; i < modeFreqsTable * 2 + 1; i += 2)
            {
                freqs[arch[i]] = arch[i + 1];
            }

            dataLength = BitConverter.ToInt32(arch, modeFreqsTable * 2 + 1);
            startIndexData = modeFreqsTable * 2 + 1 + sizeof(int);
        }

        Node root = CreateHuffmanTree(freqs);
        data = DecompressBytes(arch, startIndexData, dataLength, root);

        return data;
    }

    private static byte[] DecompressBytes(byte[] arch, int startIndexData, int dataLength, Node root)
    {
        byte[] data = new byte[dataLength];
        Node curr = root;
        int sizeData = 0;

        for (int i = startIndexData; i < arch.Length; i++)
        {
            for (int bit = 1; bit <= 128; bit <<= 1)
            {
                if (curr.bit0 != null && curr.bit1 != null)
                {
                    bool zero = (arch[i] & bit) == 0;
                    if (zero) curr = curr.bit0;
                    else curr = curr.bit1;
                }

                if (curr.bit0 == null && curr.bit1 == null && sizeData < dataLength)
                {
                    data[sizeData++] = curr.symbol;
                    curr = root;
                }
            }
        }

        return data;
    }

    public static byte[] Compress(byte[] data, out double averageLength)
    {
        byte[] freqs = CalculateFreq(data);
        byte[] freqsTable = CreateFreqsTable(freqs);
        byte[] lengthData = BitConverter.GetBytes(data.Length);
        Node root = CreateHuffmanTree(freqs);
        string[] codes = CreateHuffmanCode(root);
        averageLength = CalculateAverageCodeLength(data, codes);
        byte[] bits = CompressBytes(data, codes);

        return [.. freqsTable, .. lengthData, .. bits];
    }

    private static byte[] CreateFreqsTable(byte[] freqs)
    {
        int sizeAlphabet = freqs.Count(i => i > 0);
        byte[] newBytesFreqs = [];

        if (sizeAlphabet < 128)
        {
            newBytesFreqs = new byte[sizeAlphabet * 2 + 1];
            newBytesFreqs[0] = (byte)sizeAlphabet;

            int j = 1;
            for (int i = 0; i < freqs.Length; i++)
            {
                if (freqs[i] > 0)
                {
                    newBytesFreqs[j] = (byte)i;
                    newBytesFreqs[j + 1] = freqs[i];
                    j += 2;
                }
            }
        }
        else
        {
            newBytesFreqs = new byte[freqs.Length + 1];
            newBytesFreqs[0] = 0;
            for (int i = 0; i < freqs.Length; i++)
            {
                newBytesFreqs[i + 1] = freqs[i];
            }
        }

        return newBytesFreqs;
    }

    private static double CalculateAverageCodeLength(byte[] data, string[] codes)
    {
        int[] intFreqs = new int[byte.MaxValue + 1];
        foreach (byte b in data) intFreqs[b]++;

        double averageCodeLength = 0;
        for (int i = 0; i < byte.MaxValue + 1; i++)
        {
            if (codes[i] != null)
            {
                double probability = (double)intFreqs[i] / data.Length;
                averageCodeLength += probability * codes[i].Length;
            }
        }

        return averageCodeLength;
    }

    private static byte[] CalculateFreq(byte[] data)
    {
        int[] freqs = new int[byte.MaxValue + 1];
        foreach (byte b in data) freqs[b]++;
        NormalizeFreqs();
        byte[] bytesFreqs = freqs.Select(f => (byte)f).ToArray();

        return bytesFreqs;

        void NormalizeFreqs()
        {
            int max = freqs.Max();

            if (max < byte.MaxValue + 1) return;

            for (int i = 0; i < byte.MaxValue + 1; i++)
            {
                if (freqs[i] > 0)
                {
                    freqs[i] = 1 + freqs[i] * byte.MaxValue / (max + 1);
                } 
            }
        }
    }

    private static Node CreateHuffmanTree(byte[] freqs)
    {
        PriorityQueue<Node> pq = new();

        for (int i = 0; i < freqs.Length; i++)
        {
            if (freqs[i] > 0)
            {
                pq.Enqueue(freqs[i], new Node((byte)i, freqs[i]));
            }
        }

        while (pq.Size > 1)
        {
            Node bit0 = pq.Dequeue();
            Node bit1 = pq.Dequeue();
            int freq = bit0.freq + bit1.freq;
            Node next = new(bit0, bit1, freq);
            pq.Enqueue(freq, next);
        }

        return pq.Dequeue();
    }

    private static string[] CreateHuffmanCode(Node root)
    {
        string[] codes = new string[byte.MaxValue + 1];
        Next(root);

        return codes;

        void Next(Node node, string code = "")
        {
            if (node.bit0 == null || node.bit1 == null)
            {
                codes[node.symbol] = code;
            }
            else
            {
                Next(node.bit0, code + "0");
                Next(node.bit1, code + "1");
            }
        }
    }

    private static byte[] CompressBytes(byte[] data, string[] codes)
    {
        List<byte> bits = [];
        byte sum = 0;
        byte bit = 1;

        foreach (byte symbol in data)
        {
            foreach (char c in codes[symbol])
            {
                if (c == '1') sum |= bit;
                if (bit < 128) bit <<= 1;
                else
                {
                    bits.Add(sum);
                    sum = 0;
                    bit = 1;
                }
            }
        }

        if (bit > 1) bits.Add(sum);

        return [.. bits];
    }
}
