using Sammlerplattform.Models.CollectionItemDatabase.ObjectLayerDatabase;

namespace Sammlerplattform.Services.DatabaseProcesses.CollectionItemProcesses
{
    public interface IProcessObjectLayer
    {
        (int ObjectLayerID, int StatusCode, int Statusmessage) Insert(ObjectLayer objectLayer);
        (int ObjectLayerID, int StatusCode, int Statusmessage) Update(ObjectLayer objectLayer);
        (int StatusCode, int Statusmessage) Delete(ObjectLayer objectLayer);
        List<ObjectLayer> GetListWithPredicate(ObjectLayer objectLayer);
    }
}
