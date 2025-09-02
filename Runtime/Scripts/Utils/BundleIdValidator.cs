using System.Text.RegularExpressions;

namespace Soda.Runtime.Utils
{
    /// <summary>
    /// Utility class for validating bundle identifiers
    /// </summary>
    public static class BundleIdValidator
    {
        // Bundle ID pattern: com.company.app (reverse domain notation)
        private static readonly Regex BUNDLE_ID_PATTERN = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*(\.[a-zA-Z][a-zA-Z0-9]*)*$");
        
        private const int MIN_LENGTH = 3;
        private const int MAX_LENGTH = 100;
        
        /// <summary>
        /// Validates bundle identifier format
        /// </summary>
        /// <param name="bundleId">Bundle identifier to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValid(string bundleId)
        {
            if (string.IsNullOrWhiteSpace(bundleId))
                return false;
                
            if (bundleId.Length < MIN_LENGTH || bundleId.Length > MAX_LENGTH)
                return false;
                
            if (bundleId.Contains("..") || bundleId.StartsWith(".") || bundleId.EndsWith("."))
                return false;

            if (!bundleId.Contains("."))
                return false;

            if (!bundleId.Contains("."))
                return false;
                
            return BUNDLE_ID_PATTERN.IsMatch(bundleId);
        }
        
        /// <summary>
        /// Gets detailed validation result with error message
        /// </summary>
        /// <param name="bundleId">Bundle identifier to validate</param>
        /// <returns>Validation result with success status and error message</returns>
        public static ValidationResult Validate(string bundleId)
        {
            if (string.IsNullOrWhiteSpace(bundleId))
                return new ValidationResult(false, "Bundle ID cannot be null or empty");
                
            if (bundleId.Length < MIN_LENGTH)
                return new ValidationResult(false, $"Bundle ID too short (minimum {MIN_LENGTH} characters)");
                
            if (bundleId.Length > MAX_LENGTH)
                return new ValidationResult(false, $"Bundle ID too long (maximum {MAX_LENGTH} characters)");
                
            if (bundleId.Contains(".."))
                return new ValidationResult(false, "Bundle ID cannot contain consecutive dots");
                
            if (bundleId.StartsWith(".") || bundleId.EndsWith("."))
                return new ValidationResult(false, "Bundle ID cannot start or end with a dot");
                
            if (!BUNDLE_ID_PATTERN.IsMatch(bundleId))
                return new ValidationResult(false, "Bundle ID must follow reverse domain notation (e.g., com.mycompany.myapp)");
            
            if (!bundleId.Contains("."))
                return new ValidationResult(false, "Bundle ID must follow reverse domain notation (e.g., com.mycompany.myapp)");

            if (!bundleId.Contains("."))
                return new ValidationResult(false,"Bundle ID must follow reverse domain notation (e.g., com.mycompany.myapp)");
                
            return new ValidationResult(true, "Valid bundle ID");
        }
        
        public struct ValidationResult
        {
            public bool IsValid { get; }
            public string ErrorMessage { get; }
            
            public ValidationResult(bool isValid, string errorMessage)
            {
                IsValid = isValid;
                ErrorMessage = errorMessage;
            }
        }
    }
}