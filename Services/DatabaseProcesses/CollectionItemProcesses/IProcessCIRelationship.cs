using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Translation;
using System.Globalization;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessCIRelationship
    {
        (int StatusCode, string StatusMessage, int Id) Insert(CIRelationshipCreateDTO createDTO);
        (int StatusCode, string StatusMessage, int Id) Update(CIRelationshipEditDTO editDto);
        (int StatusCode, string StatusMessage) Delete(int id);
        List<CollectionItemRelationship> GetListWithPredicates(CIRelationshipSearchParameterModel cppRelationshipSearchParameterModel);
    }

    public class CIRelationshipProcessor(IUnitOfWork unitOfWork,
        IDeeplTranslationService translationService,
        IProcessTranslations processTranslations,
        ITranslationStore translationStore,
        ITrackEventsCSV trackEvents) : IProcessCIRelationship
    {
        public (int StatusCode, string StatusMessage) Delete(int id)
        {
            CIRelationshipSearchParameterModel searchParameterModel = new()
            {
                CollectionItemRelationshipId = [id]
            };
            CollectionItemRelationship? relationshipToDelete = GetListWithPredicates(searchParameterModel).FirstOrDefault();
            if (relationshipToDelete == null)
            {
                return (404, "Error_CIRelationship_NotFound");
            }

            if (relationshipToDelete.CollectionItemNParticipantList.Count != 0 || relationshipToDelete.CollectionItemNPlaceList.Count != 0)
            {
                trackEvents.TrackError("Attempted to delete CollecitonItemParticipantPlaceRelationship that is in use", new Dictionary<string, object>
                {
                    { "CollecitonItemParticipantPlaceRelationshipId", id }
                });
                return (409, "Error_CIRelationship_InUse");
            }

            try
            {
                using TransactionScope scope = new();

                processTranslations.Delete(new EntityTranslationSearchParameter
                {
                    EntityType = [nameof(CollectionItemRelationship)],
                    EntityId = [relationshipToDelete.CollectionItemRelationshipId],
                    FieldName = [nameof(CollectionItemRelationship.CollectionItemRelationshipName)]
                });

                unitOfWork.CppRelationshipRepository.Delete(relationshipToDelete);
                unitOfWork.Save();

                scope.Complete();
                return (200, "Success_CIRelationship_Deleted");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Attempted to delete CollecitonItemRelationship", new Dictionary<string, object>
                {
                    { "CollecitonItemRelationshipId", id }
                });
                return (500, "Error_Unknown");
            }
        }

        public List<CollectionItemRelationship> GetListWithPredicates(CIRelationshipSearchParameterModel cppRelationshipSearchParameterModel)
        {
            foreach (string name in cppRelationshipSearchParameterModel.CollectionItemRelationshipName)
            {
                int cppRelationshipId = CheckIfNameExists(name);
                if (cppRelationshipId > 0)
                {
                    cppRelationshipSearchParameterModel.CollectionItemRelationshipId.Add(cppRelationshipId);
                }
            }
            cppRelationshipSearchParameterModel.CollectionItemRelationshipName = [];

            IEnumerable<CollectionItemRelationship> cppRelationships = unitOfWork.CppRelationshipRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionItemRelationship>(cppRelationshipSearchParameterModel));
            foreach (CollectionItemRelationship cppRelationship in cppRelationships)
            {
                cppRelationship.CollectionItemRelationshipName = translationStore.GetTranslation(
                    nameof(CollectionItemRelationship),
                    cppRelationship.CollectionItemRelationshipId,
                    nameof(CollectionItemRelationship.CollectionItemRelationshipName),
                    translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name))
                    ?? string.Empty;
            }

            return [.. cppRelationships.OrderBy(c => c.CollectionItemRelationshipName)];
        }

        public (int StatusCode, string StatusMessage, int Id) Insert(CIRelationshipCreateDTO createDTO)
        {
            int cppRelationshipId = CheckIfNameExists(createDTO.Name);
            if (cppRelationshipId > 0)
            {
                return (409, "Error_CIRelationship_Exists", cppRelationshipId);
            }

            try
            {
                using TransactionScope scope = new();

                CollectionItemRelationship cppRelationship = new();
                cppRelationship = unitOfWork.CppRelationshipRepository.Insert(cppRelationship);
                unitOfWork.Save();

                processTranslations.Insert(
                    new TranslationDTO
                    {
                        TextToTranslate = createDTO.Name,
                        EntityType = nameof(CollectionItemRelationship),
                        EntityId = cppRelationship.CollectionItemRelationshipId,
                        FieldName = nameof(CollectionItemRelationship.CollectionItemRelationshipName),
                        Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                    });

                scope.Complete();
                return (201, "Success_CIRelationship_Created", cppRelationship.CollectionItemRelationshipId);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Attempted to create CollecitonItemRelationship", new Dictionary<string, object>
                {
                    { "CollecitonItemRelationship", createDTO.Name }
                });
                return (500, "Error_Unknown", 0);
            }
        }

        public (int StatusCode, string StatusMessage, int Id) Update(CIRelationshipEditDTO editDto)
        {

            CIRelationshipSearchParameterModel searchParameterModel = new()
            {
                CollectionItemRelationshipId = [editDto.Id]
            };
            CollectionItemRelationship? relationshipToUpdate = GetListWithPredicates(searchParameterModel).FirstOrDefault();
            if (relationshipToUpdate == null)
            {
                trackEvents.TrackError("Attempted to update non existing CollecitonItemParticipantPlaceRelationship", new Dictionary<string, object>
                {
                    { "CollecitonItemRelationshipId", editDto.Id }
                });
                return (404, "Error_CIRelationship_NotFound", editDto.Id);
            }

            try
            {
                using TransactionScope scope = new();

                processTranslations.Update(new TranslationDTO
                {
                    TextToTranslate = editDto.Name,
                    EntityType = nameof(CollectionItemRelationship),
                    EntityId = relationshipToUpdate.CollectionItemRelationshipId,
                    FieldName = nameof(CollectionItemRelationship.CollectionItemRelationshipName),
                    Culture = translationService.NetCultureToDeeplLanguage(CultureInfo.CurrentCulture.Name)
                });

                scope.Complete();
                return (200, "Success_CIRelationship_Updated", relationshipToUpdate.CollectionItemRelationshipId);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "Attempted to update CollecitonItemRelationship", new Dictionary<string, object>
                {
                    { "CollecitonItemRelationshipId", editDto.Id },
                    { "NewName", editDto.Name }
                });
                return (500, "Error_Unknown", editDto.Id);
            }
        }

        private int CheckIfNameExists(string name)
        {
            int? cppRelationshipId = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityType = [nameof(CollectionItemRelationship)],
                FieldName = [nameof(CollectionItemRelationship.CollectionItemRelationshipName)],
                TranslatedText = [name]
            }).Select(et => et.EntityId).FirstOrDefault();

            return cppRelationshipId ?? 0;
        }
    }
}