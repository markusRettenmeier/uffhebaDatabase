using Sammlerplattform.Data;
using Sammlerplattform.Models.CityDatabase;

namespace Sammlerplattform.Services.Processes.CityProcesses
{
    public interface IProcessOeconym
    {
        Oeconym CreateOeconym(Oeconym oeconym);
    }
    public class OeconymProcessor(IUnitOfWork unitOfWork) : IProcessOeconym
    {
        public Oeconym CreateOeconym(Oeconym oeconym)
        {
            if (string.IsNullOrEmpty(oeconym.OeconymName))
            {
                throw new NullReferenceException();
            }

            Oeconym? existingOeconym = GetOeconym(oeconym);
            if (existingOeconym != null)
            {
                return existingOeconym;
            }
            else
            {
                Oeconym newOeconym = new() { OeconymName = oeconym.OeconymName };
                newOeconym = unitOfWork.OeconymRepository.Insert(newOeconym);
                unitOfWork.Save();
                return newOeconym;
            }
        }

        public Oeconym? GetOeconym(Oeconym oeconym)
        {
            return (from o in unitOfWork.OeconymRepository.Get()
                    where o.OeconymName == oeconym.OeconymName
                    select o).FirstOrDefault();
        }
    }
}
