using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sammlerplattform.Data;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.ProcessOfManufactureDatabase;
using Sammlerplattform.Models.ProductDatabase;
using Sammlerplattform.Services;
using Sammlerplattform.Services.Processes;
using Sammlerplattform.Services.Processes.CityProcesses;
using Sammlerplattform.Services.Processes.PlaceProcesses;

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
        IProcessPlace processPlace,
        IUnitOfWork unitOfWork) : Controller
    {
        private readonly DbIdentityContext _dbIdentityContext = context;

        [HttpGet("autocompleteEraID")]
        public IActionResult AutoCompleteEraID(string term)
        {
            List<int> erasID = [.. from p in _dbIdentityContext.Era where p.EraName != null && p.EraName.Equals(term) select p.EraID];
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
                                         select p.ProductionFacilityID).FirstOrDefault();
            return Ok(productionFacilityIDs);
        }

        [HttpGet("listCities")]
        public IActionResult ListCities(string? term)
        {
            CitySearchParameterModel model = new();
            if (!string.IsNullOrEmpty(term))
            {
                model.CityOeconymList_Oeconym_OeconymName.Add(term);
            }

            List<City> CityWthItsPostalcodeAndGeography = [.. processCity.GetWithPredicates(model).Select(c => c.City)];

            return Ok(CityWthItsPostalcodeAndGeography);
        }

        [HttpPost("listPlaces")]
        public IActionResult ListPlaces([FromBody] PlaceSearchDTO placeSearchDTO)
        {
            PlaceSearchParameter model = new();
            if (placeSearchDTO != null)
            {
                if (!string.IsNullOrEmpty(placeSearchDTO.Toponym))
                {
                    model.PlaceNToponymyList_Toponymy_ToponymyName = [placeSearchDTO.Toponym];
                }
                if (placeSearchDTO.ToponymyType != null)
                {
                    model.ToponymyTypeInt = [(int)placeSearchDTO.ToponymyType];
                }
            }

            var places = processPlace.GetListWithPredicate(model);

            var placeList = places.Select(x =>
            {
                var oeconymParts = x.PlaceNToponymyList?
                    .Select(t =>
                    {
                        var name = t.Toponymy?.ToponymyName ?? "";
                        return t.IsCurrentName
                            ? $"<strong>{name}</strong>"
                            : name;
                    })
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList() ?? [];

                string oeconymDisplay = string.Join(", ", oeconymParts);

                // 2. FurtherSpecs: PLZ, Beiname, Geografie
                var specs = new List<string>();

                if (x.Settlement != null)
                {
                    var currentPostalcodeList = x.Settlement.SettlementNPostalcodeList
                        .Where(y => y.IsCurrentPostalcode)
                        .Select(y => y.Postalcode.PostalcodeNumber)
                        .ToList();

                    if (currentPostalcodeList.Count != 0)
                        specs.Add("PLZ: " + string.Join(", ", currentPostalcodeList));

                    if (!string.IsNullOrWhiteSpace(x.Settlement.Byname))
                        specs.Add("Beiname: " + x.Settlement.Byname);

                    if (x.Settlement.RelatedPlace != null)
                        specs.Add("Geo: " + x.Settlement.RelatedPlace.PlaceNToponymyList
                            .FirstOrDefault(x => x.IsCurrentName)?.Toponymy.ToponymyName);
                }

                if (x.ParentPlace != null)
                {
                    var parentName = x.ParentPlace.PlaceNToponymyList?
                        .FirstOrDefault(t => t.IsCurrentName)?.Toponymy?.ToponymyName;

                    if (!string.IsNullOrWhiteSpace(parentName))
                        specs.Add("Teil von: " + parentName);
                }

                return new PlaceDTO
                {
                    PlaceID = x.PlaceID,
                    OeconymDisplay = oeconymDisplay,
                    ToponymyType = EnumExtensions.GetDescription(x.ToponymyTypeEnum),
                    FurtherSpecs = string.Join("; ", specs)
                };
            }).ToList();

            return Ok(placeList);
        }
        public class PlaceSearchDTO
        {
            public string? Toponym { get; set; }
            public int? ToponymyType { get; set; }
        }
        public class PlaceDTO
        {
            public int PlaceID { get; set; }
            public string OeconymDisplay { get; set; } = "";
            public string? ToponymyType { get; set; }
            public string FurtherSpecs { get; set; } = "";
        }


        [HttpGet("listManufacturers")]
        public IActionResult ListManufacturers(string manufacturer, string signature)
        {
            ExpressionStarter<Person> predicate = PredicateBuilder.New<Person>();
            IQueryable<Person> manufacturerIQueryable = from p in _dbIdentityContext.Person
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

            List<Person> manufacturerList = [.. manufacturerIQueryable];

            return Ok(manufacturerList);
        }

        [HttpGet("listPersons")]
        public IActionResult ListPersons(string? name, string? signature, string? pseudonym)
        {
            PersonSearchParameterModel personSearch = new();
            if (!string.IsNullOrEmpty(name))
            {
                personSearch.Name.Add(name);
            }
            if (!string.IsNullOrEmpty(signature))
            {
                personSearch.Signature.Add(signature);
            }
            if (!string.IsNullOrEmpty(pseudonym))
            {
                personSearch.Pseudonym.Add(pseudonym);
            }

            List<PersonOperationParameterModel> personList = processPerson.GetWithPredicates(personSearch);
            List<Person> personList2 = [.. personList.Select(x => x.Person)];

            return Ok(personList2);
        }

        [HttpGet("listEras")]
        public IActionResult ListEras(string? name)
        {
            EraSearchParameterModel eraSearchParameter = new();
            if (!string.IsNullOrEmpty(name))
            {
                eraSearchParameter.EraName.Add(name);
            }

            List<EraDTO> eraList = [.. processEra.GetWithPredicates(eraSearchParameter)
                .OrderBy(x => x.EraName)
                .Select(x => new EraDTO()
                {
                    EraID = x.EraID,
                    EraName = x.EraName
                })];

            return Ok(eraList);
        }
        public class EraDTO {             
            public int EraID { get; set; }
            public string? EraName { get; set; }
        }

        [HttpPost("listManufactorys")]
        public IActionResult ListManufactorys([FromBody] ManufactorySearchDto dto)
        {
            ManufactorySearchParameterModel model = new();

            if (!string.IsNullOrWhiteSpace(dto.Manufactory))
            {
                model.ManufactoryName.Add(dto.Manufactory);
            }
            if (!string.IsNullOrWhiteSpace(dto.ProductionFacility))
            {
                model.ProductionFacility_ProductionFacilityName.Add(dto.ProductionFacility);
            }
            if (!string.IsNullOrWhiteSpace(dto.Oeconomy))
            {
                model.Oeconym.Add(dto.Oeconomy);
            }

            List<ManufactoryDTO> manufactoryList = [.. processManufactory.GetManufactoryWithPredicates(model)
                .OrderBy(x => x.Manufactory.ManufactoryName)
                .Select(m => new ManufactoryDTO
                {
                    ManufactoryID = m.Manufactory.ManufactoryID,
                    ManufactoryName = m.Manufactory.ManufactoryName,
                    CityList = [.. m.Manufactory.CityList.Select(c => new CityDTO
                    {
                        CityID = c.CityID,
                        Oeconym = c.CityOeconymList
                            .Where(x => x.CurrentName)
                            .Select(x => x.Oeconym.OeconymName).FirstOrDefault()
                    })]
                })];
            return Ok(manufactoryList);
        }
        public class ManufactorySearchDto
        {
            public string? Manufactory { get; set; }
            public string? ProductionFacility { get; set; }
            public string? Oeconomy { get; set; }
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
            List<ColorDTO> colorList = [.. unitOfWork.ColorRepository.Get()
                .OrderBy(x => x.Name)
                .Select(x => new ColorDTO
                {
                    ColorID = x.ColorID,
                    ColorName = x.Name
                })];

            return Ok(colorList);
        }
        public class ColorDTO
        {
            public int ColorID { get; set; }
            public string? ColorName { get; set; }
        }

        [HttpGet("listMaterials")]
        public IActionResult ListMaterials()
        {
            List<MaterialDTO> materialList = [.. unitOfWork.MaterialRepository.Get()
                .OrderBy(x => x.MaterialName)
                .Select(x => new MaterialDTO{
                    MaterialID = x.MaterialID,
                    Name = x.MaterialName
                })];
            return Ok(materialList);
        }
        public class MaterialDTO
        {
            public int MaterialID { get; set; }
            public string? Name { get; set; }
        }

        [HttpGet("listProductionFacilities")]
        public IActionResult ListProductionFacilities()
        {
            List<ProducitonFacilityDTO> productionFacilities = [.. unitOfWork.ProductionFacilityRepository.Get()
                .OrderBy(pf => pf.ProductionFacilityName)
                .Select(pf => new ProducitonFacilityDTO
                {
                    ID = pf.ProductionFacilityID,
                    Name = pf.ProductionFacilityName
                })];

            return Ok(productionFacilities);
        }
        public class ProducitonFacilityDTO
        {
            public int ID { get; set; }
            public string? Name { get; set; }
        }

        [HttpPost("listKeywords")]
        public IActionResult ListKeywords([FromBody] string? topic)
        {
            ExpressionStarter<Keyword> predicate = PredicateBuilder.New<Keyword>();
            IEnumerable<Keyword> keywordIQueryable = from k in unitOfWork.KeywordRepository.Get()
                                                    select k;
            //if (!string.IsNullOrEmpty(topic))
            //{
            //    predicate = predicate.And(x => x.Topic.Equals(topic));
            //}
            //if (predicate.IsStarted)
            //{
            //    keywordIQueryable = keywordIQueryable.Where(predicate);
            //}
            List<KeywordDTO> keywordList = [.. keywordIQueryable.OrderBy(x => x.KeywordName)
                .Select(x => new KeywordDTO {
                    KeywordID = x.KeywordID,
                    Name = x.KeywordName
                })];
            return Ok(keywordList);
        }
        public class KeywordDTO
        {
            public int KeywordID { get; set; }
            public string? Name { get; set; }
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
            List<Brickname> bricknameList = [.. bricknameIQueryable.OrderBy(x => x.Name)];

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
}