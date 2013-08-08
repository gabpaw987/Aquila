using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;

namespace TradingSoftware
{
    [Serializable()]
    public class Worker
    {
        public bool _isCalculating;

        [DisplayName("Is calculating?")]
        public bool IsCalculating
        {
            get { return _isCalculating; }
            set { _isCalculating = value; }
        }

        private Equity _equity;

        [DisplayName("Symbol")]
        public string Equity
        {
            get { return _equity.Symbol; }
            set { _equity = new Equity(value); }
        }

        private bool _isActive;

        [DisplayName("Is active?")]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
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

        public decimal _cutLoss;

        [DisplayName("CutLoss")]
        public decimal CutLoss
        {
            get { return _cutLoss; }
            set { _cutLoss = value; }
        }

        private List<Tuple<DateTime, decimal, decimal, decimal, decimal>> Bars;
        private List<int> Signals;
        private Thread Thread;

        private IBOutput IBOutput;

        // to stop the thread from the main method
        public bool RunThread = true;

        private bool didFirst;

        private int roundLotSize;

        public Worker(Equity equity, bool isActive, decimal amount, string barsize,
                      string dataType, decimal pricePremiumPercentage, decimal cutLoss, int roundLotSize)
        {
            this.Bars = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
            this.Signals = new List<int>();

            this.IsCalculating = false;

            this._equity = equity;
            this.IsActive = isActive;
            this.Amount = amount;
            this.Barsize = barsize;
            this.DataType = dataType;
            this.PricePremiumPercentage = pricePremiumPercentage;
            this.CutLoss = cutLoss;
            this.roundLotSize = roundLotSize;

            this.didFirst = false;

            this.Thread = new Thread(this.Run);
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
                    while (length == this.Bars.Count)
                    {
                        Thread.Sleep(1000);
                        if (!this.IsCalculating)
                        {
                            break;
                        }
                    }

                    //Calculate the decision
                    Algorithm.DecisionCalculator.startCalculation(Bars, Signals);

                    Console.WriteLine("Current Signal: " + Signals.Last());

                    length = this.Bars.Count;
                }
                if (IsCalculating)
                {
                    if ((Signals[Signals.Count - 1] != Signals[Signals.Count - 2]) || !this.didFirst)
                    {
                        int oldSignal = 0;

                        if (this.didFirst)
                        {
                            oldSignal = Signals[Signals.Count - 2];
                        }

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

                        this.IBOutput = new IBOutput(this._equity);

                        this.IBOutput.RequestTickPrice();

                        decimal roundLotPrice = 0m;

                        //DONE: peer wegen pricepremium
                        if (isBuy)
                        {
                            roundLotPrice = this.IBOutput.currentAskPrice * this.roundLotSize;
                        }
                        else
                        {
                            roundLotPrice = this.IBOutput.currentBidPrice * this.roundLotSize;
                        }

                        int one = (int)((this.Amount / roundLotPrice) / 3m);
                        int two = (int)(((this.Amount / roundLotPrice) * 2m) / 3m);
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
                        if (amountToZero != 0 && IsCalculating)
                        {
                            // iboutput place and execute
                            if (isBuy)
                                this.IBOutput.placeOrder(ActionSide.Buy, (amountToZero + amountFromZero) * this.roundLotSize);
                            else if (!isBuy)
                                this.IBOutput.placeOrder(ActionSide.Sell, (amountToZero + amountFromZero) * this.roundLotSize);

                            if (IsActive)
                            {
                                this.IBOutput.executeOrder(PricePremiumPercentage);
                            }
                            else
                            {
                                //TODO: do something if not worker active
                            }
                        }
                    }
                }
            }
        }

        public void loadHistoricalData()
        {
            var realTimeDataClient = new IBInput(this.Bars, this._equity, BarSize.OneMinute);
            var historicalDataClient = new IBInput(this.Bars, this._equity, BarSize.OneMinute);

            realTimeDataClient.Connect();

            Console.WriteLine("Start receiving realtime bars");
            realTimeDataClient.SubscribeForRealTimeBars();

            //Wait for first 5sec bar
            while (realTimeDataClient.RealTimeBarList.Count <= 1)
            {
                System.Threading.Thread.Sleep(100);
            }

            //wait until minute is full
            if (_barsize.Equals(BarSize.OneMinute))
            {
                while (realTimeDataClient.RealTimeBarList.Count != 0)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            historicalDataClient.Connect();

            //request historical data bars
            Console.WriteLine("Please wait... Historical minute bars are getting fetched!");
            historicalDataClient.GetHistoricalDataBars(new TimeSpan(0, 23, 59, 59));

            while (this.Bars.Count < historicalDataClient.totalHistoricalBars ||
                   historicalDataClient.totalHistoricalBars == 0)
            {
                System.Threading.Thread.Sleep(100);
            }

            historicalDataClient.Disconnect();
        }

        public void Start()
        {
            this.Thread.Start();
        }
    }
}