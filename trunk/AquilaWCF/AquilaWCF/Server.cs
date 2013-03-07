using System;
using System.Collections.Generic;
using Krs.Ats.IBNet;

namespace Aquila_Software
{
    public class Server : SettingsHandler
    {
        public Dictionary<string, Worker> Workers;

        public bool SetSetting(object[] args)
        {
            try
            {
                if (args.Length != 0 && args[0] is string && args[1] is string && this.Workers.ContainsKey((string)args[0]))
                {
                    //Worker workerToSet = null;
                    //this.Workers.TryGetValue((string)args[0], out workerToSet);
                    //if (args.Length == 3)
                    //{
                    //    if (args[1].Equals("IsActive") && args[2] is bool)
                    //    {
                    //        workerToSet.IsActive = (bool)args[2];
                    //        return true;
                    //    }
                    //    else if (args[1].Equals("Amount") && args[2] is float)
                    //    {
                    //        workerToSet.Amount = (float)args[2];
                    //        return true;
                    //    }
                    //    else if (args[1].Equals("BarSize") && args[2] is string)
                    //    {
                    //        if (((string)args[2]).Equals("mBar"))
                    //        {
                    //            workerToSet.BarSize = BarSize.OneMinute;
                    //            return true;
                    //        }
                    //        else if (((string)args[2]).Equals("dBar"))
                    //        {
                    //            workerToSet.BarSize = BarSize.OneDay;
                    //            return true;
                    //        }
                    //    }
                    //    else if (args[1].Equals("BarType") && args[2] is string)
                    //    {
                    //        if (((string)args[2]).Equals("Bid"))
                    //        {
                    //            workerToSet.HistoricalDataType = HistoricalDataType.Bid;
                    //            workerToSet.RealtimeBarType = RealTimeBarType.Bid;
                    //            return true;
                    //        }
                    //        else if (((string)args[2]).Equals("Ask"))
                    //        {
                    //            workerToSet.HistoricalDataType = HistoricalDataType.Ask;
                    //            workerToSet.RealtimeBarType = RealTimeBarType.Ask;
                    //            return true;
                    //        }
                    //        else if (((string)args[2]).Equals("Last") || ((string)args[2]).Equals("Trades"))
                    //        {
                    //            workerToSet.HistoricalDataType = HistoricalDataType.Trades;
                    //            workerToSet.RealtimeBarType = RealTimeBarType.Trades;
                    //            return true;
                    //        }
                    //        else if (((string)args[2]).Equals("Midpoint"))
                    //        {
                    //            workerToSet.HistoricalDataType = HistoricalDataType.Midpoint;
                    //            workerToSet.RealtimeBarType = RealTimeBarType.Midpoint;
                    //            return true;
                    //        }
                    //    }
                    //    else if (args[1].Equals("PricePremiumPercentage") && args[2] is float)
                    //    {
                    //        workerToSet.PricePremiumPercentage = (float)args[2];
                    //        return true;
                    //    }
                    //    else if (args[1].Equals("isCalculating") && args[2] is bool)
                    //    {
                    //        workerToSet.isCalculating = (bool)args[2];
                    //        return true;
                    //    }
                    //}
                }
            }
            catch (Exception)
            {
                Console.Error.WriteLine("An exception occured!");
                return false;
            }
            return false;
        }

        public bool performAction(object[] args)
        {
            //args.Clone();
            Console.WriteLine("Methodenstart");
            //start, stop, manualexecution
            if (args.Length != 0 && args[0] is string && args[1] is string)
            {
                Console.WriteLine("Erste if");
                if (args.Length == 2)
                {
                    Console.WriteLine("Zweite if");
                    if (args[1].Equals("Create"))
                    {
                        Console.WriteLine("Create drinn");
                        string temp = (string)args[0];
                        Worker tempWorker = new Worker(temp);
                        Console.WriteLine("Worker erstellt");
                        //TODO: start the Thread
                        this.Workers.Add(temp, tempWorker);
                        Console.WriteLine("geadded");
                        return true;
                    }
                    //else if (args[1].Equals("Start"))
                    //{
                    //    Worker tempWorker = null;
                    //    string temp = (string)args[0];
                    //    this.Workers.TryGetValue(temp, out tempWorker);
                    //    //TODO: start the Thread
                    //    return true;
                    //}
                    //else if (args[1].Equals("Stop"))
                    //{
                    //    //TODO: stop the thread
                    //    return true;
                    //}
                    //else if (args.Length== 3)
                    //{
                    //    if (args[1].Equals("ExecutionDecision") && args[2] is ManualExecution)
                    //    {
                    //        return true;
                    //    }
                    //}
                
                  
                }
                
            }
            return false;
        }
    }
}