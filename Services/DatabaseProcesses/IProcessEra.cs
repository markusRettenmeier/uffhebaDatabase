using Sammlerplattform.Data;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;

namespace Sammlerplattform.Services.DatabaseProcesses
{
    public interface IProcessEra
    {
        List<Era> GetWithPredicates(EraSearchParameterModel eraSearchParameter);
        (Era era, int statuscode, string message) Create(EraOperationParameterModel eraOperationParameterModel);
        (Era era, int statuscode, string message) Edit(EraOperationParameterModel eraOperationParameterModel);
    }
    public class EraProcessor(IUnitOfWork unitOfWork,
        DeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore) : IProcessEra
    {
        public (Era, int, string) Create(EraOperationParameterModel eraOperationParameterModel)
        {
            if (string.IsNullOrEmpty(eraOperationParameterModel.Era.EraName))
            {
                return (eraOperationParameterModel.Era, 404, "Error_EraName_Missing");
            }

            EraSearchParameterModel searchParameterModel = new() { EraName = [eraOperationParameterModel.Era.EraName] };
            List<int> entityIdList = [.. processTranslations.GetWithPredicate(new EntityTranslationSearchParameter
            {
                EntityType = [nameof(Toponymy)],
                TranslatedText = [eraOperationParameterModel.Era.EraName]
            }).Select(x => x.EntityId).Distinct()];
            if(entityIdList.Count > 0)
            {
                searchParameterModel.EraID = entityIdList;
            }
            Era? existingEra = GetWithPredicates(searchParameterModel).FirstOrDefault();
            if (existingEra != null)
            {
                return (existingEra, 303, "Error_Era_Exists");
            }
            
            try
            {
                Era newEra = new() { EraName = translationService.SetIntoFallbackLanguage(eraOperationParameterModel.Era.EraName) };
                if (string.IsNullOrEmpty(eraOperationParameterModel.Era.EraShort))
                {
                    newEra.EraShort = eraOperationParameterModel.Era.EraShort;
                }

                newEra = unitOfWork.EraRepository.Insert(newEra);
                unitOfWork.Save();

                processTranslations.Create(
                    new EntityTranslation
                    {
                        EntityType = nameof(Era),
                        EntityId = newEra.EraID,
                        FieldName = nameof(newEra.EraName),
                        TranslatedText = eraOperationParameterModel.Era.EraName,
                        Culture = translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name)
                    },
                    eraOperationParameterModel.Era.EraName);

                return (newEra, 201, "Success_Era_Created");
            }
            catch (Exception ex)
            {
                return (eraOperationParameterModel.Era, 500, "Error_Error_Ocurred");
            }
        }

        public (Era era, int statuscode, string message) Edit(EraOperationParameterModel eraOperationParameterModel)
        {
            if (eraOperationParameterModel.Era.EraID <= 0)
            {
                return (eraOperationParameterModel.Era, 404, "Error_Era_IdMissing");
            }
            if (string.IsNullOrEmpty(eraOperationParameterModel.Era.EraName))
            {
                return (eraOperationParameterModel.Era, 404, "Error_EraName_Missing");
            }

            Era? existingEra = (from e in unitOfWork.EraRepository.Get()
                                select e).Where(x => x.EraID == eraOperationParameterModel.Era.EraID).FirstOrDefault();
            if (existingEra == null)
            {
                return (eraOperationParameterModel.Era, 404, "Error_Era_NotFound");
            }

            string eraNameEnglish = translationService.SetIntoFallbackLanguage(eraOperationParameterModel.Era.EraName);
            if (existingEra.EraName != null &&
                !existingEra.EraName.Equals(eraNameEnglish))
            {
                existingEra.EraName = translationService.SetIntoFallbackLanguage(eraOperationParameterModel.Era.EraName);
            }
            if(existingEra.EraShort != eraOperationParameterModel.Era.EraShort) 
            { 
                existingEra.EraShort = eraOperationParameterModel.Era.EraShort;
            }
            unitOfWork.Save();
            return (existingEra, 200, "Success_Era_Updated");
        }

        public List<Era> GetWithPredicates(EraSearchParameterModel eraSearchParameter)
        {
            List<Era> eraList = [.. unitOfWork.EraRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Era>(eraSearchParameter))];

            foreach (Era era in eraList)
            {
                era.EraName = translationStore.GetTranslation(
                    nameof(Era),
                    era.EraID,
                    nameof(era.EraName),
                    translationService.NetCultureToDeeplLanguage(System.Globalization.CultureInfo.CurrentCulture.Name))
                    ?? era.EraName;
            }

            return [.. eraList.OrderBy(x => x.EraName)];
        }
    }
}
