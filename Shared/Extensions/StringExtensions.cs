using SharedKernel.Constants;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SharedKernel.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static string ToTitleCase(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return value;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public static string ToCamelCase(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return value;

            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        public static string ToPascalCase(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return value;

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        public static string ToSlug(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return value;

            value = value.ToLowerInvariant();
            value = Regex.Replace(value, @"[^a-z0-9\s-]", "");
            value = Regex.Replace(value, @"\s+", " ").Trim();
            value = Regex.Replace(value, @"\s", "-");

            return value;
        }

        public static string Truncate(this string value, int maxLength, string suffix = "...")
        {
            if (value.IsNullOrWhiteSpace() || value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength - suffix.Length) + suffix;
        }

        public static string RemoveWhitespace(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return value;

            return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        public static string ToMd5Hash(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return value;

            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(value);
                var hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        public static string ToSha256Hash(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return value;
            using (var sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(value);
                var hashBytes = sha256.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        public static bool IsValidEmail(this string email)
        {
            if (email.IsNullOrWhiteSpace())
                return false;

            return Regex.IsMatch(email, RegexPatterns.Email);
        }

        public static bool IsValidUrl(this string url)
        {
            if (url.IsNullOrWhiteSpace())
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        public static string MaskEmail(this string email)
        {
            if (email.IsNullOrWhiteSpace() || !email.Contains("@"))
                return email;

            var parts = email.Split('@');
            var username = parts[0];
            var domain = parts[1];

            if (username.Length <= 2)
                return email;

            var maskedUsername = username[0] + new string('*', username.Length - 2) + username[^1];
            return maskedUsername + "@" + domain;
        }
    }
}
