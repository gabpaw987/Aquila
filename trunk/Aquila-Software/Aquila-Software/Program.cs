using System;
using System.Collections.Generic;
using System.Threading;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;

namespace Aquila_Software
{
    internal class Program
    {
        /// <summary>
        /// The List of all bars that are in the application at the moment.
        /// </summary>
        private static List<Tuple<DateTime, decimal, decimal, decimal, decimal>> ListOfBars;

        private static List<int> ListOfSignals;

        /// <summary>
        /// Is measured in round lots(fixes sizes of stacks of shares that can be traded) and calculates from the round lot size of the equits multiplied by the<br/>
        /// amount the user wants to do his trades with. This trading amount is the size of every buy and sell done by this application.
        /// </summary>
        private static int TradingAmount;

        /// <summary>
        /// This is the main equity for the whole application. The IBInput and IBOutput classes are working with this equity when the retrieve data or place orders.
        /// </summary>
        private static Equity equity;

        /// <summary>
        /// This is the main entry point for NuTrade.Core. It parses the console arguments that were powered by the user through the NuTrade.SettingsGUI.<br/>
        /// Afterwards it starts the Start()-method in a thread in order to allow the user to quit any time he wants by pressing 'Q' on the keyboard.
        /// </summary>
        /// <param name="args">The arguments provided by the user via the NuTrade.SettingsGUI.</param>
        /// <remarks></remarks>
        public static void Main(String[] args)
        {
            try
            {
                Console.WriteLine("The program exits by pressing 'q'!\n");

                LogFileManager.CreateLog("NuTrade_Log_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt");

                Thread mainThread = null;

                //If a TextBox from NuTrade.SettingsGUI haven´t been filled with the required information an error message is displayed.
                /*if (args.Length != 14)
                {
                    Console.Error.WriteLine("False Parameters have been provided! Please try again by restarting the application!");
                }
                //If NASDAQ is closed, write an error, becuse we only support NASDAQ(It is open from 15:30 until 22:00 CET)
                else 
                if ((DateTime.Now.Hour < 16 && DateTime.Now.Minute < 30) || DateTime.Now.Hour < 15 || DateTime.Now.Hour > 22)
                {
                    Console.Error.WriteLine("The exchange NASDAQ is closed!\n" +
                        "If you didn't request an equity from NASDAQ, you chose a wrong equity.\n" +
                        "We only support NASDAQ as the primary exchange of the chosen equity.");

                    LogFileManager.WriteToLog("The exchange NASDAQ is closed!\n" +
                        "If you didn't request an equity from NASDAQ, you chose a wrong equity.\n" +
                        "We only support NASDAQ as the primary exchange of the chosen equity.");
                }
                else
                {*/
                    ListOfBars = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
                    ListOfSignals = new List<int>();

                    //parsing of arguments form the user.
                    /*TradingAmount = Convert.ToInt32(args[1]) * Convert.ToInt32(args[13]);

                    equity = new Equity(args[7], args[9]);
                    equity.SecurityType = SecurityType.Stock;
                    equity.Currency = args[11];*/

                    //This part is here for the case anyone wants to test this application without the SettingsGUI.
                    TradingAmount = 100;
                    equity = new Equity("GOOG", "SMART");
                    equity.SecurityType = SecurityType.Stock;
                    equity.Currency = "USD";

                    //Run the Start()-method in a thread to be able to keep listening on the keys pressed in the console and therefore to enable quitting by pressing 'Q'
                    mainThread = new Thread(new ThreadStart(Start));
                    mainThread.Start();
                //}

                //Listen on a key pressed at the keyboard. Close the application if the key 'Q' gets pressed.
                var abortKey = new ConsoleKey();

                while (!abortKey.Equals(ConsoleKey.Q))
                    abortKey = Console.ReadKey().Key;

                if (mainThread != null)
                    mainThread.Abort();
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured!");
            }
        }

        private static void Start()
        {
            try
            {
                var input = new IBInput(ListOfBars, equity);

                //request historical data bars
                Console.WriteLine("Please wait... Historical bars are getting fetched!");
                input.GetHistoricalDataBars(BarSize.OneMinute);

                //wait until all bars were received
                while (ListOfBars.Count < input.totalHistoricalBars || input.totalHistoricalBars == 0)
                {
                    System.Threading.Thread.Sleep(100);
                }
                Console.WriteLine("Finished fetching historical data");
                LogFileManager.WriteToLog("Fetched Historical bars.");

                //request realtime bars
                input.SubscribeForRealTimeBars();

                int length = 0;
                Boolean historical = true;
                while (true)
                {
                    //Wait until a new realtime bar is received
                    while (length == ListOfBars.Count)
                    {
                        Thread.Sleep(1000);
                    }
                    var output = new IBOutput(equity);

                    var l = new List<double>();

                    //Calculate the decision
                    Algorithm.DecisionCalculator.startCalculation(90, ListOfBars, ListOfSignals);

                    //If the decision changed to the last one and is now 1, then buy, or if it change and is now -1 sell shares in the amount of the stored Tradingamount
                    if (ListOfSignals[ListOfSignals.Count - 1] != ListOfSignals[ListOfSignals.Count - 2])
                    {
                        if (ListOfSignals[ListOfSignals.Count - 1] == 1)
                            output.placeOrder(ActionSide.Buy, TradingAmount);
                        else if (ListOfSignals[ListOfSignals.Count - 1] == -1)
                            output.placeOrder(ActionSide.Sell, TradingAmount);
                    }
                    if (historical)
                    {
                        foreach (Tuple<DateTime, decimal, decimal, decimal, decimal> t in ListOfBars)
                        {
                            //equity.LocalSymbol
                            //TODO VOLUME???
                            DatabaseHandler.executeModify("INSERT INTO mBar VALUES('" + equity.LocalSymbol + "','" + t.Item1.ToString("YYYY-MM-DD HH:mm:ss zzz") + "'," + t.Item2 + "," + t.Item3 + "," + t.Item4 + "," + t.Item5 + ",0" + "" + ")");
                        }
                        for (int i = 0; i < ListOfSignals.Count; i++)
                        {
                            DatabaseHandler.executeModify("INSERT INTO signal VALUES('" + equity.LocalSymbol + "','" + ListOfBars[i].Item1.ToString("YYYY-MM-DD HH:mm:ss zzz") + "'," + ListOfSignals[i] + ")");
                        }
                    }
                    else
                    {
                        DatabaseHandler.executeModify("INSERT INTO mBar VALUES('" + equity.LocalSymbol + "','" + ListOfBars[ListOfBars.Count].Item1.ToString("YYYY-MM-DD HH:mm:ss zzz") + "'," + ListOfBars[ListOfBars.Count].Item2 + "," + ListOfBars[ListOfBars.Count].Item3 + "," + ListOfBars[ListOfBars.Count].Item4 + "," + ListOfBars[ListOfBars.Count].Item5 + ",0" + "" + ")");
                        DatabaseHandler.executeModify("INSERT INTO signal VALUES('" + equity.LocalSymbol + "','" + ListOfBars[ListOfSignals.Count].Item1.ToString("YYYY-MM-DD HH:mm:ss zzz") + "'," + ListOfSignals[ListOfSignals.Count] + ")");
                    }

                    length = ListOfBars.Count;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("An error occured!\nCheck your IB TWS!");
            }
        }
    }
}