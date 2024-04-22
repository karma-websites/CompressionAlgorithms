namespace SuperArchiver;

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
                ushort number = BitConverter.ToUInt16(arch, i + 1);
                for (int j = 0; j < number; j++)
                {
                    decompressData.Add(arch[i + 3]);
                }
                i += 4;
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
        ushort count = 1;
        byte[] bytes;

        for (int i = 0; i < data.Length; i++)
        {
            while (i < data.Length - 1 && data[i] == data[i + 1])
            {
                count++;
                i++;
                if (count == ushort.MaxValue) break;
            }
            if (count > 4)
            {
                compressData.Add(specialByte);
                bytes = BitConverter.GetBytes(count);
                compressData.AddRange(bytes);
                compressData.Add(data[i]);
            }
            else
            {
                if (data[i] == specialByte)
                {
                    compressData.Add(specialByte);
                    bytes = BitConverter.GetBytes(count);
                    compressData.AddRange(bytes);
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
