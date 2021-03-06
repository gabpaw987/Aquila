﻿using System;
using System.Collections.Generic;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;
using MoreLinq;

namespace Aquila_Software
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
        /// <summary>
        /// A List were all bars that are in the application at the moment, are saved.
        /// </summary>
        private List<Tuple<DateTime, decimal, decimal, decimal, decimal>> ListOfBars;

        /// <summary>
        /// The client that handles all the input connections. In the constructor this one is connected to the IB with the id 0.<br/>
        /// It can be identified by this id and no more other clients can connect to IB with the same id. This oject represents the whole connection<br/>
        /// to the InteractiveBrokers API and for example all the bars are requested over it.
        /// </summary>
        private IBClient inputClient;

        /// <summary>
        /// Gets the total number of historical data bars.
        /// </summary>
        /// <remarks></remarks>
        public int totalHistoricalBars { get; private set; }

        /// <summary>
        /// Gets the equity. This is the Equity the whole IBInput class is getting the data for.
        /// </summary>
        /// <remarks></remarks>
        public Equity Equity { get; private set; }

        /// <summary>
        /// In this list the received realtime bars are saved. Whenever a new one is received, it a method checks if there are already 12 bars in the list.<br/>
        /// If this is the case, the CreateMinuteBar()-method is called to create a minute-bar out of the 12 5-second bars and this minute bar is added to the<br/>
        /// ListOfBars-list. Also when this happens the RealTimeBarList gets cleared to hold 12 new bars later on.
        /// </summary>
        private List<Tuple<DateTime, decimal, decimal, decimal, decimal>> RealTimeBarList;

        /// <summary>
        /// When this method is called, the HistrocialData bars are requested. After the request the client_HistoricalData event is called every time a bar<br/>
        /// arrives.
        /// </summary>
        /// <param name="barsize">The barsize.</param>
        /// <remarks></remarks>
        public void GetHistoricalDataBars(BarSize barsize)
        {
            inputClient.RequestHistoricalData(17, this.Equity, DateTime.Now, new TimeSpan(0, 23, 59, 59), barsize, HistoricalDataType.Trades, 0);
        }

        /// <summary>
        /// When this method is called, realtime bars are getting requested. That means an event is running that calls the client_RealTimeBar method every five seconds<br/>
        /// when a new 5-second bar is available.
        /// </summary>
        /// <remarks></remarks>
        public void SubscribeForRealTimeBars()
        {
            inputClient.RequestRealTimeBars(16, this.Equity, 5, RealTimeBarType.Trades, true);
        }

        /// <summary>
        /// Creates a minute bar. This method takes all 5-second bar of the RealTimeBarList-list and searches for the right ones to create a minute bar out of them.<br/>
        /// Once the minute bar is created it gets returned.
        /// </summary>
        /// <returns>The created minute Bar.</returns>
        /// <remarks></remarks>
        private Tuple<DateTime, decimal, decimal, decimal, decimal> CreateMinuteBar()
        {
            //First open value in the list
            decimal open = RealTimeBarList[0].Item2;
            //The highest "high" value in the RealTimeBarList
            decimal high = RealTimeBarList.MaxBy(x => x.Item3).Item3;
            //The lowest "low" value in the RealTimeBarList
            decimal low = RealTimeBarList.MinBy(x => x.Item4).Item4;
            //The Last close value in the list
            decimal close = RealTimeBarList[RealTimeBarList.ToArray().Length - 1].Item5;

            //creates the bar with these values and returns it
            return new Tuple<DateTime, decimal, decimal, decimal, decimal>(DateTime.Now, open, high, low, close);
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
        public IBInput(List<Tuple<DateTime, decimal, decimal, decimal, decimal>> LOB, Equity equity)
        {
            ListOfBars = LOB;

            //creating the IBClient
            inputClient = new IBClient();
            inputClient.ThrowExceptions = true;

            RealTimeBarList = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();

            //establishing a connection
            Console.WriteLine("Connecting to IB.");
            inputClient.Connect("127.0.0.1", 7496, 0);
            Console.WriteLine("Successfully connected.");
            LogFileManager.WriteToLog("Successfully connected to IB.");

            //Add our event-handling methods to the inputClient.
            //After this, the inputClient knows, which methods it should call when a Historical or a realtime bar arrives.
            inputClient.HistoricalData += client_HistoricalData;
            inputClient.RealTimeBar += client_RealTimeBar;

            this.Equity = equity;
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
            RealTimeBarList.Add(new Tuple<DateTime, decimal, decimal, decimal, decimal>(DateTime.Now, e.Open, e.High, e.Low, e.Close));
            Tuple<DateTime, decimal, decimal, decimal, decimal> b = null;
            //When we got 12 bars in the RealTimeBarList create a minute bar
            if (RealTimeBarList.ToArray().Length >= 12)
            {
                b = CreateMinuteBar();
                RealTimeBarList = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
                Console.WriteLine("Received Real Time Minute-Bar: " + b.Item1 + ", " + b.Item2 + ", " + b.Item3 + ", " + b.Item4 + ", " + b.Item5);
                LogFileManager.WriteToLog("Received Real Time Minute-Bar: " + b.Item1 + ", " + b.Item2 + ", " + b.Item3 + ", " + b.Item4 + ", " + b.Item5);
                ListOfBars.Add(new Tuple<DateTime, decimal, decimal, decimal, decimal>(b.Item1, b.Item2, b.Item3, b.Item4, b.Item5));
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
            //Saves how many bars were requested in total to the attribute
            totalHistoricalBars = e.RecordTotal;
            //parses the received bar to one of my bars
            ListOfBars.Add(new Tuple<DateTime, decimal, decimal, decimal, decimal>(e.Date, e.Open, e.High, e.Low, e.Close));
        }
    }
}