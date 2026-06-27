using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase.Toponymy;

namespace Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses
{
    public interface IProcessToponymy
    {
        int Insert(string name);
        void Delete(int id);
    }

    public class ToponymyProcessor(IUnitOfWork unitOfWork) : IProcessToponymy
    {
        public void Delete(int id)
        {
            unitOfWork.ToponymyRepository.Delete(id);
            unitOfWork.Save();
        }
        private List<Toponymy> GetListWithPredicate(ToponymySearchParameterModel toponymySearchParameter)
        {
            IEnumerable<Toponymy> toponymyQuery = unitOfWork.ToponymyRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Toponymy>(toponymySearchParameter));

            return [.. toponymyQuery.Order()];
        }

        public int Insert(string name)
        {
            int? toponymyId = GetListWithPredicate(new ToponymySearchParameterModel { ToponymyName = [name] }).FirstOrDefault()?.ToponymyID;
            if (toponymyId != null)
            {
                return toponymyId.Value;
            }

            Toponymy newToponymy = new()
            {
                ToponymyName = name
            };
            newToponymy = unitOfWork.ToponymyRepository.Insert(newToponymy);
            unitOfWork.Save();
            return newToponymy.ToponymyID;
        }
    }
}
