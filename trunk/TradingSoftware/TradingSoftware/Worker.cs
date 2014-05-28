using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;

namespace TradingSoftware
{
    [Serializable()]
    public class Worker : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool _isTrading;

        [DisplayName("Is trading?")]
        public bool IsTrading
        {
            get { return _isTrading; }
            set
            {
                _isTrading = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsTrading"));
            }
        }

        private Contract _equity;

        [DisplayName("Symbol")]
        public string Equity
        {
            get { return _equity.Symbol; }

            //set
            //{
            //    _equity = new Equity(value);
            //    if (PropertyChanged != null)
            //        PropertyChanged(this, new PropertyChangedEventArgs("Equity"));
            //}
        }

        private decimal _amount;

        [DisplayName("Invested amount")]
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Amount"));
            }
        }

        private BarSize _barsize;

        [DisplayName("Barsize")]
        public string Barsize
        {
            get { return _barsize.ToString(); }
            set
            {
                if (value.Equals("mBar") || value.Equals("Minute"))
                {
                    _barsize = BarSize.OneMinute;
                }
                else if (value.Equals("dBar") || value.Equals("Daily"))
                {
                    _barsize = BarSize.OneDay;
                }

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Barsize"));
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

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("DataType"));
            }
        }

        public decimal _pricePremiumPercentage;

        [DisplayName("Price premium [%]")]
        public decimal PricePremiumPercentage
        {
            get { return _pricePremiumPercentage; }
            set
            {
                _pricePremiumPercentage = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("PricePremiumPercentage"));
            }
        }

        public int _roundLotSize;

        [DisplayName("Round lot size")]
        public int RoundLotSize
        {
            get { return _roundLotSize; }
            set
            {
                _roundLotSize = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("RoundLotSize"));
            }
        }

        public int _currentPosition;

        [DisplayName("Cur. Position")]
        public int CurrentPosition
        {
            get { return _currentPosition; }
            set
            {
                _currentPosition = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentPosition"));
            }
        }

        public bool _isFutureTrading;

        [DisplayName("FutureTrading")]
        public bool IsFutureTrading
        {
            get { return _isFutureTrading; }
            set
            {
                _isFutureTrading = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsFutureTrading"));
            }
        }

        public bool _shallIgnoreFirstSignal;

        [DisplayName("Shall Ignore First Signal")]
        public bool ShallIgnoreFirstSignal
        {
            get { return _shallIgnoreFirstSignal; }
            set
            {
                _shallIgnoreFirstSignal = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ShallIgnoreFirstSignal"));
            }
        }

        private MainViewModel mainViewModel;

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

        public Worker(MainViewModel mainViewModel, Equity equity, bool isTrading, decimal amount, string barsize, string dataType,
                      decimal pricePremiumPercentage, int roundLotSize, bool isFutureTrading, int currentPosition, bool shallIgnoreFirstSignal)
        {
            this.mainViewModel = mainViewModel;

            this.Bars = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
            this.Signals = new List<int>();

            this.IsTrading = isTrading;

            if (isFutureTrading)
            {
                this._equity = ConvertToFutures(equity.Symbol);
            }
            else
            {
                this._equity = equity;
            }

            this.Amount = amount;
            this.Barsize = barsize;
            this.DataType = dataType;
            this.PricePremiumPercentage = pricePremiumPercentage;
            this.RoundLotSize = roundLotSize;

            this.didFirst = false;
            this.hasFirstSignalPassed = false;
            this.CurrentPosition = currentPosition;
            this.IsFutureTrading = isFutureTrading;
            this.ShallIgnoreFirstSignal = shallIgnoreFirstSignal;

            this.lastIgnoredSignal = -100;

            this.Thread = new Thread(this.Run);
        }

        public Future ConvertToFutures(string input)
        {
            string expiry = "201" + input.ElementAt(3);
            switch ((input.ElementAt(2) + "").ToUpper())
            {
                case ("Z"):
                    expiry += "12";
                    break;

                case ("Q"):
                    expiry += "09";
                    break;

                case ("H"):
                    expiry += "03";
                    break;

                case ("M"):
                    expiry += "06";
                    break;

                default:
                    break;
            }

            //
            return new Future(input.ElementAt(0) + "" + input.ElementAt(1), "GLOBEX", expiry);
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
                    while (length == this.Bars.Count && this.RunThread && !this.isStopTrading)
                    {
                        Thread.Sleep(1000);
                    }

                    //Calculate the decision
                    Algorithm.DecisionCalculator.startCalculation(Bars, Signals, new Dictionary<string, List<decimal>>(), new Dictionary<string, List<decimal>>());

                    //For testing purposes
                    //this.Signals[this.Signals.Count - 2] = 0;
                    //this.Signals[this.Signals.Count - 1] = 3;
                    //if (this.didFirst)
                    //    this.Signals[this.Signals.Count - 1] = 1;

                    //Stop if isStopTradingAfterSignal is true and the signal is finished
                    if (this.isStopTradingAfterSignal &&
                       (Math.Sign(this.Signals[this.Signals.Count - 1]) != Math.Sign(this.CurrentPosition) ||
                        Math.Sign(this.Signals[this.Signals.Count - 1]) == 0 ||
                        this.CurrentPosition == 0))
                    {
                        this.isStopTrading = true;
                    }

                    if (this.isStopTrading)
                    {
                        this.Signals[this.Signals.Count - 1] = 0;
                    }

                    lock (IBID.ConsoleTextLock)
                    {
                        this.mainViewModel.SignalText += this.Equity + ": Current Signal: " + this.Bars.Last().Item1.ToString() + " ... " + Signals.Last() + "\n";
                    }

                    length = this.Bars.Count;
                }
                if (IsTrading)
                {
                    if (this.Signals[this.Signals.Count - 1] == 0)
                    {
                        //just for testing purposes, but didFirst is generally almost only for testing purposes so it can stay like that
                        this.didFirst = true;

                        this.hasFirstSignalPassed = true;
                    }

                    if (((this.Signals[this.Signals.Count - 1] != this._currentPosition) || this.shallReenter)
                        && (this.lastIgnoredSignal != this.Signals[this.Signals.Count - 1]))
                    {
                        if (this.lastIgnoredSignal != -100)
                        {
                            this.lastIgnoredSignal = -100;
                        }

                        if ((this.didFirst && this.ShallIgnoreFirstSignal && (this.Signals[this.Signals.Count - 1] != this.Signals[this.Signals.Count - 2])))
                        {
                            this.hasFirstSignalPassed = true;
                        }

                        if (this.hasFirstSignalPassed || !this.ShallIgnoreFirstSignal)
                        {
                            int oldSignal = _currentPosition;

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
                                dialogResult = AutoClosingMessageBox.Show(this.Equity + ": New Signal is " + newSignal + ".\n Would you like to ignore the order?", "New Signal", 5000, MessageBoxButton.OKCancel);
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
                                    int two = 2; //(int)(((this.Amount / roundLotPrice) * 2m) / 3m);
                                    int three = 3; //(int)(this.Amount / roundLotPrice);

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

                                    if (amountToZero != 0 && IsTrading)
                                    {
                                        // iboutput place and execute
                                        if (isBuy)
                                            this.IBOutput.placeOrder(ActionSide.Buy, (amountToZero + amountFromZero) * ((this._isFutureTrading) ? 1 : this.RoundLotSize), PricePremiumPercentage);
                                        else if (!isBuy)
                                            this.IBOutput.placeOrder(ActionSide.Sell, (amountToZero + amountFromZero) * ((this._isFutureTrading) ? 1 : this.RoundLotSize), PricePremiumPercentage);

                                        this.CurrentPosition = newSignal;
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
                        this.IsTrading = false;
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
            this.realTimeDataClient = new IBInput(this.mainViewModel, this.Bars, this._equity, BarSize.OneMinute, _isFutureTrading);

            this.realTimeDataClient.Connect();

            lock (IBID.ConsoleTextLock)
            {
                this.mainViewModel.ConsoleText += this.Equity + ": Start receiving realtime bars...\n";
            }
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
                var historicalDataClient = new IBInput(this.mainViewModel, this.Bars, this._equity, BarSize.OneMinute, _isFutureTrading);
                historicalDataClient.Connect();

                //request historical data bars
                lock (IBID.ConsoleTextLock)
                {
                    this.mainViewModel.ConsoleText += this.Equity + ": Please wait... Historical minute bars are getting fetched!\n";
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