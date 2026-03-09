namespace Shared.Constants
{
    public static class AppConstants
    {
        // Pagination
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        public const int MinPageSize = 1;

        // Password
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 100;

        // File Upload
        public const long MaxFileSize = 10 * 1024 * 1024; 
        public static readonly string[] AllowedImageTypes = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        public static readonly string[] AllowedDocumentTypes = { ".pdf", ".doc", ".docx", ".txt" };

        // Cache
        public const int DefaultCacheExpirationMinutes = 30;
        public const int ShortCacheExpirationMinutes = 5;
        public const int LongCacheExpirationMinutes = 60;

        // JWT
        public const string JwtSecurityAlgorithm = "HS256";
        public const int JwtExpirationHours = 24;
        public const int RefreshTokenExpirationDays = 7;

        // Rate Limiting
        public const int DefaultRateLimitRequests = 100;
        public const int DefaultRateLimitWindowMinutes = 1;
    }
}
