using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Steam_Library_Manager.Framework
{
    /// <summary>
    /// The primary class for accessing the functionality of the StringFormat library.
    /// </summary>
    public static class StringFormat
    {
        /*
         * Explanation:
         * (?<=(^|[^\{]|(\{\{)+)) -- This is a lookback, it says that when the next character in the regex is matched, it should
         *                          only be considered a match if it is the start of the line, immediately preceded with a non '{' character, or an even number of '{' characters
         *                          IE: '{one}' and '{{{one}' are both valid (the first two '{' in the second example will result in a single '{'
         *                              but '{{one}' is not a valid because String.Format will combine the first two '{' into a single one
         * \{                   --Find a '{' character
         * (?!\{)               --This is a negative look ahead, it says that after you find a the preceding character,
         *                          look to make sure it isn't immediately proceeded with another '{'
         *\w                    --The very next character must be a word character (alpha numeric)
         *.*                    --Continue reading the string for any non-linebreaking character
         *?\}                   --Stop reading at the very first '}' character (don't be greedy)
        */
        private const string TokenizeRegex = @"(?<=(^|[^\{]|(\{\{)+))\{(?!\{)\w.*?\}";

        /// <summary>
        /// Formats the string using the placeholder for the property names.
        /// </summary>
        /// <param name="format">The string to format.</param>
        /// <param name="values">The object to pull the values from. Usually an anonymous type.</param>
        /// <returns>The formatted string.</returns>
        public static string Format(string format, object values)
        {
            return Format(null, format, values);
        }

        /// <summary>
        /// Formats the string using the placeholder for the property names.
        /// </summary>
        /// <param name="provider">The provider to use for formatting dates and numeric values.</param>
        /// <param name="format">The string to format.</param>
        /// <param name="values">The object to pull the values from. Usually an anonymous type.</param>
        /// <returns>The formatted string.</returns>
        public static string Format(IFormatProvider provider, string format, object values)
        {
            return Format(provider, format, AnonymousObjectToDictionary(values));
        }

        /// <summary>
        /// Formats the string using the placeholder for the property names.
        /// </summary>
        /// <param name="format">The string to format.</param>
        /// <param name="values">The dictionary to pull the values from.</param>
        /// <returns>The formatted string.</returns>
        public static string Format(string format, IDictionary<string, object> values)
        {
            return Format(null, format, values);
        }

        /// <summary>
        /// Formats the string using the placeholder for the property names.
        /// </summary>
        /// <param name="provider">The provider to use for formatting dates and numeric values.</param>
        /// <param name="format">The string to format.</param>
        /// <param name="values">The dictionary to pull the values from.</param>
        /// <returns>The formatted string.</returns>
        public static string Format(IFormatProvider provider, string format, IDictionary<string, object> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            IEnumerable<string> tokens;

            var tokenizedString = TokenizeString(format, out tokens);

            return String.Format(provider, tokenizedString, tokens.Select(s => values[s]).ToArray());
        }

        /// <summary>
        /// Returns the format string with the tokens replaced as ordinals. Exposed for developer benefit. Most likely used only in debugging.
        /// </summary>
        /// <param name="format">The string to format.</param>
        /// <returns>A string where the tokens are replaced with ordinal values.</returns>
        public static string TokenizeString(string format)
        {
            IEnumerable<string> junk;

            return TokenizeString(format, out junk);
        }

        /// <summary>
        /// Returns the format string with the tokens replaced as ordinals. Exposed for developer benefit. Most likely used only in debugging.
        /// </summary>
        /// <param name="format">The string to format.</param>
        /// <param name="tokens">The tokens that were extracted from the format string.</param>
        /// <returns>A string where the tokens are replaced with ordinal values.</returns>
        public static string TokenizeString(string format, out IEnumerable<string> tokens)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }

            //performance: minimize the number of times the builder will have to "grow", while keeping the initial size reasonable
            var sb = new StringBuilder(format.Length);

            var match = Regex.Match(format, TokenizeRegex, RegexOptions.Compiled);

            var tokenList = new List<string>();

            var currentIndex = 0;
            while (match.Success)
            {
                sb.Append(format.Substring(currentIndex, match.Index - currentIndex));

                var fullToken = match.ToString();

                var name = ParseName(fullToken);

                var index = IndexOfName(tokenList, name);

                sb.Append(BuildNewToken(fullToken, name, index));

                currentIndex = match.Index + match.Length;

                match = match.NextMatch();
            }

            tokens = tokenList;
            sb.Append(format.Substring(currentIndex));

            return sb.ToString();
        }

        #region Private Methods

        private static string ParseName(string fullToken)
        {
            var token = fullToken.Substring(1, fullToken.Length - 2);

            var colonIndex = token.IndexOf(':');

            if (colonIndex >= 0) token = token.Substring(0, colonIndex);

            return token.TrimEnd();
        }

        private static int IndexOfName(IList<string> names, string name)
        {
            var index = names.IndexOf(name);

            if (index < 0)
            {
                names.Add(name);
                index = names.IndexOf(name);
            }

            return index;
        }

        private static string BuildNewToken(string fullToken, string name, int index)
        {
            fullToken = fullToken.Remove(1, name.Length);

            return fullToken.Insert(1, index.ToString());
        }

        private static IDictionary<string, object> AnonymousObjectToDictionary(object values)
        {
            var valueDictionary = new Dictionary<string, object>();
            if (values != null)
            {
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(values))
                {
                    valueDictionary.Add(propertyDescriptor.Name, propertyDescriptor.GetValue(values));
                }
            }
            return valueDictionary;
        }

        #endregion
    }
}
