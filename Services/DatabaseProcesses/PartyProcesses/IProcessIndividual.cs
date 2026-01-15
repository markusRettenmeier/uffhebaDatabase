using Sammlerplattform.Data;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PartyProcesses
{
    public interface IProcessIndividual
    {
        (int Statuscode, string StatusMessage, int PartyID) Insert(IndividualOperationParameterModel individualOperationParameterModel);
        (int Statuscode, string StatusMessage, int PartyID) Update(IndividualOperationParameterModel individualOperationParameterModel);
    }
    public class IndividualProcessor(IProcessParty processParty
        , IUnitOfWork unitOfWork
        , ITrackEvents trackEvents) : IProcessIndividual
    {
        public (int Statuscode, string StatusMessage, int PartyID) Insert(IndividualOperationParameterModel individualOperationParameterModel)
        {
            if (string.IsNullOrWhiteSpace(individualOperationParameterModel.Party.PartyName))
            {
                trackEvents.TrackWarning("IndividualProcessor.Create: PartyName is missing.", new Dictionary<string, object>
                {
                    { "Party", individualOperationParameterModel.Party},
                    { "Individual", individualOperationParameterModel.Individual }
                });
                return (412, "Error_Party_NameMissing", 0);
            }

            (bool flowControl, (int Statuscode, string StatusMessage, int PartyID) value) = IsPlaceExistingProcessCreate(individualOperationParameterModel);
            if (!flowControl)
            {
                return value;
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                PartyOperationParameterModel partyOperationParameterModel = new()
                {
                    Party = individualOperationParameterModel.Party,
                    PlaceList = individualOperationParameterModel.PlaceList
                };
                Party newParty = processParty.Insert(partyOperationParameterModel).Party;

                individualOperationParameterModel.Individual.PartyID = newParty.PartyID;
                Individual newIndividual = unitOfWork.IndividualRepository.Insert(individualOperationParameterModel.Individual);
                unitOfWork.Save();

                scope.Complete();
                return (201, "Success_Individual_Created", newIndividual.PartyID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "IndividualProcessor.Create: Exception occurred.", new Dictionary<string, object>
                {
                    { "Party", individualOperationParameterModel.Party},
                    { "Individual", individualOperationParameterModel.Individual }
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        private (bool flowControl, (int Statuscode, string StatusMessage, int PartyID) value) IsPlaceExistingProcessCreate(IndividualOperationParameterModel individualOperationParameterModel)
        {
            PartySearchParameterModel partySearchParameterModel = new()
            {
                PartyName = [individualOperationParameterModel.Party.PartyName],
                PartyTypeInt = [individualOperationParameterModel.Party.PartyTypeInt]
            };
            if (individualOperationParameterModel.Individual.Pseudonym != null)
                partySearchParameterModel.Individual_Pseudonym = [individualOperationParameterModel.Individual.Pseudonym];
            if (individualOperationParameterModel.Individual.Signature != null)
                partySearchParameterModel.Individual_Signature = [individualOperationParameterModel.Individual.Signature];
            Party? existingParty = processParty.GetListWithPredicate(partySearchParameterModel).FirstOrDefault();
            if (existingParty != null)
            {
                trackEvents.TrackWarning("IndividualProcessor.Create: Individual already exists.", new Dictionary<string, object>
                {
                    { "Party", individualOperationParameterModel.Party},
                    { "Individual", individualOperationParameterModel.Individual }
                });
                return (flowControl: false, value: (409, "Error_Party_Exists", 0));
            }

            return (flowControl: true, value: default);
        }

        public (int Statuscode, string StatusMessage, int PartyID) Update(IndividualOperationParameterModel individualOperationParameterModel)
        {
            if (individualOperationParameterModel.Party.PartyID <= 0)
            {
                trackEvents.TrackWarning("IndividualProcessor.Edit: PartyID is missing or invalid.", new Dictionary<string, object>
                {
                    { "Party", individualOperationParameterModel.Party },
                    { "Individual", individualOperationParameterModel.Individual }
                });
                return (412, "Error_PartyID_Missing", 0);
            }
            if (string.IsNullOrWhiteSpace(individualOperationParameterModel.Party.PartyName))
            {
                trackEvents.TrackWarning("IndividualProcessor.Edit: PartyName is missing.", new Dictionary<string, object>
                {
                    { "Party", individualOperationParameterModel.Party },
                    { "Individual", individualOperationParameterModel.Individual }
                });
                return (412, "Error_Party_NameMissing", 0);
            }

            Individual? individualToEdit = processParty.GetListWithPredicate(
                new PartySearchParameterModel { PartyID = [individualOperationParameterModel.Party.PartyID] }
                ).FirstOrDefault()?.Individual;
            if (individualToEdit == null)
            {
                trackEvents.TrackWarning("IndividualProcessor.Edit: Individual not found.", new Dictionary<string, object>
                {
                    { "Party", individualOperationParameterModel.Party },
                    { "Individual", individualOperationParameterModel.Individual }
                });
                return (404, "Error_Individual_NotFound", 0);
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                if (individualToEdit.Pseudonym != individualOperationParameterModel.Individual.Pseudonym
                    || individualToEdit.Signature != individualOperationParameterModel.Individual.Signature)
                {
                    individualToEdit.Pseudonym = individualOperationParameterModel.Individual.Pseudonym;
                    individualToEdit.Signature = individualOperationParameterModel.Individual.Signature;
                    unitOfWork.Save();
                }

                PartyOperationParameterModel partyOperationParameterModel = new()
                {
                    Party = individualOperationParameterModel.Party,
                    PlaceList = individualOperationParameterModel.PlaceList
                };
                Party editedParty = processParty.Update(partyOperationParameterModel).Party;

                scope.Complete();
                return (200, "Success_Individual_Updated", editedParty.PartyID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "IndividualProcessor.Edit: Exception occurred.", new Dictionary<string, object>
                {
                    { "Party", individualOperationParameterModel.Party },
                    { "Individual", individualOperationParameterModel.Individual }
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }
    }
}
