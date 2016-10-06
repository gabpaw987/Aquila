using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Aquila_Software
{
    public class ServerHostFactory : ServiceHostFactory
    {
        private Dictionary<string, WorkerInfo> dep;

        public ServerHostFactory()
        {
            this.dep = new Dictionary<string, WorkerInfo>();
        }

        protected override ServiceHost CreateServiceHost(Type serviceType,
            Uri[] baseAddresses)
        {
            return new ServerHost(this.dep, serviceType, baseAddresses);
        }
    }
}