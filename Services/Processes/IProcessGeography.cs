using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Services.UnitOfWork;

namespace Sammlerplattform.Services.Processes
{
    public interface IProcessGeography
    {
        Geography CreateGeography(string geographyName);
    }

    public class GeographyProcessor(IUnitOfWork unitOfWork) : IProcessGeography
    {
        public Geography CreateGeography(string geographyName)
        {
            if (string.IsNullOrEmpty(geographyName))
            {
                throw new NullReferenceException();
            }

            Geography? existingGeography = (from l in unitOfWork.GeographyRepository.Get()
                                            select l).Where(x => x.GeographyName != null && x.GeographyName.Equals(geographyName)).FirstOrDefault();

            if (existingGeography != null)
            {
                return existingGeography;
            }
            else
            {
                Geography newGeography = new() { GeographyName = geographyName };
                newGeography = unitOfWork.GeographyRepository.Insert(newGeography);
                unitOfWork.Save();
                return newGeography;
            }
        }
    }
}
