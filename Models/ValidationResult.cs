using AutoChecker.Models.Enums;

namespace AutoChecker.Models;

public class ValidationResult
{
    public string Message { get; set; } = null!;
    public ConstraintSeverity Severity { get; set; }
}
