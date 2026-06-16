using Sammlerplattform.Data;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.ParticipantDatabase.IndividualDatabase;
using Sammlerplattform.Services.Extensions;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.ParticipantProcesses
{
    public interface IProcessIndividual
    {
        (int Statuscode, string StatusMessage, int ParticipantID) Insert(IndividualCreateDTO createDTO);
        (int Statuscode, string StatusMessage, int ParticipantID) Update(IndividualEditDTO editDTO);
        (int Statuscode, string StatusMessage) Delete(int participantID);
    }
    public class IndividualProcessor(IProcessParticipant processParticipant
        , IUnitOfWork unitOfWork
        , ITrackEventsCSV trackEvents) : IProcessIndividual
    {
        public (int Statuscode, string StatusMessage, int ParticipantID) Insert(IndividualCreateDTO createDTO)
        {
            (bool flowControl, (int Statuscode, string StatusMessage, int ParticipantID) value) = IsParticipantExistingProcessCreate(createDTO);
            if (!flowControl)
            {
                return value;
            }

            try
            {
                using TransactionScope scope = new();

                ParticipantOperationParameterModel participantOperationParameterModel = new()
                {
                    Participant = new Participant
                    {
                        ParticipantName = createDTO.Name,
                        ParticipantTypeInt = 0,
                        StartYear = createDTO.BirthYear,
                        EndYear = createDTO.DeathYear,
                        WikipediaUrl = createDTO.WikipediaUrl.ChangeStringToUriToRemoveSubdomain()
                    },
                    ConnectedPlaceIdList = [.. createDTO.ConnectedPlaceList.Select(p => p.Id)],
                    ConnectedEraIdList = [.. createDTO.ConnectedEraList.Select(p => p.Id)]
                };
                Participant newParticipant = processParticipant.Insert(participantOperationParameterModel).Participant;

                Individual individual = new()
                {
                    Pseudonym = createDTO.Pseudonym,
                    Signature = createDTO.Signature,
                    ParticipantID = newParticipant.ParticipantID
                };
                Individual newIndividual = unitOfWork.IndividualRepository.Insert(individual);
                unitOfWork.Save();

                scope.Complete();
                return (201, "Success_Individual_Created", newIndividual.ParticipantID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "IndividualProcessor.Create: Exception occurred.", new Dictionary<string, object>
                {
                    {"IndividualCreateDTO", createDTO }
                });
                return (500, "Error_Unknown", 0);
            }
        }

        private (bool flowControl, (int Statuscode, string StatusMessage, int ParticipantID) value) IsParticipantExistingProcessCreate(IndividualCreateDTO createDTO)
        {
            ParticipantSearchParameterModel searchParamters = new()
            {
                ParticipantName = [createDTO.Name],
                ParticipantTypeInt = [0]
            };
            if (createDTO.Pseudonym != null)
                searchParamters.Individual_Pseudonym = [createDTO.Pseudonym];
            if (createDTO.Signature != null)
                searchParamters.Individual_Signature = [createDTO.Signature];
            Participant? existingParticipant = processParticipant.GetListWithPredicate(searchParamters).FirstOrDefault();
            if (existingParticipant != null)
            {
                trackEvents.TrackError("IndividualProcessor.Create: Individual already exists.", new Dictionary<string, object>
                {
                    {"IndividualCreateDTO", createDTO }
                });
                return (flowControl: false, value: (409, "Error_Participant_Exists", 0));
            }

            return (flowControl: true, value: default);
        }

        public (int Statuscode, string StatusMessage, int ParticipantID) Update(IndividualEditDTO editDTO)
        {
            Individual? individualToEdit = processParticipant
                .GetListWithPredicate(new ParticipantSearchParameterModel { ParticipantID = [editDTO.Id] })
                .FirstOrDefault()?.Individual;
            if (individualToEdit == null)
            {
                trackEvents.TrackError("IndividualProcessor.Edit: Individual not found.", new Dictionary<string, object>
                {
                    { "IndividualEditDTO", editDTO  }
                });
                return (404, "Error_Individual_NotFound", 0);
            }

            try
            {
                using TransactionScope scope = new();

                bool isChanged = false;
                if (individualToEdit.Pseudonym != editDTO.Pseudonym)
                {
                    individualToEdit.Pseudonym = editDTO.Pseudonym;
                    isChanged = true;
                }
                if (individualToEdit.Signature != editDTO.Signature)
                {
                    individualToEdit.Signature = editDTO.Signature;
                    isChanged = true;
                }
                if (isChanged)
                {
                    unitOfWork.Save();
                }

                ParticipantOperationParameterModel partyOperationParameter = new()
                {
                    Participant = new Participant
                    {
                        ParticipantID = editDTO.Id,
                        ParticipantName = editDTO.Name,
                        StartYear = editDTO.BirthYear,
                        EndYear = editDTO.DeathYear,
                        WikipediaUrl = editDTO.WikipediaUrl.ChangeStringToUriToRemoveSubdomain()
                    },
                    ConnectedPlaceIdList = [.. editDTO.ConnectedPlaceList.Select(p => p.Id)],
                    ConnectedEraIdList = [.. editDTO.ConnectedEraList.Select(p => p.Id)]
                };
                Participant editedParticipant = processParticipant.Update(partyOperationParameter).Participant;

                scope.Complete();
                return (200, "Success_Individual_Updated", editedParticipant.ParticipantID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "IndividualProcessor.Edit: Exception occurred.", new Dictionary<string, object>
                {
                    { "IndividualEditDTO", editDTO  }
                });
                return (500, "Error_Unknown", 0);
            }
        }

        public (int Statuscode, string StatusMessage) Delete(int id)
        {
            Participant? party = processParticipant
                .GetListWithPredicate(new ParticipantSearchParameterModel { ParticipantID = [id] })
                .FirstOrDefault();
            if (party == null || party.Individual == null)
            {
                trackEvents.TrackError("IndividualProcessor.Delete: Individual not found.", new Dictionary<string, object>
                {
                    { "ParticipantID", id  }
                });
                return (404, "Error_Individual_NotFound");
            }
            if (party.CollectionItemNParticipantList != null && party.CollectionItemNParticipantList.Count > 0)
            {
                trackEvents.TrackError("IndividualProcessor.Delete: Individual is connected to collection items.", new Dictionary<string, object>
                {
                    { "ParticipantID", id  },
                    { "ConnectedCollectionItemsCount", party.CollectionItemNParticipantList.Count }
                });
                return (400, "Error_Individual_ConnectedToCollectionItems");
            }

            try
            {
                using TransactionScope scope = new();

                unitOfWork.IndividualRepository.Delete(party.Individual);
                unitOfWork.Save();

                (int statuscode, string message) = processParticipant.Delete(party);
                if (statuscode != 200)
                {
                    trackEvents.TrackError("IndividualProcessor.Delete: Error occurred in party deletion.", new Dictionary<string, object>
                    {
                        { "ParticipantID", id  },
                        { "ProcessParticipantStatuscode", statuscode },
                        { "ProcessParticipantMessage", message }
                    });
                    scope.Dispose();
                    return (statuscode, message);
                }

                scope.Complete();
                return (200, "Success_Individual_Deleted");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "IndividualProcessor.Delete: Exception occurred.", new Dictionary<string, object>
                {
                    { "ParticipantID", id  }
                });
                return (500, "Error_Unknown");
            }
        }
    }
}
