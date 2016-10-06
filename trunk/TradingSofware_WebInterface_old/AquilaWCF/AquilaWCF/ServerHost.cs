using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Aquila_Software
{
    public class ServerHost : ServiceHost
    {
        public ServerHost(Dictionary<string, WorkerInfo> dep, Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            if (dep == null)
            {
                throw new ArgumentNullException("dep");
            }

            foreach (var cd in this.ImplementedContracts.Values)
            {
                cd.Behaviors.Add(new ServerProvider(dep));
            }
        }
    }
}