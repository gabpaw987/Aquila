using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;

namespace Aquila_Software
{
    internal class WCFCommunication
    {
        private static Dictionary<string, WorkerInfo> workerInfos;
        private static Dictionary<string, Worker> workers;
        private static int lastLength;

        public static void Main(string[] args)
        {
            LogFileManager.CreateLog("Aquila_Log_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt");

            WorkerInfo workerInfo = new WorkerInfo("AAPL:US");
            workerInfo.Amount = 800000;
            workerInfo.BarSize = "mBar";
            workerInfo.BarType = "last";
            workerInfo.CutLoss = 1000;
            workerInfo.IsActive = false;
            workerInfo.isCalculating = true;
            workerInfo.ManualExecution = "pending";
            workerInfo.PricePremiumPercentage = 100;

            WorkerInfo workerInfo2 = new WorkerInfo("MSFT:US");
            workerInfo2.Amount = 800000;
            workerInfo2.BarSize = "mBar";
            workerInfo2.BarType = "last";
            workerInfo2.CutLoss = 1000;
            workerInfo2.IsActive = false;
            workerInfo2.isCalculating = true;
            workerInfo2.ManualExecution = "pending";
            workerInfo2.PricePremiumPercentage = 100;

            Worker worker = new Worker(workerInfo);
            Worker worker2 = new Worker(workerInfo2);
            worker.Start();
            Thread.Sleep(100);
            worker2.Start();
            Thread.Sleep(20000);
            worker.executeOrder(); Thread.Sleep(10000);
            worker2.executeOrder();
        }

        private static void Main1(string[] args)
        {
            LogFileManager.CreateLog("Aquila_Log_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt");

            // Step 1 of the address configuration procedure: Create a URI to serve as the base address.
            Uri baseAddress = new Uri("http://localhost:8000/SettingsHandler");

            // Step 2 of the hosting procedure: Create ServiceHost
            //ServiceHost selfHost; = new ServiceHost(typeof(Server), baseAddress);
            workers = new Dictionary<string, Worker>();
            workerInfos = new Dictionary<string, WorkerInfo>();
            lastLength = 0;

            ServerHost selfHost = new ServerHost(workerInfos, typeof(Server), baseAddress);
            //selfHost.AddDefaultEndpoints();

            try
            {
                // Step 3 of the hosting procedure: Add a service endpoint.
                selfHost.AddServiceEndpoint(typeof(SettingsHandler), new WSHttpBinding(), "SettingsHandlerService");

                // Step 4 of the hosting procedure: Enable metadata exchange.
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                selfHost.Description.Behaviors.Add(smb);

                // Step 5 of the hosting procedure: Start (and then stop) the service.
                selfHost.Open();
                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press <ENTER> to terminate service.");
                Console.WriteLine();

                //TODO: make stoppable
                while (true)
                {
                    if (workerInfos.Count > lastLength)
                    {
                        foreach (WorkerInfo workerInfo in workerInfos.Values)
                        {
                            if (!workers.ContainsKey(workerInfo.Equity))
                            {
                                Worker tempWorker = new Worker(workerInfo);
                                //TODO: run tempWorker
                                workers.Add(workerInfo.Equity, tempWorker);
                            }
                        }
                        lastLength = workerInfos.Count;
                        Console.WriteLine("erste if");
                    }
                    else if (workerInfos.Count < lastLength)
                    {
                        foreach (Worker worker in workers.Values)
                        {
                            if (!workerInfos.ContainsKey(worker.Equity.Symbol))
                            {
                                //TODO: interrupt and delete worker
                            }
                        }
                        Console.WriteLine("zweite if");
                    }
                    //TODO: foreach both dictionaries for new manualexecution
                    foreach (WorkerInfo workerInfo in workerInfos.Values)
                    {
                        if (!workerInfo.IsActive)
                        {
                            if (workerInfo.ManualExecution.Equals(ManualExecution.Accepted))
                            {
                                Worker worker;
                                workers.TryGetValue(workerInfo.Equity, out worker);
                                if (worker != null)
                                {
                                    worker.executeOrder();
                                }
                            }
                            else if (workerInfo.ManualExecution.Equals(ManualExecution.Denied))
                            {
                                Worker worker;
                                workers.TryGetValue(workerInfo.Equity, out worker);
                                if (worker != null)
                                {
                                    worker.dismissOrder();
                                }
                            }
                        }
                    }
                    Thread.Sleep(1000);
                }

                // Close the ServiceHostBase to shutdown the service.
                selfHost.Close();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                Console.ReadLine();
                selfHost.Abort();
            }
        }
    }
}