using AutoChecker.Services;
using AutoChecker.Models;
using AutoChecker.Interfaces;
using AutoChecker.Services.Geocoders;
using AutoChecker.Services.RouteCheckers;  
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8080")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var geocodingServiceName = builder.Configuration["Services:Geocoding"];
var routeCheckerName = builder.Configuration["Services:RouteChecker"];

if (geocodingServiceName == "Mapbox")
{
    builder.Services.AddHttpClient<IGeocodingService, MapboxGeocodingService>();
}
else if (geocodingServiceName == "Nominatim")
{
    builder.Services.AddHttpClient<IGeocodingService, NominatimGeocodingService>();
}
else
{
    builder.Services.AddHttpClient<IGeocodingService, MapboxGeocodingService>();
}

if (routeCheckerName == "Mapbox")
{
    builder.Services.AddHttpClient<IRouteChecker, MapboxRouteChecker>();
}
else if (routeCheckerName == "Osrm")
{
    builder.Services.AddHttpClient<IRouteChecker, OsrmRouteChecker>();
}
else
{
    builder.Services.AddHttpClient<IRouteChecker, MapboxRouteChecker>();
}

builder.Services.AddScoped<FileUploadController>();

var enableLogging = builder.Configuration.GetValue<bool>("Logging:Enabled");
if (enableLogging)
{
    builder.Services.AddLogging(configure => configure.AddConsole());
}
else
{
    builder.Services.AddLogging(configure => configure.ClearProviders());
}

var app = builder.Build();




app.UseCors("AllowFrontend");
app.UseRouting();
app.MapControllers();
app.MapGet("/", () => "AutoChecker Backend API is running");


var console = builder.Configuration.GetValue<bool>("ConsoleMode");


if (console)
{
    using var scope = app.Services.CreateScope();
    var geocodingService = scope.ServiceProvider.GetRequiredService<IGeocodingService>();
    var routeChecker = scope.ServiceProvider.GetRequiredService<IRouteChecker>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var basePath = AppContext.BaseDirectory;
    var inputExcelPath = Path.Combine(basePath, "Data", "Input", "Routing pilot data input (Biome) -all services - FINAL.xlsx");
    var inputJsonPath = Path.Combine(basePath, "Data", "Input", "Interior output.json");
    var outputJsonPathTechnicians = Path.Combine(basePath, "Data", "Output", "technicians.json");
    var outputJsonPathServiceSites = Path.Combine(basePath, "Data", "Output", "service_sites.json");
    var outputJsonPathVisits = Path.Combine(basePath, "Data", "Output", "visits.json");

    
    
    
    var visits = JsonLoader.LoadVisits(inputJsonPath);
    var technicians = ExcelLoader.LoadTechnicians(inputExcelPath);
    var serviceSites = ExcelLoader.LoadServiceSites(inputExcelPath);
    
    var techDict = technicians.ToDictionary(x => x.Name);
    var sitesDict = serviceSites.ToDictionary(x => (x.Name, x.Service));
    
    var context = new ValidationContext
    {
        Technicians = techDict,
        ServiceSites = sitesDict,
        Visits = visits
    };
    
    logger.LogInformation("Starting geocoding...");
    await Geocoder.GeocodeAsync(context, geocodingService);
    logger.LogInformation("Geocoding completed");

    logger.LogInformation("Checking routes...");
    await routeChecker.CheckRoutesAsync(context);
    logger.LogInformation("Route checking completed");
    
    var checker = new AutoChecker.Services.AutoChecker();
    checker.Run(context);
    
    
    
    
    var result = new
    {                
        Technicians = context.Technicians,  
        ServiceSites = context.ServiceSites.ToDictionary(
            kvp => $"{kvp.Key.Name},{kvp.Key.Service}",
            kvp => kvp.Value
        ),
        Visits = context.Visits,
    };
    
    JsonWriter.SerializeVisits(context.Visits, outputJsonPathVisits);
    JsonWriter.SerializeTechnicians(context.Technicians, outputJsonPathTechnicians);
    JsonWriter.SerializeServiceSites(context.ServiceSites, outputJsonPathServiceSites);
}
else{
    app.Run();
}






/*var basePath = AppContext.BaseDirectory;
var inputExcelPath = Path.Combine(basePath, "Data", "Input", "Routing pilot data input (Biome) -all services - FINAL.xlsx");
var inputJsonPath = Path.Combine(basePath, "Data", "Input", "Interior output.json");
var outputJsonPathTechnicians = Path.Combine(basePath, "Data", "Output", "technicians.json");
var outputJsonPathServiceSites = Path.Combine(basePath, "Data", "Output", "service_sites.json");
var outputJsonPathVisits = Path.Combine(basePath, "Data", "Output", "visits.json");

var visits = JsonLoader.Load(inputJsonPath);

var technicians = ExcelLoader.LoadTechnicians(inputExcelPath);
var serviceSites = ExcelLoader.LoadServiceSites(inputExcelPath);

var techDict = technicians.ToDictionary(x => x.Name);
var sitesDict = serviceSites.ToDictionary(x => (x.Name, x.Service));

var context = new ValidationContext
{
    Technicians = techDict,
    ServiceSites = sitesDict,
    Visits = visits
};

var checker = new AutoChecker.Services.AutoChecker();

checker.Run(context);

JsonWriter.SerializeVisits(context.Visits, outputJsonPathVisits);
JsonWriter.SerializeTechnicians(context.Technicians, outputJsonPathTechnicians);
JsonWriter.SerializeServiceSites(context.ServiceSites, outputJsonPathServiceSites);*/


