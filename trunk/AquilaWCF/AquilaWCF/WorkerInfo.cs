using System.Runtime.Serialization;

namespace Aquila_Software
{
    public class WorkerInfo
    {
        [DataMember]
        public string ManualExecution;
        [DataMember]
        public string Equity;
        [DataMember]
        public bool IsActive;
        [DataMember]
        public float Amount;
        [DataMember]
        public string BarSize;
        [DataMember]
        public string BarType;
        [DataMember]
        public bool isCalculating;
        [DataMember]
        public float PricePremiumPercentage;

        public WorkerInfo(string symbol)
        {
            this.Equity = symbol;
        }
    }
}