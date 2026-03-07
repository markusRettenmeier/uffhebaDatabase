using AspNetCoreGeneratedDocument;
using Sammlerplattform.Data;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PartyDatabase.OrganizationDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.DatabaseProcesses.PictureProcesses;
using Sammlerplattform.Services.Translation;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.PartyProcesses
{
    public interface IProcessOrganization
    {
        (int Statuscode, string StatusMessage, int PartyID) Insert(OrganizationCreateDTO createDTO);
        (int Statuscode, string StatusMessage, int PartyID) Update(OrganizationOperationParameterModel organizationOperationParameterModel);
        (int Statuscode, string StatusMessage) Delete(int partyID);
    }
    public class OrganizationProcessor(IProcessParty processParty
        , IUnitOfWork unitOfWork
        , ITrackEventsCSV trackEvents
        , IProcessTranslations processTranslations
        , IProcessIndustry processIndustry) : IProcessOrganization
    {
        public (int Statuscode, string StatusMessage) Delete(int partyID)
        {
            Party? party = processParty
                .GetListWithPredicate(new PartySearchParameterModel { PartyID = [partyID] })
                .FirstOrDefault();
            if (party == null || party.Organization == null)
            {
                trackEvents.TrackError("IndividualProcessor.Delete: Individual not found.", new Dictionary<string, object>
                {
                    { "PartyID", partyID  }
                });
                return (404, "Error_Individual_NotFound");
            }

            try
            {
                TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                unitOfWork.IndividualRepository.Delete(party.Organization);
                unitOfWork.Save();

                (int statuscode, string message) = processParty.Delete(party);
                if (statuscode != 200)
                {
                    trackEvents.TrackError("IndividualProcessor.Delete: Error occurred in party deletion.", new Dictionary<string, object>
                    {
                        { "PartyID", partyID  },
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
                    { "PartyID", partyID  }
                });
                return (500, "Error_Error_Ocurred");
            }
        }

        public (int Statuscode, string StatusMessage, int PartyID) Insert(OrganizationCreateDTO createDTO)
        {
            PartySearchParameterModel partySearchParameterModel = new()
            {
                PartyName = [createDTO.Name],
                PartyTypeInt = [createDTO.PartyTypeInt]
            };
            if (createDTO.Industry != null)
            {
                partySearchParameterModel.Organization_Industry_IndustryName = [createDTO.Industry];
            }
            Party? existingParty = processParty.GetListWithPredicate(partySearchParameterModel).FirstOrDefault();
            if (existingParty != null)
            {
                trackEvents.TrackError("OrganizationProcessor.Insert: Party already exists.", new Dictionary<string, object>
                {
                    { "CreatePartyDTO", createDTO }
                });
                return (409, "Error_Party_Exists", 0);
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

                Organization organization = new()
                {
                    PartyID = newParty.PartyID
                };
                Organization newOrganization = unitOfWork.OrganizationRepository.Insert(organization);
                unitOfWork.Save();
                if(!string.IsNullOrEmpty(createDTO.Industry))
                    ConnectIndustryToOrganization(newOrganization, createDTO.Industry);

                scope.Complete();
                return (201, "Success_Organization_Created", newOrganization.PartyID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "OrganizationProcessor.Insert: Exception occurred.", new Dictionary<string, object>
                {
                    { "CreatePartyDTO", createDTO }
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        public (int Statuscode, string StatusMessage, int PartyID) Update(OrganizationOperationParameterModel organizationOperationParameterModel)
        {
            if (organizationOperationParameterModel.Party == null || organizationOperationParameterModel.Party.PartyID <= 0)
            {
                trackEvents.TrackError("OrganizationProcessor.Update: PartyID is missing.", new Dictionary<string, object>
                {
                    { "Organization", organizationOperationParameterModel.Organization }
                });
                return (412, "Error_PartyID_Missing", 0);
            }
            if (string.IsNullOrWhiteSpace(organizationOperationParameterModel.Party.PartyName))
            {
                trackEvents.TrackError("OrganizationProcessor.Update: PartyName is missing.", new Dictionary<string, object>
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
                trackEvents.TrackError("OrganizationProcessor.Update: Organization not found.", new Dictionary<string, object>
                {
                    { "Party", organizationOperationParameterModel.Party },
                    { "Organization", organizationOperationParameterModel.Organization }
                });
                return (404, "Error_Party_NotFound", 0);
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                if (existingOrganization.IndustryID != organizationOperationParameterModel.Organization.IndustryID)
                {
                    existingOrganization.IndustryID = organizationOperationParameterModel.Organization.IndustryID;
                    unitOfWork.Save();
                }

                PartyOperationParameterModel partyOperationParameterModel = new()
                {
                    Party = organizationOperationParameterModel.Party,
                    //PlaceList = organizationOperationParameterModel.PlaceList
                };
                Party editedParty = processParty.Update(partyOperationParameterModel).Party;

                SyncIndustry(existingOrganization, organizationOperationParameterModel.Organization.Industry?.IndustryName);

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

        private void ConnectIndustryToOrganization(Organization organization, string name)
        {
            int? industryId = GetIndustryId(name);
            if(industryId != null)
            {
                industryId = processIndustry.Insert(new Industry { IndustryName = name }).id;
            }

            organization.IndustryID = industryId;
            unitOfWork.Save();
        }

        private void SyncIndustry(Organization organization, string? name)
        {
            if(organization.IndustryID != null && string.IsNullOrWhiteSpace(name))
            {
                DisconnectIndustry(organization);
                return;
            } 
            else if(organization.IndustryID == null && !string.IsNullOrWhiteSpace(name))
            {
                ConnectIndustryToOrganization(organization, name);
                return;
            }
        }
        private void DisconnectIndustry(Organization organization)
        {
            organization.IndustryID = null;
            unitOfWork.Save();
        }

        private int? GetIndustryId(string name)
        {
            return processTranslations.GetWithPredicate(new EntityTranslationSearchParameter
            {
                EntityType = [nameof(Industry)],
                TranslatedText = [name],
            }).Select(x => x.EntityId).FirstOrDefault();
        }
    }
}
