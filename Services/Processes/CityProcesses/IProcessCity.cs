using Sammlerplattform.Data;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Services.Processes.PlaceProcesses;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.CityProcesses
{
    public interface IProcessCity
    {
        (City City, int Statuscode, string Message) Create(CityOperationParameterModel model);
        (City City, int Statuscode, string Message) Edit(CityOperationParameterModel model);
        List<CityOperationParameterModel> GetWithPredicates(CitySearchParameterModel model);
        CitySearchParameterModel ParametersOperationToSearch(CityOperationParameterModel model);
    }

    public class CityProcessor(IProcessGeography processGeography,
                                IProcessPostalcode processPostalcode,
                                IProcessOeconym processOeconym,
                                IUnitOfWork unitOfWork, ILogger<CityProcessor> logger) : IProcessCity
    {
        public (City City, int Statuscode, string Message) Create(CityOperationParameterModel model)
        {
            if (model.CityOeconymList.Count == 0)
            {
                return (new(), 412, "Ortsnamen angeben.");
            }

            List<CityOperationParameterModel> cityExists = GetWithPredicates(ParametersOperationToSearch(model));
            if (cityExists != null)
            {
                return (cityExists.First().City, 302, "Eintrag existiert bereits.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                City newCity = unitOfWork.CityRepository.Insert(model.City);
                unitOfWork.Save();

                foreach(CityPostalcode? postalcode in model.CityPostalcodeList)
                {
                    ConnectPostalcode(newCity, postalcode.Postalcode.PostalcodeNumber, postalcode.EraID);
                }
                foreach(CityOeconym? cityOeconym in model.CityOeconymList)
                {
                    ConnectOeconym(newCity, cityOeconym);
                }
                ConnectGeography(newCity, model.Geography);

                scope.Complete();
                return (newCity, 201, "Ort wurde erstellt.");
            }
            catch (Exception ex)
            {
                logger.LogError("Fehler beim Hinzufügen des Ortes: {ex}", ex);
                return (new(), 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public (City City, int Statuscode, string Message) Edit(CityOperationParameterModel model)
        {
            if (model.CityOeconymList.Count == 0)
            {
                return (model.City, 412, "Ortsnamen angeben.");
            }

            City? citySelect = unitOfWork.CityRepository.GetByID(model.City.CityID);
            if (citySelect == null)
            {
                return (model.City, 302, "Eintrag existiert nicht.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                citySelect.Byname = model.City.Byname;
                unitOfWork.Save();

                SyncPostalcode(citySelect, model.CityPostalcodeList);
                SyncOeconym(citySelect, model.CityOeconymList);
                SyncGeography(model.Geography, citySelect);

                scope.Complete();
                return (citySelect, 201, "Ort wurde geändert.");
            }
            catch (Exception ex)
            {
                logger.LogError("Fehler beim Ändern des Ortes: {ex}", ex);
                return (citySelect, 500, "Es ist ein Fehler beim Hinzufügen des Ortes aufgetreten. Der Support wurde benachrichtigt.");
            }
        }

        public List<CityOperationParameterModel> GetWithPredicates(CitySearchParameterModel model)
        {
            IEnumerable<City> cityResults = unitOfWork.CityRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<City>(model),
                includeProperties: "CityOeconymList.Oeconym," +
                "CityPostalcodeList.Postalcode," +
                "Geography," +
                "ParentCity.CityOeconymList.Oeconym"
            );

            return [.. from c in cityResults
                       select new CityOperationParameterModel {
                           City = c,
                           CityPostalcodeList = c.CityPostalcodeList,
                           CityOeconymList = c.CityOeconymList,
                       }];
        }

        public CitySearchParameterModel ParametersOperationToSearch(CityOperationParameterModel model)
        {
            CitySearchParameterModel citySearchParameterModel = new();
            citySearchParameterModel.CityID.Add(model.City.CityID);
            citySearchParameterModel.CityOeconymList_Oeconym_OeconymName = [.. model.CityOeconymList.Select(x => x.Oeconym.OeconymName)];
            citySearchParameterModel.CityPostalcodeList_Postalcode_PostalcodeNumber = [.. model.CityPostalcodeList.Select(x => x.Postalcode.PostalcodeNumber)];
            citySearchParameterModel.Geography_GeographyName = model.Geography?.GeographyName != null ? [model.Geography.GeographyName] : [];
            citySearchParameterModel.ParentCityID = model.City.ParentCityID != null ? [model.City.ParentCityID.Value] : [];

            if (!string.IsNullOrEmpty(model.City.Byname))
            {
                citySearchParameterModel.Byname.Add(model.City.Byname);
            }
            if (model.City.ParentCityID != null)
            {
                citySearchParameterModel.ParentCityID.Add((int)model.City.ParentCityID);
            }

            return citySearchParameterModel;
        }

        private void ConnectPostalcode(City city, string Postalcode, int? eraID)
        {
            if (string.IsNullOrEmpty(Postalcode))
            {
                return;
            }

            Postalcode? postalcode = processPostalcode.CreateOrGetPostalcode(Postalcode);

            CityPostalcode cityPostalcode = new()
            {
                CityID = city.CityID,
                PostalcodeID = postalcode.PostalcodeID,
                EraID = eraID
            };
            _ = unitOfWork.CityPostalcodeRepository.Insert(cityPostalcode);
            unitOfWork.Save();
        }
        private void SyncPostalcode(City city, List<CityPostalcode> newConnections)
        {
            List<CityPostalcode> currentConnections = city.CityPostalcodeList;

            foreach (CityPostalcode? currentConnection in currentConnections)
            {
                CityPostalcode? updatedConnection = newConnections.FirstOrDefault(c => c.PostalcodeID == currentConnection.PostalcodeID);
                if(updatedConnection == null)
                {
                    DisconnectPostalcode(city, currentConnection.Postalcode.PostalcodeID);
                }
                else if(updatedConnection.EraID != currentConnection.EraID)
                {
                    UpdateCityPostalcode(city, currentConnection.Postalcode, updatedConnection.EraID);
                }
            }

            foreach (CityPostalcode newItem in newConnections)
            {
                bool exists = currentConnections.Any(c => c.PostalcodeID == newItem.PostalcodeID);
                if(!exists)
                {
                    ConnectPostalcode(city, newItem.Postalcode.PostalcodeNumber, newItem.EraID);
                }
            }
        }
        private void UpdateCityPostalcode(City city, Postalcode postalcode, int? eraID)
        {
            CityPostalcode? cityPostalcode = unitOfWork.CityPostalcodeRepository.Get(
                filter: c => c.CityID == city.CityID && c.PostalcodeID == postalcode.PostalcodeID).FirstOrDefault();

            if (cityPostalcode != null)
            {                 
                cityPostalcode.EraID = eraID;
                unitOfWork.Save();
            }
        }
        private void DisconnectPostalcode(City city, int postalcodeID)
        {
            if(postalcodeID == 0 || postalcodeID == 0)
            {
                return;
            }
            
            CityPostalcode? cityPostalcode = unitOfWork.CityPostalcodeRepository.Get(
                filter: c => c.CityID == city.CityID && c.PostalcodeID == postalcodeID).FirstOrDefault();
            if (cityPostalcode != null)
            {
                unitOfWork.CityPostalcodeRepository.Delete(cityPostalcode);
                unitOfWork.Save();
            }
        }

        private void ConnectOeconym(City city, CityOeconym cityOeconym)
        {
            if(string.IsNullOrEmpty(cityOeconym.Oeconym.OeconymName))
            {
                return;
            }

                Oeconym newOeconym = new() { OeconymName = cityOeconym.Oeconym.OeconymName };
                newOeconym = processOeconym.CreateOeconym(newOeconym);

                CityOeconym newCityNOeconym = new() { 
                    CityID = city.CityID,
                    OeconymID = newOeconym.OeconymID,
                    CurrentName = cityOeconym.CurrentName,
                    EraID = cityOeconym.EraID
                };
                _ = unitOfWork.CityNOeconymRepository.Insert(newCityNOeconym);
                unitOfWork.Save();
        }
        private void SyncOeconym(City city, List<CityOeconym> newConnections)
        {
            List<CityOeconym> currentConnections = city.CityOeconymList;

            foreach (CityOeconym? current in currentConnections)
            {
                CityOeconym? updatedConnection = newConnections.FirstOrDefault(x => x.OeconymID == current.OeconymID);
                if (updatedConnection == null)
                {
                    DisconnectOeconym(city, current.OeconymID);
                }
                else if (updatedConnection != null && (updatedConnection.EraID != current.EraID || updatedConnection.CurrentName != current.CurrentName))
                {
                    UpdateCityOeconym(city, current, updatedConnection.EraID, updatedConnection.CurrentName);
                }
                // else: Beziehung ist gleich, keine Änderung notwendig
            }

            foreach (CityOeconym newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.OeconymID == newItem.OeconymID);
                if (!exists)
                {
                    ConnectOeconym(city, newItem);
                }
            }
        }
        private void UpdateCityOeconym(City city, CityOeconym currentConnection, int? newEraID, bool currentName)
        {
            CityOeconym? cityOeconym = unitOfWork.CityNOeconymRepository.Get(
                filter: c => c.CityID == city.CityID && c.OeconymID == currentConnection.OeconymID).FirstOrDefault();
            if (cityOeconym != null)
            {
                cityOeconym.EraID = newEraID;
                cityOeconym.CurrentName = currentName;
                unitOfWork.Save();
            }
        }
        private void DisconnectOeconym(City city, int oeconymID)
        {
            if(city.CityID == 0 || oeconymID == 0)
            {
                return;
            }

            CityOeconym? cityOeconym = unitOfWork.CityNOeconymRepository.Get(
                filter: c => c.CityID == city.CityID && c.OeconymID == oeconymID).FirstOrDefault();
            if (cityOeconym == null)
            {
                return;
            }
            unitOfWork.CityNOeconymRepository.Delete(cityOeconym);
            unitOfWork.Save();
        }

        private void ConnectGeography(City city, Geography? geography)
        {
            if (!string.IsNullOrEmpty(geography?.GeographyName))
            {
                Geography newGeography = processGeography.Create(geography.GeographyName);
                unitOfWork.CityRepository.SetForeignKey(city, c => c.GeographyID, newGeography.Geography_ID);
                unitOfWork.Save();
            }
        }
        private void SyncGeography(Geography? geography, City city)
        {
                if (geography == null)
                {
                    DisconnectGeography(city);
            }

                bool exists = geography != null && city.GeographyID == geography.Geography_ID;
                if (!exists)
                {
                    ConnectGeography(city, geography);
                }
        }
        private void DisconnectGeography(City city)
        {
            if (city.GeographyID != null && city.Geography != null)
            {
                unitOfWork.CityRepository.SetForeignKey(city, c => c.GeographyID, null);
                unitOfWork.Save();
            }
        }
    }
}
