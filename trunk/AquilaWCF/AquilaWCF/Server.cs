using System;
using System.Collections.Generic;

namespace Aquila_Software
{
    public class Server : SettingsHandler
    {
        public Dictionary<string, WorkerInfo> WorkerInfos;

        public Server(Dictionary<string, WorkerInfo> workerInfos)
        {
            this.WorkerInfos = workerInfos;
        }

        public bool SetSetting(object[] args)
        {
            try
            {
                if (args.Length != 0 && args[0] is string && args[1] is string && this.WorkerInfos.ContainsKey((string)args[0]))
                {
                    Console.WriteLine((string)args[2]);
                    WorkerInfo workerInfoToSet = null;
                    this.WorkerInfos.TryGetValue((string)args[0], out workerInfoToSet);
                    if (args.Length == 3)
                    {
                        //TODO: finish tryparses and edit float trys
                        float trys;
                        if (args[1].Equals("IsActive") && args[2] is bool)
                        {
                            workerInfoToSet.IsActive = (bool)args[2];
                            return true;
                        }
                        else if (args[1].Equals("Amount") && Single.TryParse((string)args[2], out trys))
                        {
                            workerInfoToSet.Amount = trys;
                            return true;
                        }
                        else if (args[1].Equals("BarSize") && args[2] is string)
                        {
                            workerInfoToSet.BarSize = (string)args[2];
                            return true;
                        }
                        else if (args[1].Equals("BarType") && args[2] is string)
                        {
                            workerInfoToSet.BarType = (string)args[2];
                            return true;
                        }
                        else if (args[1].Equals("PricePremiumPercentage") && args[2] is float)
                        {
                            workerInfoToSet.PricePremiumPercentage = (float)args[2];
                            return true;
                        }
                        else if (args[1].Equals("isCalculating") && args[2] is bool)
                        {
                            workerInfoToSet.isCalculating = (bool)args[2];
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("An exception occured: " + e.Message);
                return false;
            }
            return false;
        }

        public bool performAction(object[] args)
        {
            //start, stop, manualexecution
            if (args.Length != 0 && args[0] is string && args[1] is string)
            {
                if (args.Length == 2)
                {
                    if (args[1].Equals("Create"))
                    {
                        string temp = (string)args[0];
                        WorkerInfo tempWorkerInfo = new WorkerInfo(temp);
                        Console.WriteLine("Worker erstellt");
                        //TODO: start the Thread
                        this.WorkerInfos.Add(temp, tempWorkerInfo);
                        Console.WriteLine("geadded");
                        return true;
                    }
                    else if (args[1].Equals("Start"))
                    {
                        WorkerInfo tempWorkerInfo = null;
                        string temp = (string)args[0];
                        this.WorkerInfos.TryGetValue(temp, out tempWorkerInfo);
                        //TODO: start the Thread
                        return true;
                    }
                    else if (args[1].Equals("Stop"))
                    {
                        //TODO: stop the thread
                        return true;
                    }
                    else if (args.Length == 3)
                    {
                        if (args[1].Equals("ExecutionDecision") && args[2] is ManualExecution)
                        {
                            //TODO:ExecutionDecision
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}