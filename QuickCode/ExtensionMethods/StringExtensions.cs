using System;
using System.Globalization;
using System.Text;

namespace MycroftToolkit.QuickCode {
    //
    // 摘要:
    //     String method extensions.
    public static class StringExtensions {
        //
        // 摘要:
        //     Eg MY_INT_VALUE => MyIntValue
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

        //
        // 摘要:
        //     Returns whether or not the specified string is contained with this string
        public static bool Contains(this string source, string toCheck, StringComparison comparisonType) {
            return source.IndexOf(toCheck, comparisonType) >= 0;
        }

        //
        // 摘要:
        //     Ex: "thisIsCamelCase" -> "This Is Camel Case"
        public static string SplitPascalCase(this string input) {
            if (input == null || input.Length == 0) {
                return input;
            }

            StringBuilder stringBuilder = new StringBuilder(input.Length);
            if (char.IsLetter(input[0])) {
                stringBuilder.Append(char.ToUpper(input[0]));
            } else {
                stringBuilder.Append(input[0]);
            }

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