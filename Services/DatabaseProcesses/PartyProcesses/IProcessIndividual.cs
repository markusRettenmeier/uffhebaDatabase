using Sammlerplattform.Data;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PartyProcesses
{
    public interface IProcessIndividual
    {
        (int PartyID, int Statuscode, string StatusMessage) Create(IndividualOperationParameterModel individualOperationParameterModel);
        (int PartyID, int Statuscode, string StatusMessage) Edit(IndividualOperationParameterModel individualOperationParameterModel);
    }
    public class IndividualProcessor(IProcessParty processParty, IUnitOfWork unitOfWork) : IProcessIndividual
    {
        public (int PartyID, int Statuscode, string StatusMessage) Create(IndividualOperationParameterModel individualOperationParameterModel)
        {
            if (string.IsNullOrWhiteSpace(individualOperationParameterModel.Party.PartyName))
            {
                return (0, 412, "Error_PartyName_Missing");
            }

            (bool flowControl, (int PartyID, int Statuscode, string StatusMessage) value) = IsPlaceExistingProcessCreate(individualOperationParameterModel);
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
                Party newParty = processParty.CreateParty(partyOperationParameterModel).Party;

                individualOperationParameterModel.Individual.PartyID = newParty.PartyID;
                Individual newIndividual = unitOfWork.IndividualRepository.Insert(individualOperationParameterModel.Individual);
                unitOfWork.Save();

                scope.Complete();
                return (newIndividual.PartyID, 201, "Success_Individual_Created");
            }
            catch (Exception ex)
            {
                return (0, 500, "Error_Error_Ocurred");
            }
        }

        private (bool flowControl, (int PartyID, int Statuscode, string StatusMessage) value) IsPlaceExistingProcessCreate(IndividualOperationParameterModel individualOperationParameterModel)
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
                return (flowControl: false, value: (0, 409, "Error_Party_Exists"));
            }

            return (flowControl: true, value: default);
        }

        public (int PartyID, int Statuscode, string StatusMessage) Edit(IndividualOperationParameterModel individualOperationParameterModel)
        {
            if (individualOperationParameterModel.Party.PartyID <= 0)
            {
                return (0, 412, "Error_PartyID_Missing");
            }
            if (string.IsNullOrWhiteSpace(individualOperationParameterModel.Party.PartyName))
            {
                return (0, 412, "Error_PartyName_Missing");
            }

            Individual? individualToEdit = processParty.GetListWithPredicate(
                new PartySearchParameterModel { PartyID = [individualOperationParameterModel.Party.PartyID] }
                ).FirstOrDefault()?.Individual;
            if (individualToEdit == null)
            {
                return (0, 404, "Error_Individual_NotFound");
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
                Party editedParty = processParty.EditParty(partyOperationParameterModel).Party;

                scope.Complete();
                return (editedParty.PartyID, 200, "Success_Individual_Updated");
            }
            catch (Exception ex)
            {
                return (0, 500, "Error_Error_Ocurred");
            }
        }
    }
}
