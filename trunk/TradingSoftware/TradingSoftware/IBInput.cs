using System;
using System.Collections.Generic;
using System.Linq;
using Krs.Ats.IBNet;
using MoreLinq;

namespace TradingSoftware
{
    /// <summary>
    /// This class is used to get all the Input data for the specified Equity. This contains the histrocial data in bars for the last 23 hours 59 minutes 59 seconds<br/>
    /// because this is the maximal count of minute bars available from InteractiveBrokers and at the time this software only supports minute bars.<br/>
    /// This class also requests 5-Second Realtime-Bars from InteractiveBrokers and collects 12 of them and converts them to a new minute bar. These minute bars<br/>
    /// are appended to the ListOfBars so that the other classes always have the realtime bars to calculate their decision.
    /// </summary>
    /// <remarks></remarks>
    internal class IBInput
    {
        public bool IsConnected { get; set; }

        /// <summary>
        /// A List were all bars that are in the application at the moment, are saved.
        /// </summary>
        private List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>> ListOfBars;

        /// <summary>
        /// The client that handles all the input connections. In the constructor this one is connected to the IB with the id 0.<br/>
        /// It can be identified by this id and no more other clients can connect to IB with the same id. This object represents the whole connection<br/>
        /// to the InteractiveBrokers API and for example all the bars are requested over it.
        /// </summary>
        private IBClient inputClient;

        private bool IsFuture;

        /// <summary>
        /// Gets the total number of historical data bars.
        /// </summary>
        /// <remarks></remarks>
        public int totalHistoricalBars { get; private set; }

        /// <summary>
        /// Gets the equity. This is the Equity the whole IBInput class is getting the data for.
        /// </summary>
        /// <remarks></remarks>
        public Contract Equity { get; private set; }

        /// <summary>
        /// In this list the received realtime bars are saved. Whenever a new one is received, it a method checks if there are already 12 bars in the list.<br/>
        /// If this is the case, the CreateMinuteBar()-method is called to create a minute-bar out of the 12 5-second bars and this minute bar is added to the<br/>
        /// ListOfBars-list. Also when this happens the RealTimeBarList gets cleared to hold 12 new bars later on.
        /// </summary>
        public List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>> RealTimeBarList;

        public BarSize Barsize { get; private set; }

        public bool hadFirst { get; set; }

        public CSVWriter csvWriter { get; set; }

        public int noOfHistoricalBarsReceived { get; set; }

        private WorkerViewModel workerViewModel;

        /// <summary>
        /// When this method is called, the HistrocialData bars are requested. After the request the client_HistoricalData event is called every time a bar<br/>
        /// arrives.
        /// </summary>
        /// <remarks></remarks>
        public void GetHistoricalDataBars(TimeSpan timeSpan)
        {
            //inputClient.RequestHistoricalData(IBID.TickerID++,this.Equity, DateTime.Now, timeSpan,Barsize, HistoricalDataType.Trades,1)
            lock (IBID.TickerLock)
            {
                if (this.Barsize == BarSize.OneMinute)
                    inputClient.RequestHistoricalData(IBID.TickerID++, this.Equity, DateTime.Now, timeSpan, Barsize, HistoricalDataType.Trades, (this.IsFuture) ? 0 : 1);
                else if (this.Barsize == BarSize.OneDay)
                    inputClient.RequestHistoricalData(IBID.TickerID++, this.Equity, DateTime.Now, timeSpan, Barsize, HistoricalDataType.Trades, (this.IsFuture) ? 0 : 1);
            }
        }

        /// <summary>
        /// When this method is called, realtime bars are getting requested. That means an event is running that calls the client_RealTimeBar method every five seconds<br/>
        /// when a new 5-second bar is available.
        /// </summary>
        /// <remarks></remarks>
        public void SubscribeForRealTimeBars()
        {
            lock (IBID.TickerLock)
            {
                inputClient.RequestRealTimeBars(IBID.TickerID++, this.Equity, 5, RealTimeBarType.Trades, (this.IsFuture) ? false : true);
            }
        }

        /// <summary>
        /// Creates a minute bar. This method takes all 5-second bar of the RealTimeBarList-list and searches for the right ones to create a minute bar out of them.<br/>
        /// Once the minute bar is created it gets returned.
        /// </summary>
        /// <returns>The created minute Bar.</returns>
        /// <remarks></remarks>
        private Tuple<DateTime, decimal, decimal, decimal, decimal, long> AggregateBar()
        {
            //First open value in the list
            decimal open = RealTimeBarList[0].Item2;

            //The highest "high" value in the RealTimeBarList
            decimal high = RealTimeBarList.MaxBy(x => x.Item3).Item3;

            //The lowest "low" value in the RealTimeBarList
            decimal low = RealTimeBarList.MinBy(x => x.Item4).Item4;

            //The Last close value in the list
            decimal close = RealTimeBarList[RealTimeBarList.ToArray().Length - 1].Item5;
            
            //sum all volumes of the 5 sec bars
            long volume = RealTimeBarList.Sum(x => x.Item6);

            //creates the bar with these values and returns it
            return new Tuple<DateTime, decimal, decimal, decimal, decimal, long>(RealTimeBarList.First().Item1, open, high, low, close, volume);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IBInput"/> class. In addition to that it creates a new IBClient Object for the input-operations<br/>
        /// that is also connected to the InterActiveBrokers API with the id 0 on the ip "127.0.0.1" and the port 7496. This ip-adres has to be a trusted one in<br/>
        /// your InteractiveBrokers TWS installation in order to make realtime trading possible.(this can be specified in settings->api)
        /// </summary>
        /// <param name="LOB">The reference for the ListOfBars-list. This one is needed to know where to save the received bars in a way all the other classes<br/>
        /// can connect to it.</param>
        /// <param name="equity">The equity this class shall represent.</param>
        /// <remarks></remarks>
        public IBInput(WorkerViewModel workerViewModel, List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>> LOB, Contract equity, BarSize barsize, bool isFuture)
        {
            this.workerViewModel = workerViewModel;

            ListOfBars = LOB;

            this.Barsize = barsize;

            //creating the IBClient
            inputClient = new IBClient();
            inputClient.ThrowExceptions = true;

            RealTimeBarList = new List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>>();
            this.Equity = equity;
            this.hadFirst = false;
            this.IsConnected = false;
            this.IsFuture = isFuture;

            this.noOfHistoricalBarsReceived = 0;

            this.csvWriter = new CSVWriter(this.workerViewModel);
            this.ListOfBars.AddRange(this.csvWriter.CreateOrReadCSV());
        }

        /// <summary>
        /// Handles the RealTimeBar event of the IBClient. This method parses the bars that arrived when this method got triggered(5-second bars) and<br/>
        /// saves them to the RealTimeBarList. When there are 12 bars in the list, the CreateMinuteBar method is called and the RealTimeBarList gets cleared<br/>
        /// to be ready for new bars.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Krs.Ats.IBNet.RealTimeBarEventArgs"/> instance containing the event data.</param>
        /// <remarks></remarks>
        private void client_RealTimeBar(object sender, RealTimeBarEventArgs e)
        {
            if (this.IsConnected)
            {
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                dtDateTime = dtDateTime.AddSeconds(e.Time).ToLocalTime();

<<<<<<< HEAD
                bool receivedLessBarsThisTimePeriod = false;
                if (RealTimeBarList.Count != 0)
                {
                    if (this.Barsize == BarSize.OneMinute)
                    {
                        if (dtDateTime.Minute != RealTimeBarList.First().Item1.Minute)
                        {
                            receivedLessBarsThisTimePeriod = true;
                        }
                    }
                    else if (this.Barsize == BarSize.OneDay)
                    {
                        if (dtDateTime.Day != RealTimeBarList.First().Item1.Day)
                        {
                            receivedLessBarsThisTimePeriod = true;
                        }
                    }
                }

                if (!receivedLessBarsThisTimePeriod)
                {
                    RealTimeBarList.Add(new Tuple<DateTime, decimal, decimal, decimal, decimal, long>(dtDateTime, e.Open, e.High, e.Low, e.Close, e.Volume));
                }
=======
                RealTimeBarList.Add(new Tuple<DateTime, decimal, decimal, decimal, decimal, long>(dtDateTime, e.Open, e.High, e.Low, e.Close, e.Volume));
>>>>>>> 9808b6b... Updated TS to use volume
                Tuple<DateTime, decimal, decimal, decimal, decimal, long> b = null;

                if (this.hadFirst)
                {
                    //When we got 12 bars in the RealTimeBarList create a minute bar
                    //TODO: the 4680 only make a day if started in the morning
                    if ((RealTimeBarList.ToArray().Length >= 12 && this.Barsize == BarSize.OneMinute) ||
                        (RealTimeBarList.ToArray().Length >= 4680 && this.Barsize == BarSize.OneDay) ||
                        receivedLessBarsThisTimePeriod)
                    {
                        b = AggregateBar();
                        RealTimeBarList = new List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>>();
                        lock (IBID.ConsoleTextLock)
                        {
                            this.workerViewModel.ConsoleText += this.workerViewModel.EquityAsString + ": Real-time-Bar: " + b.Item1 + ", " + b.Item2 + ", " + b.Item3 + ", " + b.Item4 + ", " + b.Item5 + ", " + b.Item6 + "\n";
                        }

                        this.ListOfBars.Add(b);
                        this.csvWriter.WriteBar(b);
<<<<<<< HEAD

                        if (receivedLessBarsThisTimePeriod)
                        {
                            RealTimeBarList.Add(new Tuple<DateTime, decimal, decimal, decimal, decimal, long>(dtDateTime, e.Open, e.High, e.Low, e.Close, e.Volume));
                        }
=======
>>>>>>> 9808b6b... Updated TS to use volume
                    }
                }
                else
                {
                    if (this.Barsize.Equals(BarSize.OneMinute))
                    {
                        if (this.RealTimeBarList.Last().Item1.Second == 55)
                        {
                            this.RealTimeBarList.Clear();
                            this.hadFirst = true;
                        }
                    }
                    else if (this.Barsize.Equals(BarSize.OneDay))
                    {
                        if (this.RealTimeBarList.Last().Item1.Hour == 23 &&
                            this.RealTimeBarList.Last().Item1.Minute == 59 &&
                            this.RealTimeBarList.Last().Item1.Second == 55)
                        {
                            this.RealTimeBarList.Clear();
                            this.hadFirst = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the HistoricalData event of the IBClient. This one is triggered after HistoricalDataBars are requested for each bar received.<br/>
        /// Inside this method, the received bars are parsed to bars represented by the Bar class from NuTrade.Core
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Krs.Ats.IBNet.HistoricalDataEventArgs"/> instance containing the event data.</param>
        /// <remarks></remarks>
        private void client_HistoricalData(object sender, HistoricalDataEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(this.IsConnected);
            if (this.IsConnected)
            {
                //Saves how many bars were requested in total to the attribute
                totalHistoricalBars = e.RecordTotal;
<<<<<<< HEAD
=======
                lock (IBID.ConsoleTextLock)
                {
                    this.workerViewModel.ConsoleText += this.workerViewModel.EquityAsString + ": Historical-Bar: " + e.Date + ", " + e.Open + ", " + e.High + ", " + e.Low + ", " + e.Close + ", " + e.Volume + "\n";
                }
>>>>>>> 9808b6b... Updated TS to use volume

                this.noOfHistoricalBarsReceived++;
                
                //parses the received bar to one of my bars
                Tuple<DateTime, decimal, decimal, decimal, decimal, long> newBar = new Tuple<DateTime, decimal, decimal, decimal, decimal, long>(e.Date, e.Open, e.High, e.Low, e.Close, e.Volume);
<<<<<<< HEAD
                if (!this.ListOfBars.Contains(newBar))
                {
                    lock (IBID.ConsoleTextLock)
                    {
                        this.workerViewModel.ConsoleText += this.workerViewModel.EquityAsString + ": Historical-Bar: " + e.Date + ", " + e.Open + ", " + e.High + ", " + e.Low + ", " + e.Close + ", " + e.Volume + "\n";
                    }

                    this.ListOfBars.Add(newBar);
                    this.csvWriter.WriteBar(newBar);
                }
=======
                this.ListOfBars.Add(newBar);
                this.csvWriter.WriteBar(newBar);
>>>>>>> 9808b6b... Updated TS to use volume
            }
        }

        public void Disconnect()
        {
            this.IsConnected = false;

            this.inputClient.Disconnect();
            this.hadFirst = false;
            this.totalHistoricalBars = 0;

            this.RealTimeBarList.Clear();
        }

        public String Connect()
        {
            //establishing a connection
            try
            {
                lock (IBID.ConsoleTextLock)
                {
                    this.workerViewModel.ConsoleText += this.workerViewModel.EquityAsString + ": Connecting to IB.\n";
                }
                lock (IBID.ConnectionLock)
                {
                    inputClient.Connect("127.0.0.1", 7496, IBID.ConnectionID++);
                }
                lock (IBID.ConsoleTextLock)
                {
                    this.workerViewModel.ConsoleText += this.workerViewModel.EquityAsString + ": Successfully connected.\n";
                }

                //Add our event-handling methods to the inputClient.
                //After this, the inputClient knows, which methods it should call when a Historical or a realtime bar arrives.
                inputClient.HistoricalData += client_HistoricalData;
                inputClient.RealTimeBar += client_RealTimeBar;

                this.IsConnected = true;

                return "";
            }
            catch (Exception)
            {
                return "IB not available or sockets closed!";
            }
        }
    }
}