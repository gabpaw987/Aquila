using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;

namespace TradingSoftware
{
    [Serializable()]
    public class Worker
    {
        public bool _isTrading;

        [DisplayName("Is trading?")]
        public bool IsTrading
        {
            get { return _isTrading; }
            set { _isTrading = value; }
        }

        private Equity _equity;

        [DisplayName("Symbol")]
        public string Equity
        {
            get { return _equity.Symbol; }
            set { _equity = new Equity(value); }
        }

        private decimal _amount;

        [DisplayName("Invested amount")]
        public decimal Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        private BarSize _barsize;

        [DisplayName("Barsize")]
        public string Barsize
        {
            get { return _barsize.ToString(); }
            set
            {
                if (value.Equals("mBar"))
                {
                    _barsize = BarSize.OneMinute;
                }
                else if (value.Equals("dBar"))
                {
                    _barsize = BarSize.OneDay;
                }
            }
        }

        public HistoricalDataType _historicalType;
        public RealTimeBarType _realtimeType;

        [DisplayName("Data type")]
        public string DataType
        {
            get
            {
                if (_historicalType.ToString().Equals(_realtimeType.ToString()))
                    return _historicalType.ToString();
                else
                    return "inconsistent";
            }
            set
            {
                if (value.Equals("Bid"))
                {
                    _historicalType = HistoricalDataType.Bid;
                    _realtimeType = RealTimeBarType.Bid;
                }
                else if (value.Equals("Ask"))
                {
                    _historicalType = HistoricalDataType.Ask;
                    _realtimeType = RealTimeBarType.Ask;
                }
                else if (value.Equals("Last") || value.Equals("Trades"))
                {
                    _historicalType = HistoricalDataType.Trades;
                    _realtimeType = RealTimeBarType.Trades;
                }
                else if (value.Equals("Midpoint"))
                {
                    _historicalType = HistoricalDataType.Midpoint;
                    _realtimeType = RealTimeBarType.Midpoint;
                }
            }
        }

        public decimal _pricePremiumPercentage;

        [DisplayName("Price premium [%]")]
        public decimal PricePremiumPercentage
        {
            get { return _pricePremiumPercentage; }
            set { _pricePremiumPercentage = value; }
        }

        public int _roundLotSize;

        [DisplayName("Round lot size")]
        public int RoundLotSize
        {
            get { return _roundLotSize; }
            set { _roundLotSize = value; }
        }

        public int _currentPosition;

        [DisplayName("Cur. Position")]
        public int CurrentPosition
        {
            get { return _currentPosition; }
            set { _currentPosition = value; }
        }

        private MainViewModel mainViewModel;

        private List<Tuple<DateTime, decimal, decimal, decimal, decimal>> Bars;
        private List<int> Signals;
        private Thread Thread;

        private IBOutput IBOutput;

        // to stop the thread from the main method
        public bool RunThread = true;

        private bool didFirst;

        private IBInput realTimeDataClient;

        public Worker(MainViewModel mainViewModel, Equity equity, bool isTrading, decimal amount,
                      string barsize, string dataType, decimal pricePremiumPercentage, int roundLotSize)
        {
            this.mainViewModel = mainViewModel;

            this.Bars = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
            this.Signals = new List<int>();

            this.IsTrading = isTrading;

            this._equity = equity;
            this.Amount = amount;
            this.Barsize = barsize;
            this.DataType = dataType;
            this.PricePremiumPercentage = pricePremiumPercentage;
            this.RoundLotSize = roundLotSize;

            this.didFirst = false;
            this._currentPosition = 0;

            this.Thread = new Thread(this.Run);
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
                //DONE: wait till new bar is ready
                if (_barsize.Equals(BarSize.OneMinute))
                {
                    while (length == this.Bars.Count && this.RunThread)
                    {
                        Thread.Sleep(1000);
                    }

                    //Calculate the decision
                    Algorithm.DecisionCalculator.startCalculation(Bars, Signals);

                    this.mainViewModel.SignalText += "Current Signal: " + this.Bars.Last().Item1.ToString() + " ... " + Signals.Last() + "\n";

                    length = this.Bars.Count;
                }
                if (IsTrading)
                {
                    if ((Signals[Signals.Count - 1] != Signals[Signals.Count - 2]) || !this.didFirst)
                    {
                        this.didFirst = true;
                        int oldSignal = 0;

                        if (this.didFirst)
                        {
                            oldSignal = _currentPosition;
                        }

                        int newSignal = Signals[Signals.Count - 1];

                        MessageBoxResult dialogResult = AutoClosingMessageBox.Show("New Signal is " + newSignal + ".\n Would you like to ignore the order?", "New Signal", 5000, MessageBoxButton.OKCancel);
                        if (dialogResult == MessageBoxResult.Cancel)
                        {
                            if (_currentPosition != newSignal)
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

                                this.IBOutput = new IBOutput(this.mainViewModel, this._equity);
                                this.IBOutput.Connect();

                                this.IBOutput.RequestTickPrice();

                                decimal roundLotPrice = 0m;

                                if (isBuy)
                                {
                                    roundLotPrice = this.IBOutput.currentAskPrice * this.RoundLotSize;
                                }
                                else
                                {
                                    roundLotPrice = this.IBOutput.currentBidPrice * this.RoundLotSize;
                                }

                                int one = 1; //(int)((this.Amount / roundLotPrice) / 3m);

                                //Not used at the moment
                                int two = (int)(((this.Amount / roundLotPrice) * 2m) / 3m);
                                int three = (int)(this.Amount / roundLotPrice);

                                int amountToZero = 0;
                                switch (toZero)
                                {
                                    case 1:
                                    case -1: { amountToZero = one; break; }

                                    //Not used at the moment
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

                                        //Not used at the moment
                                        case 2:
                                        case -2: { amountFromZero = two; break; }
                                        case 3:
                                        case -3: { amountFromZero = three; break; }
                                    }
                                }

                                if (amountToZero != 0 && IsTrading)
                                {
                                    // iboutput place and execute
                                    if (isBuy)
                                        this.IBOutput.placeOrder(ActionSide.Buy, (amountToZero + amountFromZero) * this.RoundLotSize, PricePremiumPercentage);
                                    else if (!isBuy)
                                        this.IBOutput.placeOrder(ActionSide.Sell, (amountToZero + amountFromZero) * this.RoundLotSize, PricePremiumPercentage);

                                    this._currentPosition = newSignal;
                                    this.IBOutput.Disconnect();
                                }
                            }
                        }
                    }
                }
            }
        }

        public void loadHistoricalData()
        {
            this.realTimeDataClient = new IBInput(this.mainViewModel, this.Bars, this._equity, BarSize.OneMinute);

            this.realTimeDataClient.Connect();

            this.mainViewModel.ConsoleText += "Start receiving realtime bars...\n";
            this.realTimeDataClient.SubscribeForRealTimeBars();

            //Wait for first 5sec bar
            while (this.realTimeDataClient.RealTimeBarList.Count <= 1 && this.RunThread)
            {
                System.Threading.Thread.Sleep(100);
            }

            //wait until minute is full
            if (_barsize.Equals(BarSize.OneMinute))
            {
                while (this.realTimeDataClient.RealTimeBarList.Count != 0 && this.RunThread)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }

            if (RunThread)
            {
                var historicalDataClient = new IBInput(this.mainViewModel, this.Bars, this._equity, BarSize.OneMinute);
                historicalDataClient.Connect();

                //request historical data bars
                this.mainViewModel.ConsoleText += "Please wait... Historical minute bars are getting fetched!\n";
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
    }
}