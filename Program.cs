using AutoChecker.Models;
using AutoChecker.Services;

var basePath = AppContext.BaseDirectory;
var inputExcelPath = Path.Combine(basePath, "Data", "Input", "Routing pilot data input (Biome) -all services - FINAL.xlsx");
var inputJsonPath = Path.Combine(basePath, "Data", "Input", "Interior output.json");

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