using Sammlerplattform.Data;
using Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase.IndustryDatabase;
using Sammlerplattform.Models.Translations;

namespace Sammlerplattform.Services.DatabaseProcesses.ParticipantProcesses
{
    public interface IProcessIndustry
    {
        (int Statuscode, string Message, int id) Insert(IndustryCreateDTO industryCreateDto);
        (int Statuscode, string Message) Delete(int id);
        List<IndustryDisplayDTO> GetWithTranslationsListViaPredicates();
    }

    public class IndustryProcessor(IUnitOfWork unitOfWork
        , IProcessTranslations processTranslations) : IProcessIndustry
    {
        public (int Statuscode, string Message) Delete(int id)
        {
            unitOfWork.IndustryRepository.Delete(id);
            unitOfWork.Save();

            processTranslations.Delete(new EntityTranslationSearchParameter
            {
                EntityName = [nameof(Industry)],
                EntityId = [id],
                PropertyName = [nameof(IndustryDisplayDTO.IndustryName)]
            });

            return (200, "Success_Industry_Deleted");
        }

        public List<IndustryDisplayDTO> GetWithTranslationsListViaPredicates()
        {
            IQueryable<IndustryDisplayDTO> query = unitOfWork.IndustryRepository.Get().Select(i => new IndustryDisplayDTO
            {
                Id = i.Id
            });
            foreach (IndustryDisplayDTO industry in query)
            {
                industry.IndustryName = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
                {
                    EntityName = [nameof(Industry)],
                    EntityId = [industry.Id],
                    PropertyName = [nameof(IndustryDisplayDTO.IndustryName)]
                }).Select(t => t.TranslatedText).FirstOrDefault() ?? string.Empty;
            }
            return [.. query.OrderBy(x => x.IndustryName)];
        }

        public (int Statuscode, string Message, int id) Insert(IndustryCreateDTO industryCreateDto)
        {
            Industry newIndustry = unitOfWork.IndustryRepository.Insert(new Industry());
            unitOfWork.Save();

            processTranslations.Insert(new TranslationDTO
            {
                TextToTranslate = industryCreateDto.IndustryName,
                EntityName = nameof(Industry),
                EntityId = newIndustry.Id,
                PropertyName = nameof(IndustryDisplayDTO.IndustryName)
            });

            return (201, "Success_Industry_Created", newIndustry.Id);
        }
    }

}
