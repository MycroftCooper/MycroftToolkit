using System;
using System.Globalization;
using System.Text;

namespace MycroftToolkit.QuickCode {
    public static class StringExtensions {
        // Eg MY_INT_VALUE => MyIntValue
        public static string ToTitleCase(this string input) {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < input.Length; i++) {
                char c = input[i];
                if (c == '_' && i + 1 < input.Length) {
                    char c2 = input[i + 1];
                    if (char.IsLower(c2)) {
                        c2 = char.ToUpper(c2, CultureInfo.InvariantCulture);
                    }

                    stringBuilder.Append(c2);
                    i++;
                } else {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }
        
        //     Returns whether or not the specified string is contained with this string
        public static bool Contains(this string source, string toCheck, StringComparison comparisonType) {
            return source.IndexOf(toCheck, comparisonType) >= 0;
        }
        
        //     Ex: "thisIsCamelCase" -> "This Is Camel Case"
        public static string SplitPascalCase(this string input) {
            if (string.IsNullOrEmpty(input)) {
                return input;
            }

            StringBuilder stringBuilder = new StringBuilder(input.Length);
            stringBuilder.Append(char.IsLetter(input[0]) ? 
                char.ToUpper(input[0]) : input[0]);

            for (int i = 1; i < input.Length; i++) {
                char c = input[i];
                if (char.IsUpper(c) && !char.IsUpper(input[i - 1])) {
                    stringBuilder.Append(' ');
                }

                stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }
    }
}