using Sammlerplattform.Data;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PartyProcesses
{
    public interface IProcessOrganization
    {
        (int Statuscode, string StatusMessage, int PartyID) Insert(OrganizationOperationParameterModel organizationOperationParameterModel);
        (int Statuscode, string StatusMessage, int PartyID) Update(OrganizationOperationParameterModel organizationOperationParameterModel);
    }
    public class OrganizationProcessor(IProcessParty processParty
        , IUnitOfWork unitOfWork
        , ITrackEvents trackEvents) : IProcessOrganization
    {
        public (int Statuscode, string StatusMessage, int PartyID) Insert(OrganizationOperationParameterModel organizationOperationParameterModel)
        {
            if (string.IsNullOrWhiteSpace(organizationOperationParameterModel.Party.PartyName))
            {
                trackEvents.TrackWarning("OrganizationProcessor.Insert: PartyName is missing.", new Dictionary<string, object>
                {
                    { "Party", organizationOperationParameterModel.Party },
                    { "Organization", organizationOperationParameterModel.Organization }
                });
                return (412, "Error_Party_NameMissing", 0);
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
                trackEvents.TrackWarning("OrganizationProcessor.Insert: Party already exists.", new Dictionary<string, object>
                {
                    { "Party", organizationOperationParameterModel.Party },
                    { "Organization", organizationOperationParameterModel.Organization }
                });
                return (409, "Error_Party_Exists", 0);
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                PartyOperationParameterModel partyOperationParameterModel = new()
                {
                    Party = organizationOperationParameterModel.Party,
                    PlaceList = organizationOperationParameterModel.PlaceList
                };
                Party newParty = processParty.Insert(partyOperationParameterModel).Party;

                organizationOperationParameterModel.Organization.PartyID = newParty.PartyID;
                Organization newOrganization = unitOfWork.OrganizationRepository.Insert(organizationOperationParameterModel.Organization);
                unitOfWork.Save();

                scope.Complete();
                return (201, "Success_Organization_Created", newOrganization.PartyID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "OrganizationProcessor.Insert: Exception occurred.", new Dictionary<string, object>
                {
                    { "Party", organizationOperationParameterModel.Party },
                    { "Organization", organizationOperationParameterModel.Organization }
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        public (int Statuscode, string StatusMessage, int PartyID) Update(OrganizationOperationParameterModel organizationOperationParameterModel)
        {
            if (organizationOperationParameterModel.Party == null || organizationOperationParameterModel.Party.PartyID <= 0)
            {
                trackEvents.TrackWarning("OrganizationProcessor.Update: PartyID is missing.", new Dictionary<string, object>
                {
                    { "Organization", organizationOperationParameterModel.Organization }
                });
                return (412, "Error_PartyID_Missing", 0);
            }
            if (string.IsNullOrWhiteSpace(organizationOperationParameterModel.Party.PartyName))
            {
                trackEvents.TrackWarning("OrganizationProcessor.Update: PartyName is missing.", new Dictionary<string, object>
                {
                    { "Party", organizationOperationParameterModel.Party },
                    { "Organization", organizationOperationParameterModel.Organization }
                });
                return (412, "Error_Party_NameMissing", 0);
            }

            Organization? existingOrganization = processParty.GetListWithPredicate(
                new PartySearchParameterModel { PartyID = [organizationOperationParameterModel.Party.PartyID] })
                .FirstOrDefault()?.Organization;
            if (existingOrganization == null)
            {
                trackEvents.TrackWarning("OrganizationProcessor.Update: Organization not found.", new Dictionary<string, object>
                {
                    { "Party", organizationOperationParameterModel.Party },
                    { "Organization", organizationOperationParameterModel.Organization }
                });
                return (404, "Error_Party_NotFound", 0);
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
                Party editedParty = processParty.Update(partyOperationParameterModel).Party;

                scope.Complete();
                return (200, "Success_Organization_Created", editedParty.PartyID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "OrganizationProcessor.Update: Exception occurred.", new Dictionary<string, object>
                {
                    { "Party", organizationOperationParameterModel.Party },
                    { "Organization", organizationOperationParameterModel.Organization }
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }
    }
}
