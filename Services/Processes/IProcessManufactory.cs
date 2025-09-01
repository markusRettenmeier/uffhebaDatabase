using Sammlerplattform.Data;
using Sammlerplattform.Models.ManufactoryDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.Processes
{
    public interface IProcessManufactory
    {
        (Manufactory manufactory, int statuscode, string statusMessage) CreateManufactory(ManufactoryOperationParameterModel manufactoryParameterModel);
        (Manufactory manufactory, int statusCode, string statusMessage) EditManufactory(ManufactoryOperationParameterModel model);
        List<ManufactoryOperationParameterModel> GetManufactoryWithPredicates(ManufactorySearchParameterModel model);
        ManufactorySearchParameterModel ManufactoryParametersOperationToSearch(ManufactoryOperationParameterModel model);
    }

    public class ManufactoryProcessor(IUnitOfWork unitOfWork, ILogger<ManufactoryProcessor> logger) : IProcessManufactory
    {
        public (Manufactory manufactory, int statuscode, string statusMessage) CreateManufactory(ManufactoryOperationParameterModel model)
        {
            if (string.IsNullOrEmpty(model.Manufactory.ManufactoryName))
            {
                return (new() { ManufactoryName = string.Empty }, 412, "Herstellername fehlt.");
            }

            Manufactory? manufactorySelect = GetManufactoryWithPredicates(ManufactoryParametersOperationToSearch(model)).FirstOrDefault()?.Manufactory;
            if (manufactorySelect != null)
            {
                return (manufactorySelect, 302, "Hersteller existiert bereits.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                Manufactory newManufactory = unitOfWork.ManufactoryRepository.Insert(model.Manufactory);
                unitOfWork.Save();

                ConnectCityToManufactory(model.CityIDList, newManufactory);
                ConnectProductionFacilityToManufactory(model.ProductionFacility.ProductionFacilityName, newManufactory);

                scope.Complete();
                return (newManufactory, 201, "Hersteller wurde erstellt.");
            }
            catch (Exception ex)
            {
                logger.LogError("Fehler beim Erstellen des Herstellers: {ex}", ex);
                return (new() { ManufactoryName = string.Empty }, 500, "Es ist ein Fehler beim Hinzufügen des Ortes aufgetreten. Der Support wurde benachrichtigt.");
            }
        }

        public (Manufactory manufactory, int statusCode, string statusMessage) EditManufactory(ManufactoryOperationParameterModel model)
        {
            if (model.Manufactory.ManufactoryID == 0)
            {
                return (new() { ManufactoryName = string.Empty }, 412, "Hersteller_ID fehlt.");
            }
            else if (string.IsNullOrEmpty(model.Manufactory.ManufactoryName))
            {
                return (new() { ManufactoryName = string.Empty }, 412, "Herstellername fehlt.");
            }

            Manufactory? manufactorySelect = GetManufactoryWithPredicates(ManufactoryParametersOperationToSearch(model)).FirstOrDefault()?.Manufactory;
            if (manufactorySelect == null)
            {
                return (new() { ManufactoryName = string.Empty }, 302, "Hersteller existiert nicht.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                manufactorySelect.ManufactoryName = model.Manufactory.ManufactoryName;
                unitOfWork.Save();

                ChangeCityOfManufactory(model.CityIDList, manufactorySelect);
                ChangeProductionFacilityOfManufactory(model.ProductionFacility.ProductionFacilityName, manufactorySelect);

                scope.Complete();
                return (manufactorySelect, 201, "Hersteller wurde erstellt.");
            }
            catch (Exception ex)
            {
                logger.LogError("Fehler beim Hinzufügen des Herstellers: {ex}", ex);
                return (new() { ManufactoryName = string.Empty }, 500, "Es ist ein Fehler beim Hinzufügen des Ortes aufgetreten. Der Support wurde benachrichtigt.");
            }
        }

        private void ConnectCityToManufactory(List<int> cityList, Manufactory manufactory)
        {
            foreach (int city in cityList)
            {
                if (city > 0)
                {
                    Models.CityDatabase.City? existingCity = (from c in unitOfWork.CityRepository.Get()
                                                              where c.CityID == city
                                                              select c).FirstOrDefault();
                    if (existingCity != null)
                    {
                        unitOfWork.ManufactoryRepository.AddMemberToCollection(manufactory, m => m.CityList, existingCity);
                        unitOfWork.CityRepository.AddMemberToCollection(existingCity, c => c.ManufactoryList, manufactory);
                        unitOfWork.Save();
                    }
                    else
                    {
                        logger.LogWarning("Eingegebener Ort {city} nicht verfügbar.", city);
                    }
                }
            }
        }
        private void ChangeCityOfManufactory(List<int> cityList, Manufactory manufactory)
        {
            RemoveCityFromManufactory(manufactory);
            ConnectCityToManufactory(cityList, manufactory);
        }
        private void RemoveCityFromManufactory(Manufactory manufactory)
        {
            List<Models.CityDatabase.City> citiesToRemove = [.. manufactory.CityList];

            foreach (Models.CityDatabase.City city in citiesToRemove)
            {
                unitOfWork.ManufactoryRepository.RemoveMemberFromCollection(manufactory, m => m.CityList, city);
                unitOfWork.CityRepository.RemoveMemberFromCollection(city, c => c.ManufactoryList, manufactory);
                unitOfWork.Save();
            }
        }

        private void ConnectProductionFacilityToManufactory(string productionFacilityName, Manufactory manufactory)
        {
            if (!string.IsNullOrEmpty(productionFacilityName))
            {
                ProductionFacility? existingSector = (from s in unitOfWork.ProductionFacilityRepository.Get()
                                                      where s.ProductionFacilityName == productionFacilityName
                                                      select s).FirstOrDefault();

                if (existingSector != null)
                {
                    unitOfWork.ProductionFacilityRepository.AddMemberToCollection(existingSector, i => i.ManufactoryList, manufactory);
                    unitOfWork.ManufactoryRepository.SetForeignKey(manufactory, m => m.ProductionFacility_ID, existingSector.ProductionFacilityID);
                    unitOfWork.Save();
                }
                else
                {
                    logger.LogWarning("Eingegebene Produktionsstätte {productionFacilityName} nicht vorhanden", productionFacilityName);
                }
            }
        }
        private void ChangeProductionFacilityOfManufactory(string productionFacilityName, Manufactory manufactory)
        {
            RemoveProductionFacilityFromManufactory(manufactory);
            ConnectProductionFacilityToManufactory(productionFacilityName, manufactory);
        }
        private void RemoveProductionFacilityFromManufactory(Manufactory manufactory)
        {
            if (manufactory.ProductionFacility != null)
            {
                unitOfWork.ProductionFacilityRepository.RemoveMemberFromCollection(manufactory.ProductionFacility, i => i.ManufactoryList, manufactory);
                unitOfWork.ManufactoryRepository.SetForeignKey(manufactory, m => m.ProductionFacility_ID, null);
                unitOfWork.Save();
            }
        }

        public List<ManufactoryOperationParameterModel> GetManufactoryWithPredicates(ManufactorySearchParameterModel model)
        {
            IEnumerable<Manufactory> manufactorySelect = unitOfWork.ManufactoryRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Manufactory>(model),
                includeProperties: "CityList,CityList.CityOeconymList.Oeconym,ProductionFacility"
                );

            return [.. from c in manufactorySelect
                       select new ManufactoryOperationParameterModel{
                           Manufactory = c
                       }];
        }

        public ManufactorySearchParameterModel ManufactoryParametersOperationToSearch(ManufactoryOperationParameterModel model)
        {
            ManufactorySearchParameterModel searchParameterModel = new();
            searchParameterModel.ManufactoryID.Add(model.Manufactory.ManufactoryID);
            searchParameterModel.ManufactoryName.Add(model.Manufactory.ManufactoryName);
            searchParameterModel.CityID.Add(model.City.CityID);
            searchParameterModel.ProductionFacility_ProductionFacilityName.Add(model.ProductionFacility.ProductionFacilityName);

            return searchParameterModel;
        }
    }
}
