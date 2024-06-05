namespace Sammlerplattform.Models.PersonDatabase
{
    public interface IPersonSearchParameterModel
    {
        ICollection<int> SearchPersonID { get; set; }
        ICollection<string> SearchPersonName { get; set; }
        ICollection<string> SearchSignature { get; set; }
    }
}
