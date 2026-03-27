using AutoChecker.Models;
using AutoChecker.Models.Enums;

namespace AutoChecker.Services;

public static class ValidationResultFactory
{
    public static ValidationResult Hard(string message) =>
        new() { Message = message, Severity = ConstraintSeverity.Hard };

    public static ValidationResult Soft(string message) =>
        new() { Message = message, Severity = ConstraintSeverity.Soft };
}
