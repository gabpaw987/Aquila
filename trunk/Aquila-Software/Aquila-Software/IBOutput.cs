using System;
using System.Threading;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;

namespace Aquila_Software
{
    /// <summary>
    /// This class is an output interface to the InteractiveBrokers API. Whenever and order shall be placed, this interface is used to set the right properties.<br/>
    /// It provides methods to place an oder as well as to check the current order information.
    /// </summary>
    /// <remarks></remarks>
    internal class IBOutput
    {
        /// <summary>
        /// This is the client connection to the InteractiveBrokers TWS. It´s main purpose is to place orders. Because every time an order gets placed and transmitted,<br/>
        /// the IBClient crashes, we connect it with a new id(stored in index) whenever we waant to place a new order.
        /// </summary>
        private IBClient outputClient;

        /// <summary>
        /// Gets the equity. This is the equity that is used for each order placed through the current IBOutput object.
        /// </summary>
        /// <remarks></remarks>
        public Equity Equity { get; private set; }

        /// <summary>
        /// This orderId is the orderId that is handed over to every placeOrder-call in this class. An order can only be placed ONE time with the same ID.<br/>
        /// In the beginning it requests the next valid order ID and the it increments it every time an order gets placed. Because of this process, every time an<br/>
        /// order gets placed a new valid ID is stored in this attribute.
        /// </summary>
        private static int orderId;

        /// <summary>
        /// This attribute represents the current(the last one received) Ask price of the specified equity. It gets received again every time an order gets placed.
        /// </summary>
        private static decimal currentAskPrice = 0;
        /// <summary>
        /// This attribute represents the current(the last one received) Bid price of the specified equity. It gets received again every time an order gets placed.
        /// </summary>
        private static decimal currentBidPrice = 0;

        /// <summary>
        /// This is the connection ID that is used for the next connection by and output client to the InteractiveBrokers TWS. It increments every time a new connection<br/>
        /// to IB is built and it starts at 1 because 0 is the ID of the IBInput. The IBInput only needs one connection which means one ID.
        /// </summary>
        private static int connectionID = 1;

        /// <summary>
        /// This is a boolean indicating if the a current Bid price is set. It is used to know when the requestMarketData method is finished and the bid price<br/>
        /// is recieved. In cooperation with the isCurrentAskSet variable it is used when an order shall be placed, if the valiues for the limit price have already<br/>
        /// been received.
        /// </summary>
        private static Boolean isCurrentBidSet = false;
        /// <summary>
        /// This is a boolean indicating if the a current Ask price is set. It is used to know when the requestMarketData method is finished and the ask price<br/>
        /// is recieved. In cooperation with the isCurrentBidSet variable it is used when an order shall be placed, if the valiues for the limit price have already<br/>
        /// been received.
        /// </summary>
        private static Boolean isCurrentAskSet = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="IBOutput"/> class. It sets the equity from the parameters, initialises a new outputClient and adds the<br/>
        /// inportant eventhandler to it.
        /// </summary>
        /// <param name="equity">The equity used throughout the whole object.</param>
        /// <remarks></remarks>
        public IBOutput(Equity equity)
        {
            this.Equity = equity;

            outputClient = new IBClient();
            outputClient.ThrowExceptions = true;

            //Add the important eventhandler to the outputClient
            outputClient.NextValidId += client_NextValidId;
            outputClient.TickPrice += client_TickPrice;
        }

        /// <summary>
        /// Places the order and transmits it. Transmit means that it is really submitted from IB to the dealer network.
        /// </summary>
        /// <param name="buyOrSell">The Actionside that indicates if a order to buy or to sell shall be placed. Possible values are: ActionSide.Buy and ActionSide.Sell.</param>
        /// <param name="totalQuantity">The total quantity of shares that shall be bought/sold.</param>
        /// <remarks></remarks>
        public void placeOrder(ActionSide buyOrSell, int totalQuantity)
        {
            try
            {
                //Connect to ib with a new ID.
                outputClient.Connect("127.0.0.1", 7496, connectionID++);

                //Request the current market data(bid and ask prices) that are then received thorugh the client_TickPrice eventhandler.
                //This data is used to set the limit price.
                outputClient.RequestMarketData(14, this.Equity, null, true, false);

                //Make a new order like the user specified and also trade outside regular trading hours.
                Order BuyContract = new Order();
                BuyContract.Action = buyOrSell;
                BuyContract.OutsideRth = true;

                //When the limit prices are already recieved, set the proper variables back to false and apply the limit price
                while (!isCurrentAskSet || !isCurrentBidSet)
                    Thread.Sleep(100);

                isCurrentAskSet = false;
                isCurrentBidSet = false;

                if (buyOrSell.Equals(ActionSide.Buy))
                    BuyContract.LimitPrice = currentAskPrice;
                else if (buyOrSell.Equals(ActionSide.Sell))
                    BuyContract.LimitPrice = currentBidPrice;

                //Finish the order with the totalQuantity from the parameters. Tif is the Time an order stays in the TWS when it´s not filled.
                BuyContract.OrderType = OrderType.Limit;
                BuyContract.TotalQuantity = totalQuantity;
                BuyContract.Tif = TimeInForce.Day;
                //really transmit the order
                BuyContract.Transmit = true;

                //place it and request its execution.
                outputClient.PlaceOrder(orderId++, this.Equity, BuyContract);
                outputClient.RequestExecutions(34, new ExecutionFilter());

                //disconnec the outputClient again after placing the order.
                outputClient.Disconnect();

                //Writes what happened to the Console and the log file
                Console.WriteLine("Order Placed with Order ID: " + (orderId - 1) + "!");
                LogFileManager.WriteToLog("Order Placed with Order ID: " + (orderId - 1) + "!");
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured while placing the order!");
            }
        }

        /// <summary>
        /// Handles the TickPrice event of the ouputClient control. The TickPrices are like much other data requested though a call outputClient.RequestMarketData().<br/>
        /// This handler decides whether the received value is the bid or the ask and sets the appropriate attribues.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Krs.Ats.IBNet.TickPriceEventArgs"/> instance containing the event data.</param>
        /// <remarks></remarks>
        private static void client_TickPrice(object sender, TickPriceEventArgs e)
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
            if (orderId == 0)
                orderId = e.OrderId;
        }
    }
}