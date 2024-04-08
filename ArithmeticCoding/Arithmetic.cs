namespace ArithmeticCoding;

internal class Arithmetic
{
    private record struct Segment(byte Symbol, decimal Left, decimal Right);

    public static Dictionary<byte, int> CreateDictionary(byte[] inputData)
    {
        Dictionary<byte, int> dict = [];

        foreach (byte b in inputData)
        {
            if (dict.TryGetValue(b, out int value)) dict[b] = ++value;
            else dict.Add(b, 1);
        }

        return dict;
    }

    public static byte[] FindAlphabet(Dictionary<byte, int> dict)
    {
        return [.. dict.Keys];
    }

    public static decimal[] FindProbability(Dictionary<byte, int> dict, int lengthData)
    {
        List<decimal> probability = [];

        foreach (var letter in dict)
        {
            probability.Add((decimal)letter.Value / lengthData);
        }

        return [.. probability];
    }

    private static Segment[] DefineSegment(byte[] alphabet, decimal[] probability)
    {
        Segment[] segments = new Segment[alphabet.Length];
        decimal left = 0;

        for (int i = 0; i < alphabet.Length; i++)
        {
            segments[i].Left = left;
            segments[i].Right = left + probability[i];
            segments[i].Symbol = alphabet[i];
            left = segments[i].Right;
        }

        return segments;
    }

    private static bool CheckBorders(Segment[] segments, byte[] alphabet, decimal newLeft, decimal newRight)
    {
        bool equalBorders = false;

        for (int j = 0; j < alphabet.Length; j++) // проверка новых границ на вместимость всего алфавита
        {
            decimal alphabetLeft = newLeft + (newRight - newLeft) * segments[j].Left;
            decimal alphabetRight = newLeft + (newRight - newLeft) * segments[j].Right;

            if (alphabetRight.Equals(alphabetLeft))
            {
                equalBorders = true;
                break;
            }
        }

        return equalBorders;
    }

    public static List<Tuple<decimal, byte>> Encode(Dictionary<byte, int> dict, byte[] inputData)
    {
        byte[] alphabet = FindAlphabet(dict);
        decimal[] probability = FindProbability(dict, inputData.Length);
        Segment[] segments = DefineSegment(alphabet, probability);

        List<Tuple<decimal, byte>> codes = [];

        decimal left = 0;
        decimal right = 1;
        decimal oldLeft = left;
        decimal oldRight = right;

        int i = 0;
        int count = 0;

        while (i < inputData.Length)
        {
            int symbolIndex = Array.IndexOf(alphabet, inputData[i]);
            decimal newLeft = left + (right - left) * segments[symbolIndex].Left;
            decimal newRight = left + (right - left) * segments[symbolIndex].Right;

            if (CheckBorders(segments, alphabet, newLeft, newRight) == true)
            {
                codes.Add(new Tuple<decimal, byte>((left + right) / 2, (byte)count));
                count = 0; left = 0; right = 1;
            }
            else if (i == inputData.Length - 1)
            {
                codes.Add(new Tuple<decimal, byte>((newLeft + newRight) / 2, (byte)(count + 1)));
                i++;
            }
            else
            {
                left = newLeft;
                right = newRight;
                count++;
                i++;
            }
        }

        return codes;
    }

    public static byte[] Decode(Dictionary<byte, int> dict, List<Tuple<decimal, byte>> codes, int lengthData)
    {
        byte[] alphabet = FindAlphabet(dict);
        decimal[] probability = FindProbability(dict, lengthData);
        Segment[] segments = DefineSegment(alphabet, probability);

        byte[] result = new byte[lengthData];

        int resultIndex = 0;
        for (int codeIndex = 0; codeIndex < codes.Count; codeIndex++)
        {
            decimal code = codes[codeIndex].Item1;
            for (int i = 0; i < codes[codeIndex].Item2; i++)
            {
                for (int j = 0; j < alphabet.Length; j++)
                {
                    if (code >= segments[j].Left && code < segments[j].Right)
                    {
                        result[resultIndex] = segments[j].Symbol;
                        code = (code - segments[j].Left) / (segments[j].Right - segments[j].Left);
                        break;
                    }
                }
                resultIndex++;
            }
        }

        return result;
    }
}
