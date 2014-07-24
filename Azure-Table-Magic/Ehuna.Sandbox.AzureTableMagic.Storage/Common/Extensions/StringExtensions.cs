using System;

namespace Ehuna.Sandbox.AzureTableMagic.Storage.Common.Extensions
{
    public static class StringExtensions
    {
        #region Private Static Properties



        #endregion

        #region Static Constructors

        static StringExtensions()
        {

        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Determines whether a sequence contains a specified element by using the default equality comparer
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <param name="comparison"></param>
        /// <returns>Returns a value indicating whether the specified System.String object occurs within this string</returns>
        public static bool Contains(this string source, string value, StringComparison comparison)
        {
            return source.IndexOf(value, comparison) >= 0;
        }

        /// <summary>
        /// Shortens a string to a maximum length and optionally appends an ellipsis
        /// </summary>
        /// <param name="value">String to inspect</param>
        /// <param name="maxLength">Maximum length</param>
        /// <param name="ellipsis">Append ... or not</param>
        /// <returns></returns>
        public static string Shorten(this string source, int maxLength, bool ellipsis = true)
        {
            return source.Length > maxLength ?
                source.Substring(0, maxLength - 1) + (ellipsis ? "…" : string.Empty) :
                source;
        }

        /// <summary>
        /// Returns true if string is null or empty
        /// </summary>
        public static bool IsNot(this string source)
        {
            return String.IsNullOrEmpty(source);
        }

        /// <summary>
        /// Returns true if string is NOT (null or empty)
        /// </summary>
        public static bool Is(this string source)
        {
            return !String.IsNullOrEmpty(source);
        }

        /// <summary>
        /// Returns true if string is null, empty or white space
        /// </summary>
        public static bool HasNot(this string source)
        {
            return String.IsNullOrWhiteSpace(source);
        }

        /// <summary>
        /// Returns true if string is NOT (null, empty or white space)
        /// </summary>
        public static bool Has(this string source)
        {
            return !String.IsNullOrWhiteSpace(source);
        }

        /// <summary>
        /// Fixes the length of a string by appropriately added or removing trailing characters
        /// </summary>
        public static string ToLength(this string source, int totalWidth, char paddingChar = ' ')
        {
            return (source ?? "")
                        .Truncate(totalWidth)
                        .PadRight(totalWidth, paddingChar);
        }

        public static string Truncate(this string source, int len)
        {
            return source != null
                        ? source.Length <= len
                                ? source
                                : source.Substring(0, len)
                        : "";
        }

        /// <summary>
        /// Extract length characters from the end of a string
        /// </summary>
        public
        static
        string
        SubstringEnd(
            this string source,
            int len)
        {
            if (source == null)
                source = "";

            if (len > source.Length)
                len = source.Length;

            return source.Substring(source.Length - len, len);
        }

        public static string SubstringSafe(this string source, int start, int len)
        {
            if (source == null)
                source = "";

            if (start + len > source.Length)
                source = source.ToLength(start + len);

            return source.Substring(start, len);
        }

        public static string Safe(this string source)
        {
            if (source == null)
                source = "";

            return source;
        }

        /// <summary>
        /// Returns the rest of the string after the first occurrence of the string to find.
        /// If string not found, return the source string.
        /// </summary>
        public
        static
        string
        After(
            this string source,
            string find)
        {
            int idx;

            return (idx = source.IndexOf(find)) == -1
                        ? source
                        : source.Substring(idx + find.Length);	// skip past the found character	
        }

        // -------------------------------------------------------------------------------------------------------------

        public static string ToBase64(this byte[] source)
        {
            return Convert.ToBase64String(source);
        }

        public static string ToBase64(this string source)
        {
            return ToBase64(System.Text.Encoding.UTF8.GetBytes(source));
        }

        public static string ToBase64Safe(this string source)
        {
            return source.ToBase64().ToUrlSafe();
        }

        public static string ToBase64Safe(this byte[] source)
        {
            return source.ToBase64().ToUrlSafe();
        }

        static
        char[] _trimChar = new char[] { '=' };

        public static string Base64Trim(this string source)
        {
            return source.TrimEnd(_trimChar);
        }

        // -------------------------------------------------------------------------------------------------------------

        public static string FromBase64(this byte[] source)
        {
            return System.Text.Encoding.UTF8.GetString(
                                                    source,
                                                    0,
                                                    source.Length);
        }

        public static string FromBase64(this string source)
        {
            return FromBase64(Convert.FromBase64String(source));
        }

        public static string FromBase64Safe(this string source)
        {
            return source.FromBase64().FromUrlSafe();
        }

        public static string FromBase64Safe(this byte[] source)
        {
            return source.FromBase64().FromUrlSafe();
        }

        // -------------------------------------------------------------------------------------------------------------

        public static string ToUrlSafe(this string source)
        {
            return source.Replace('/', '_').Replace('+', '-');
        }

        public static string FromUrlSafe(this string source)
        {
            return source.Replace('_', '/').Replace('-', '+');
        }

        // -------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns with prefix removed if present
        /// </summary>
        public
        static
        string
        TrimIfStartsWith(
            this string value,
            string prefix)
        {
            return value.StartsWith(prefix)
                        ? value.Substring(prefix.Length)
                        : value;

        }

        /// <summary>
        /// Returns with substring removed if present
        /// </summary>
        public
        static
        string
        RemoveIfContains(
            this string value,
            string substring)
        {
            int idx;

            return (idx = value.IndexOf(substring)) != -1
                        ? value.Remove(idx, substring.Length)
                        : value;
        }

        /// <summary>
        /// Returns suffixed if value is present. Else returns empty string.
        /// </summary>
        public
        static
        string
        AppendIfIs(
            this string value,
            string suffix)
        {
            return value.Is()
                        ? value + suffix
                        : String.Empty;
        }

        /// <summary>
        /// Returns value if it 'is' (not null or empty). Else returns alternate.
        /// </summary>
        public
        static
        string
        IfIsElse(
            this string value,
            string alternate)
        {
            return value.Is()
                        ? value
                        : alternate;
        }

        public
        static
        Decimal
        AsDecimal(
            this string str,
            Decimal defaultValue = default(Decimal))
        {
            Decimal value;

            return Decimal.TryParse(str, out value)
                        ? value
                        : defaultValue;
        }

        public
        static
        DateTime
        AsDateTime(
            this string str,
            DateTime defaultValue = default(DateTime))
        {
            DateTime value;

            return DateTime.TryParse(str, out value)
                        ? value
                        : defaultValue;
        }

        public
        static
        int
        AsInt(
            this string str,
            int defaultValue = default(int))
        {
            int value;

            return int.TryParse(str, out value)
                        ? value
                        : defaultValue;
        }

        public
        static
        double
        AsDouble(
            this string str,
            double defaultValue = default(double))
        {
            double value;

            return double.TryParse(str, out value)
                            ? value
                            : defaultValue;
        }

        #endregion

        public
        static
        string
        Fmt(
            this string format,
            params object[] args)
        {
            return String.Format(format, args);
        }

        public
        static
        int
        SubStringCount(
            this string data,
            string substring)
        {
            var subStringLen = substring.Length;
            var count = 0;
            var idx = 0;

            while ((idx = data.IndexOf(substring, idx)) != -1)
            {
                ++count;
                idx += subStringLen;

            };

            return count;
        }
    }
}
