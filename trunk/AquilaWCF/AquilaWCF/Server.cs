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
                if (args.Length != 0 && args[0] is string && args[1] is string && this.Workers.ContainsKey((string)args[0]))
                {
                    Worker workerToSet = null;
                    this.Workers.TryGetValue((string)args[0], out workerToSet);
                    if (args.Length == 3)
                    {
                        if (args[1].Equals("IsActive") && args[2] is bool)
                        {
                            workerToSet.IsActive = (bool)args[2];
                        }
                        /*else */
                        else if (args[1].Equals("Amount") && args[2] is float)
                        {
                            workerToSet.Amount = (float)args[2];
                        }
                        else if (args[1].Equals("BarSize") && args[2] is string)
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
                        else if (args[1].Equals("BarType") && args[2] is string)
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
                        else if (args[1].Equals("PricePremiumPercentage") && args[2] is float)
                        {
                            workerToSet.PricePremiumPercentage = (float)args[2];
                        }
                        else if (args[1].Equals("isCalculating") && args[2] is bool)
                        {
                            workerToSet.isCalculating = (bool)args[2];
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

        public bool performAction(params object[] args)
        {
            try
            {
                //start, stop, manualexecution
                if (args.Length != 0 && args[0] is string && args[1] is string && this.Workers.ContainsKey((string)args[0]))
                {
                    if (args.Length == 2)
                    {
                        if (args[1].Equals("Create"))
                        {
                            Worker tempWorker = new Worker((string)args[0]);
                            //TODO: start the Thread
                            this.Workers.Add((string)args[0], tempWorker);
                        }
                        else if (args[1].Equals("Start"))
                        {
                            Worker tempWorker = null;
                            this.Workers.TryGetValue((string)args[0], out tempWorker);
                            //TODO: start the Thread
                        }
                        else if (args[1].Equals("Stop"))
                        {
                            //TODO: stop the thread
                        }
                    }
                    else if (args.Length == 3)
                    {
                        if (args[1].Equals("ExecutionDecision") && args[2] is ManualExecution)
                        {
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