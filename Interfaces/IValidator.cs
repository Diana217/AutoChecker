using AutoChecker.Models;

namespace AutoChecker.Interfaces;

public interface IValidator
{
    List<ValidationResult> Validate(ValidationContext context);
}
