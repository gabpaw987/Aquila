using System.ServiceModel;

namespace Aquila_Software
{
    [ServiceContract(Namespace = "http://aquila.com/ServiceHandler")]
    public interface SettingsHandler
    {
        [OperationContract]
        bool SetSetting(object[] args);

        [OperationContract]
        bool PerformAction(object[] args);
    }
}