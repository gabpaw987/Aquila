using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Krs.Ats.IBNet.Contracts;
using Krs.Ats.IBNet;

namespace AquilaWCF
{
    class Worker
    {
        public volatile ManualExecution ManualExecution; 
        public volatile Equity Equity;
        public volatile bool IsActive;
        public volatile decimal Amount;
        public volatile BarSize BarSize;
        public volatile HistoricalDataType historicalDataType;
        public volatile RealTimeBarType RealtimeBarType;
        public volatile bool _Running;
        public volatile decimal pricePremium;
    }
}
