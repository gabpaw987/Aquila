using System.Collections.Generic;
using System.ComponentModel;

namespace TradingSoftware
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<Worker> _workers;

        public List<Worker> Workers
        {
            get
            {
                return _workers;
            }
            set
            {
                if (value != _workers)
                {
                    _workers = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Workers"));
                }
            }
        }

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

        private string _creationSymbol;

        public string CreationSymbol
        {
            get
            {
                return _creationSymbol;
            }
            set
            {
                if (value != _creationSymbol)
                {
                    _creationSymbol = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationSymbol"));
                }
            }
        }

        private bool _creationIsTrading;

        public bool CreationIsTrading
        {
            get
            {
                return _creationIsTrading;
            }
            set
            {
                if (value != _creationIsTrading)
                {
                    _creationIsTrading = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationIsTrading"));
                }
            }
        }

        private decimal _creationAmount;

        public decimal CreationAmount
        {
            get
            {
                return _creationAmount;
            }
            set
            {
                if (value != _creationAmount)
                {
                    _creationAmount = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationAmount"));
                }
            }
        }

        private string _creationBarSize;

        public string CreationBarSize
        {
            get
            {
                return _creationBarSize;
            }
            set
            {
                if (value != _creationBarSize)
                {
                    _creationBarSize = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationBarSize"));
                }
            }
        }

        private string _creationDataType;

        public string CreationDataType
        {
            get
            {
                return _creationDataType;
            }
            set
            {
                if (value != _creationDataType)
                {
                    _creationDataType = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationDataType"));
                }
            }
        }

        private decimal _creationPricePremiumPercentage;

        public decimal CreationPricePremiumPercentage
        {
            get
            {
                return _creationPricePremiumPercentage;
            }
            set
            {
                if (value != _creationPricePremiumPercentage)
                {
                    _creationPricePremiumPercentage = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationPricePremiumPercentage"));
                }
            }
        }

        private decimal _creationCutLoss;

        public decimal CreationCutLoss
        {
            get
            {
                return _creationCutLoss;
            }
            set
            {
                if (value != _creationCutLoss)
                {
                    _creationCutLoss = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationCutLoss"));
                }
            }
        }
    }
}