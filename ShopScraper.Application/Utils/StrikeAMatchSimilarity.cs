namespace ShopScraper.Application.Utils;

using System.Text.RegularExpressions;

/// <summary>
/// Используется метод StrikeAMatch вот отсюда: http://www.catalysoft.com/articles/StrikeAMatch.html
/// </summary>
public static partial class StrikeAMatchSimilarity
{
    public static double StrikeAMatchCompare(this string str1, string str2)
    {
        var pairs1 = WordLetterPairs(str1.ToUpper());
        var pairs2 = WordLetterPairs(str2.ToUpper());

        var intersection = 0;
        var union = pairs1.Count + pairs2.Count;

        foreach (var t in pairs1)
        {
            for (var j = 0; j < pairs2.Count; j++)
            {
                if (t == pairs2[j])
                {
                    intersection++;
                    pairs2.RemoveAt(j);

                    break;
                }
            }
        }

        return (2.0 * intersection) / union;
    }

    /// <summary>
    /// Gets all letter pairs for each
    /// individual word in the string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static List<string> WordLetterPairs(string str)
    {
        var allPairs = new List<string>();

        // Tokenize the string and put the tokens/words into an array
        var words = WordRegex().Split(str);

        // For each word
        foreach (var t in words)
        {
            if (!string.IsNullOrEmpty(t))
            {
                // Find the pairs of characters
                var pairsInWord = LetterPairs(t);

                allPairs.AddRange(pairsInWord);
            }
        }

        return allPairs;
    }

    /// <summary>
    /// Generates an array containing every 
    /// two consecutive letters in the input string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string[] LetterPairs(string str)
    {
        var numPairs = str.Length - 1;

        var pairs = new string[numPairs];

        for (var i = 0; i < numPairs; i++)
        {
            pairs[i] = str.Substring(i, 2);
        }

        return pairs;
    }

    [GeneratedRegex(@"\s")]
    private static partial Regex WordRegex();
}