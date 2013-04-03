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
                    WorkerInfo workerInfoToSet = null;
                    this.WorkerInfos.TryGetValue((string)args[0], out workerInfoToSet);
                    if (args.Length == 3)
                    {
                        bool parseBool;
                        float parseFloat;
                        if (args[1].Equals("IsActive") && Boolean.TryParse((string)args[2], out parseBool))
                        {
                            workerInfoToSet.IsActive = parseBool;
                            return true;
                        }
                        else if (args[1].Equals("Amount") && Single.TryParse((string)args[2], out parseFloat))
                        {
                            workerInfoToSet.Amount = parseFloat;
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
                        else if (args[1].Equals("PricePremiumPercentage") && Single.TryParse((string)args[2], out parseFloat))
                        {
                            workerInfoToSet.PricePremiumPercentage = parseFloat;
                            return true;
                        }
                        else if (args[1].Equals("isCalculating") && Boolean.TryParse((string)args[2], out parseBool))
                        {
                            workerInfoToSet.isCalculating = parseBool;
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
                    else if (args[1].Equals("Delete"))
                    {
                        if (WorkerInfos.ContainsKey((string)args[0]))
                        {
                            WorkerInfos.Remove((string)args[0]);
                            return true;
                        }
                        return false;
                    }
                    else if (args.Length == 3)
                    {
                        if (args[1].Equals("ExecutionDecision") && args[2] is string)
                        {
                            if (WorkerInfos.ContainsKey((string)args[0]))
                            {
                                WorkerInfo wi = null;
                                WorkerInfos.TryGetValue((string)args[0], out wi);
                                wi.ManualExecution = (string)args[2];
                                return true;
                            }
                            return false;
                        }
                    }
                }
            }
            return false;
        }
    }
}