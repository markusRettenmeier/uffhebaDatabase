using Sammlerplattform.Data;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using System.Transactions;
using static Sammlerplattform.Controllers.RestController;

namespace Sammlerplattform.Services.DatabaseProcesses.PartyProcesses
{
    public interface IProcessIndividual
    {
        (int Statuscode, string StatusMessage, int PartyID) Insert(IndividualCreateDTO createDTO);
        (int Statuscode, string StatusMessage, int PartyID) Update(IndividualEditDTO editDTO);
        (int Statuscode, string StatusMessage) Delete(int partyID);
    }
    public class IndividualProcessor(IProcessParty processParty
        , IUnitOfWork unitOfWork
        , ITrackEventsCSV trackEvents) : IProcessIndividual
    {
        public (int Statuscode, string StatusMessage, int PartyID) Insert(IndividualCreateDTO createDTO)
        {
            (bool flowControl, (int Statuscode, string StatusMessage, int PartyID) value) = IsPartyExistingProcessCreate(createDTO);
            if (!flowControl)
            {
                return value;
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                PartyOperationParameterModel partyOperationParameterModel = new()
                {
                    Party = new Party { PartyName = createDTO.Name, PartyTypeInt = createDTO.PartyTypeInt, WikipediaUrl = createDTO.WikipediaUrl },
                    //ConnectedPlaceIDList = [.. createDTO.ConnectedPlaceList.Select(p => p.PlaceID)]
                };
                Party newParty = processParty.Insert(partyOperationParameterModel).Party;

                Individual individual = new()
                {
                    Pseudonym = createDTO.Pseudonym,
                    Signature = createDTO.Signature,
                    PartyID = newParty.PartyID
                };
                Individual newIndividual = unitOfWork.IndividualRepository.Insert(individual);
                unitOfWork.Save();

                scope.Complete();
                return (201, "Success_Individual_Created", newIndividual.PartyID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "IndividualProcessor.Create: Exception occurred.", new Dictionary<string, object>
                {
                    {"IndividualCreateDTO", createDTO }
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        private (bool flowControl, (int Statuscode, string StatusMessage, int PartyID) value) IsPartyExistingProcessCreate(IndividualCreateDTO createDTO)
        {
            PartySearchParameterModel searchParamters = new()
            {
                PartyName = [createDTO.Name],
                PartyTypeInt = [createDTO.PartyTypeInt]
            };
            if (createDTO.Pseudonym != null)
                searchParamters.Individual_Pseudonym = [createDTO.Pseudonym];
            if (createDTO.Signature != null)
                searchParamters.Individual_Signature = [createDTO.Signature];
            Party? existingParty = processParty.GetListWithPredicate(searchParamters).FirstOrDefault();
            if (existingParty != null)
            {
                trackEvents.TrackError("IndividualProcessor.Create: Individual already exists.", new Dictionary<string, object>
                {
                    {"IndividualCreateDTO", createDTO }
                });
                return (flowControl: false, value: (409, "Error_Party_Exists", 0));
            }

            return (flowControl: true, value: default);
        }

        public (int Statuscode, string StatusMessage, int PartyID) Update(IndividualEditDTO editDTO)
        {
            Individual? individualToEdit = processParty
                .GetListWithPredicate(new PartySearchParameterModel { PartyID = [editDTO.PartyID] })
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
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                if (individualToEdit.Pseudonym != editDTO.Pseudonym
                    || individualToEdit.Signature != editDTO.Signature)
                {
                    individualToEdit.Pseudonym = editDTO.Pseudonym;
                    individualToEdit.Signature = editDTO.Signature;
                    unitOfWork.Save();
                }

                PartyOperationParameterModel partyOperationParameter = new()
                {
                    Party = new Party
                    {
                        PartyID = editDTO.PartyID,
                        PartyName = editDTO.Name,
                        PartyTypeInt = editDTO.PartyTypeInt,
                        WikipediaUrl = editDTO.WikipediaUrl
                    },
                    //ConnectedPlaceIDList = [.. editDTO.ConnectedPlaceList.Select(p => p.PlaceID)]
                };
                Party editedParty = processParty.Update(partyOperationParameter).Party;

                scope.Complete();
                return (200, "Success_Individual_Updated", editedParty.PartyID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "IndividualProcessor.Edit: Exception occurred.", new Dictionary<string, object>
                {
                    { "IndividualEditDTO", editDTO  }
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        public (int Statuscode, string StatusMessage) Delete(int id)
        {
            Party? party = processParty
                .GetListWithPredicate(new PartySearchParameterModel { PartyID = [id] })
                .FirstOrDefault();
            if (party == null || party.Individual == null)
            {
                trackEvents.TrackError("IndividualProcessor.Delete: Individual not found.", new Dictionary<string, object>
                {
                    { "PartyID", id  }
                });
                return (404, "Error_Individual_NotFound");
            }

            try
            {
                TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                unitOfWork.IndividualRepository.Delete(party.Individual);
                unitOfWork.Save();

                (int statuscode, string message) = processParty.Delete(party);
                if(statuscode != 200)
                {
                    trackEvents.TrackError("IndividualProcessor.Delete: Error occurred in party deletion.", new Dictionary<string, object>
                    {
                        { "PartyID", id  },
                        { "ProcessPartyStatuscode", statuscode },
                        { "ProcessPartyMessage", message }
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
                    { "PartyID", id  }
                });
                return (500, "Error_Error_Ocurred");
            }
        }
    }
}
