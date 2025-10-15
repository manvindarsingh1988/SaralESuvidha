using System.Text;

namespace SaralESuvidha.Models
{
    public class NumberToHindiRupees
    {
        private static readonly string[] Units =
    {
        "", "एक", "दो", "तीन", "चार", "पाँच", "छह", "सात", "आठ", "नौ",
        "दस", "ग्यारह", "बारह", "तेरह", "चौदह", "पंद्रह", "सोलह", "सत्रह", "अठारह", "उन्नीस",
        "बीस", "इक्कीस", "बाईस", "तेईस", "चौबीस", "पच्चीस", "छब्बीस", "सत्ताईस", "अट्ठाईस", "उनतीस",
        "तीस", "इकतीस", "बत्तीस", "तैंतीस", "चौंतीस", "पैंतीस", "छत्तीस", "सैंतीस", "अड़तीस", "उनतालीस",
        "चालीस", "इकतालीस", "बयालीस", "तैंतालीस", "चवालीस", "पैंतालीस", "छियालीस", "सैंतालीस", "अड़तालीस", "उनचास",
        "पचास", "इक्यावन", "बावन", "तिरपन", "चौवन", "पचपन", "छप्पन", "सत्तावन", "अट्ठावन", "उनसठ",
        "साठ", "इकसठ", "बासठ", "तिरसठ", "चौंसठ", "पैंसठ", "छियासठ", "सड़सठ", "अड़सठ", "उनहत्तर",
        "सत्तर", "इकहत्तर", "बहत्तर", "तिहत्तर", "चौहत्तर", "पचहत्तर", "छहत्तर", "सतहत्तर", "अठहत्तर", "उनासी",
        "अस्सी", "इक्यासी", "बयासी", "तिरासी", "चौरासी", "पचासी", "छियासी", "सतासी", "अट्ठासी", "नवासी",
        "नब्बे", "इक्यानवे", "बानवे", "तिरानवे", "चौरानवे", "पचानवे", "छियानवे", "सत्तानवे", "अट्ठानवे", "निन्यानवे"
    };

        private static readonly string[] Tens = { "", "", "बीस", "तीस", "चालीस", "पचास", "साठ", "सत्तर", "अस्सी", "नब्बे" };
        private static readonly string[] PlaceValues = { "", "हज़ार", "लाख", "करोड़", "अरब", "खरब" };

        public static string ConvertToHindiRupees(decimal number)
        {
            if (number == 0)
                return "शून्य रुपये";

            long wholeNumber = (long)number;
            if (wholeNumber < 0)
                return "ऋणात्मक संख्याएँ समर्थित नहीं हैं";

            string result = ConvertNumberToHindiWords(wholeNumber);
            return $"{result} रुपये";
        }

        private static string ConvertNumberToHindiWords(long number)
        {
            if (number == 0)
                return "शून्य";

            StringBuilder words = new StringBuilder();
            int placeIndex = 0;

            // Handle the first three digits (units, tens, hundreds)
            if (number % 1000 > 0)
            {
                string chunkWords = ConvertChunkToHindiWords((int)(number % 1000));
                if (!string.IsNullOrEmpty(chunkWords))
                {
                    words.Insert(0, chunkWords);
                }
            }
            number /= 1000;
            placeIndex++;

            // Handle subsequent pairs of digits (thousands, lakhs, crores, etc.)
            while (number > 0)
            {
                int chunk = (int)(number % 100);
                if (chunk > 0)
                {
                    string chunkWords = ConvertChunkToHindiWords(chunk);
                    if (!string.IsNullOrEmpty(chunkWords))
                    {
                        words.Insert(0, $"{chunkWords} {PlaceValues[placeIndex]} ");
                    }
                }
                number /= 100;
                placeIndex++;
            }

            return words.ToString().Trim();
        }

        private static string ConvertChunkToHindiWords(int number)
        {
            StringBuilder result = new StringBuilder();

            if (number >= 100)
            {
                int hundreds = number / 100;
                result.Append($"{Units[hundreds]} सौ ");
                number %= 100;
            }

            if (number > 0)
            {
                result.Append(Units[number]);
            }

            return result.ToString().Trim();
        }
    }
}
