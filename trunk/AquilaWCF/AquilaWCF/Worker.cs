using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;

namespace Aquila_Software
{
    internal class Worker
    {
        //public volatile ManualExecution ManualExecution;
        public volatile Equity Equity;
        public volatile bool IsActive;
        public volatile float Amount;
        public volatile BarSize BarSize;
        public volatile HistoricalDataType HistoricalDataType;
        public volatile RealTimeBarType RealtimeBarType;
        public volatile bool _Running;
        public volatile float PricePremiumPercentage;
    }
}