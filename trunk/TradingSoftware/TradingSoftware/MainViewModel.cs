using System.Collections.Generic;
using System.ComponentModel;

namespace TradingSoftware
{
    internal class MainViewModel : INotifyPropertyChanged
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

        private bool _creationIsActive;

        public bool CreationIsActive
        {
            get
            {
                return _creationIsActive;
            }
            set
            {
                if (value != _creationIsActive)
                {
                    _creationIsActive = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationIsActive"));
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