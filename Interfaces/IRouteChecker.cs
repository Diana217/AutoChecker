using System.Collections.Generic;
using System.Threading.Tasks;
using AutoChecker.Models;

namespace AutoChecker.Interfaces;

public interface IRouteChecker
{
    Task CheckRoutesAsync(ValidationContext context);
}