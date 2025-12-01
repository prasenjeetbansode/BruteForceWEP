using System;
using System.Threading.Tasks;

public static class WordGenerator
{
    public static string[] GenerateAll4LetterWords()
    {
        const int total = 26 * 26 * 26 * 26;
        string[] words = new string[total];

        Parallel.For(0, total, i =>
        {
            int d = i % 26;
            int c = (i / 26) % 26;
            int b = (i / (26 * 26)) % 26;
            int a = (i / (26 * 26 * 26)) % 26;

            words[i] = new string(new[]
            {
            (char)('a' + a),
            (char)('a' + b),
            (char)('a' + c),
            (char)('a' + d)
        });
        });

        return words;
    }
}