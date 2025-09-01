using Sammlerplattform.Data;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.IndividualDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.Processes.PartyProcesses
{
    public interface IProcessIndividual
    {
        (int PartyID, int Statuscode, string StatusMessage) CreateIndividual(IndividualOperationParameterModel individualOperationParameterModel);
        (int PartyID, int Statuscode, string StatusMessage) EditIndividual(IndividualOperationParameterModel individualOperationParameterModel);
    }
    public class IndividualProcessor(IProcessParty processParty, IUnitOfWork unitOfWork) : IProcessIndividual
    {
        public (int PartyID, int Statuscode, string StatusMessage) CreateIndividual(IndividualOperationParameterModel individualOperationParameterModel)
        {
            if (string.IsNullOrWhiteSpace(individualOperationParameterModel.Party.PartyName))
            {
                return (0, 412, "Parteiname angeben.");
            }

            PartySearchParameterModel partySearchParameterModel = new()
            {
                PartyName = [individualOperationParameterModel.Party.PartyName],
                PartyTypeInt = [individualOperationParameterModel.Party.PartyTypeInt],
                Individual_Pseudonym = [individualOperationParameterModel.Individual.Pseudonym],
                Individual_Signature = [individualOperationParameterModel.Individual.Signature]
            };
            Party? existingParty = processParty.GetListWithPredicate(partySearchParameterModel).FirstOrDefault();
            if (existingParty != null)
            {
                return (0, 409, "Partei mit diesen Angaben existiert bereits.");
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
                return (newIndividual.PartyID, 201, "Individuum erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                return (0, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public (int PartyID, int Statuscode, string StatusMessage) EditIndividual(IndividualOperationParameterModel individualOperationParameterModel)
        {
            if (individualOperationParameterModel.Party.PartyID <= 0)
            {
                return (0, 412, "ParteiID fehlt.");
            }
            if (string.IsNullOrWhiteSpace(individualOperationParameterModel.Party.PartyName))
            {
                return (0, 412, "Parteiname angeben.");
            }

            Individual? individualToEdit = processParty.GetListWithPredicate(
                new PartySearchParameterModel { PartyID = [individualOperationParameterModel.Party.PartyID] }
                ).FirstOrDefault()?.Individual;
            if (individualToEdit == null)
            {
                return (0, 404, "Individuum nicht gefunden.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                if(individualToEdit.Pseudonym != individualOperationParameterModel.Individual.Pseudonym
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
                return (editedParty.PartyID, 200, "Individuum erfolgreich bearbeitet.");
            }
            catch (Exception ex)
            {
                return (0, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }
    }
}
