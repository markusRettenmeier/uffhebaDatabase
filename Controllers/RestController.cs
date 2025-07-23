using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Data;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.ProcessOfManufactureDatabase;
using Sammlerplattform.Services.Processes;
using Sammlerplattform.Services.Processes.CityProcesses;
using Sammlerplattform.Services.UnitOfWork;

namespace Sammlerplattform.Controllers
{
    [AllowAnonymous]
    [Route("api/collections")]
    public class RestController(DbIdentityContext context, 
        IProcessCity processCity, 
        IProcessManufactory processManufactory, 
        IProcessPerson processPerson,
        IProcessEra processEra,
        IProcessProcessOfManufacture processProcessOfManufacture,
        IUnitOfWork unitOfWork) : Controller
    {
        private readonly DbIdentityContext _dbIdentityContext = context;

        [HttpGet("autocompleteGeographyID")]
        public IActionResult AutoCompleteGeographyID(string term)
        {
            int selectGeographyId = (from l in _dbIdentityContext.Geography where l.GeographyName != null && l.GeographyName.Equals(term) select l.Geography_ID).FirstOrDefault();
            return Ok(selectGeographyId);
        }
        [HttpGet("autocompleteEraID")]
        public IActionResult AutoCompleteEraID(string term)
        {
            List<int> erasID = [.. (from p in _dbIdentityContext.Era where p.EraName != null && p.EraName.Equals(term) select p.EraID)];
            return Ok(erasID);
        }
        [HttpGet("autocompleteManufactoryID")]
        public IActionResult AutoCompleteManufactoryID(string term)
        {
            int ManufactoryIDs = (from p in _dbIdentityContext.Manufactory
                                  where p.ManufactoryName != null && p.ManufactoryName.Equals(term)
                                  select p.ManufactoryID).FirstOrDefault();
            return Ok(ManufactoryIDs);
        }
        [HttpGet("autocompleteProductionFacilityID")]
        public IActionResult AutoCompleteProductionFacilityID(string term)
        {
            int productionFacilityIDs = (from p in _dbIdentityContext.ProductionFacility
                                         where p.ProductionFacilityName.Equals(term)
                                         select p.ProductionFacility_ID).FirstOrDefault();
            return Ok(productionFacilityIDs);
        }

        [HttpGet("listCities")]
        public IActionResult ListCities(string? term)
        {
            CitySearchParameterModel model = new();
            if(!string.IsNullOrEmpty(term))
                model.CityNOeconymList_Oeconym_OeconymName.Add(term);
            List<City> CityWthItsPostalcodeAndGeography = [.. processCity.GetCityOPMWithPredicates(model).Select(c => c.City)];

            return Ok(CityWthItsPostalcodeAndGeography);
        }

        [HttpGet("listManufacturers")]
        public IActionResult ListManufacturers(string manufacturer, string signature, string profession)
        {
            ExpressionStarter<Person> predicate = PredicateBuilder.New<Person>();
            IQueryable<Person> manufacturerIQueryable = from p in _dbIdentityContext.Person.Include(y => y.ProfessionICollection)
                                           where p.ProfessionICollection.Any(x => x.Name == profession)
                                           select p;
            if (!string.IsNullOrEmpty(manufacturer))
            {
                predicate = predicate.And(x => x.Name == manufacturer);
            }
            if (!string.IsNullOrEmpty(signature))
            {
                predicate = predicate.And(x => x.Signature != null && x.Signature == signature);
            }

            if (predicate.IsStarted)
            {
                manufacturerIQueryable = manufacturerIQueryable.Where(predicate);
            }
            var manufacturerList = manufacturerIQueryable.ToList();

            return Ok(manufacturerList);
        }

        [HttpGet("listPersons")]
        public IActionResult ListPersons(string? name, string? signature, string? pseudonym)
        {
            PersonSearchParameterModel personSearch = new();
            if(name != null)
                personSearch.Name.Add(name);
            if(signature != null)
                personSearch.Signature.Add(signature);
            if(pseudonym != null)
                personSearch.Pseudonym.Add(pseudonym);

            List<PersonOperationParameterModel> personList = processPerson.GetWithPredicates(personSearch);
            List<Person> personList2 = [.. personList.Select(x => x.Person)];

            return Ok(personList2);
        }

        [HttpGet("listEras")]
        public IActionResult ListEras(string? name)
        {
            EraSearchParameterModel eraSearchParameter = new();
            if(name != null)
                eraSearchParameter.EraName.Add(name);

            List<Era> eraList = [.. processEra.GetWithPredicates(eraSearchParameter)];

            return Ok(eraList);
        }

        [HttpPost("listManufactorys")]
        public IActionResult ListManufactorys([FromBody] ManufactorySearchDto dto)
        {
            ManufactorySearchParameterModel model = new();

            if (!string.IsNullOrWhiteSpace(dto.Manufactory))
                model.ManufactoryName.Add(dto.Manufactory);
            if (!string.IsNullOrWhiteSpace(dto.ProductionFacility))
                model.ProductionFacility_ProductionFacilityName.Add(dto.ProductionFacility);
            if (!string.IsNullOrWhiteSpace(dto.Oeconomy))
                model.Oeconym.Add(dto.Oeconomy);
            //var manufactoryList = processManufactory.GetManufactoryWithPredicates(model).Select(m => new Manufactory
            //{
            //    ManufactoryID = m.Manufactory.ManufactoryID,
            //    ManufactoryName = m.Manufactory.ManufactoryName,
            //    CityICollection = [.. m.Manufactory.CityICollection.Select(c => new City
            //    {
            //        CityID = c.CityID,
            //        CityNOeconymICollection = [.. c.CityNOeconymICollection.Where(x => x.CurrentName)]
            //    })]
            //});
            var manufactoryList = processManufactory.GetManufactoryWithPredicates(model)
                .Select(m => new ManufactoryDTO
                {
                    ManufactoryID = m.Manufactory.ManufactoryID,
                    ManufactoryName = m.Manufactory.ManufactoryName,
                    CityList = [.. m.Manufactory.CityList.Select(c => new CityDTO
                    {
                        CityID = c.CityID,
                        Oeconym = c.CityNOeconymList
                            .Where(x => x.CurrentName)
                            .Select(x => x.Oeconym.OeconymName).FirstOrDefault()
                    })]
                }).ToList();
            return Ok(manufactoryList);
        }
        public class ManufactoryDTO
        {
            public int ManufactoryID { get; set; }
            public string? ManufactoryName { get; set; }
            public List<CityDTO>? CityList { get; set; }
        }

        public class CityDTO
        {
            public int CityID { get; set; }
            public string? Oeconym { get; set; }
        }

        [HttpGet("listColors")]
        public IActionResult ListColors()
        {
            var colorList = unitOfWork.ColorRepository.Get()
                .Select(x => new ColorDTO
                {
                    ColorID = x.ColorID,
                    ColorName = x.Name
                }).ToList();
                
            return Ok(colorList);
        }
        public class ColorDTO
        {
            public int ColorID { get; set; }
            public string? ColorName { get; set; }
        }


        [HttpGet("listBricknames")]
        public IActionResult ListBricknames(string brickname, int usageInt)
        {
            ExpressionStarter<Brickname> predicate = PredicateBuilder.New<Brickname>();
            IQueryable<Brickname> bricknameIQueryable = from b in _dbIdentityContext.Brickname.Include(x => x.BrickPotential)
            select b;
            if (!string.IsNullOrEmpty(brickname))
            {
                predicate = predicate.And(x => x.Name.Contains(brickname));
            }
            if (usageInt > 0)
            {
                predicate = predicate.And(x => x.BrickPotential != null && x.BrickPotential.UsageInt == usageInt);
            }

            if (predicate.IsStarted)
            {
                bricknameIQueryable = bricknameIQueryable.Where(predicate);
            }
            var bricknameList = bricknameIQueryable.OrderBy(x => x.Name).ToList();

            return Ok(bricknameList);
        }

        [HttpGet("listProcessOfManufacture")]
        public IActionResult ListProcessOfManufacture(string? process)
        {
            ProcessOfManufactureSearchParameter searchParameter = new();
            if (!string.IsNullOrEmpty(process))
            {
                searchParameter.ProcessOfManufactureName.Add(process);
                searchParameter.Mainprocess.Add(process);
                searchParameter.Technique.Add(process);
            }

            List<ProcessOfManufacture> processOfManufactureList = [.. processProcessOfManufacture.GetWithPredicates(searchParameter)
                 .OrderBy(x => x.Mainprocess)
                 .ThenBy(x => x.ProcessOfManufactureName)];
            return Ok(processOfManufactureList);
        }
    }

    public class ManufactorySearchDto
    {
        public string? Manufactory { get; set; }
        public string? ProductionFacility { get; set; }
        public string? Oeconomy { get; set; }
    }
}