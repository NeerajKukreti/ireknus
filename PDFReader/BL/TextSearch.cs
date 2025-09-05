using System;
using System.Collections.Generic;
using System.Linq;

public class TextSearcher
{
    public static string[] SearchAllWithContext(string input, string searchText, int wordsBefore = 3, int wordsAfter = 3)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(searchText))
            return Array.Empty<string>();

        List<string> results = new List<string>();
        string searchLower = searchText.ToLower();
        string inputLower = input.ToLower();

        int currentIndex = 0;

        // Find all occurrences of the search text
        while (currentIndex < input.Length)
        {
            int searchIndex = inputLower.IndexOf(searchLower, currentIndex);

            if (searchIndex == -1)
                break;

            // Extract the context for this occurrence
            string result = 
                ExtractContext(input, searchIndex, searchText.Length, wordsBefore, wordsAfter);
            results.Add(result);

            // Move to the next position to avoid infinite loop with same search text
            currentIndex = searchIndex + searchText.Length;
        }

        return results.ToArray();
    }

    private static string ExtractContext(string input, int startIndex, int length, int wordsBefore, int wordsAfter)
    {
        // Find the start of the context (wordsBefore words before the search text)
        int contextStart = FindWordStart(input, startIndex, wordsBefore);

        // Find the end of the context (wordsAfter words after the search text)
        int contextEnd = FindWordEnd(input, startIndex + length, wordsAfter);

        // Ensure we don't go out of bounds
        contextStart = Math.Max(contextStart, 0);
        contextEnd = Math.Min(contextEnd, input.Length);

        // Extract and return the context
        return input.Substring(contextStart, contextEnd - contextStart).Trim();
    }

    private static int FindWordStart(string input, int position, int wordCount)
    {
        int currentPosition = position;
        int wordsFound = 0;

        // Move backwards until we find the start of the specified number of words
        while (currentPosition > 0 && wordsFound < wordCount)
        {
            currentPosition--;

            // If we find a space and the next character is not a space, we found a word boundary
            if (input[currentPosition] == ' ' &&
                (currentPosition == 0 || input[currentPosition - 1] != ' '))
            {
                wordsFound++;
            }
        }

        return currentPosition;
    }

    private static int FindWordEnd(string input, int position, int wordCount)
    {
        int currentPosition = position;
        int wordsFound = 0;
        int length = input.Length;

        // Move forwards until we find the end of the specified number of words
        while (currentPosition < length && wordsFound < wordCount)
        {
            // If we find a space and the previous character is not a space, we found a word boundary
            if (currentPosition < length - 1 &&
                input[currentPosition] == ' ' &&
                input[currentPosition + 1] != ' ')
            {
                wordsFound++;
            }

            currentPosition++;

            // If we've reached the end of the string, break
            if (currentPosition >= length)
                break;
        }

        return currentPosition;
    }
}

// Example usage:
 