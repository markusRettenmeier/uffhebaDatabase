using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Services.UnitOfWork;

namespace Sammlerplattform.Services.Processes.CityProcesses
{
    public interface IProcessCityNOeconym
    {
        CityNOeconym CreateCityNOeconym(Models.CityDatabase.City city, Oeconym oeconym, CityNOeconym cityNOeconym);
    }

    public class CityNOeconymProcessor(IUnitOfWork unitOfWork) : IProcessCityNOeconym
    {
        public CityNOeconym CreateCityNOeconym(Models.CityDatabase.City city, Oeconym oeconym, CityNOeconym cityNOeconym)
        {
            if (city.CityID == 0 || oeconym.Oeconym_ID == 0)
            {
                throw new NullReferenceException();
            }

            cityNOeconym.City_ID = city.CityID;
            cityNOeconym.City = city;
            cityNOeconym.Oeconym_ID = oeconym.Oeconym_ID;
            cityNOeconym.Oeconym = oeconym;
            CityNOeconym newCityNOeconym = unitOfWork.CityNOeconymRepository.Insert(cityNOeconym);
            unitOfWork.Save();

            return newCityNOeconym;
        }
    }
}

