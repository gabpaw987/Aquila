using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;
using System.Threading;
using System;
using System.Runtime.Serialization;

namespace Aquila_Software
{
    [DataContract]
    public class Worker
    {
        ////public volatile ManualExecution ManualExecution;
        //[DataMember]
        //public Equity Equity;
        [DataMember]
        public bool IsActive;
        [DataMember]
        public float Amount;
        //[DataMember]

        private BarSize _barSize;
        
        [DataMember]
        public string BarSize 
        {
            get
            {
                if (_barSize.Equals(Krs.Ats.IBNet.BarSize.OneMinute)) return "1min";
                else if (_barSize.Equals(Krs.Ats.IBNet.BarSize.OneHour)) return "1hour";
                else if (_barSize.Equals(Krs.Ats.IBNet.BarSize.OneDay)) return "1day";
                else return "";
            }
            set {
                if (value.Equals("1min")) _barSize = Krs.Ats.IBNet.BarSize.OneMinute;
                else if (value.Equals("1hour")) _barSize = Krs.Ats.IBNet.BarSize.OneHour;
                else if (value.Equals("1day")) _barSize = Krs.Ats.IBNet.BarSize.OneDay;
            }
        }

        //[DataMember]
        //public HistoricalDataType HistoricalDataType;
        //[DataMember]
        //public RealTimeBarType RealtimeBarType;
        [DataMember]
        public bool isCalculating;
        [DataMember]
        public float PricePremiumPercentage;

        ////private Thread thread;

        public Worker(string symbol)
        {            

        }

        //public void run()
        //{
        //    while (true)
        //    {
        //        //Thread.Sleep(1000);
        //        printInfo();
        //    }
        //}

        //public void printInfo()
        //{
        //    Console.WriteLine("Hallo" + Amount);
        //}

    }
}