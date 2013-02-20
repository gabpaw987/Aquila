using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;
using System.Threading;
using System;

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

        private Thread thread;

        public Worker(string symbol)
        {
            this.Equity = new Equity(symbol);
            thread = new Thread(run);
            thread.Start();
        }

        public void run()
        {
            while (true)
            {
                Thread.Sleep(1000);
                printInfo();
            }
        }

        public void printInfo()
        {
            Console.WriteLine("Hallo" + Amount);
        }

    }
}