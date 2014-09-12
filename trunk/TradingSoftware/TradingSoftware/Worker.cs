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
using System.Reflection;

namespace TradingSoftware
{
    [Serializable()]
    public class Worker
    {
        private MainViewModel mainViewModel;
        public WorkerViewModel workerViewModel;

        private List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>> Bars;
        private List<int> Signals;
        private Thread Thread;

        private IBOutput IBOutput;

        // to stop the thread from the main method
        public bool RunThread = false;

        //To stop the trading and close all current positions
        private bool isStopTrading = false;
        //To let the current signal run through and stop trading after that
        private bool isStopTradingAfterSignal = false;

        public bool didFirst;
        public bool hasFirstSignalPassed;

        private IBInput realTimeDataClient;

        private Type algorithmType;

        public Worker(MainViewModel mainViewModel, WorkerViewModel workerViewModel, string equity, string exchange, bool isTrading,
                      string barsize, string dataType, decimal pricePremiumPercentage, int roundLotSize, bool isFutureTrading,
                      int currentPosition, bool shallIgnoreFirstSignal, bool hasAlgorithmParameters, string algorithmFilePath,
                      string algorithmParameters)
        {
            this.mainViewModel = mainViewModel;
            this.workerViewModel = workerViewModel;

            this.Bars = new List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>>();
            this.Signals = new List<int>();

            this.workerViewModel.Exchange = exchange;

            //Just so equity knows how to convert the symbol
            this.workerViewModel._isFutureTrading = isFutureTrading;

            this.workerViewModel.EquityAsString = equity;
            this.workerViewModel.IsFutureTrading = isFutureTrading;
            this.workerViewModel.IsTrading = isTrading;
            this.workerViewModel.BarsizeAsString = barsize;
            this.workerViewModel.DataType = dataType;
            this.workerViewModel.PricePremiumPercentage = pricePremiumPercentage;
            if (!this.workerViewModel.IsFutureTrading)
            {
                this.workerViewModel.RoundLotSize = roundLotSize;
            }
            this.didFirst = false;
            this.hasFirstSignalPassed = false;
            this.workerViewModel.CurrentPosition = currentPosition;
            this.workerViewModel.ShallIgnoreFirstSignal = shallIgnoreFirstSignal;
            this.workerViewModel.AlgorithmFilePath = algorithmFilePath;
            this.workerViewModel._parsedAlgorithmParameters = new Dictionary<string, decimal>();
            this.workerViewModel.HasAlgorithmParameters = hasAlgorithmParameters;
            this.workerViewModel.AlgorithmParameters = algorithmParameters;
        }

        public void Stop(bool WriteToConsole)
        {
            if (this.RunThread)
            {
                this.RunThread = false;
                if (this.realTimeDataClient != null)
                {
                    this.realTimeDataClient.Disconnect();
                }

                while (this.Thread.IsAlive)
                {
                    Thread.Sleep(200);
                }

                this.workerViewModel.IsThreadRunning = false;

                this.Bars.Clear();
                this.Signals.Clear();
                this.isStopTrading = false;
                this.isStopTradingAfterSignal = false;
                this.didFirst = false;
                this.hasFirstSignalPassed = false;

                if (WriteToConsole)
                {
                    this.workerViewModel.ConsoleText += this.workerViewModel.EquityAsString + ": Worker stopped.\n";
                    this.workerViewModel.SignalText += this.workerViewModel.EquityAsString + ": Worker stopped.\n";
                }
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

                    if (this.RunThread)
                    {
                        //Load algorithm
                        this.algorithmType = this.LoadAlgorithmFile();

                        if (this.algorithmType != null)
                        {
                            int noOfBarsGivenToAlgorithm = IBID.NoOfBarsGivenToAlgorithm;
                            List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>> BarsGivenToAlgorithm;
                            if (noOfBarsGivenToAlgorithm != 0 && noOfBarsGivenToAlgorithm < this.Bars.Count)
                            {
                                BarsGivenToAlgorithm = this.Bars.GetRange(this.Bars.Count - noOfBarsGivenToAlgorithm, noOfBarsGivenToAlgorithm);
                            }
                            else
                            {
                                BarsGivenToAlgorithm = this.Bars;
                            }

                            if (this.workerViewModel.HasAlgorithmParameters)
                            {
                                Dictionary<string, decimal> tempParsedAlgorithmParameters = this.workerViewModel.parseAlgorithmParameters(this.workerViewModel.AlgorithmParameters);
                                Object[] oa = { BarsGivenToAlgorithm, Signals, new Dictionary<string, List<decimal>>(), new Dictionary<string, List<decimal>>(), tempParsedAlgorithmParameters };
                                this.algorithmType.GetMethod("startCalculation").Invoke(null, oa);

                                if (tempParsedAlgorithmParameters != this.workerViewModel.ParsedAlgorithmParameters)
                                {
                                    this.workerViewModel.ParsedAlgorithmParameters = tempParsedAlgorithmParameters;
                                }
                            }
                            else
                            {
                                Object[] oa = { BarsGivenToAlgorithm, Signals, new Dictionary<string, List<decimal>>(), new Dictionary<string, List<decimal>>() };
                                this.algorithmType.GetMethod("startCalculation").Invoke(null, oa);
                            }
                        }
                        else
                        {
                            this.workerViewModel.ConsoleText += this.workerViewModel.EquityAsString + ": Error while loading algorithm.\n";
                        }

                        this.algorithmType = null;

                        //For testing purposes
                        //this.Signals[this.Signals.Count - 2] = 0;
                        //this.Signals[this.Signals.Count - 1] = 3;
                        //if (this.didFirst)
                        //    this.Signals[this.Signals.Count - 1] = -1;

                        //Stop if isStopTradingAfterSignal is true and the signal is finished
                        if (this.isStopTradingAfterSignal &&
                           ((Math.Sign(this.Signals[this.Signals.Count - 1]) != Math.Sign(this.workerViewModel.CurrentPosition)) ||
                            (Math.Sign(this.Signals[this.Signals.Count - 1]) == 0) ||
                            (this.workerViewModel.CurrentPosition == 0)))
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

                        if (((this.Signals[this.Signals.Count - 1] != this.workerViewModel.CurrentPosition)))
                        {
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
                                    //Ensure that only one order is placed at a time
                                    Thread.Sleep(200);

                                    SoundPlayer sound = new SoundPlayer(@"../../sounds/BIGHORN.wav");
                                    sound.Play();
                                }

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

                                    int amountToZero = Math.Abs(toZero);
                                    int amountFromZero = Math.Abs(fromZero);

                                    if (amountToZero != 0 && this.workerViewModel.IsTrading)
                                    {
                                        if (isBuy)
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

                while ((historicalDataClient.noOfHistoricalBarsReceived < historicalDataClient.totalHistoricalBars ||
                       historicalDataClient.totalHistoricalBars == 0) && this.RunThread)
                {
                    System.Threading.Thread.Sleep(100);
                }

                historicalDataClient.Disconnect();
            }
        }

        public Type LoadAlgorithmFile()
        {
            string absoluteAlgorithmFilePath = "";

            if (Path.IsPathRooted(this.workerViewModel.AlgorithmFilePath))
            {
                absoluteAlgorithmFilePath = this.workerViewModel.AlgorithmFilePath;
            }
            else
            {
                absoluteAlgorithmFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), this.workerViewModel.AlgorithmFilePath);
                absoluteAlgorithmFilePath = Path.GetFullPath((new Uri(absoluteAlgorithmFilePath)).LocalPath);
            }

            Assembly assembly = Assembly.LoadFile(absoluteAlgorithmFilePath);
            AppDomain.CurrentDomain.Load(assembly.GetName());
            return assembly.GetType("Algorithm.DecisionCalculator");
        }

        public void Start(bool WriteToConsole)
        {
            if (!this.RunThread)
            {
                this.Thread = new Thread(this.Run);
                this.Thread.Start();

                this.workerViewModel.IsThreadRunning = true;

                this.RunThread = true;

                if (WriteToConsole)
                {
                    this.workerViewModel.ConsoleText += this.workerViewModel.EquityAsString + ": Worker Started.\n";
                    this.workerViewModel.SignalText += this.workerViewModel.EquityAsString + ": Worker Started.\n";
                }
            }
        }

        public bool IsRunning()
        {
            if (this.Thread != null)
            {
                return this.Thread.IsAlive;
            }
            else
            {
                return false;
            }
        }

        public void StopTrading()
        {
            if (this.workerViewModel.IsTrading && this.workerViewModel.CurrentPosition != 0)
            {
                this.isStopTrading = true;
            }
        }

        public void StopTradingAfterSignal()
        {
            if (this.workerViewModel.IsTrading && this.workerViewModel.CurrentPosition != 0)
            {
                if (this.isStopTradingAfterSignal)
                {
                    this.isStopTradingAfterSignal = false;
                }
                else
                {
                    this.isStopTradingAfterSignal = true;
                }
            }
        }

        public bool doesStopTradingAfterSignal()
        {
            return this.isStopTradingAfterSignal;
        }
    }
}