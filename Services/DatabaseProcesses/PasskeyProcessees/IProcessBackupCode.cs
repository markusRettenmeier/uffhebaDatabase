using Sammlerplattform.Data;
using Sammlerplattform.Models.UserSettings;

namespace Sammlerplattform.Services.DatabaseProcesses.PasskeyProcessees
{
    public interface IProcessBackupCode
    {
        List<BackupCode> GetByUserId(string userId);
        List<BackupCode> GetById(int backupCodeId);
        (int Statuscode, string Statusmessage) InsertRange(List<BackupCode> backupCodes);
        (int Statuscode, string Statusmessage) MarkAsUsed(int backupCodeId);
        (int Statuscode, string Statusmessage) DeleteRangeByUserId(string userId);
    }

    public class BackupCodeProcessor(IUnitOfWork unitOfWork
        , ITrackEventsCSV trackEvents) : IProcessBackupCode
    {
        public (int Statuscode, string Statusmessage) DeleteRangeByUserId(string userId)
        {
            var backupCodeList = unitOfWork.BackupCodeRepository
                .Get(bc => bc.UserId == userId);
            if (backupCodeList == null)
            {
                return (404, "Error_BackupCode_NotFound");
            }

            try
            {
                foreach (var backupCode in backupCodeList)
                {
                    unitOfWork.BackupCodeRepository.Delete(backupCode);
                }
                unitOfWork.Save();
                return (200, "Success_BackupCode_Deleted");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "BackupCodeProcessor-Delete");
                return (500, "Error_Unknown");
            }
        }
        public List<BackupCode> GetByUserId(string userId)
        {
            try
            {
                return [.. unitOfWork.BackupCodeRepository.Get(filter: bc => bc.UserId == userId)];
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "BackupCodeProcessor-GetByUserId");
                return [];
            }
        }
        public List<BackupCode> GetById(int backupCodeId)
        {
            try
            {
                return [.. unitOfWork.BackupCodeRepository.Get(filter: bc => bc.Id == backupCodeId)];
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "BackupCodeProcessor-GetById");
                return [];
            }
        }
        public (int Statuscode, string Statusmessage) InsertRange(List<BackupCode> backupCodeList)
        {
            try
            {
                foreach (var backupCode in backupCodeList)
                {
                    unitOfWork.BackupCodeRepository.Insert(backupCode);
                }
                unitOfWork.Save();
                return (201, "Created_BackupCode");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "BackupCodeProcessor-Insert");
                return (500, "Error_Unknown");
            }
        }
        public (int Statuscode, string Statusmessage) MarkAsUsed(int backupCodeId)
        {
            var backupCode = GetById(backupCodeId).FirstOrDefault();
            if (backupCode == null)
            {
                return (404, "Error_BackupCode_NotFound");
            }

            try
            {
                backupCode.IsUsed = true;
                backupCode.UsedAt = DateTime.UtcNow;
                unitOfWork.Save();
                return (200, "Success_BackupCode_MarkedAsUsed");
            }
            catch (Exception ex)
            {
                trackEvents.TrackException(ex, "BackupCodeProcessor-MarkAsUsed");
                return (500, "Error_Unknown");
            }
        }
    }
}
