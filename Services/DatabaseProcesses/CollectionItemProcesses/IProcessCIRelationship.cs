using Sammlerplattform.Data;
using Sammlerplattform.Models.CollectionItemDatabase.CollectionItemRelationshipDatabase;
using Sammlerplattform.Models.Translations;
using System.Data.Entity;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessCIRelationship
    {
        (int StatusCode, string StatusMessage, int Id) Insert(CIRelationshipCreateDTO createDTO);
        (int StatusCode, string StatusMessage, int Id) Update(CIRelationshipEditDTO editDto);
        (int StatusCode, string StatusMessage) Delete(int id);
        List<CollectionItemRelationship> GetEntityListViaPredicates(CIRelationshipSearchParameterModel cppRelationshipSearchParameterModel);
        List<CIRelationshipDisplayDTO> GetWithTranslationsListViaPredicates(CIRelationshipSearchParameterModel cppRelationshipSearchParameterModel);
    }

    public class CIRelationshipProcessor(IUnitOfWork unitOfWork,
        IProcessTranslations processTranslations,
        ITrackEventsText trackEvents) : IProcessCIRelationship
    {
        public (int StatusCode, string StatusMessage) Delete(int id)
        {
            CIRelationshipSearchParameterModel searchParameterModel = new()
            {
                CollectionItemRelationshipId = [id]
            };
            CollectionItemRelationship? relationshipToDelete = GetEntityListViaPredicates(searchParameterModel).FirstOrDefault();
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
                    EntityName = [nameof(CollectionItemRelationship)],
                    EntityId = [relationshipToDelete.CollectionItemRelationshipId],
                    PropertyName = [nameof(CIRelationshipDisplayDTO.CollectionItemRelationshipName)]
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

        public List<CIRelationshipDisplayDTO> GetWithTranslationsListViaPredicates(CIRelationshipSearchParameterModel cppRelationshipSearchParameterModel)
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

            List<CIRelationshipDisplayDTO> cppRelationshipList = [.. unitOfWork.CppRelationshipRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionItemRelationship>(cppRelationshipSearchParameterModel))
                .AsNoTracking()
                .Select(c => new CIRelationshipDisplayDTO
                {
                    Id = c.CollectionItemRelationshipId,
                    CollectionItemNParticipantList = c.CollectionItemNParticipantList,
                    CollectionItemNPlaceList = c.CollectionItemNPlaceList
                })];
            SetTranslations(cppRelationshipList);

            return [.. cppRelationshipList.OrderBy(c => c.CollectionItemRelationshipName)];
        }

        private void SetTranslations(List<CIRelationshipDisplayDTO> cppRelationshipList)
        {
            var allTranslations = processTranslations.GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityName = [nameof(CollectionItemRelationship)],
                PropertyName = [nameof(CIRelationshipDisplayDTO.CollectionItemRelationshipName)]
            }).ToList();
            foreach (CIRelationshipDisplayDTO cppRelationship in cppRelationshipList)
            {
                cppRelationship.CollectionItemRelationshipName = allTranslations.FirstOrDefault(t => t.EntityId == cppRelationship.Id)?.TranslatedText ?? string.Empty;
            }
        }

        public List<CollectionItemRelationship> GetEntityListViaPredicates(CIRelationshipSearchParameterModel cppRelationshipSearchParameterModel)
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

            IQueryable<CollectionItemRelationship> cppRelationships = unitOfWork.CppRelationshipRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<CollectionItemRelationship>(cppRelationshipSearchParameterModel));

            return [.. cppRelationships];
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
                        EntityName = nameof(CollectionItemRelationship),
                        EntityId = cppRelationship.CollectionItemRelationshipId,
                        PropertyName = nameof(CIRelationshipDisplayDTO.CollectionItemRelationshipName)
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
            CollectionItemRelationship? relationshipToUpdate = GetEntityListViaPredicates(searchParameterModel).FirstOrDefault();
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
                    EntityName = nameof(CollectionItemRelationship),
                    EntityId = relationshipToUpdate.CollectionItemRelationshipId,
                    PropertyName = nameof(CIRelationshipDisplayDTO.CollectionItemRelationshipName)
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
                EntityName = [nameof(CollectionItemRelationship)],
                PropertyName = [nameof(CIRelationshipDisplayDTO.CollectionItemRelationshipName)],
                TranslatedText = [name]
            }).Select(et => et.EntityId).FirstOrDefault();

            return cppRelationshipId ?? 0;
        }
    }
}