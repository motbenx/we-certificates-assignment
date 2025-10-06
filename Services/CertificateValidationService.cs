namespace InsuranceCertificates.Services;

public class CertificateValidationService
{
    private readonly Dictionary<(decimal Min, decimal Max), decimal> _pricingRules = new()
    {
        { (20.00m, 50.00m), 8m },
        { (50.01m, 100.00m), 15m },
        { (100.01m, 200.00m), 25m }
    };

    /// <summary>
    /// Validates that the customer is at least 18 years old at the time of certificate creation
    /// </summary>
    public ValidationResult ValidateCustomerAge(DateTime dateOfBirth, DateTime creationDate)
    {
        var age = CalculateAge(dateOfBirth, creationDate);
        
        if (age < 18)
        {
            return ValidationResult.Failure($"Customer must be at least 18 years old. Current age: {age}");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates the insured sum against pricing rules and returns the appropriate certificate sum
    /// </summary>
    public ValidationResult<decimal> ValidateAndCalculateCertificateSum(decimal insuredSum)
    {
        foreach (var (range, certificateSum) in _pricingRules)
        {
            if (insuredSum >= range.Min && insuredSum <= range.Max)
            {
                return ValidationResult<decimal>.Success(certificateSum);
            }
        }

        return ValidationResult<decimal>.Failure(
            $"Insured sum ${insuredSum:F2} is not within any valid pricing range. " +
            "Valid ranges: $20.00-$50.00, $50.01-$100.00, $100.01-$200.00");
    }

    /// <summary>
    /// Calculates certificate validity dates: from creation date for exactly 1 year
    /// The last day is valid for the whole day.
    /// Example: created 2022-09-01 14:30 → valid until 2023-09-01 00:00
    /// </summary>
    public (DateTime ValidFrom, DateTime ValidTo) CalculateValidityDates(DateTime creationDate)
    {
        var validFrom = creationDate;
        
        // Convert creation date to local time to get the correct date in user's timezone
        var localCreationDate = creationDate.Kind == DateTimeKind.Utc 
            ? creationDate.ToLocalTime() 
            : creationDate;
        
        // Calculate the date one year later in local time
        var oneYearLaterLocal = localCreationDate.AddYears(1);
        
        // Create ValidTo at midnight of that date in local time
        var validToLocal = new DateTime(oneYearLaterLocal.Year, oneYearLaterLocal.Month, oneYearLaterLocal.Day, 0, 0, 0, DateTimeKind.Local);
        
        // Convert back to UTC for storage (to match the ValidFrom format)
        var validTo = creationDate.Kind == DateTimeKind.Utc 
            ? validToLocal.ToUniversalTime() 
            : validToLocal;
        
        return (validFrom, validTo);
    }

    /// <summary>
    /// Generates the next certificate number in sequence (5 digits, zero-padded)
    /// </summary>
    public string GenerateNextCertificateNumber(int lastCertificateNumber)
    {
        var nextNumber = lastCertificateNumber + 1;
        return nextNumber.ToString("D5"); // 5-digit zero-padded format
    }

    private static int CalculateAge(DateTime dateOfBirth, DateTime referenceDate)
    {
        var age = referenceDate.Year - dateOfBirth.Year;
        
        // If the birthday hasn't occurred this year yet, subtract one
        if (dateOfBirth.Date > referenceDate.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }

    protected ValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(string errorMessage) => new(false, errorMessage);
}

public class ValidationResult<T> : ValidationResult
{
    public T? Value { get; private set; }

    private ValidationResult(bool isValid, T? value = default, string? errorMessage = null) 
        : base(isValid, errorMessage)
    {
        Value = value;
    }

    public static ValidationResult<T> Success(T value) => new(true, value);
    public static new ValidationResult<T> Failure(string errorMessage) => new(false, default, errorMessage);
    
    // Add explicit constructors to make base class accessible
    public static ValidationResult<T> FromBase(ValidationResult baseResult) => new(baseResult.IsValid, default, baseResult.ErrorMessage);
}