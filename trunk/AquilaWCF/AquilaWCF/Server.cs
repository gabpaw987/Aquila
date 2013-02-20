using System;
using System.Collections.Generic;
using Krs.Ats.IBNet;

namespace Aquila_Software
{
    public class Server : SettingsHandler
    {
        public Dictionary<string, Worker> Workers;

        public bool SetSetting(params object[] args)
        {
            try
            {
                if (args.Length != 0 && args[0] is string && args[1] is string && this.Workers.ContainsKey((string)args[1]))
                {
                    Worker workerToSet = null;
                    this.Workers.TryGetValue((string)args[1], out workerToSet);
                    if (args.Length == 3)
                    {
                        if (args[0].Equals("IsActive") && args[2] is bool)
                        {
                            workerToSet.IsActive = (bool)args[2];
                        }
                        /*else if (args[0].Equals("Equity") && args[2] is string)
                        {
                            workerToSet.Equity = new Equity((string)args[2]);
                        }*/
                        else if (args[0].Equals("Amount") && args[2] is float)
                        {
                            workerToSet.Amount = (float)args[2];
                        }
                        else if (args[0].Equals("BarSize") && args[2] is string)
                        {
                            if (((string)args[2]).Equals("mBar"))
                            {
                                workerToSet.BarSize = BarSize.OneMinute;
                            }
                            else if (((string)args[2]).Equals("dBar"))
                            {
                                workerToSet.BarSize = BarSize.OneDay;
                            }
                        }
                        else if (args[0].Equals("BarType") && args[2] is string)
                        {
                            if (((string)args[2]).Equals("Bid"))
                            {
                                workerToSet.HistoricalDataType = HistoricalDataType.Bid;
                                workerToSet.RealtimeBarType = RealTimeBarType.Bid;
                            }
                            else if (((string)args[2]).Equals("Ask"))
                            {
                                workerToSet.HistoricalDataType = HistoricalDataType.Ask;
                                workerToSet.RealtimeBarType = RealTimeBarType.Ask;
                            }
                            else if (((string)args[2]).Equals("Last") || ((string)args[2]).Equals("Trades"))
                            {
                                workerToSet.HistoricalDataType = HistoricalDataType.Trades;
                                workerToSet.RealtimeBarType = RealTimeBarType.Trades;
                            }
                            else if (((string)args[2]).Equals("Midpoint"))
                            {
                                workerToSet.HistoricalDataType = HistoricalDataType.Midpoint;
                                workerToSet.RealtimeBarType = RealTimeBarType.Midpoint;
                            }
                        }
                        else if (args[0].Equals("PricePremiumPercentage") && args[2] is float)
                        {
                            workerToSet.PricePremiumPercentage = (float)args[2];
                        }
                        else if (args[0].Equals("Running") && args[2] is bool)
                        {
                            workerToSet._Running = (bool)args[2];
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.Error.WriteLine("An exception occured!");
                return false;
            }
            return true;
        }
    }
}