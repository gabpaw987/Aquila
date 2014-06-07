using System;
using System.Threading;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;

namespace TradingSoftware
{
    /// <summary>
    /// This class is an output interface to the InteractiveBrokers API. Whenever and order shall be placed, this interface is used to set the right properties.<br/>
    /// It provides methods to place an oder as well as to check the current order information.
    /// </summary>
    /// <remarks></remarks>
    internal class IBOutput
    {
        public bool IsConnected { get; set; }

        /// <summary>
        /// This is the client connection to the InteractiveBrokers TWS. It´s main purpose is to place orders. Because every time an order gets placed and transmitted,<br/>
        /// the IBClient crashes, we connect it with a new id(stored in index) whenever we waant to place a new order.
        /// </summary>
        private IBClient outputClient;

        /// <summary>
        /// Gets the equity. This is the equity that is used for each order placed through the current IBOutput object.
        /// </summary>
        /// <remarks></remarks>
        public Contract Equity { get; private set; }

        /// <summary>
        /// This orderId is the orderId that is handed over to every placeOrder-call in this class. An order can only be placed ONE time with the same ID.<br/>
        /// In the beginning it requests the next valid order ID and the it increments it every time an order gets placed. Because of this process, every time an<br/>
        /// order gets placed a new valid ID is stored in this attribute.
        /// </summary>
        //private static int orderId;

        /// <summary>
        /// This attribute represents the current(the last one received) Ask price of the specified equity. It gets received again every time an order gets placed.
        /// </summary>
        public decimal currentAskPrice = 0;

        /// <summary>
        /// This attribute represents the current(the last one received) Bid price of the specified equity. It gets received again every time an order gets placed.
        /// </summary>
        public decimal currentBidPrice = 0;

        //DONE: next 2 booleans private
        /// <summary>
        /// This is a boolean indicating if the a current Bid price is set. It is used to know when the requestMarketData method is finished and the bid price<br/>
        /// is recieved. In cooperation with the isCurrentAskSet variable it is used when an order shall be placed, if the valiues for the limit price have already<br/>
        /// been received.
        /// </summary>
        private Boolean isCurrentBidSet = false;

        /// <summary>
        /// This is a boolean indicating if the a current Ask price is set. It is used to know when the requestMarketData method is finished and the ask price<br/>
        /// is recieved. In cooperation with the isCurrentBidSet variable it is used when an order shall be placed, if the valiues for the limit price have already<br/>
        /// been received.
        /// </summary>
        private Boolean isCurrentAskSet = false;

        private WorkerViewModel workerViewModel;

        private Order BuyContract;
        private ActionSide buyOrSell;

        /// <summary>
        /// Initializes a new instance of the <see cref="IBOutput"/> class. It sets the equity from the parameters, initialises a new outputClient and adds the<br/>
        /// inportant eventhandler to it.
        /// </summary>
        /// <param name="equity">The equity used throughout the whole object.</param>
        /// <remarks></remarks>
        public IBOutput(WorkerViewModel workerViewModel, Contract equity)
        {
            this.workerViewModel = workerViewModel;

            this.Equity = equity;

            outputClient = new IBClient();
            outputClient.ThrowExceptions = true;
        }

        /// <summary>
        /// Places the order and transmits it. Transmit means that it is really submitted from IB to the dealer network.
        /// </summary>
        /// <param name="buyOrSell">The Actionside that indicates if a order to buy or to sell shall be placed. Possible values are: ActionSide.Buy and ActionSide.Sell.</param>
        /// <param name="totalQuantity">The total quantity of shares that shall be bought/sold.</param>
        /// <remarks></remarks>
        public int placeOrder(ActionSide buyOrSell, int totalQuantity, decimal pricePremiumPercentage)
        {
            lock (IBID.OrderLock)
            {
                try
                {
                    this.buyOrSell = buyOrSell;

                    //Make a new order like the user specified and also trade outside regular trading hours.
                    this.BuyContract = new Order();
                    this.BuyContract.Action = buyOrSell;
                    this.BuyContract.OutsideRth = false;

                    //Finish the order with the totalQuantity from the parameters. Tif is the Time an order stays in the TWS when it´s not filled.
                    this.BuyContract.OrderType = OrderType.Limit;
                    this.BuyContract.TotalQuantity = totalQuantity;
                    this.BuyContract.Tif = TimeInForce.Day;

                    //really transmit the order
                    this.BuyContract.Transmit = true;

                    this.RequestTickPrice();

                    if (pricePremiumPercentage > 0)
                    {
                        if (buyOrSell.Equals(ActionSide.Buy))
                            this.BuyContract.LimitPrice = currentAskPrice + ((currentAskPrice - currentBidPrice) * pricePremiumPercentage) / 100;
                        else if (buyOrSell.Equals(ActionSide.Sell))
                            this.BuyContract.LimitPrice = currentBidPrice - ((currentAskPrice - currentBidPrice) * pricePremiumPercentage) / 100;
                    }

                    //place it and request its execution.
                    outputClient.PlaceOrder(IBID.OrderID, this.Equity, BuyContract);

                    //Writes what happened to the Console and the log file
                    lock (IBID.ConsoleTextLock)
                    {
                        this.workerViewModel.ConsoleText += this.Equity.Symbol + ": Order Executed with Order ID: " + (IBID.OrderID) + "!\n";
                    }


                    return IBID.OrderID++;
                }
                catch (Exception)
                {
                    lock (IBID.ConsoleTextLock)
                    {
                        this.workerViewModel.ConsoleText += this.Equity.Symbol + ": An error occured while placing the order!\n";
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// Handles the TickPrice event of the ouputClient control. The TickPrices are like much other data requested though a call outputClient.RequestMarketData().<br/>
        /// This handler decides whether the received value is the bid or the ask and sets the appropriate attribues.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Krs.Ats.IBNet.TickPriceEventArgs"/> instance containing the event data.</param>
        /// <remarks></remarks>
        private void client_TickPrice(object sender, TickPriceEventArgs e)
        {
            if (e.TickType.Equals(TickType.AskPrice))
            {
                isCurrentAskSet = true;
                currentAskPrice = e.Price;
            }
            else if (e.TickType.Equals(TickType.BidPrice))
            {
                isCurrentBidSet = true;
                currentBidPrice = e.Price;
            }
        }

        /// <summary>
        /// Handles the NextValidId event of the outpurClient control. Whenever a new outputclient is initialised this eventhandler is added to it, in order to receive<br/>
        /// its next valid Order ID. Because the different connectionIDs result in not understandable orderIDs I made the orderID static and only set it to the<br/>
        /// new value at the first time an outputClient is initialised, in order to keep the orderID managemanet clear.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Krs.Ats.IBNet.NextValidIdEventArgs"/> instance containing the event data.</param>
        /// <remarks></remarks>
        private void client_NextValidId(object sender, NextValidIdEventArgs e)
        {
            if (IBID.OrderID == 1)
            {
                lock (IBID.OrderLock)
                {
                    IBID.OrderID = e.OrderId;
                }
            }
        }

        public void RequestTickPrice()
        {
            lock (IBID.TickerLock)
            {
                this.outputClient.RequestMarketData(IBID.TickerID++, this.Equity, null, false, false);
            }

            //When the limit prices are already recieved, set the proper variables back to false and apply the limit price
            while (!isCurrentAskSet || !isCurrentBidSet)
                Thread.Sleep(100);

            isCurrentAskSet = false;
            isCurrentBidSet = false;
        }

        private static void client_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("Msg: " + e.ErrorMsg + "." + e.ErrorCode + "." + e.TickerId);
        }

        public void Disconnect()
        {
            this.IsConnected = false;

            this.outputClient.Disconnect();
        }

        public String Connect()
        {
            //establishing a connection
            try
            {
                lock (IBID.ConsoleTextLock)
                {
                    this.workerViewModel.ConsoleText += this.Equity.Symbol + ": Connecting to IB.\n";
                }
                lock (IBID.ConnectionLock)
                {
                    outputClient.Connect("127.0.0.1", 7496, IBID.ConnectionID++);
                }
                lock (IBID.ConsoleTextLock)
                {
                    this.workerViewModel.ConsoleText += this.Equity.Symbol + ": Successfully connected.\n";
                }

                //Add the important eventhandlers to the outputClient
                outputClient.NextValidId += client_NextValidId;
                outputClient.TickPrice += client_TickPrice;
                outputClient.Error += client_Error;

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