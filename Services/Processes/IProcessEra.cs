using Sammlerplattform.Data;
using Sammlerplattform.Models.EraDatabase;

namespace Sammlerplattform.Services.Processes
{
    public interface IProcessEra
    {
        List<Era> GetWithPredicates(EraSearchParameterModel eraSearchParameter);
        (Era era, int statuscode, string message) Create(EraOperationParameterModel eraOperationParameterModel);
        (Era era, int statuscode, string message) Edit(EraOperationParameterModel eraOperationParameterModel);
    }
    public class EraProcessor(IUnitOfWork unitOfWork) : IProcessEra
    {
        public (Era, int, string) Create(EraOperationParameterModel eraOperationParameterModel)
        {
            if (string.IsNullOrEmpty(eraOperationParameterModel.Era.EraName))
            {
                return (eraOperationParameterModel.Era, 404, "Epochenname fehlt.");
            }
            Era? existingEra = (from e in unitOfWork.EraRepository.Get()
                                select e).Where(x => x.EraName != null && x.EraName.Equals(eraOperationParameterModel.Era.EraName)).FirstOrDefault();

            if (existingEra != null)
            {
                return (existingEra, 303, "Epoche existiert bereits.");
            }
            else
            {
                Era newEra = new() { EraName = eraOperationParameterModel.Era.EraName };
                if (string.IsNullOrEmpty(eraOperationParameterModel.Era.EraShort))
                {
                    newEra.EraShort = eraOperationParameterModel.Era.EraShort;
                }

                newEra = unitOfWork.EraRepository.Insert(newEra);
                unitOfWork.Save();
                return (newEra, 201, "Epoche wurde erstellt.");
            }
        }

        public (Era era, int statuscode, string message) Edit(EraOperationParameterModel eraOperationParameterModel)
        {
            if (string.IsNullOrEmpty(eraOperationParameterModel.Era.EraName))
            {
                return (eraOperationParameterModel.Era, 404, "Epochenname fehlt.");
            }

            Era? existingEra = (from e in unitOfWork.EraRepository.Get()
                                select e).Where(x => x.EraID == eraOperationParameterModel.Era.EraID).FirstOrDefault();
            if (existingEra == null)
            {
                return (eraOperationParameterModel.Era, 404, "Epoche nicht gefunden.");
            }

            existingEra.EraName = eraOperationParameterModel.Era.EraName;
            existingEra.EraShort = eraOperationParameterModel.Era.EraShort;
            unitOfWork.Save();
            return (existingEra, 200, "Epoche wurde aktualisiert.");
        }

        public List<Era> GetWithPredicates(EraSearchParameterModel eraSearchParameter)
        {
            return [.. unitOfWork.EraRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Era>(eraSearchParameter))];
        }
    }
}
