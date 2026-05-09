using Sammlerplattform.Data;
using Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Globalization;

namespace Sammlerplattform.Services.DatabaseProcesses.ParticipantProcesses
{
    public interface IProcessIndustry
    {
        (int Statuscode, string Message, int id) Insert(Industry industry);
        (int Statuscode, string Message) Delete(int id);
    }

    public class IndustryProcessor(IUnitOfWork unitOfWork
        , IProcessTranslations processTranslations
        , IDeeplTranslationService translationService) : IProcessIndustry
    {
        public (int Statuscode, string Message) Delete(int id)
        {
            unitOfWork.IndustryRepository.Delete(id);
            unitOfWork.Save();

            processTranslations.Delete(new EntityTranslationSearchParameter
            {
                EntityType = [nameof(Industry)],
                EntityId = [id],
                FieldName = [nameof(Industry.IndustryName)]
            });

            return (200, "Success_Industry_Deleted");
        }

        public (int Statuscode, string Message, int id) Insert(Industry industry)
        {
            Industry newIndustry = unitOfWork.IndustryRepository.Insert(industry);
            unitOfWork.Save();

            processTranslations.Insert(new TranslationDTO
            {
                TextToTranslate = industry.IndustryName,
                EntityType = nameof(Industry),
                EntityId = newIndustry.Id,
                FieldName = nameof(Industry.IndustryName),
                Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
            });

            return (201, "Success_Industry_Created", newIndustry.Id);
        }
    }

}
