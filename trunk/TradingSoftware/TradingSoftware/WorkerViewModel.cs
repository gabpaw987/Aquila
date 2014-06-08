﻿using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TradingSoftware
{
    public class WorkerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _consoleText;

        public string ConsoleText
        {
            get
            {
                return _consoleText;
            }
            set
            {
                if (value != _consoleText)
                {
                    _consoleText = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ConsoleText"));
                }
            }
        }

        private string _signalText;

        public string SignalText
        {
            get
            {
                return _signalText;
            }
            set
            {
                if (value != _signalText)
                {
                    _signalText = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("SignalText"));
                }
            }
        }

        public bool _isTrading;

        [DisplayName("Is trading?")]
        public bool IsTrading
        {
            get 
            {
                bool newValue = XMLHandler.ReadValueFromXML(this.EquityAsString, "isTrading").Equals("true") ? true : false;
                if (!_isTrading.Equals(newValue))
                {
                    _isTrading = newValue;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsTrading"));
                }
                return _isTrading;
            }
            set
            {
                _isTrading = value;

                XMLHandler.WriteValueToXML(this.EquityAsString, "isTrading", _isTrading.Equals(true) ? "true" : "false");

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsTrading"));
            }
        }

        private Contract _equity;

        public Contract EquityAsContract
        {
            get
            {
                string tmpEquity = this.EquityAsString;
                return _equity;
            }
            set
            {
                string newEquitySymbol = "";

                if (this.IsFutureTrading)
                {
                    newEquitySymbol = ConvertFutureToString((Future)_equity);
                }
                else
                {
                    newEquitySymbol = _equity.Symbol;
                }

                this.EquityAsString = newEquitySymbol;
            }
        }

        [DisplayName("Symbol")]
        public string EquityAsString
        {
            get
            {
                string currentValue = "";
                if (this.IsFutureTrading)
                {
                    currentValue = ConvertFutureToString((Future)_equity);
                }
                else
                {
                    currentValue = _equity.Symbol;
                }

                string newValue = XMLHandler.ReadValueFromXML(currentValue, "symbol");
                if (!currentValue.Equals(newValue) && newValue.Length != 0)
                {
                    this.EquityAsString = newValue;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("EquityAsString"));
                }

                if (this.IsFutureTrading)
                {
                    return ConvertFutureToString((Future)_equity);
                }
                else
                {
                    return _equity.Symbol;
                }
            }
            set
            {
                if (value.Length != 0)
                {
                    if (_equity != null)
                    {
                        string currentValue = "";

                        if (this.IsFutureTrading)
                        {
                            currentValue = ConvertFutureToString((Future)_equity);
                        }
                        else
                        {
                            currentValue = _equity.Symbol;
                        }

                        if(currentValue != value)
                            XMLHandler.WriteValueToXML(currentValue, "symbol", value);
                    }

                    if (this.IsFutureTrading)
                    {
                        _equity = ConvertToFutures(value);
                    }
                    else
                    {
                        _equity = new Equity(value);
                    }


                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("EquityAsString"));
                }
            }
        }

        private BarSize _barsize;

        [DisplayName("Barsize")]
        public BarSize BarsizeAsObject
        {
            get
            {
                string tmpBarSize = this.BarsizeAsString;
                return _barsize;
            }
            set
            {
                this.BarsizeAsString = value.ToString();

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("BarsizeAsString"));
                    PropertyChanged(this, new PropertyChangedEventArgs("BarsizeAsObject"));
                }
            }
        }

        [DisplayName("Barsize")]
        public string BarsizeAsString
        {
            get 
            {
                string newValue = XMLHandler.ReadValueFromXML(this.EquityAsString, "barsize");
                if (!_barsize.Equals(newValue))
                {
                    this.BarsizeAsString = newValue;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("BarsizeAsString"));
                        PropertyChanged(this, new PropertyChangedEventArgs("BarsizeAsObject"));
                    }
                }
                return _barsize.ToString();
            }
            set
            {
                if (value.Equals("mBar") || value.Equals("Minute"))
                {
                    _barsize = BarSize.OneMinute;
                }
                else if (value.Equals("dBar") || value.Equals("Daily"))
                {
                    _barsize = BarSize.OneDay;
                }

                XMLHandler.WriteValueToXML(this.EquityAsString, "barsize", value);

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("BarsizeAsString"));
                    PropertyChanged(this, new PropertyChangedEventArgs("BarsizeAsObject"));
                }
            }
        }

        public HistoricalDataType _historicalType;
        public RealTimeBarType _realtimeType;

        [DisplayName("Data type")]
        public string DataType
        {
            get
            {
                string newValue = XMLHandler.ReadValueFromXML(this.EquityAsString, "dataType");
                if (!_historicalType.ToString().Equals(newValue) || !_realtimeType.ToString().Equals(newValue))
                {
                    this.DataType = newValue;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("DataType"));
                }

                if (_historicalType.ToString().Equals(_realtimeType.ToString()))
                    return _historicalType.ToString();
                else
                    return "inconsistent";
            }
            set
            {
                if (value.Equals("Bid"))
                {
                    _historicalType = HistoricalDataType.Bid;
                    _realtimeType = RealTimeBarType.Bid;
                }
                else if (value.Equals("Ask"))
                {
                    _historicalType = HistoricalDataType.Ask;
                    _realtimeType = RealTimeBarType.Ask;
                }
                else if (value.Equals("Last") || value.Equals("Trades"))
                {
                    _historicalType = HistoricalDataType.Trades;
                    _realtimeType = RealTimeBarType.Trades;
                }
                else if (value.Equals("Midpoint"))
                {
                    _historicalType = HistoricalDataType.Midpoint;
                    _realtimeType = RealTimeBarType.Midpoint;
                }

                XMLHandler.WriteValueToXML(this.EquityAsString, "dataType", value);

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("DataType"));
            }
        }

        public decimal _pricePremiumPercentage;

        [DisplayName("Price premium [%]")]
        public decimal PricePremiumPercentage
        {
            get { return _pricePremiumPercentage; }
            set
            {
                _pricePremiumPercentage = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("PricePremiumPercentage"));
            }
        }

        public int _currentPosition;

        [DisplayName("Cur. Position")]
        public int CurrentPosition
        {
            get { return _currentPosition; }
            set
            {
                _currentPosition = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentPosition"));
            }
        }

        public int _roundLotSize;

        [DisplayName("Round Lot Size")]
        public int RoundLotSize
        {
            get { return _roundLotSize; }
            set
            {
                _roundLotSize = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("RoundLotSize"));
            }
        }

        public bool _isFutureTrading;

        [DisplayName("FutureTrading")]
        public bool IsFutureTrading
        {
            get { return _isFutureTrading; }
            set
            {
                _isFutureTrading = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsFutureTrading"));
            }
        }

        public bool _shallIgnoreFirstSignal;

        [DisplayName("Shall Ignore First Signal")]
        public bool ShallIgnoreFirstSignal
        {
            get { return _shallIgnoreFirstSignal; }
            set
            {
                _shallIgnoreFirstSignal = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ShallIgnoreFirstSignal"));
            }
        }
        public bool _hasAlgorithmParameters;

        [DisplayName("Algorithm with parameters?")]
        public bool HasAlgorithmParameters
        {
            get { return _hasAlgorithmParameters; }
            set
            {
                _hasAlgorithmParameters = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("HasAlgorithmParameters"));
            }
        }


        public Dictionary<string, decimal> _parsedAlgorithmParameters;

        public Dictionary<string, decimal> ParsedAlgorithmParameters
        {
            get { return _parsedAlgorithmParameters; }
        }

        public string _algorithmParameters;

        public string AlgorithmParameters
        {
            get { return _algorithmParameters; }
            set
            {
                _algorithmParameters = value;
                if(value != null)
                    if(value.Length != 0)
                        _parsedAlgorithmParameters = this.parseAlgorithmParameters(value);

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AlgorithmParameters"));
                    PropertyChanged(this, new PropertyChangedEventArgs("ParsedAlgorithmParameters"));
                }
            }
        }

        private string _algorithmFilePath;

        public string AlgorithmFilePath
        {
            get
            {
                return _algorithmFilePath;
            }
            set
            {
                if (value != _algorithmFilePath)
                {
                    _algorithmFilePath = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("AlgorithmFilePath"));
                }
            }
        }

        public Future ConvertToFutures(string input)
        {
            string expiry = "201" + input.ElementAt(3);
            switch ((input.ElementAt(2) + "").ToUpper())
            {
                case ("Z"):
                    expiry += "12";
                    break;

                case ("Q"):
                    expiry += "09";
                    break;

                case ("H"):
                    expiry += "03";
                    break;

                case ("M"):
                    expiry += "06";
                    break;

                default:
                    break;
            }

            return new Future(input.ElementAt(0) + "" + input.ElementAt(1), "GLOBEX", expiry);
        }

        public string ConvertFutureToString(Future input)
        {
            string displayableExpiry = "";
            string month = ("" + input.Expiry.ElementAt(4) + input.Expiry.ElementAt(5));
            switch (month)
            {
                case ("12"):
                    displayableExpiry += "Z";
                    break;

                case ("09"):
                    displayableExpiry += "Q";
                    break;

                case ("03"):
                    displayableExpiry += "H";
                    break;

                case ("06"):
                    displayableExpiry += "M";
                    break;

                default:
                    break;
            }

            displayableExpiry += input.Expiry.ElementAt(3);

            return input.Symbol + displayableExpiry;
        }

        public Dictionary<string, decimal> parseAlgorithmParameters(string rawAlgorithmParameters)
        {
            Dictionary<string, decimal> parameters = new Dictionary<string, decimal>();

            if (rawAlgorithmParameters.Length != 0)
            {
                try
                {
                    string[] separatedAlgorithmParameters = rawAlgorithmParameters.Split('\n');
                    foreach (string parameter in separatedAlgorithmParameters)
                    {
                        string[] separatedParameter = parameter.Split(',');
                        parameters.Add(separatedParameter[0], decimal.Parse(separatedParameter[1], CultureInfo.InvariantCulture));
                    }
                }
                catch (Exception)
                {
                    this.ConsoleText += this.EquityAsString + ": Exception while parsing parameters.";
                    parameters = null;
                }
            }

            return parameters;
        }
    }
}
