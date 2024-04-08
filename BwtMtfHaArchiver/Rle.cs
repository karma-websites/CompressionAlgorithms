namespace BwtMtfHaArchiver;

internal class Rle
{
    private static byte CalculateSpecialByte(byte[] data)
    {
        int[] freqs = new int[byte.MaxValue + 1];
        foreach (byte b in data) freqs[b]++;

        int minFreqs = freqs.Min();
        byte specialByte = (byte)Array.IndexOf(freqs, minFreqs);

        return specialByte;
    }

    public static byte[] Decompress(byte[] arch)
    {
        List<byte> decompressData = [];
        byte specialByte = arch[0];
        int i = 1;

        while (i < arch.Length)
        {
            if (arch[i] == specialByte)
            {
                for (int j = 0; j < arch[i + 1]; j++)
                {
                    decompressData.Add(arch[i + 2]);
                }
                i += 3;
            }
            else
            {
                decompressData.Add(arch[i]);
                i++;
            }
        }

        return [.. decompressData];
    }

    public static byte[] Compress(byte[] data)
    {
        byte specialByte = CalculateSpecialByte(data);
        List<byte> compressData = [];
        compressData.Add(specialByte);
        byte count = 1;

        for (int i = 0; i < data.Length; i++)
        {
            while (i < data.Length - 1 && data[i] == data[i + 1])
            {
                count++;
                i++;
                if (count == byte.MaxValue) break;
            }
            if (count > 3)
            {
                compressData.Add(specialByte);
                compressData.Add(count);
                compressData.Add(data[i]);
            }
            else
            {
                if (data[i] == specialByte)
                {
                    compressData.Add(specialByte);
                    compressData.Add(count);
                    count = 1;
                }
                for (int j = 0; j < count; j++)
                {
                    compressData.Add(data[i]);
                }
            }
            count = 1;
        }

        return [.. compressData];
    }
}
