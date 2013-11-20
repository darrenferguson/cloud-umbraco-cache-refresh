namespace Moriyama.Cloud.Umbraco.Interfaces.Application
{
    public interface IServiceInstanceService
    {
        void Register(string hostName);
        
        void Publish(string hostName, int documentId);

        void RefreshCache(string hostName);



    }
}
