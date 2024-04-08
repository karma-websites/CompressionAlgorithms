namespace Lz77Algorithm;

using System;
using System.Collections.Generic;


class LZ77
{
    // Функция кодирования алгоритма LZ77
    public static List<(int, int, char)> Encode(string input, int windowSize, int bufferSize)
    {
        List<(int, int, char)> encodedData = [];
        int index = 0;

        while (index < input.Length)
        {
            int maxLength = Math.Min(bufferSize, input.Length - index);
            int maxMatchLength = 0;
            int maxMatchDistance = 0;

            // Поиск наилучшего совпадения в окне и буфере
            for (int i = 1; i <= Math.Min(windowSize, index); i++)
            {
                int matchLength = 0;
                while (matchLength < maxLength && input[index - i + matchLength] == input[index + matchLength])
                {
                    matchLength++;
                }

                if (matchLength > maxMatchLength)
                {
                    maxMatchLength = matchLength;
                    maxMatchDistance = i;
                }
            }

            // Добавление токена в закодированные данные
            encodedData.Add((maxMatchDistance, maxMatchLength, (index + maxMatchLength < input.Length) ? input[index + maxMatchLength] : '\0'));

            index += (maxMatchLength + 1);
        }

        return encodedData;
    }

    // Функция декодирования алгоритма LZ77
    public static string Decode(List<(int, int, char)> encodedData)
    {
        string decodedString = "";

        foreach (var tuple in encodedData)
        {
            int distance = tuple.Item1;
            int length = tuple.Item2;
            char nextChar = tuple.Item3;

            if (distance == 0)
            {
                decodedString += nextChar;
            }
            else
            {
                int startPos = decodedString.Length - distance;
                for (int i = 0; i < length; i++)
                {
                    decodedString += decodedString[startPos + i];
                }
                decodedString += nextChar;
            }
        }

        return decodedString;
    }

    static void Main()
    {
        string input = "abraabracadabraabra"; // Входная строка
        Console.WriteLine(input.Length);
        int windowSize = 30; // Размер окна
        int bufferSize = 30; // Размер буфера

        List<(int, int, char)> encodedData = Encode(input, windowSize, bufferSize);
        Console.WriteLine("Encoded data:");
        foreach (var tuple in encodedData)
        {
            Console.WriteLine(tuple);
        }

        string decodedString = Decode(encodedData);
        Console.WriteLine("Decoded string: " + decodedString);
    }
}
