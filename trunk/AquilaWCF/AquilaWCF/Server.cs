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
                        double parseFloat;
                        if (args[1].Equals("IsActive") && Boolean.TryParse((string)args[2], out parseBool))
                        {
                            workerInfoToSet.IsActive = parseBool;
                            return true;
                        }
                        else if (args[1].Equals("Amount") && double.TryParse((string)args[2], out parseFloat))
                        {
                            workerInfoToSet.Amount = (float)parseFloat;
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
                        else if (args[1].Equals("CutLoss") && double.TryParse((string)args[2], out parseFloat))
                        {
                            workerInfoToSet.CutLoss = (float)parseFloat;
                            return true;
                        }
                        else if (args[1].Equals("PricePremiumPercentage") && double.TryParse((string)args[2], out parseFloat))
                        {
                            workerInfoToSet.PricePremiumPercentage = (float)parseFloat;
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

        public bool PerformAction(object[] args)
        {
            //start, stop, manualexecution
            if (args.Length != 0 && args[0] is string && args[1] is string)
            {
                if (args.Length == 2)
                {
                    if (args[1].Equals("Delete"))
                    {
                        if (WorkerInfos.ContainsKey((string)args[0]))
                        {
                            WorkerInfos.Remove((string)args[0]);
                            return true;
                        }
                        return false;
                    }
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
                else if (args.Length == 8)
                {
                    if (args[1].Equals("Create"))
                    {
                        // set equity
                        string name = (string)args[0];
                        WorkerInfo tempWorkerInfo = new WorkerInfo(name);
                        float parseFloat;
                        bool parseBool;

                        // set amount
                        if (Single.TryParse((string)args[2], out parseFloat))
                            tempWorkerInfo.Amount = parseFloat;

                        // set CutLoss
                        if (Single.TryParse((string)args[3], out parseFloat))
                            tempWorkerInfo.CutLoss = parseFloat;

                        //set IsActive
                        if (Boolean.TryParse((string)args[4], out parseBool))
                            tempWorkerInfo.IsActive = parseBool;

                        //set ppp
                        if (Single.TryParse((string)args[5], out parseFloat))
                            tempWorkerInfo.PricePremiumPercentage = parseFloat;

                        //set barsize
                        tempWorkerInfo.BarSize = (string)args[6];

                        //set BarType
                        tempWorkerInfo.BarType = (string)args[7];

                        Console.WriteLine("Worker erstellt");

                        //TODO: start the Thread
                        this.WorkerInfos.Add(name, tempWorkerInfo);
                        Console.WriteLine("geadded");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}