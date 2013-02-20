using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;

namespace Aquila_Software
{
    public class Worker
    {
        //public volatile ManualExecution ManualExecution;
        public volatile Equity Equity;
        public volatile bool IsActive;
        public volatile float Amount;
        public volatile BarSize BarSize;
        public volatile HistoricalDataType HistoricalDataType;
        public volatile RealTimeBarType RealtimeBarType;
        public volatile bool isCalculating;
        public volatile float PricePremiumPercentage;

        public Worker(string symbol)
        {
            this.Equity = new Equity(symbol);
        }
    }
}