using System.Runtime.Serialization;

namespace Aquila_Software
{
    [DataContract]
    public class WCFMessage
    {
        [DataMember(IsRequired = true)]
        public string args { get; set; }

        public WCFMessage(string args)
        {
            this.args = args;
        }
    }
}