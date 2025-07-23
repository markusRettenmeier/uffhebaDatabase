using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Services.UnitOfWork;

namespace Sammlerplattform.Services.Processes.CityProcesses
{
    public interface IProcessPostalcode
    {
        Postalcode CreatePostalcode(string postalcodeNumber);
    }

    public class PostalcodeProcessor(IUnitOfWork unitOfWork) : IProcessPostalcode
    {
        public Postalcode CreatePostalcode(string postalcodeNumber)
        {
            if (string.IsNullOrEmpty(postalcodeNumber))
            {
                throw new NullReferenceException();
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
