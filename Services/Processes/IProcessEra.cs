using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Services.GenericClasses;
using Sammlerplattform.Services.UnitOfWork;

namespace Sammlerplattform.Services.Processes
{
    public interface IProcessEra
    {
        List<Era> GetWithPredicates(EraSearchParameterModel eraSearchParameter);
        Era Create(string eraLong, string? eraShort = null);
    }
    public class EraProcessor(IUnitOfWork unitOfWork) : IProcessEra
    {
        public Era Create(string eraLong, string? eraShort = null)
        {
            if (string.IsNullOrEmpty(eraLong))
            {
                throw new NullReferenceException();
            }
            Era? existingEra = (from e in unitOfWork.EraRepository.Get()
                                select e).Where(x => x.EraName != null && x.EraName.Equals(eraLong)).FirstOrDefault();

            if (existingEra != null)
            {
                return existingEra;
            }
            else
            {
                Era newEra = new() { EraName = eraLong };
                if (string.IsNullOrEmpty(eraShort))
                {
                    newEra.EraShort = eraShort;
                }

                newEra = unitOfWork.EraRepository.Insert(newEra);
                unitOfWork.Save();
                return newEra;
            }
        }

        public List<Era> GetWithPredicates(EraSearchParameterModel eraSearchParameter)
        {
            return [.. unitOfWork.EraRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Era>(eraSearchParameter))];
        }
    }
}
