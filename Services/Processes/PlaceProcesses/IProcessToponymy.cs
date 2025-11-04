using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase;

namespace Sammlerplattform.Services.Processes.PlaceProcesses
{
    public interface IProcessToponymy
    {
        Toponymy CreateOrEditToponymy(Toponymy toponomy);
    }

    public class ToponymyProcessor(IUnitOfWork unitOfWork) : IProcessToponymy
    {
        public Toponymy CreateOrEditToponymy(Toponymy toponymy)
        {
            if (string.IsNullOrEmpty(toponymy.ToponymyName))
            {
                return new Toponymy() { ToponymyName = string.Empty };
            }

            Toponymy? existingToponymy = GetFirst(toponymy);
            if (existingToponymy != null)
            {
                return existingToponymy;
            }
            else
            {
                Toponymy newToponymy = unitOfWork.ToponymyRepository.Insert(toponymy);
                unitOfWork.Save();
                return newToponymy;
            }
        }

        private Toponymy? GetFirst(Toponymy toponymy)
        {
            return unitOfWork.ToponymyRepository
                .Get(m => m.ToponymyName == toponymy.ToponymyName)
                .FirstOrDefault();
        }
    }
}
