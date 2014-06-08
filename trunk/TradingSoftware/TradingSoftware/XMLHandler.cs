using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace TradingSoftware
{
    static class XMLHandler
    {
        private static string settingsFilePath = "settings.xml";
        private static string schemaFilePath = "settings.xsd";

        public static bool CreateSettingsFileIfNecessary()
        {
            if(!File.Exists(settingsFilePath)){
                try
                {
                    XDocument document = new XDocument();
                    XElement rootElement = new XElement("TradingSoftware");
                    document.Add(rootElement);

                    document.Save(settingsFilePath);

                    if (ValidateXMLDocument(document))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public static bool checkIfXMLHasWorkers()
        {
            lock (IBID.XMLReadLock)
            {
                if (XDocument.Load(settingsFilePath).Root.Elements("Worker").Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool RemoveWorker(string workerSymbol)
        {
            XDocument document = null;
            lock (IBID.XMLReadLock)
            {
                document = XDocument.Load(settingsFilePath);
            }
            foreach(XElement workerElement in document.Root.Elements("Worker"))
            {
                if(workerElement.Attribute("symbol").Value.Equals(workerSymbol))
                {
                    workerElement.Remove();

                    document.Save(settingsFilePath);

                    if (ValidateXMLDocument(document))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public static void LoadWorkersFromXML(MainWindow mainWindow)
        {
            if (checkIfXMLHasWorkers())
            {
                XDocument document = null;
                lock (IBID.XMLReadLock)
                {
                    document = XDocument.Load(settingsFilePath);
                }
                List<XElement> workerElements = document.Root.Elements("Worker").ToList();

                foreach (XElement workerElement in workerElements)
                {
                    WorkerTab workerTab = new WorkerTab(mainWindow);

                    bool hasAlgorithmParameters = workerElement.Attribute("hasAlgorithmParameters").Value.Equals("true") ? true : false;
                    bool isWorkerFurtureTrading = workerElement.Attribute("isFutureTrading").Value.Equals("true") ? true : false;

                    string algorithmParameters = "";
                    if (hasAlgorithmParameters && workerElement.Value != null && workerElement.Value.Length != 0)
                    {
                        algorithmParameters = workerElement.Value;
                    }

                    Worker worker = new Worker(mainWindow.mainViewModel, workerTab.workerViewModel,
                                               workerElement.Attribute("symbol").Value,
                                               workerElement.Attribute("isTrading").Value.Equals("true") ? true : false,
                                               workerElement.Attribute("barsize").Value,
                                               workerElement.Attribute("dataType").Value,
                                               decimal.Parse(workerElement.Attribute("pricePremiumPercentage").Value, CultureInfo.InvariantCulture),
                                               isWorkerFurtureTrading ? int.Parse(workerElement.Attribute("roundLotSize").Value, CultureInfo.InvariantCulture) : 1,
                                               isWorkerFurtureTrading,
                                               int.Parse(workerElement.Attribute("currentPosition").Value, CultureInfo.InvariantCulture),
                                               workerElement.Attribute("shallIgnoreFirstSignal").Value.Equals("true") ? true : false,
                                               hasAlgorithmParameters,
                                               workerElement.Attribute("algorithmFilePath").Value,
                                               algorithmParameters);

                    

                    worker.Start();

                    mainWindow.mainViewModel.Workers.Add(worker);
                    workerTab.setUpTabWorkerConnection(worker);
                    mainWindow.mainViewModel.WorkerViewModels.Add(workerTab.workerViewModel);

                    mainWindow.MainTabControl.Items.Insert(mainWindow.MainTabControl.Items.Count - 1, workerTab);
                }
                mainWindow.workersGrid.Items.Refresh();
            }
        }

        public static bool CreateWorker(string equity, bool isTrading, string barsize, string dataType, string algorithmFilePath,
                                        decimal pricePremiumPercentage, bool isFutureTrading, int currentPosition,
                                        bool shallIgnoreFirstSignal, bool hasAlgorithmParameters, int roundLotSize,
                                        string algorithmParamters)
        {
            try
            {
                XDocument document = null;
                lock (IBID.XMLReadLock)
                {
                    document = XDocument.Load(settingsFilePath);
                }

                XElement workerElement = new XElement("Worker");
                workerElement.Add(new XAttribute("symbol", equity));
                workerElement.Add(new XAttribute("isTrading", isTrading));
                workerElement.Add(new XAttribute("barsize", barsize));
                workerElement.Add(new XAttribute("dataType", dataType));
                workerElement.Add(new XAttribute("pricePremiumPercentage", pricePremiumPercentage));
                workerElement.Add(new XAttribute("isFutureTrading", isFutureTrading));
                workerElement.Add(new XAttribute("currentPosition", currentPosition));
                workerElement.Add(new XAttribute("shallIgnoreFirstSignal", shallIgnoreFirstSignal));
                workerElement.Add(new XAttribute("hasAlgorithmParameters", hasAlgorithmParameters));
                workerElement.Add(new XAttribute("algorithmFilePath", algorithmFilePath));

                if (isFutureTrading)
                {
                    workerElement.Add(new XAttribute("roundLotSize", roundLotSize));
                }

                if (hasAlgorithmParameters)
                {
                    workerElement.Value = algorithmParamters;
                }

                document.Root.Add(workerElement);

                document.Save(settingsFilePath);

                if (ValidateXMLDocument(document))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool ValidateXMLDocument(XDocument documentToValidate)
        {
            bool wasValidationSuccessful = true;
            XDocument doc = null;
            lock (IBID.XMLReadLock)
            {
                doc = XDocument.Load(settingsFilePath);
            }

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.Add(null, schemaFilePath);

            XmlReaderSettings xrs = new XmlReaderSettings();
            xrs.ValidationType = ValidationType.Schema;
            xrs.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            xrs.Schemas = schemaSet;
            xrs.ValidationEventHandler += (o, s) =>
            {
                wasValidationSuccessful = false;
                //To write validation errors into the console
                //Console.WriteLine("{0}: {1}", s.Severity, s.Message);
            };

            using (XmlReader xr = XmlReader.Create(doc.CreateReader(), xrs))
            {
                while (xr.Read()) { }
            }

            return wasValidationSuccessful;
        }

        public static string ReadValueFromXML(string workerSymbol, string attributeToRead)
        {
            try
            {
                XDocument document = null;
                lock (IBID.XMLReadLock)
                {
                    document = XDocument.Load(settingsFilePath);
                }
                foreach (XElement workerElement in document.Root.Elements("Worker"))
                {
                    if (workerElement.Attribute("symbol").Value.Equals(workerSymbol))
                    {
                        return workerElement.Attribute(attributeToRead).Value;
                    }
                }                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool WriteValueToXML(string workerSymbol, string attributeToRead, string valueToWrite)
        {
            try
            {
                XDocument document = null;
                lock (IBID.XMLReadLock)
                {
                    document = XDocument.Load(settingsFilePath);
                }
                foreach (XElement workerElement in document.Root.Elements("Worker"))
                {
                    if (workerElement.Attribute("symbol").Value.Equals(workerSymbol))
                    {
                        workerElement.Attribute(attributeToRead).Value = valueToWrite;

                        document.Save(settingsFilePath);

                        if (ValidateXMLDocument(document))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
