using Sammlerplattform.Models.ProcessOfManufactureDatabase;
using Sammlerplattform.Services.GenericClasses;
using Sammlerplattform.Services.UnitOfWork;

namespace Sammlerplattform.Services.Processes
{
    public interface IProcessProcessOfManufacture
    {
        List<ProcessOfManufacture> GetWithPredicates(ProcessOfManufactureSearchParameter processOfManufactureSearchParameterModel);
        (ProcessOfManufacture ProcessOfManufacture, int statuscode, string message) Create(ProcessOfManufacture processOfManufacture);
        (ProcessOfManufacture ProcessOfManufacture, int statuscode, string message) Edit(ProcessOfManufacture processOfManufacture);
        ProcessOfManufactureSearchParameter ParametersOperationToSearch(ProcessOfManufacture processOfManufacture);
    }

    public class ProcessOfManufactureProcessor(IUnitOfWork unitOfWork) : IProcessProcessOfManufacture
    {
        public (ProcessOfManufacture ProcessOfManufacture, int statuscode, string message) Create(ProcessOfManufacture processOfManufacture)
        {
            throw new NotImplementedException();
        }

        public (ProcessOfManufacture ProcessOfManufacture, int statuscode, string message) Edit(ProcessOfManufacture processOfManufacture)
        {
            throw new NotImplementedException();
        }

        public List<ProcessOfManufacture> GetWithPredicates(ProcessOfManufactureSearchParameter processOfManufactureSearchParameterModel)
        {
            return [.. unitOfWork.ProcessOfManufactureRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<ProcessOfManufacture>(processOfManufactureSearchParameterModel))];
        }

        public ProcessOfManufactureSearchParameter ParametersOperationToSearch(ProcessOfManufacture processOfManufacture)
        {
            ProcessOfManufactureSearchParameter searchParameter = new()
            {
                ProcessOfManufactureID = [processOfManufacture.ProcessOfManufactureID],
                Mainprocess = [processOfManufacture.Mainprocess],
                ProcessOfManufactureName = [processOfManufacture.ProcessOfManufactureName],
                Technique = [processOfManufacture.Technique ?? string.Empty],
                Description = [processOfManufacture.Description ?? string.Empty]
            };
            return searchParameter;
        }
    }
}
