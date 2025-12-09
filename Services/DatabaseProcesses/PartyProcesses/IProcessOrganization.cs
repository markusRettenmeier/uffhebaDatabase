using Sammlerplattform.Data;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PartyProcesses
{
    public interface IProcessOrganization
    {
        (int PartyID, int Statuscode, string StatusMessage) CreateOrganization(OrganizationOperationParameterModel organizationOperationParameterModel);
        (int PartyID, int Statuscode, string StatusMessage) EditOrganization(OrganizationOperationParameterModel organizationOperationParameterModel);
    }
    public class OrganizationProcessor(IProcessParty processParty, IUnitOfWork unitOfWork) : IProcessOrganization
    {
        public (int PartyID, int Statuscode, string StatusMessage) CreateOrganization(OrganizationOperationParameterModel organizationOperationParameterModel)
        {
            if (string.IsNullOrWhiteSpace(organizationOperationParameterModel.Party.PartyName))
            {
                return (0, 412, "Error_PartyName_Missing");
            }

            PartySearchParameterModel partySearchParameterModel = new()
            {
                PartyName = [organizationOperationParameterModel.Party.PartyName],
                PartyTypeInt = [organizationOperationParameterModel.Party.PartyTypeInt],
                PlaceList_PlaceNToponymyList_Toponymy_ToponymyName = [.. organizationOperationParameterModel.PlaceList.Select(x => x.PlaceNToponymyList.FirstOrDefault()?.Toponymy.ToponymyName ?? string.Empty)],
                Organization_OrganizationTypeInt = [organizationOperationParameterModel.Organization.OrganizationTypeInt]
            };
            if (organizationOperationParameterModel.Organization.ProductionFacility?.ProductionFacilityName != null)
            {
                partySearchParameterModel.Organization_ProductionFacility_ProductionFacilityName = [organizationOperationParameterModel.Organization.ProductionFacility.ProductionFacilityName];
            }
            Party? existingParty = processParty.GetListWithPredicate(partySearchParameterModel).FirstOrDefault();
            if (existingParty != null)
            {
                return (0, 409, "Error_Party_Exists");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                PartyOperationParameterModel partyOperationParameterModel = new()
                {
                    Party = organizationOperationParameterModel.Party,
                    PlaceList = organizationOperationParameterModel.PlaceList
                };
                Party newParty = processParty.CreateParty(partyOperationParameterModel).Party;

                organizationOperationParameterModel.Organization.PartyID = newParty.PartyID;
                Organization newOrganization = unitOfWork.OrganizationRepository.Insert(organizationOperationParameterModel.Organization);
                unitOfWork.Save();

                scope.Complete();
                return (newOrganization.PartyID, 201, "Success_Organization_Created");
            }
            catch (Exception ex)
            {
                return (0, 500, "Error_Error_Ocurred");
            }
        }

        public (int PartyID, int Statuscode, string StatusMessage) EditOrganization(OrganizationOperationParameterModel organizationOperationParameterModel)
        {
            if (organizationOperationParameterModel.Party == null || organizationOperationParameterModel.Party.PartyID <= 0)
            {
                return (0, 412, "Error_PartyID_Missing");
            }
            if (string.IsNullOrWhiteSpace(organizationOperationParameterModel.Party.PartyName))
            {
                return (0, 412, "Error_PartyName_Missing");
            }

            Organization? existingOrganization = processParty.GetListWithPredicate(
                new PartySearchParameterModel { PartyID = [organizationOperationParameterModel.Party.PartyID] })
                .FirstOrDefault()?.Organization;
            if (existingOrganization == null)
            {
                return (0, 404, "Error_Party_NotFound");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                if (existingOrganization.OrganizationTypeInt != organizationOperationParameterModel.Organization.OrganizationTypeInt ||
                    existingOrganization.ProductionFacilityID != organizationOperationParameterModel.Organization.ProductionFacilityID)
                {
                    existingOrganization.OrganizationTypeInt = organizationOperationParameterModel.Organization.OrganizationTypeInt;
                    existingOrganization.ProductionFacilityID = organizationOperationParameterModel.Organization.ProductionFacilityID;
                    unitOfWork.Save();
                }

                PartyOperationParameterModel partyOperationParameterModel = new()
                {
                    Party = organizationOperationParameterModel.Party,
                    PlaceList = organizationOperationParameterModel.PlaceList
                };
                Party editedParty = processParty.EditParty(partyOperationParameterModel).Party;

                scope.Complete();
                return (editedParty.PartyID, 200, "Success_Organization_Created");
            }
            catch (Exception ex)
            {
                return (0, 500, "Error_Error_Ocurred");
            }
        }
    }
}
