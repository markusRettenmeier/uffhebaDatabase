using Sammlerplattform.Data;
using Sammlerplattform.Models.EraDatabase;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase;
using Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase.IndustryDatabase;
using Sammlerplattform.Models.PlaceDatabase;
using Sammlerplattform.Models.PlaceDatabase.Toponymy;
using Sammlerplattform.Services.DatabaseProcesses.PlaceProcesses;
using System.Data.Entity;
using System.Globalization;

namespace Sammlerplattform.Services.DatabaseProcesses.ParticipantProcesses
{
    public interface IProcessParticipant
    {
        List<Participant> GetEntityListViaPredicate(ParticipantSearchParameterModel participantSearchParameterModel);
        List<ParticipantDisplayDTO> GetTranslationsListViaPredicate(ParticipantSearchParameterModel participantSearchParameterModel);
        (int Statuscode, string Message, Participant Participant) Insert(ParticipantOperationParameterModel participantOperationParameterModel);
        (int Statuscode, string Message, Participant Participant) Update(ParticipantOperationParameterModel participantOperationParameterModel);
        (int Statuscode, string Message) Delete(Participant participant);
    }

    public class ParticipantProcessor(IUnitOfWork unitOfWork
        , IProcessPlace processPlace
        , IProcessEra processEra
        , IProcessTranslations processTranslations
        , ITrackEventsText trackEvents) : IProcessParticipant
    {
        public (int Statuscode, string Message, Participant Participant) Insert(ParticipantOperationParameterModel participantOperationParameterModel)
        {
            Participant newParticipant = unitOfWork.ParticipantRepository.Insert(participantOperationParameterModel.Participant);
            unitOfWork.Save();

            foreach (int placeID in participantOperationParameterModel.ConnectedPlaceIdList)
            {
                ConnectPlaceToParticipant(newParticipant, placeID);
            }
            foreach (int eraId in participantOperationParameterModel.ConnectedEraIdList)
            {
                ConnectEraToParticipant(newParticipant, eraId);
            }

            return (201, "Success_Participant_Created", newParticipant);
        }

        public (int Statuscode, string Message) Delete(Participant participant)
        {
            if (participant.CollectionItemNParticipantList != null && participant.CollectionItemNParticipantList.Count > 0)
            {
                trackEvents.TrackError("OrganizationProcessor.Delete: Organization is connected to collection items.", new Dictionary<string, object>
                {
                    { "Participant", participant },
                    { "ConnectedCollectionItemsCount", participant.CollectionItemNParticipantList.Count }
                });
                return (400, "Error_Individual_ConnectedToCollectionItems");
            }

            for (int i = participant.ParticipantNPlaceList.Count - 1; i > 0; i--)
            {
                DisconnectPlaceFromParticipant(participant.ParticipantNPlaceList[i].ParticipantID, participant.ParticipantNPlaceList[i].PlaceID);
            }
            for (int i = participant.ParticipantNEraList.Count - 1; i > 0; i--)
            {
                DisconnectEraFromParticipant(participant.ParticipantNEraList[i].ParticipantId, participant.ParticipantNEraList[i].EraId);
            }
            unitOfWork.ParticipantRepository.Delete(participant);
            unitOfWork.Save();

            return (200, "Success_Participant_Deleted");
        }

        public (int Statuscode, string Message, Participant Participant) Update(ParticipantOperationParameterModel participantOperationParameterModel)
        {
            Participant? existingParticipant = GetEntityListViaPredicate(
                new ParticipantSearchParameterModel { ParticipantID = [participantOperationParameterModel.Participant.ParticipantID] }
                ).FirstOrDefault();
            if (existingParticipant == null)
            {
                return (404, "Error_Participant_NotFound", new Participant() { ParticipantName = string.Empty });
            }

            bool isChanged = false;
            if (existingParticipant.ParticipantName != participantOperationParameterModel.Participant.ParticipantName)
            {
                existingParticipant.ParticipantName = participantOperationParameterModel.Participant.ParticipantName;
                isChanged = true;
            }
            if (existingParticipant.StartYear != participantOperationParameterModel.Participant.StartYear)
            {
                existingParticipant.StartYear = participantOperationParameterModel.Participant.StartYear;
                isChanged = true;
            }
            if (existingParticipant.EndYear != participantOperationParameterModel.Participant.EndYear)
            {
                existingParticipant.EndYear = participantOperationParameterModel.Participant.EndYear;
                isChanged = true;
            }
            if (existingParticipant.WikipediaUrl != participantOperationParameterModel.Participant.WikipediaUrl)
            {
                existingParticipant.WikipediaUrl = participantOperationParameterModel.Participant.WikipediaUrl;
                isChanged = true;
            }
            if (isChanged)
            {
                unitOfWork.Save();
            }

            SyncPlace(existingParticipant, participantOperationParameterModel.ConnectedPlaceIdList);
            SyncEra(existingParticipant, participantOperationParameterModel.ConnectedEraIdList);

            return (200, "Success_Participant_Updated", existingParticipant);
        }

        public List<Participant> GetEntityListViaPredicate(ParticipantSearchParameterModel searchParameterModel)
        {
            IQueryable<Participant> participantIQueryable = unitOfWork.ParticipantRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Participant>(searchParameterModel),
                includeProperties: $"{nameof(Participant.Individual)}," +
                                   $"{nameof(Participant.Organization)}.{nameof(Organization.Industry)}," +
                                   $"{nameof(Participant.ParticipantNPlaceList)}.{nameof(ParticipantNPlace.Place)}.{nameof(Place.PlaceNToponymyList)}.{nameof(PlaceNToponymy.Toponymy)}," +
                                   $"{nameof(Participant.ParticipantNEraList)}.{nameof(ParticipantNEra.Era)}");
            return [.. participantIQueryable];
        }
        public List<ParticipantDisplayDTO> GetTranslationsListViaPredicate(ParticipantSearchParameterModel searchParameterModel)
        {
            List<ParticipantDisplayDTO> participantList = [.. unitOfWork.ParticipantRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<Participant>(searchParameterModel),
                orderBy: q => q.OrderBy(x => x.ParticipantName),
                includeProperties: $"{nameof(Participant.Individual)}," +
                                   $"{nameof(Participant.Organization)}.{nameof(Organization.Industry)}," +
                                   $"{nameof(Participant.ParticipantNPlaceList)}.{nameof(ParticipantNPlace.Place)}.{nameof(Place.PlaceNToponymyList)}.{nameof(PlaceNToponymy.Toponymy)}," +
                                   $"{nameof(Participant.ParticipantNEraList)}.{nameof(ParticipantNEra.Era)}")
                .AsNoTracking()
                .Select(p => new ParticipantDisplayDTO
                {
                    ParticipantID = p.ParticipantID,
                    Name = p.ParticipantName,
                    Pseudonym = p.Individual != null ? p.Individual.Pseudonym : null,
                    Signature = p.Individual != null ? p.Individual.Signature : null,
                    IndustryId = p.Organization != null ? p.Organization.IndustryID : null,
                    StartYear = p.StartYear,
                    EndYear = p.EndYear,
                    WikipediaUrl = p.WikipediaUrl,
                    ConnectedEraList = p.ParticipantNEraList.Select(pe => new EraDisplayDTO
                    {
                        EraID = pe.EraId
                    }),
                    ConnectedPlaceList = p.ParticipantNPlaceList.Select(pp => new PlaceDisplayDTO
                    {
                        PlaceID = pp.PlaceID,
                        ToponymyList = pp.Place.PlaceNToponymyList.Select(x => new ToponymyDisplayDTO
                        {
                            Id = x.ToponymyID,
                            Name = x.Toponymy.ToponymyName,
                            IsCurrentName = x.IsCurrentName
                        }).ToList()
                    }),
                    CollectionItemNParticipantList = p.CollectionItemNParticipantList
                })];

            SetTranslationsAndCultureDate(participantList);

            return [.. participantList];
        }

        private void SetTranslationsAndCultureDate(List<ParticipantDisplayDTO> participantList)
        {
            var allTranslations = processTranslations.GetWithFallback(new Models.Translations.EntityTranslationSearchParameter()).ToList();
            foreach (ParticipantDisplayDTO participant in participantList)
            {
                if (participant.IndustryId != null)
                {
                    participant.IndustryName = allTranslations.FirstOrDefault(
                        t => t.EntityId == participant.IndustryId.Value && 
                        t.PropertyName == nameof(ParticipantDisplayDTO.IndustryName))?.TranslatedText;
                }
                foreach (EraDisplayDTO eraDisplayDTO in participant.ConnectedEraList)
                {
                    eraDisplayDTO.EraName = allTranslations.FirstOrDefault(
                        t => t.EntityId == eraDisplayDTO.EraID && 
                        t.PropertyName == nameof(EraDisplayDTO.EraName))?.TranslatedText ?? string.Empty;
                }
            }

            if (CultureInfo.CurrentCulture.Name.Contains("zh"))
            {
                ChineseLunisolarCalendar lunisolarCalendar = new();
                foreach (ParticipantDisplayDTO participant in participantList)
                {
                    if (participant.StartYear != null)
                        participant.StartYear = lunisolarCalendar.GetYear(new DateTime((int)participant.StartYear, 01, 1));
                    if (participant.EndYear != null)
                        participant.EndYear = lunisolarCalendar.GetYear(new DateTime((int)participant.EndYear, 12, 31));
                }
            }
            else if (CultureInfo.CurrentCulture.Name.Contains("ja"))
            {
                JapaneseLunisolarCalendar japaneseLunisolarCalendar = new();
                foreach (ParticipantDisplayDTO participant in participantList)
                {
                    if (participant.StartYear != null)
                        participant.StartYear = japaneseLunisolarCalendar.GetYear(new DateTime((int)participant.StartYear, 01, 1));
                    if (participant.EndYear != null)
                        participant.EndYear = japaneseLunisolarCalendar.GetYear(new DateTime((int)participant.EndYear, 12, 31));
                }
            }
        }

        private void ConnectPlaceToParticipant(Participant participant, int placeID)
        {
            Place? place = processPlace.GetEntityListViaPredicate(
                new PlaceSearchParameterModel { PlaceID = [placeID] }
                ).FirstOrDefault();
            if (place == null)
            {
                return;
            }

            ParticipantNPlace participantNPlace = new()
            {
                ParticipantID = participant.ParticipantID,
                PlaceID = place.PlaceID
            };
            unitOfWork.ParticipantNPlaceRepository.Insert(participantNPlace);
            unitOfWork.Save();
        }
        private void SyncPlace(Participant participant, List<int> newPlaceIDList)
        {
            List<ParticipantNPlace> pNpToRemove = [.. participant.ParticipantNPlaceList.Where(p => !newPlaceIDList.Contains(p.PlaceID))];
            for (int i = pNpToRemove.Count - 1; i == 0; i--)
            {
                DisconnectPlaceFromParticipant(pNpToRemove[i].ParticipantID, pNpToRemove[i].PlaceID);
            }
            List<int> placesToAdd = [.. newPlaceIDList.Where(np => !participant.ParticipantNPlaceList.Any(p => p.PlaceID == np))];
            foreach (int id in placesToAdd)
            {
                ConnectPlaceToParticipant(participant, id);
            }
        }
        private void DisconnectPlaceFromParticipant(int participantID, int placeID)
        {
            ParticipantNPlace? participantNPlace = unitOfWork.ParticipantNPlaceRepository.Get(
                filter: p => p.ParticipantID == participantID && p.PlaceID == placeID
                ).FirstOrDefault();
            if (participantNPlace == null)
            {
                return;
            }

            unitOfWork.ParticipantNPlaceRepository.Delete(participantNPlace);
            unitOfWork.Save();
        }

        private void ConnectEraToParticipant(Participant participant, int eraId)
        {
            Era? era = processEra.GetEntityListViaPredicates(new EraSearchParameterModel { EraID = [eraId] })
                .FirstOrDefault();
            if (era == null)
            {
                return;
            }

            ParticipantNEra participantNEra = new()
            {
                EraId = eraId,
                ParticipantId = participant.ParticipantID
            };
            unitOfWork.ParticipantNEraRepository.Insert(participantNEra);
            unitOfWork.Save();
        }
        private void SyncEra(Participant participant, List<int> eraIdList)
        {
            List<ParticipantNEra> pNeToRemove = [.. participant.ParticipantNEraList.Where(p => !eraIdList.Contains(p.EraId))];
            for (int i = pNeToRemove.Count - 1; i == 0; i--)
            {
                DisconnectEraFromParticipant(pNeToRemove[i].ParticipantId, pNeToRemove[i].EraId);
            }

            List<int> erasToAdd = [.. eraIdList.Where(np => !participant.ParticipantNEraList.Any(p => p.EraId == np))];
            foreach (int id in erasToAdd)
            {
                ConnectEraToParticipant(participant, id);
            }
        }
        private void DisconnectEraFromParticipant(int participantID, int eraId)
        {
            ParticipantNEra? participantNEra = unitOfWork.ParticipantNEraRepository.Get(
                filter: p => p.ParticipantId == participantID && p.EraId == eraId
                ).FirstOrDefault();
            if (participantNEra == null)
            {
                return;
            }

            unitOfWork.ParticipantNEraRepository.Delete(participantNEra);
            unitOfWork.Save();
        }
    }
}