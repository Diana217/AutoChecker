using AutoChecker.Models;
using AutoChecker.Models.Enums;
using AutoChecker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AutoChecker.Interfaces;
using Microsoft.Extensions.Logging;


[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IGeocodingService _geocodingService;
    private readonly IRouteChecker _routeChecker;
    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(
        IGeocodingService geocodingService,
        IRouteChecker routeChecker,
        ILogger<FileUploadController> logger)
    {
        _geocodingService = geocodingService;
        _routeChecker = routeChecker;
        _logger = logger;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessFiles(
        [FromForm] IFormFile visitsFile,
        [FromForm] IFormFile excelFile)
    {
        try
        {
            var tempDir = Path.GetTempPath();
            var visitsPath = Path.Combine(tempDir, $"visits_{Guid.NewGuid()}.json");
            var excelPath = Path.Combine(tempDir, $"excel_{Guid.NewGuid()}.xlsx");
            
            using (var stream = new FileStream(visitsPath, FileMode.Create))
            {
                await visitsFile.CopyToAsync(stream);
            }
            
            using (var stream = new FileStream(excelPath, FileMode.Create))
            {
                await excelFile.CopyToAsync(stream);
            }
            
            var visits = JsonLoader.LoadVisits(visitsPath);
            var technicians = ExcelLoader.LoadTechnicians(excelPath);
            var serviceSites = ExcelLoader.LoadServiceSites(excelPath);
            
            var techDict = technicians.ToDictionary(x => x.Name);
            var sitesDict = serviceSites.ToDictionary(x => (x.Name, x.Service));
            
            var context = new ValidationContext
            {
                Technicians = techDict,
                ServiceSites = sitesDict,
                Visits = visits
            };
            
            _logger.LogInformation("Starting geocoding...");
            await Geocoder.GeocodeAsync(context, _geocodingService);
            _logger.LogInformation("Geocoding completed");
            
            var checker = new AutoChecker.Services.AutoChecker();
            checker.Run(context);
            
            _logger.LogInformation("Checking routes...");
            await _routeChecker.CheckRoutesAsync(context);
            _logger.LogInformation("Route checking completed");
            
            var result = new
            {                
                Technicians = context.Technicians,  
                ServiceSites = context.ServiceSites.ToDictionary(
                    kvp => $"{kvp.Key.Name},{kvp.Key.Service}",
                    kvp => kvp.Value
                ),
                Visits = context.Visits,
            };
            
            /*
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
            Directory.CreateDirectory(outputDir);
            
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            JsonWriter.SerializeVisits(
                context.Visits, 
                Path.Combine(outputDir, $"visits_{timestamp}.json"));
            JsonWriter.SerializeTechnicians(
                context.Technicians, 
                Path.Combine(outputDir, $"technicians_{timestamp}.json"));
            JsonWriter.SerializeServiceSites(
                context.ServiceSites, 
                Path.Combine(outputDir, $"serviceSites_{timestamp}.json"));
                
            */
            
            
            System.IO.File.Delete(visitsPath);
            System.IO.File.Delete(excelPath);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}