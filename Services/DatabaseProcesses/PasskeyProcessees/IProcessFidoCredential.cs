using Sammlerplattform.Data;
using Sammlerplattform.Models.Passkey;

namespace Sammlerplattform.Services.DatabaseProcesses.PasskeyProcessees
{
    public interface IProcessFidoCredential
    {
        FidoCredential? GetCredentialById(byte[] credentialId);
        List<FidoCredential> GetCredentialsByUserId(string userId);
        (int Statuscode, string Statusmessage) Insert(FidoCredential fidoCredential);
        (int Statuscode, string Statusmessage) UpdateSignatureCounter(byte[] credentialId, long counter);
        (int Statuscode, string Statusmessage) Delete(byte[] credentialId, string userId);
    }

    public class FidoCredentialProcessor(IUnitOfWork unitOfWork
        , ITrackEventsCSV trackEvents
        , DbIdentityContext dbIdentityContext) : IProcessFidoCredential
    {
        public (int Statuscode, string Statusmessage) Delete(byte[] credentialId, string userId)
        {
            var credential = unitOfWork.FidoCredentialRepository
                .Get(c => c.CredentialId == credentialId && c.UserId == userId);
            if (credential == null)
            {
                return (404, "FidoCredential_NotFound");
            }

            try
            {
                unitOfWork.FidoCredentialRepository.Delete(credential);
                unitOfWork.Save();
                return (200, "Deleted_FidoCredential");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "FidoCredentialProceessor-Delete");
                return (500, "Error_FidoCredential");
            }

        }

        public FidoCredential? GetCredentialById(byte[] credentialIdBytes)
        {
            try
            {
                // Option 1: Mit ToList() zuerst in Memory laden
                var allCredentials = dbIdentityContext.FidoCredential.ToList();
                var credential = allCredentials.FirstOrDefault(
                    fc => fc.CredentialId.SequenceEqual(credentialIdBytes));

                return credential;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCredentialById: {ex.Message}");
                return null;
            }
        }
        public List<FidoCredential> GetCredentialsByUserId(string userId)
        {
            return [.. unitOfWork.FidoCredentialRepository.Get(
                filter: fc => fc.UserId == userId)];
        }

        public (int Statuscode, string Statusmessage) Insert(FidoCredential fidoCredential)
        {
            try
            {
                unitOfWork.FidoCredentialRepository.Insert(new FidoCredential
                {
                    CredentialId = fidoCredential.CredentialId,
                    UserId = fidoCredential.UserId,
                    PublicKey = fidoCredential.PublicKey,
                });
                unitOfWork.Save();
                return (201, "Created_FidoCredential");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "FidoCredentialProceessor-Insert");
                return (500, "Error_FidoCredential");
            }
        }

        public (int Statuscode, string Statusmessage) UpdateSignatureCounter(byte[] credentialId, long counter)
        {
            var credential = GetCredentialById(credentialId);
            if (credential == null)
            {
                return (404, "FidoCredential_NotFound");
            }

            try
            {
                credential.SignatureCounter = counter;
                unitOfWork.Save();
                return (200, "Updated_FidoCredential");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "FidoCredentialProceessor-Update");
                return (500, "Error_FidoCredential");
            }
        }
    }
}
