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

        private int _creationRoundLotSize;

        public int CreationRoundLotSize
        {
            get
            {
                return _creationRoundLotSize;
            }
            set
            {
                if (value != _creationRoundLotSize)
                {
                    _creationRoundLotSize = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationRoundLotSize"));
                }
            }
        }

        private int _creationCurrentPosition;

        public int CreationCurrentPosition
        {
            get
            {
                return _creationCurrentPosition;
            }
            set
            {
                if (value != _creationCurrentPosition)
                {
                    _creationCurrentPosition = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationCurrentPosition"));
                }
            }
        }

        private bool _creationIsFuture;

        public bool CreationIsFuture
        {
            get
            {
                return _creationIsFuture;
            }
            set
            {
                if (value != _creationIsFuture)
                {
                    _creationIsFuture = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationIsFuture"));
                }
            }
        }
    }
}