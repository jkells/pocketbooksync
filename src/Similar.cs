using System.Collections.Generic;

namespace PocketBookSync
{
    /// <summary>
    ///     Simple inefficient function for comparing transaction descriptions using pairs of characters.
    /// </summary>
    public static class Similar
    {
        private static List<string> GetPairs(string value)
        {
            var pairs = new List<string>();
            var last = ' ';
            foreach (var character in value)
            {
                if (char.IsWhiteSpace(character))
                    continue;

                pairs.Add($"{last}{character}");

                last = character;
            }
            return pairs;
        }

        public static double Compare(string a, string b)
        {
            var aPairs = GetPairs(a);
            var bPairs = GetPairs(b);

            // Switch these around so aPairs is always the shortest
            if (aPairs.Count > bPairs.Count)
            {
                var temp = aPairs;
                aPairs = bPairs;
                bPairs = temp;
            }

            return aPairs.Count(bPairs.Contains)/(double) aPairs.Count;
        }
    }
}