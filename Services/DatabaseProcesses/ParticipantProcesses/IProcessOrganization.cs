using Sammlerplattform.Data;
using Sammlerplattform.Models.ParticipantDatabase;
using Sammlerplattform.Models.ParticipantDatabase.OrganizationDatabase;
using Sammlerplattform.Models.Translations;
using Sammlerplattform.Services.Extensions;
using System.Transactions;

namespace Sammlerplattform.Services.DatabaseProcesses.ParticipantProcesses
{
    public interface IProcessOrganization
    {
        (int Statuscode, string StatusMessage, int ParticipantID) Insert(OrganizationCreateDTO createDTO);
        (int Statuscode, string StatusMessage, int ParticipantID) Update(OrganizationEditDTO editDto);
        (int Statuscode, string StatusMessage) Delete(int participantID);
    }
    public class OrganizationProcessor(IProcessParticpant processParty
        , IUnitOfWork unitOfWork
        , ITrackEventsCSV trackEvents
        , IProcessTranslations processTranslations
        , IProcessIndustry processIndustry) : IProcessOrganization
    {
        public (int Statuscode, string StatusMessage) Delete(int particpantID)
        {
            Participant? party = processParty
                .GetListWithPredicate(new ParticipantSearchParameterModel { ParticipantID = [particpantID] })
                .FirstOrDefault();
            if (party == null || party.Organization == null)
            {
                trackEvents.TrackError("OrganizationProcessor.Delete: Organization not found.", new Dictionary<string, object>
                {
                    { "PartyID", particpantID  }
                });
                return (404, "Error_Organization_NotFound");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                unitOfWork.OrganizationRepository.Delete(party.Organization);
                unitOfWork.Save();

                (int statuscode, string message) = processParty.Delete(party);
                if (statuscode != 200)
                {
                    trackEvents.TrackError("OrganizationProcessor.Delete: Error occurred in party deletion.", new Dictionary<string, object>
                    {
                        { "PartyID", particpantID  },
                        { "ProcessPartyStatuscode", statuscode },
                        { "ProcessPartyMessage", message }
                    });
                    scope.Dispose();
                    return (statuscode, message);
                }

                scope.Complete();
                return (200, "Success_Organization_Deleted");
            }
            //catch (SqlException sqlEx)
            //{
            //    trackEvents.TrackException(sqlEx, "OrganizationProcessor.Delete: SQL Exception occurred, likely due to foreign key constraints.", new Dictionary<string, object>
            //    {
            //        { "ParticipantID", particpantID  }
            //    });
            //    return (500, "Error_Organization_Delete_Failed_ForeignKeyConstraint");
            //}
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "OrganizationProcessor.Delete: Exception occurred.", new Dictionary<string, object>
                {
                    { "PartyID", particpantID  }
                });
                return (500, "Error_Error_Ocurred");
            }
        }

        public (int Statuscode, string StatusMessage, int ParticipantID) Insert(OrganizationCreateDTO createDTO)
        {
            ParticipantSearchParameterModel partySearchParameterModel = new()
            {
                ParticipantName = [createDTO.Name],
                ParticipantTypeInt = [1]
            };
            if (createDTO.Industry != null)
            {
                partySearchParameterModel.Organization_Industry_Id = [.. processTranslations
                    .GetWithFallback(new EntityTranslationSearchParameter
                    {
                        EntityType = [nameof(Industry)],
                        TranslatedText = [createDTO.Industry]
                    }).Select(x => x.EntityId)];
            }
            Participant? existingParty = processParty.GetListWithPredicate(partySearchParameterModel).FirstOrDefault();
            if (existingParty != null)
            {
                trackEvents.TrackError("OrganizationProcessor.Insert: Party already exists.", new Dictionary<string, object>
                {
                    { "OrganizationCreateDTO", createDTO }
                });
                return (409, "Error_Party_Exists", 0);
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                ParticipantOperationParameterModel partyOperationParameterModel = new()
                {
                    Participant = new Participant
                    {
                        ParticipantName = createDTO.Name,
                        ParticipantTypeInt = 1,
                        WikipediaUrl = createDTO.WikipediaUrl.ChangeStringToUriToRemoveSubdomain()
                    },
                    ConnectedPlaceIdList = [.. createDTO.ConnectedPlaceList.Select(p => p.Id)],
                    ConnectedEraIdList = [.. createDTO.ConnectedEraList.Select(p => p.Id)]
                };
                Participant newParty = processParty.Insert(partyOperationParameterModel).Participant;

                Organization organization = new()
                {
                    ParticipantID = newParty.ParticipantID
                };
                Organization newOrganization = unitOfWork.OrganizationRepository.Insert(organization);
                unitOfWork.Save();

                if (!string.IsNullOrEmpty(createDTO.Industry))
                {
                    int statusCode = ConnectIndustryToOrganization(newOrganization, createDTO.Industry);
                    if (statusCode != 200)
                    {
                        scope.Dispose();
                        return (statusCode, "Error_Error_Ocurred", 0);
                    }
                }

                scope.Complete();
                return (201, "Success_Organization_Created", newOrganization.ParticipantID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "OrganizationProcessor.Insert: Exception occurred.", new Dictionary<string, object>
                {
                    { "OrganizationCreateDTO", createDTO }
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        public (int Statuscode, string StatusMessage, int ParticipantID) Update(OrganizationEditDTO editDTO)
        {
            Organization? existingOrganization = processParty.GetListWithPredicate(
                new ParticipantSearchParameterModel { ParticipantID = [editDTO.Id] })
                .FirstOrDefault()?.Organization;
            if (existingOrganization == null)
            {
                trackEvents.TrackError("OrganizationProcessor.Update: Organization not found.", new Dictionary<string, object>
                {
                    { "OrganizationEditDTO", editDTO }
                });
                return (404, "Error_Party_NotFound", 0);
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                ParticipantOperationParameterModel partyOperationParameterModel = new()
                {
                    Participant = new Participant
                    {
                        ParticipantID = editDTO.Id,
                        ParticipantName = editDTO.Name,
                        WikipediaUrl = editDTO.WikipediaUrl.ChangeStringToUriToRemoveSubdomain()
                    },
                    ConnectedPlaceIdList = [.. editDTO.ConnectedPlaceList.Select(p => p.Id)],
                    ConnectedEraIdList = [.. editDTO.ConnectedEraList.Select(p => p.Id)]
                };
                Participant editedParty = processParty.Update(partyOperationParameterModel).Participant;

                int statusCode = SyncIndustry(existingOrganization, editDTO.Industry);
                if (statusCode != 200)
                {
                    scope.Dispose();
                    return (statusCode, "Error_Error_Ocurred", 0);
                }

                scope.Complete();
                return (200, "Success_Organization_Updated", editedParty.ParticipantID);
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "OrganizationProcessor.Update: Exception occurred.", new Dictionary<string, object>
                {
                    { "OrganizationEditDTO", editDTO }
                });
                return (500, "Error_Error_Ocurred", 0);
            }
        }

        private int ConnectIndustryToOrganization(Organization organization, string name)
        {
            int? industryId = GetIndustryId(name);
            if (industryId != null)
            {
                (int statusCode, string stringMessage, industryId) = processIndustry.Insert(new Industry { IndustryName = name });
                if (statusCode != 201)
                {
                    trackEvents.TrackError("OrganizationProcessor.ConnectIndustryToOrganization: Error occurred in industry insertion.", new Dictionary<string, object>
                    {
                        { "OrganizationID", organization.OrganizationID  },
                        { "IndustryName", name },
                        { "ProcessIndustryStatuscode", statusCode },
                        { "ProcessIndustryMessage", stringMessage }
                    });
                    return statusCode;
                }
            }

            organization.IndustryID = industryId;
            unitOfWork.Save();
            return 200;
        }

        private int SyncIndustry(Organization organization, string? name)
        {
            if (organization.IndustryID != null && string.IsNullOrWhiteSpace(name))
            {
                DisconnectIndustry(organization);
            }
            else if (organization.IndustryID == null && !string.IsNullOrWhiteSpace(name))
            {
                int statusCode = ConnectIndustryToOrganization(organization, name);
                if (statusCode != 200)
                {
                    return statusCode;
                }
            }

            return 200;
        }
        private void DisconnectIndustry(Organization organization)
        {
            organization.IndustryID = null;
            unitOfWork.Save();
        }

        private int? GetIndustryId(string name)
        {
            return processTranslations.GetWithFallback(new EntityTranslationSearchParameter
            {
                EntityType = [nameof(Industry)],
                TranslatedText = [name],
            }).Select(x => x.EntityId).FirstOrDefault();
        }
    }
}
