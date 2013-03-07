using System;
using System.Threading;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;

namespace Aquila_Software
{
    public class Worker
    {
        public ManualExecution ManualExecution;
        public Equity Equity;
        public bool IsActive;
        public float Amount;
        public BarSize BarSize;
        public HistoricalDataType HistoricalDataType;
        public RealTimeBarType RealtimeBarType;
        public bool isCalculating;
        public float PricePremiumPercentage;

        private Thread thread;

        private WorkerInfo workerInfo;

        public Worker(WorkerInfo workerInfo)
        {
            this.workerInfo = workerInfo;
        }

        public void run()
        {
            while (true)
            {
                Thread.Sleep(1000);
                getSettings();
                Console.WriteLine(this.Amount);
            }
        }

        public void getSettings()
        {
            this.Equity = new Equity(workerInfo.Equity);
            this.IsActive = workerInfo.IsActive;
            this.Amount = workerInfo.Amount;

            if (workerInfo.BarSize.Equals("mBar"))
            {
                this.BarSize = BarSize.OneMinute;
            }
            else if (workerInfo.BarSize.Equals("dBar"))
            {
                this.BarSize = BarSize.OneDay;
            }

            if (this.workerInfo.BarType.Equals("Bid"))
            {
                this.HistoricalDataType = HistoricalDataType.Bid;
                this.RealtimeBarType = RealTimeBarType.Bid;
            }
            else if (this.workerInfo.BarType.Equals("Ask"))
            {
                this.HistoricalDataType = HistoricalDataType.Ask;
                this.RealtimeBarType = RealTimeBarType.Ask;
            }
            else if (this.workerInfo.BarType.Equals("Last") || this.workerInfo.BarType.Equals("Trades"))
            {
                this.HistoricalDataType = HistoricalDataType.Trades;
                this.RealtimeBarType = RealTimeBarType.Trades;
            }
            else if (this.workerInfo.BarType.Equals("Midpoint"))
            {
                this.HistoricalDataType = HistoricalDataType.Midpoint;
                this.RealtimeBarType = RealTimeBarType.Midpoint;
            }

            this.isCalculating = workerInfo.isCalculating;
            this.PricePremiumPercentage = workerInfo.PricePremiumPercentage;
        }
    }
}