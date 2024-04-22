using System;

namespace SuperArchiver;

internal class Mtf
{
    private static byte[] alphabet = [];

    public static byte[] Alphabet => alphabet;

    private static void SetByteAlphabet()
    {
        alphabet = new byte[byte.MaxValue + 1];
        for (int i = 0; i < alphabet.Length; i++)
        {
            alphabet[i] = (byte)i;
        }
    }

    private static byte Search(byte inputByte)
    {
        for (int i = 0; i < alphabet.Length; i++)
        {
            if (alphabet[i] == inputByte)
            {
                return (byte)i;
            }
        }

        return 0;
    }

    private static void MoveToFront(byte byteIndex)
    {
        byte toFront = alphabet[byteIndex];
        for (byte j = byteIndex; j > 0; j--)
        {
            alphabet[j] = alphabet[j - 1];
        }
        alphabet[0] = toFront;
    }

    public static byte[] Encode(byte[] input)
    {
        SetByteAlphabet();

        byte[] output = new byte[input.Length];
        for (int j = 0; j < input.Length; j++)
        {
            output[j] = Search(input[j]);
            MoveToFront(output[j]);
        }

        return output;
    }

    public static byte[] Decode(byte[] input)
    {
        SetByteAlphabet();

        byte[] output = new byte[input.Length];
        for (int j = 0; j < input.Length; j++)
        {
            output[j] = alphabet[input[j]];
            MoveToFront(input[j]);
        }

        return output;
    }

    public static void PrintByteArray(byte[] input, Func<byte, object>? format = null)
    {
        foreach (byte item in input)
        {
            Console.Write(format != null ? format(item) : item + " ");
        }
        Console.WriteLine();
    }
}
