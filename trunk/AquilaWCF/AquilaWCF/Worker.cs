using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;

namespace Aquila_Software
{
    public class Worker
    {
        public Equity Equity;
        public bool IsActive;
        public float Amount;
        public BarSize BarSize;
        public HistoricalDataType HistoricalDataType;
        public RealTimeBarType RealtimeBarType;
        public bool isCalculating;
        public float PricePremiumPercentage;
        public string LocalSymbol;

        private volatile List<Tuple<DateTime, decimal, decimal, decimal, decimal>> MinuteBars;
        private volatile List<Tuple<DateTime, decimal, decimal, decimal, decimal>> DailyBars;
        private List<int> Signals;

        private Thread Thread;
        private WorkerInfo workerInfo;

        private IBOutput IBOutput;

        // to stop the thread from the main method
        public bool RunThread = true;

        public Worker(WorkerInfo workerInfo)
        {
            this.workerInfo = workerInfo;
            this.Thread = new Thread(this.Run);
            this.LocalSymbol = workerInfo.Equity;
            this.MinuteBars = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
            this.DailyBars = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
            this.Signals = new List<int>();
        }

        public void Run()
        {
            GetSettings();
            this.loadHistoricalData();
            var length = 0;
            while (RunThread)
            {
                GetSettings();

                //DONE: wait till new bar is ready
                if (BarSize.Equals(BarSize.OneDay))
                {
                    while (length == DailyBars.Count)
                    {
                        Thread.Sleep(1000);
                    }

                    //Calculate the decision
                    //TODO: Add "MAP" to Algorithm
                    Algorithm.DecisionCalculator.startCalculation(DailyBars, Signals);

                    int countToInsert = DailyBars.Count - length;
                    for (int i = DailyBars.Count - countToInsert; i < DailyBars.Count; i++)
                    {
                        QueryHandler.insertSignal(this.LocalSymbol, this.Signals[i], this.DailyBars[i].Item1);
                    }

                    length = this.DailyBars.Count;
                }
                else if (BarSize.Equals(BarSize.OneMinute))
                {
                    while (length == MinuteBars.Count)
                    {
                        Thread.Sleep(1000);
                    }

                    //Calculate the decision
                    //TODO: Add "MAP" to Algorithm
                    Algorithm.DecisionCalculator.startCalculation(MinuteBars, Signals);

                    int countToInsert = MinuteBars.Count - length;
                    for (int i = MinuteBars.Count - countToInsert; i < MinuteBars.Count; i++)
                    {
                        QueryHandler.insertSignal(this.LocalSymbol, this.Signals[i], this.MinuteBars[i].Item1);
                    }

                    length = this.MinuteBars.Count;
                }
                if (isCalculating)
                {
                    //TODO: remove this
                    Signals.Add(3);
                    Signals.Add(1);
                    if (Signals[Signals.Count - 1] != Signals[Signals.Count - 2])
                    {
                        int oldSignal = Signals[Signals.Count - 2];
                        int newSignal = Signals[Signals.Count - 1];
                        bool isBuy = false;
                        int toZero = 0;
                        int fromZero = 0;

                        if ((newSignal > 0 && oldSignal < 0) ||
                            (newSignal < 0 && oldSignal > 0))
                        {
                            toZero = 0 - oldSignal;
                            fromZero = newSignal;
                        }
                        else if (newSignal > oldSignal)
                        {
                            //kaufen newSignal - oldSignal
                            toZero = newSignal - oldSignal;
                        }
                        else if (newSignal < oldSignal)
                        {
                            //verkaufen newSignal - oldSignal
                            toZero = -(newSignal - oldSignal);
                        }
                        else if (newSignal == 0)
                        {
                            toZero = 0 - oldSignal;
                        }

                        if (Math.Sign(toZero) == 1)
                        {
                            isBuy = true;
                        }

                        this.IBOutput = new IBOutput(this.Equity);

                        this.IBOutput.RequestTickPrice();

                        float roundLotPrice = 0f;

                        //TODO: peer wegen pricepremium
                        if (isBuy)
                        {
                            roundLotPrice = (float)(this.IBOutput.currentAskPrice * 100);
                        }
                        else
                        {
                            roundLotPrice = (float)(this.IBOutput.currentBidPrice * 100);
                        }

                        int one = (int)((this.Amount / roundLotPrice) / 3);
                        int two = (int)(((this.Amount / roundLotPrice) * 2) / 3);
                        int three = (int)(this.Amount / roundLotPrice);

                        int amountToZero = 0;
                        switch (toZero)
                        {
                            case 1:
                            case -1: { amountToZero = one; break; }
                            case 2:
                            case -2: { amountToZero = two; break; }
                            case 3:
                            case -3: { amountToZero = three; break; }
                        }
                        int amountFromZero = 0;
                        if (fromZero != 0)
                        {
                            switch (fromZero)
                            {
                                case 1:
                                case -1: { amountFromZero = one; break; }
                                case 2:
                                case -2: { amountFromZero = two; break; }
                                case 3:
                                case -3: { amountFromZero = three; break; }
                            }
                        }
                        if (amountToZero != 0)
                        {
                            // iboutput place and execute
                            if (isBuy)
                                this.IBOutput.placeOrder(ActionSide.Buy, amountToZero + amountFromZero);
                            else if (!isBuy)
                                this.IBOutput.placeOrder(ActionSide.Sell, amountToZero + amountFromZero);

                            if (IsActive)
                            {
                                this.IBOutput.executeOrder(PricePremiumPercentage);
                            }
                            else
                            {
                                //DONE: set manualexecution for pieer via http get
                                this.NewTradeNotification(newSignal, oldSignal, (amountToZero + amountFromZero) * roundLotPrice,
                                    amountToZero + amountFromZero);
                            }
                        }
                    }
                }
            }
        }

        public void loadHistoricalData()
        {
            var inputMinute = new IBInput(this.MinuteBars, this.DailyBars, this.Equity, this.LocalSymbol, BarSize.OneMinute);
            var inputDaily = new IBInput(this.MinuteBars, this.DailyBars, this.Equity, this.LocalSymbol, BarSize.OneDay);

            //request historical data bars
            Console.WriteLine("Please wait... Historical minute bars are getting fetched!");
            inputMinute.GetHistoricalDataBars(new TimeSpan(0, 1, 56, 59));

            //wait until all bars were received
            while (this.MinuteBars.Count < inputMinute.totalHistoricalBars || inputMinute.totalHistoricalBars == 0)
            {
                System.Threading.Thread.Sleep(100);
            }

            //request historical data bars
            Console.WriteLine("Please wait... Historical daily bars are getting fetched!");
            inputDaily.GetHistoricalDataBars(new TimeSpan(6, 0, 0, 0));

            //wait until all bars were received
            while (this.MinuteBars.Count < inputDaily.totalHistoricalBars || inputDaily.totalHistoricalBars == 0)
            {
                System.Threading.Thread.Sleep(100);
            }
            Console.WriteLine("Finished fetching historical data for " + this.Equity + ".");
            LogFileManager.WriteToLog("Fetched Historical bars for " + this.Equity + ".");

            Thread t = new Thread(this.startHistoricalBarsInsert);
            t.Start();
        }

        public void NewTradeNotification(int newDecision, int oldDecision, float transactionPrice, int size)
        {
            //TODO: remove this
            return;
            //TODO: change localhost
            string url = "http://localhost:80/notifications.aspx?symbol=" + this.Equity.LocalSymbol + "&newDecision=" + newDecision +
                "&oldDecision=" + oldDecision + "&transactionPrice=" + transactionPrice + "&size=" + size;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
        }

        public void Start()
        {
            this.Thread.Start();
        }

        public void GetSettings()
        {
            this.Equity = new Equity(workerInfo.Equity.Split(':')[0]);
            this.LocalSymbol = workerInfo.Equity;
            this.IsActive = workerInfo.IsActive;
            this.Amount = workerInfo.Amount;

            //TODO: cutloss dazu
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

        public bool executeOrder()
        {
            this.IBOutput.executeOrder(PricePremiumPercentage);
            return true;
        }

        public bool dismissOrder()
        {
            return false;
        }

        public void startHistoricalBarsInsert()
        {
            foreach (Tuple<DateTime, decimal, decimal, decimal, decimal> t in MinuteBars)
                QueryHandler.insertBar(this.LocalSymbol, "mBar", 0, t);

            foreach (Tuple<DateTime, decimal, decimal, decimal, decimal> t in DailyBars)
                QueryHandler.insertBar(this.LocalSymbol, "dBar", 0, t);
        }
    }
}