using System;
using System.Text.RegularExpressions;

namespace SaralESuvidha.Models
{
        public class NumberToHindiConverter
        {
            private static readonly string[] EnglishNumbers = {
            "", "One ", "Two ", "Three ", "Four ", "Five ", "Six ", "Seven ", "Eight ", "Nine ",
            "Ten ", "Eleven ", "Twelve ", "Thirteen ", "Fourteen ", "Fifteen ", "Sixteen ", "Seventeen ", "Eighteen ", "Nineteen ",
            "Twenty ", "Thirty ", "Forty ", "Fifty ", "Sixty ", "Seventy ", "Eighty ", "Ninety ",
            "Hundred ", "Thousand ", "Lakh ", "Crore "
        };

            private static readonly string[] HindiNumbers = {
            "", "एक ", "दो ", "तीन ", "चार ", "पाँच ", "छह ", "सात ", "आठ ", "नौ ",
            "दस ", "ग्यारह ", "बारह ", "तेरह ", "चौदह ", "पंद्रह ", "सोलह ", "सत्रह ", "अठारह ", "उन्नीस ",
            "बीस ", "तीस ", "चालीस ", "पचास ", "साठ ", "सत्तर ", "अस्सी ", "नब्बे ",
            "सौ ", "हज़ार ", "लाख ", "करोड़ "
        };

        public static string ConvertToHindiNumbers(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string result = input;

            // Iterate through each English number word and replace all occurrences with Hindi equivalent
            for (int i = 0; i < EnglishNumbers.Length; i++)
            {
                if (!string.IsNullOrEmpty(EnglishNumbers[i]))
                {
                    // Use word boundaries to ensure whole word replacement, case-insensitive
                    string pattern = $@"\b{Regex.Escape(EnglishNumbers[i].TrimEnd())}\b";
                    result = Regex.Replace(result, pattern, HindiNumbers[i].TrimEnd(), RegexOptions.IgnoreCase);
                }
            }

            return result;
        }
    }
}
