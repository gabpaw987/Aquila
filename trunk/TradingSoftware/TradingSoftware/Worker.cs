using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;
using System.Globalization;
using System.IO;

namespace TradingSoftware
{
    [Serializable()]
    public class Worker
    {
        private MainViewModel mainViewModel;
        private WorkerViewModel workerViewModel;

        private List<Tuple<DateTime, decimal, decimal, decimal, decimal>> Bars;
        private List<int> Signals;
        private Thread Thread;

        private IBOutput IBOutput;

        // to stop the thread from the main method
        public bool RunThread = true;

        //To stop the trading and close all current positions
        private bool isStopTrading = false;
        //To let the current signal run through and stop trading after that
        private bool isStopTradingAfterSignal = false;

        private bool didFirst;
        public bool shallReenter;
        public bool hasFirstSignalPassed;

        //is -100 if no signal was ignored recently
        public int lastIgnoredSignal;

        private IBInput realTimeDataClient;

        public Worker(MainViewModel mainViewModel, WorkerViewModel workerViewModel, string equity, bool isTrading, string barsize,
                      string dataType, decimal pricePremiumPercentage, int roundLotSize, bool isFutureTrading, int currentPosition,
                      bool shallIgnoreFirstSignal, bool hasAlgorithmParameters, string algorithmFilePath)
        {
            this.mainViewModel = mainViewModel;
            this.workerViewModel = workerViewModel;

            this.Bars = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
            this.Signals = new List<int>();

            this.workerViewModel.IsTrading = isTrading;
            this.workerViewModel.EquityAsString = equity;
            this.workerViewModel.BarsizeAsString = barsize;
            this.workerViewModel.DataType = dataType;
            this.workerViewModel.PricePremiumPercentage = pricePremiumPercentage;
            this.workerViewModel.RoundLotSize = roundLotSize;
            this.didFirst = false;
            this.hasFirstSignalPassed = false;
            this.workerViewModel.CurrentPosition = currentPosition;
            this.workerViewModel.IsFutureTrading = isFutureTrading;
            this.workerViewModel.ShallIgnoreFirstSignal = shallIgnoreFirstSignal;
            this.workerViewModel.AlgorithmFilePath = algorithmFilePath;
            this.workerViewModel._parsedAlgorithmParameters = new Dictionary<string, decimal>();
            this.workerViewModel.HasAlgorithmParameters = hasAlgorithmParameters;
            this.lastIgnoredSignal = -100;

            XMLHandler.CreateWorker(equity, isTrading, barsize, dataType, "mustbechanged", pricePremiumPercentage,
                                    isFutureTrading, currentPosition, shallIgnoreFirstSignal, hasAlgorithmParameters,
                                    roundLotSize, "mustbeChanged");
            
            this.Thread = new Thread(this.Run);
        }

        public string readAlgorithmParameters()
        {
            try
            {
                return File.ReadAllText("Parameters/" + this.workerViewModel.EquityAsString + ".param");
            }
            catch (Exception)
            {
                this.workerViewModel.ConsoleText += this.workerViewModel.EquityAsString + ": Param-File not found.";
                return null;
            }
        }        

        public void Stop()
        {
            this.RunThread = false;
            if (this.realTimeDataClient != null)
            {
                this.realTimeDataClient.Disconnect();
            }
        }

        public void Run()
        {
            loadHistoricalData();

            var length = 0;

            while (RunThread)
            {
                //TODO: exit on exception
                if (this.workerViewModel.BarsizeAsObject.Equals(BarSize.OneMinute))
                {
                    while (length == this.Bars.Count && this.RunThread && !this.isStopTrading)
                    {
                        Thread.Sleep(1000);
                    }

                    //Calculate the decision
                    if (this.workerViewModel.HasAlgorithmParameters)
                    {
                        this.workerViewModel.AlgorithmParameters = this.readAlgorithmParameters();
                        Algorithm.DecisionCalculator.startCalculation(Bars, Signals, new Dictionary<string, List<decimal>>(), new Dictionary<string, List<decimal>>(), this.workerViewModel.ParsedAlgorithmParameters);
                    }
                    else
                    {
                        Algorithm.DecisionCalculator.startCalculation(Bars, Signals, new Dictionary<string, List<decimal>>(), new Dictionary<string, List<decimal>>(), new Dictionary<string, decimal>());
                    }
                    
                    //For testing purposes
                    //this.Signals[this.Signals.Count - 2] = 0;
                    //this.Signals[this.Signals.Count - 1] = 3;
                    //if (this.didFirst)
                    //    this.Signals[this.Signals.Count - 1] = 1;

                    //Stop if isStopTradingAfterSignal is true and the signal is finished
                    if (this.isStopTradingAfterSignal &&
                       (Math.Sign(this.Signals[this.Signals.Count - 1]) != Math.Sign(this.workerViewModel.CurrentPosition) ||
                        Math.Sign(this.Signals[this.Signals.Count - 1]) == 0 ||
                        this.workerViewModel.CurrentPosition == 0))
                    {
                        this.isStopTrading = true;
                    }

                    if (this.isStopTrading)
                    {
                        this.Signals[this.Signals.Count - 1] = 0;
                    }

                    lock (IBID.ConsoleTextLock)
                    {
                        this.workerViewModel.SignalText += this.workerViewModel.EquityAsString + ": Current Signal: " + this.Bars.Last().Item1.ToString() + " ... " + Signals.Last() + "\n";
                    }

                    length = this.Bars.Count;
                }
                if (this.workerViewModel.IsTrading)
                {
                    if (this.Signals[this.Signals.Count - 1] == 0)
                    {
                        //just for testing purposes, but didFirst is generally almost only for testing purposes so it can stay like that
                        this.didFirst = true;

                        this.hasFirstSignalPassed = true;
                    }

                    if (((this.Signals[this.Signals.Count - 1] != this.workerViewModel.CurrentPosition) || this.shallReenter)
                        && (this.lastIgnoredSignal != this.Signals[this.Signals.Count - 1]))
                    {
                        if (this.lastIgnoredSignal != -100)
                        {
                            this.lastIgnoredSignal = -100;
                        }

                        if ((this.didFirst && this.workerViewModel.ShallIgnoreFirstSignal && (this.Signals[this.Signals.Count - 1] != this.Signals[this.Signals.Count - 2])))
                        {
                            this.hasFirstSignalPassed = true;
                        }

                        if (this.hasFirstSignalPassed || !this.workerViewModel.ShallIgnoreFirstSignal)
                        {
                            int oldSignal = this.workerViewModel.CurrentPosition;

                            int newSignal = Signals[Signals.Count - 1];

                            lock (IBID.SoundLock)
                            {
                                //Ensure that only one AutoClosingMessageBox gets opened at a time
                                Thread.Sleep(200);

                                SoundPlayer sound = new SoundPlayer(@"../../sounds/BIGHORN.wav");
                                sound.Play();
                            }

                            MessageBoxResult dialogResult;
                            
                            if (!isStopTrading)
                            {
                                dialogResult = AutoClosingMessageBox.Show(this.workerViewModel.EquityAsString + ": New Signal is " + newSignal + ".\n Would you like to ignore the order?", "New Signal", 5000, MessageBoxButton.OKCancel);
                            }
                            else
                            {
                                dialogResult = MessageBoxResult.Cancel;
                            }

                            if (dialogResult == MessageBoxResult.Cancel)
                            {
                                if (oldSignal != newSignal)
                                {
                                    bool isBuy = false;
                                    int toZero = 0;
                                    int fromZero = 0;

                                    if ((newSignal > 0 && oldSignal < 0) ||
                                        (newSignal < 0 && oldSignal > 0))
                                    {
                                        toZero = 0 - oldSignal;
                                        fromZero = newSignal;
                                    }
                                    else if (newSignal != 0)
                                    {
                                        //kaufen und verkaufen newSignal - oldSignal
                                        toZero = newSignal - oldSignal;
                                    }
                                    else if (newSignal == 0)
                                    {
                                        toZero = 0 - oldSignal;
                                    }

                                    if (Math.Sign(toZero) == 1)
                                    {
                                        isBuy = true;
                                    }

                                    this.IBOutput = new IBOutput(this.workerViewModel, this.workerViewModel.EquityAsContract);
                                    this.IBOutput.Connect();

                                    this.IBOutput.RequestTickPrice();

                                    int one = 1;
                                    int two = 2;
                                    int three = 3;

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

                                    if (amountToZero != 0 && this.workerViewModel.IsTrading)
                                    {
                                        if(isBuy)
                                            this.IBOutput.placeOrder(ActionSide.Buy, (amountToZero + amountFromZero) * ((this.workerViewModel.IsFutureTrading) ? 1 : this.workerViewModel.RoundLotSize), this.workerViewModel.PricePremiumPercentage);
                                        else if (!isBuy)
                                            this.IBOutput.placeOrder(ActionSide.Sell, (amountToZero + amountFromZero) * ((this.workerViewModel.IsFutureTrading) ? 1 : this.workerViewModel.RoundLotSize), this.workerViewModel.PricePremiumPercentage);

                                        this.workerViewModel.CurrentPosition = newSignal;
                                        this.IBOutput.Disconnect();
                                    }
                                }

                                this.didFirst = true;
                            }
                            else
                            {
                                this.didFirst = true;
                                this.lastIgnoredSignal = this.Signals[this.Signals.Count - 1];
                            }
                        }
                        else
                        {
                            this.didFirst = true;
                        }
                    }

                    if (isStopTradingAfterSignal)
                    {
                        this.isStopTradingAfterSignal = false;
                    }

                    if (this.isStopTrading)
                    {
                        this.workerViewModel.IsTrading = false;
                        this.isStopTrading = false;

                        //To Ignore the first signal again after stopping, if wanted
                        this.didFirst = false;
                        this.hasFirstSignalPassed = false;
                    }
                }
            }
        }

        public void loadHistoricalData()
        {
            this.realTimeDataClient = new IBInput(this.workerViewModel, this.Bars, this.workerViewModel.EquityAsContract, BarSize.OneMinute, this.workerViewModel.IsFutureTrading);

            this.realTimeDataClient.Connect();

            lock (IBID.ConsoleTextLock)
            {
                this.workerViewModel.ConsoleText += this.workerViewModel.EquityAsString + ": Start receiving realtime bars...\n";
            }
            this.realTimeDataClient.SubscribeForRealTimeBars();

            //Wait for first 5sec bar
            while (this.realTimeDataClient.RealTimeBarList.Count <= 1 && this.RunThread)
            {
                System.Threading.Thread.Sleep(100);
            }

            //wait until minute is full
            if (this.workerViewModel.BarsizeAsObject.Equals(BarSize.OneMinute))
            {
                while (this.realTimeDataClient.RealTimeBarList.Count != 0 && this.RunThread)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }

            if (RunThread)
            {
                var historicalDataClient = new IBInput(this.workerViewModel, this.Bars, this.workerViewModel.EquityAsContract, this.workerViewModel.BarsizeAsObject, this.workerViewModel.IsFutureTrading);
                historicalDataClient.Connect();

                //request historical data bars
                lock (IBID.ConsoleTextLock)
                {
                    this.workerViewModel.ConsoleText += this.workerViewModel.EquityAsString + ": Please wait... Historical minute bars are getting fetched!\n";
                }
                historicalDataClient.GetHistoricalDataBars(new TimeSpan(0, 23, 59, 59));

                while ((this.Bars.Count < historicalDataClient.totalHistoricalBars ||
                       historicalDataClient.totalHistoricalBars == 0) && this.RunThread)
                {
                    System.Threading.Thread.Sleep(100);
                }

                historicalDataClient.Disconnect();
            }
        }

        public void Start()
        {
            this.Thread.Start();
        }

        public void StopTrading()
        {
            this.isStopTrading = true;
        }

        public void StopTradingAfterSignal()
        {
            this.isStopTradingAfterSignal = true;
        }
    }
}