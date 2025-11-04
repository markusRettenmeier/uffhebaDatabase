using Sammlerplattform.Data;
using Sammlerplattform.Models.PlaceDatabase.SettlementDatabase;

namespace Sammlerplattform.Services.Processes.PlaceProcesses
{
    public interface IProcessPostalcode
    {
        Postalcode CreateOrGetPostalcode(string postalcodeNumber);
        Postalcode? GetPostalcode(string postalcodeNumber);
    }

    public class PostalcodeProcessor(IUnitOfWork unitOfWork) : IProcessPostalcode
    {
        public Postalcode CreateOrGetPostalcode(string postalcodeNumber)
        {
            if (string.IsNullOrEmpty(postalcodeNumber))
            {
                return new Postalcode() { PostalcodeNumber = postalcodeNumber }; ;
            }
            Postalcode? existingPostalcode = GetPostalcode(postalcodeNumber);

            if (existingPostalcode != null)
            {
                return existingPostalcode;
            }
            else
            {
                Postalcode newPostalcode = new() { PostalcodeNumber = postalcodeNumber };
                newPostalcode = unitOfWork.PostalcodeRepository.Insert(newPostalcode);
                unitOfWork.Save();
                return newPostalcode;
            }
        }

        public Postalcode? GetPostalcode(string postalcodeNumber)
        {
            Postalcode? existingPostalcode = (from p in unitOfWork.PostalcodeRepository.Get()
                                              select p).Where(x => x.PostalcodeNumber == postalcodeNumber).FirstOrDefault();
            return existingPostalcode;
        }
    }
}
