using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Services.GenericClasses;
using Sammlerplattform.Services.UnitOfWork;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.CityProcesses
{
    public interface IProcessCity
    {
        (Models.CityDatabase.City city, int statuscode, string message) CreateCity(CityOperationParameterModel model);
        (Models.CityDatabase.City city, int statuscode, string message) EditCity(CityOperationParameterModel model);
        List<CityOperationParameterModel> GetCityOPMWithPredicates(CitySearchParameterModel model);
        CitySearchParameterModel CityParametersOperationToSearch(CityOperationParameterModel model);
    }

    public class CityProcessor(IProcessGeography processGeography,
                                IProcessPostalcode processPostalcode,
                                IProcessOeconym processOeconym,
                                IProcessCityNOeconym processCityNOeconym,
                                IUnitOfWork unitOfWork, ILogger<CityProcessor> logger) : IProcessCity
    {
        public (City city, int statuscode, string message) CreateCity(CityOperationParameterModel model)
        {
            model.Geography.IsGeographyNameRequired = false;


            if (model.OeconymList.Count == 0)
            {
                return (new(), 412, "Ortsnamen angeben.");
            }
            else if (model.PostalcodeNumberList.Count == 0)
            {
                return (new(), 412, "Mindestens 1 PLZ angeben.");
            }

            List<CityOperationParameterModel> cityExists = GetCityOPMWithPredicates(CityParametersOperationToSearch(model));
            if (cityExists != null)
            {
                return (cityExists.First().City, 302, "Eintrag existiert bereits.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                Models.CityDatabase.City newCity = unitOfWork.CityRepository.Insert(model.City);
                unitOfWork.Save();

                ConnectPostalcodeToCity(model.PostalcodeNumberList, newCity);
                ConnectOeconymToCity(model.OeconymList, newCity);
                ConnectGeographyToCity(model.Geography, newCity);

                scope.Complete();
                return (newCity, 201, "Ort wurde erstellt.");
            }
            catch (Exception ex)
            {
                logger.LogError("Fehler beim Hinzufügen des Ortes: {ex}", ex);
                return (new(), 500, "Es ist ein Fehler beim Hinzufügen des Ortes aufgetreten. Der Support wurde benachrichtigt.");
            }
        }

        public (Models.CityDatabase.City city, int statuscode, string message) EditCity(CityOperationParameterModel model)
        {
            model.Geography.IsGeographyNameRequired = false;

            if (model.OeconymList.Count == 0)
            {
                return (model.City, 412, "Ortsnamen angeben.");
            }
            else if (model.PostalcodeNumberList.Count == 0)
            {
                return (model.City, 412, "Mindestens 1 PLZ angeben.");
            }

            Models.CityDatabase.City? citySelect = unitOfWork.CityRepository.GetByID(model.City.CityID);
            if (citySelect == null)
            {
                return (model.City, 302, "Eintrag existiert nicht.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                citySelect.Byname = model.City.Byname;
                unitOfWork.CityRepository.Update(citySelect);
                unitOfWork.Save();

                ChangePostalcodeOfCity(model.PostalcodeNumberList, citySelect);
                ChangeOeconymOfCity(model.OeconymList, citySelect);
                ChangeGeographyOfCity(model.Geography, citySelect);

                scope.Complete();
                return (citySelect, 201, "Ort wurde geändert.");
            }
            catch (Exception ex)
            {
                logger.LogError("Fehler beim Ändern des Ortes: {ex}", ex);
                return (citySelect, 500, "Es ist ein Fehler beim Hinzufügen des Ortes aufgetreten. Der Support wurde benachrichtigt.");
            }
        }

        public List<CityOperationParameterModel> GetCityOPMWithPredicates(CitySearchParameterModel model)
        {
            IEnumerable<City> cityResults = unitOfWork.CityRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<City>(model),
                includeProperties: "CityNOeconymList.Oeconym,PostalcodeList,Geography,ParentCity.CityNOeconymList"
            );

            return [.. from c in cityResults
                       select new CityOperationParameterModel {
                           City = c,
                       }];
        }

        public CitySearchParameterModel CityParametersOperationToSearch(CityOperationParameterModel model)
        {
            CitySearchParameterModel citySearchParameterModel = new();
            citySearchParameterModel.CityID.Add(model.City.CityID);
            citySearchParameterModel.CityNOeconymList_Oeconym_OeconymName = model.OeconymList;
            citySearchParameterModel.PostalcodeList_PostalcodeNumber = model.PostalcodeNumberList;
            if (model.City.Byname != null)
            {
                citySearchParameterModel.Byname.Add(model.City.Byname);
            }
            if (model.Geography.GeographyName != null)
            {
                citySearchParameterModel.Geography_GeographyName.Add(model.Geography.GeographyName);
            }
            if (model.City.ParentCityID != null)
            {
                citySearchParameterModel.ParentCityID.Add((int)model.City.ParentCityID);
            }

            return citySearchParameterModel;
        }

        private void ConnectPostalcodeToCity(List<string> postalcodeNumberList, Models.CityDatabase.City newCity)
        {
            foreach (string postalcodeNo in postalcodeNumberList.Where(p => !string.IsNullOrEmpty(p)))
            {
                //if (!string.IsNullOrEmpty(postalcodeNo))
                //{
                Postalcode postalcode = processPostalcode.CreatePostalcode(postalcodeNo);
                unitOfWork.PostalcodeRepository.AddMemberToCollection(postalcode, c => c.CityICollection, newCity);
                unitOfWork.CityRepository.AddMemberToCollection(newCity, c => c.PostalcodeList, postalcode);
                unitOfWork.Save();
                //}
            }
        }
        private void ChangePostalcodeOfCity(List<string> postalcodeList, Models.CityDatabase.City city)
        {
            RemovePostalcodeFromCity(city);
            ConnectPostalcodeToCity(postalcodeList, city);
        }
        private void RemovePostalcodeFromCity(Models.CityDatabase.City city)
        {
            List<Postalcode> postalcodesToRemove = [.. city.PostalcodeList];

            foreach (Postalcode? postalcode in postalcodesToRemove)
            {
                unitOfWork.CityRepository.RemoveMemberFromCollection(city, c => c.PostalcodeList, postalcode);
                unitOfWork.PostalcodeRepository.RemoveMemberFromCollection(postalcode, p => p.CityICollection, city);
                unitOfWork.Save();
            }
        }

        private void ConnectOeconymToCity(List<string> oeconymList, Models.CityDatabase.City newCity)
        {
            foreach (string? oeconym in oeconymList.Where(p => !string.IsNullOrEmpty(p)))
            {
                //if (!string.IsNullOrEmpty(oeconym))
                //{
                string[] splittedOeconym = oeconym.Split("§§");
                if (string.IsNullOrEmpty(splittedOeconym[0]))
                {
                    continue;
                }

                Oeconym newOeconym = new() { OeconymName = splittedOeconym[0] };
                newOeconym = processOeconym.CreateOeconym(newOeconym);

                bool currentName = bool.Parse(splittedOeconym[1]);
                if (oeconymList.Count == 1 && currentName == false)
                {
                    currentName = true;
                }

                CityNOeconym newCityNOeconym = new() { CurrentName = currentName };
                _ = processCityNOeconym.CreateCityNOeconym(newCity, newOeconym, newCityNOeconym);

                //unitOfWork.OeconymRepository.SetForeignKey(newOeconym, n => n.Oeconym_ID, newCityNOeconym.Oeconym_ID);
                unitOfWork.OeconymRepository.AddMemberToCollection(newOeconym, o => o.CityNOeconymList, newCityNOeconym);
                //unitOfWork.CityRepository.SetForeignKey(newCity, c => c.City_ID, newCityNOeconym.City_ID);
                unitOfWork.CityRepository.AddMemberToCollection(newCity, c => c.CityNOeconymList, newCityNOeconym);
                unitOfWork.Save();
                //}
            }
        }

        private void ChangeOeconymOfCity(List<string> oeconymList, Models.CityDatabase.City city)
        {
            RemoveOeconymFromCity(city);
            ConnectOeconymToCity(oeconymList, city);
        }
        private void RemoveOeconymFromCity(Models.CityDatabase.City city)
        {
            List<CityNOeconym> cnoToRemove = [.. city.CityNOeconymList];

            foreach (CityNOeconym? cno in cnoToRemove)
            {
                unitOfWork.OeconymRepository.RemoveMemberFromCollection(cno.Oeconym, o => o.CityNOeconymList, cno);
                unitOfWork.CityRepository.RemoveMemberFromCollection(cno.City, c => c.CityNOeconymList, cno);
                unitOfWork.CityNOeconymRepository.Delete(cno);
                unitOfWork.Save();
            }
        }

        private void ConnectGeographyToCity(Geography? geography, Models.CityDatabase.City city)
        {
            if (!string.IsNullOrEmpty(geography?.GeographyName))
            {
                Geography newGeography = processGeography.CreateGeography(geography.GeographyName);
                unitOfWork.CityRepository.SetForeignKey(city, c => c.GeographyID, newGeography.Geography_ID);
                unitOfWork.GeographyRepository.AddMemberToCollection(newGeography, l => l.CityICollection, city);
                unitOfWork.Save();
            }
        }

        private void ChangeGeographyOfCity(Geography? geography, Models.CityDatabase.City city)
        {
            RemoveGeographyFromCity(city);
            ConnectGeographyToCity(geography, city);
        }

        private void RemoveGeographyFromCity(Models.CityDatabase.City city)
        {
            if (city.GeographyID != null && city.Geography != null)
            {
                unitOfWork.CityRepository.SetForeignKey(city, c => c.GeographyID, null);
                unitOfWork.GeographyRepository.RemoveMemberFromCollection(city.Geography, g => g.CityICollection, city);
                unitOfWork.Save();
            }
        }
    }
}
