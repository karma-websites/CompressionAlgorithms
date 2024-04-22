﻿namespace Lz77Algorithm;

using System;
using System.IO;
using System.Text;

public static class Lzss
{
    private static readonly char[] SlidingWindow = new char[Constants.WindowSize];
    private static readonly char[] UncodedLookahead = new char[Constants.MaxCoded]; // Characters to be encoded

    private static readonly int[] HashTable = new int[Constants.HashSize]; // List head for each hask key
    private static readonly int[] Next = new int[Constants.WindowSize]; // Indices of next elements in the hash list

    public static void Encode(string inputFileNameForEncode, string outputFileNameForEncode)
    {
        using var reader = new StreamReader(inputFileNameForEncode);
        using var writer = new StreamWriter(outputFileNameForEncode, false, Encoding.UTF8);
        var readChar = 0;

        // Copy MAX_CODED bytes from the input file into the uncoded lookahead buffer
        var length = 0; // Length of string
        for (length = 0; length < Constants.MaxCoded; length++)
        {
            readChar = reader.Read();
            if (readChar == -1) break;

            UncodedLookahead[length] = (char)readChar;
        }

        InitializeDataStructures();

        // 8 code flags and encoded strings
        var flags = 0;
        var flagPosition = 1;
        var encodedData = new char[16];
        var nextEncoded = 0; // Encoded data next index

        var windowHead = 0; // Head of sliding window
        var uncodedHead = 0; // Head of uncoded lookahead

        var i = 0;

        var matchData = FindMatch(uncodedHead);

        // Now encoding the rest of the file
        while (length > 0)
        {
            // Garbage beyond last data expands match length
            if (matchData.Length > length)
            {
                matchData.Length = length;
            }

            // Not long enough match -> write uncoded byte
            if (matchData.Length <= Constants.MaxUncoded)
            {
                matchData.Length = 1; // Set to 1 for 1 byte uncoded
                flags |= flagPosition; // Mark with uncoded byte flag
                encodedData[nextEncoded] = UncodedLookahead[uncodedHead];
                nextEncoded++;
            }
            else // match.Length > MAX_UNCODED -> encode as offset and length
            {
                encodedData[nextEncoded] = (char)((matchData.Offset & 0x0FFF) >> 4);
                nextEncoded++;

                encodedData[nextEncoded] = (char)(((matchData.Offset & 0x000F) << 4) |
                    (matchData.Length - (Constants.MaxUncoded + 1)));
                nextEncoded++;
            }

            // We have 8 code flags -> write out flags and code buffer
            if (flagPosition == 0x80)
            {
                //Console.WriteLine("Offset: " + matchData.Offset + " Length: " + matchData.Length);

                writer.Write((char)flags);
                for (i = 0; i < nextEncoded; i++)
                {
                    writer.Write(encodedData[i]); // Writing the encoded data to the output file
                }

                flags = 0;
                flagPosition = 1;
                nextEncoded = 0;
            }
            else
            {
                flagPosition <<= 1;
            }

            // Replace the matchData.Length worth of bytes we've matched in the
            // sliding window with new bytes from the input file
            i = 0;
            while (i < matchData.Length)
            {
                readChar = reader.Read();
                if (readChar == -1) break;

                // Add old byte into sliding window and new into lookahead
                ReplaceChar(windowHead, UncodedLookahead[uncodedHead]);
                UncodedLookahead[uncodedHead] = (char)readChar;
                windowHead = (windowHead + 1) % Constants.WindowSize;
                uncodedHead = (uncodedHead + 1) % Constants.MaxCoded;
                i++;
            }

            // Handle case where we reach the end of file before filling lookahead
            while (i < matchData.Length)
            {
                ReplaceChar(windowHead, UncodedLookahead[uncodedHead]);

                // Nothing to add to lookahead here
                windowHead = (windowHead + 1) % Constants.WindowSize;
                uncodedHead = (uncodedHead + 1) % Constants.MaxCoded;
                length--;
                i++;
            }

            // Find match for the remaining characters
            matchData = FindMatch(uncodedHead);

            //PrintSlidingWindow();
            //PrintUncodedLookahead();
            //Console.WriteLine();
        }

        // Write out any remaining encoded data
        if (nextEncoded != 0)
        {
            writer.Write((char)flags);
            for (i = 0; i < nextEncoded; i++)
            {
                writer.Write(encodedData[i]);
            }
        }
    }

    public struct EncodedString
    {
        // Offset to start of longest match
        public int Offset { get; set; }

        // Length of longest match
        public int Length { get; set; }
    }

    public static void Decode(string inputFileNameForDecode, string outputFileNameForDecode)
    {
        using var reader = new StreamReader(inputFileNameForDecode);
        using var writer = new StreamWriter(outputFileNameForDecode);

        InitializeDataStructures();

        var flags = 0; // Encoded flag
        var flagsUsed = 7; // Not encoded flag
        var nextChar = 0; // Next char in sliding window
        var code = new EncodedString();

        while (true)
        {
            flags >>= 1;
            flagsUsed++;

            // Shifted out all the flag bits -> read a new flag
            var readChar = 0;
            if (flagsUsed == 8)
            {
                if ((readChar = reader.Read()) == -1)
                {
                    break;
                }

                flags = readChar & 0xFF;
                flagsUsed = 0;
            }

            // Uncoded character
            if ((flags & 1) != 0)
            {
                if ((readChar = reader.Read()) == -1)
                {
                    break;
                }

                // Write out byte and put it in sliding window
                writer.Write((char)readChar);
                SlidingWindow[nextChar] = (char)readChar;
                nextChar = (nextChar + 1) % Constants.WindowSize;
            }
            else
            {
                if ((code.Offset = reader.Read()) == -1)
                {
                    break;
                }

                if ((code.Length = reader.Read()) == -1)
                {
                    break;
                }

                // Unpack offset and length
                code.Offset <<= 4;
                code.Offset |= (code.Length & 0x00F0) >> 4;
                code.Length = (code.Length & 0x000F) + Constants.MaxUncoded + 1;

                // Write out decoded string to file and lookahead
                for (var i = 0; i < code.Length; i++)
                {
                    readChar = SlidingWindow[(code.Offset + i) % Constants.WindowSize];
                    writer.Write((char)readChar);
                    UncodedLookahead[i] = (char)readChar;
                }

                // Write out decoded string to sliding window
                for (var i = 0; i < code.Length; i++)
                {
                    SlidingWindow[(nextChar + i) % Constants.WindowSize] = UncodedLookahead[i];
                }

                nextChar = (nextChar + code.Length) % Constants.WindowSize;

                // PrintSlidingWindow();
                // PrintUncodedLookahead();
                // Console.WriteLine();
            }
        }
    }

    /// <summary>
    /// This method generates a hash key for a (MAX_UNCODED + 1)
    /// long string located either in the sliding window or in the
    /// uncoded lookahead
    /// </summary>
    /// <param name="offset">Shows the offset from the start of the window</param>
    /// <param name="isInLookahead">Initiates if the hash key is for the 
    /// uncodedLookahead or for the slidingWindow</param>
    /// <returns>A hash key for the offset in the sliding windows or in the lookahead</returns>
    private static int GetHashKey(int offset, bool isInLookahead)
    {
        var hashKey = 0;

        if (isInLookahead)
        {
            for (var i = 0; i < Constants.MaxUncoded + 1; i++)
            {
                hashKey = (hashKey << 5) ^ UncodedLookahead[offset];
                hashKey %= Constants.HashSize;
                offset = (offset + 1) % Constants.MaxCoded;
            }
        }
        else
        {
            for (var i = 0; i < Constants.MaxUncoded + 1; i++)
            {
                hashKey = (hashKey << 5) ^ SlidingWindow[offset];
                hashKey %= Constants.HashSize;
                offset = (offset + 1) % Constants.WindowSize;
            }
        }

        return hashKey;
    }

    /// <summary>
    /// This method searches through the slidingWindow
    /// dictionary for the longest sequence matching the MAX_CODED
    /// long string stored in the uncodedLookahead
    /// </summary>
    /// <param name="uncodedHead">Index of the character from which the coding begins</param>
    private static EncodedString FindMatch(int uncodedHead)
    {
        var matchData = new EncodedString();

        var i = HashTable[GetHashKey(uncodedHead, true)];
        var j = 0;

        while (i != Constants.NullIndex)
        {
            // We've matched the first symbol
            if (SlidingWindow[i] == UncodedLookahead[uncodedHead])
            {
                j = 1;

                while (SlidingWindow[(i + j) % Constants.WindowSize] ==
                    UncodedLookahead[(uncodedHead + j) % Constants.MaxCoded])
                {
                    if (j >= Constants.MaxCoded)
                    {
                        break;
                    }

                    j++;
                }

                if (j > matchData.Length)
                {
                    matchData.Length = j;
                    matchData.Offset = i;
                }
            }

            if (j >= Constants.MaxCoded)
            {
                matchData.Length = Constants.MaxCoded;
                break;
            }

            i = Next[i];
        }

        return matchData;
    }

    /// <summary>
    /// This method adds the (MAX_UNCODED + 1) long string
    /// starting at slidingWindow[charIndex] to the hash table's
    /// linked list associated with its hash key
    /// </summary>
    /// <param name="charIndex">Sliding window index of the string to be
    /// added to the linked list</param>
    private static void AddString(int charIndex)
    {
        // Inserted character will be at the end of the list
        Next[charIndex] = Constants.NullIndex;

        var hashKey = GetHashKey(charIndex, false);

        // This is the only character in the list
        if (HashTable[hashKey] == Constants.NullIndex)
        {
            HashTable[hashKey] = charIndex;
            return;
        }

        // Find the end of the list
        var i = HashTable[hashKey];
        while (Next[i] != Constants.NullIndex)
        {
            i = Next[i];
        }

        // Add new character to the list end
        Next[i] = charIndex;
    }

    /// <summary>
    /// This method removes the (MAX_UNCODED + 1) long string
    /// starting at SlidingWindow[charIndex] from the hash table's
    /// linked list associated with its hash key
    /// </summary>
    /// <param name="charIndex">Sliding window index of the string 
    /// to be removed from the linked list</param>
    private static void RemoveString(int charIndex)
    {
        var nextIndex = Next[charIndex];
        Next[charIndex] = Constants.NullIndex;

        var hashKey = GetHashKey(charIndex, false);

        // We're deleting a list head
        if (HashTable[hashKey] == charIndex)
        {
            HashTable[hashKey] = nextIndex;
            return;
        }

        // Find character pointing to ours
        var i = HashTable[hashKey];
        while (Next[i] != charIndex)
        {
            i = Next[i];
        }

        Next[i] = nextIndex;
    }

    /// <summary>
    /// This method replaces the character stored in slidingWindow[charIndex] 
    /// with the one specified by replacement
    /// </summary>
    /// <param name="charIndex">The index in of the character in the sliding 
    /// window which will be replaced</param>
    /// <param name="newChar">The replacement character</param>
    private static void ReplaceChar(int charIndex, char newChar)
    {
        var firstIndex = charIndex - Constants.MaxUncoded - 1;
        if (firstIndex < 0)
        {
            firstIndex += Constants.WindowSize;
        }

        // Remove all hash entries containing character at charIndex
        for (var i = 0; i < Constants.MaxUncoded + 1; i++)
        {
            RemoveString((firstIndex + i) % Constants.WindowSize);
        }

        SlidingWindow[charIndex] = newChar;

        for (var i = 0; i < Constants.MaxUncoded + 1; i++)
        {
            AddString((firstIndex + i) % Constants.WindowSize);
        }
    }

    /// <summary>
    /// This method initialized all data structures to be used
    /// </summary>
    private static void InitializeDataStructures()
    {
        // Initializing the sliding window with same values which means 
        // there is only 1 hash key for the entier sliding window
        for (var i = 0; i < Constants.WindowSize; i++)
        {
            SlidingWindow[i] = ' ';
            Next[i] = i + 1;
        }

        // There is no next for the last character
        Next[Constants.WindowSize - 1] = Constants.NullIndex;

        // The only list now is the list with spaces
        for (var i = 0; i < Constants.HashSize; i++)
        {
            HashTable[i] = Constants.NullIndex;
        }

        HashTable[GetHashKey(0, false)] = 0;
    }

    private static void PrintUncodedLookahead()
    {
        for (var i = 0; i < UncodedLookahead.Length; i++)
        {
            Console.Write(UncodedLookahead[i]);
        }

        Console.WriteLine();
    }

    private static void PrintSlidingWindow()
    {
        for (var i = 0; i < SlidingWindow.Length; i++)
        {
            Console.Write(SlidingWindow[i]);
        }
        Console.WriteLine();
    }
}

public static class Constants
{
    public const int WindowSize = 4096; // 4096;

    public const int NullIndex = WindowSize + 1;

    public const int MaxUncoded = 2;

    public const int MaxCoded = MaxUncoded + 16;

    public const int HashSize = 1024; // 1024;
}
