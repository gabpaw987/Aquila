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
            if(XDocument.Load(settingsFilePath).Root.Elements("Worker").Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<WorkerTab> LoadWorkersFromXML(List<Worker> workers, MainViewModel mainViewModel)
        {
            List<WorkerTab> workerTabs = new List<WorkerTab>();
            if (checkIfXMLHasWorkers())
            {
                XDocument document = XDocument.Load(settingsFilePath);
                List<XElement> workerElements = (List<XElement>) document.Root.Elements("Worker");

                foreach (XElement workerElement in workerElements)
                {
                    WorkerTab workerTab = new WorkerTab();

                    bool isWorkerFurtureTrading = workerElement.Attribute("isFutureTrading").Value.Equals("True") ? true : false;

                    Worker worker = new Worker(mainViewModel, workerTab.workerViewModel,
                                               workerElement.Attribute("equity").Value,
                                               workerElement.Attribute("isTrading").Value.Equals("True") ? true : false,
                                               workerElement.Attribute("barsize").Value,
                                               workerElement.Attribute("datatype").Value,
                                               decimal.Parse(workerElement.Attribute("pricePremiumPercentage").Value, CultureInfo.InvariantCulture),
                                               isWorkerFurtureTrading ? int.Parse(workerElement.Attribute("roundLotSize").Value, CultureInfo.InvariantCulture) : 1,
                                               isWorkerFurtureTrading,
                                               int.Parse(workerElement.Attribute("currentPosition").Value, CultureInfo.InvariantCulture),
                                               workerElement.Attribute("shallIgnoreFirstSignal").Value.Equals("True") ? true : false,
                                               workerElement.Attribute("hasAlgorithmParameters").Value.Equals("True") ? true : false,
                                               workerElement.Attribute("algorithmFilePath").Value);

                }
            }
            return workerTabs;
        }

        public static bool CreateWorker(string equity, bool isTrading, string barsize, string dataType, string algorithmFilePath,
                                        decimal pricePremiumPercentage, bool isFutureTrading, int currentPosition,
                                        bool shallIgnoreFirstSignal, bool hasAlgorithmParameters, int roundLotSize,
                                        string algorithmParamters)
        {
            try
            {
                XDocument document = XDocument.Load(settingsFilePath);

                XElement workerElement = new XElement("Worker");
                workerElement.Add(new XAttribute("symbol", equity));
                workerElement.Add(new XAttribute("isTrading", isTrading));
                workerElement.Add(new XAttribute("barsize", barsize));
                workerElement.Add(new XAttribute("dataType", dataType));
                workerElement.Add(new XAttribute("pricePremiumPercentage", pricePremiumPercentage));
                workerElement.Add(new XAttribute("isFutureTrading", isFutureTrading));
                workerElement.Add(new XAttribute("shallIgnoreFirstSignal", shallIgnoreFirstSignal));
                workerElement.Add(new XAttribute("hasAlgorithmParameters", hasAlgorithmParameters));
                workerElement.Add(new XAttribute("algorithmFilePath", algorithmFilePath));

                if (isFutureTrading)
                {
                    workerElement.Add(new XAttribute("roundLotSize", roundLotSize));
                }

                if (hasAlgorithmParameters)
                {
                    workerElement.Add(new XElement("AlgorithmParameters", algorithmParamters));
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

            XDocument doc = XDocument.Load(settingsFilePath);

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

        /// <summary>
        /// Reads the data of specified node provided in the parameter
        /// </summary>
        /// <param name="pstrValueToRead">Node to be read</param>
        /// <returns>string containing the value</returns>
        private static string ReadValueFromXML(string pstrValueToRead)
        {
            try
            {
                //settingsFilePath is a string variable storing the path of the settings file 
                XPathDocument doc = new XPathDocument(settingsFilePath);
                XPathNavigator nav = doc.CreateNavigator();
                // Compile a standard XPath expression
                XPathExpression expr;
                expr = nav.Compile(@"/TradingSoftware/" + pstrValueToRead);
                XPathNodeIterator iterator = nav.Select(expr);
                // Iterate on the node set
                while (iterator.MoveNext())
                {
                    return iterator.Current.Value;
                }
                return string.Empty;
            }
            catch
            {
                //do some error logging here. Leaving for you to do 
                return string.Empty;
            }
        }

        /// <summary>
        /// Writes the updated value to XML
        /// </summary>
        /// <param name="pstrValueToRead">Node of XML to read</param>
        /// <param name="pstrValueToWrite">Value to write to that node</param>
        /// <returns></returns>
        private static bool WriteValueTOXML(string pstrValueToRead, string pstrValueToWrite)
        {
            try
            {
                //settingsFilePath is a string variable storing the path of the settings file 
                XmlTextReader reader = new XmlTextReader(settingsFilePath);
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                //we have loaded the XML, so it's time to close the reader.
                reader.Close();
                XmlNode oldNode;
                XmlElement root = doc.DocumentElement;
                oldNode = root.SelectSingleNode("/TradingSoftware/" + pstrValueToRead);
                oldNode.InnerText = pstrValueToWrite;
                doc.Save(settingsFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
