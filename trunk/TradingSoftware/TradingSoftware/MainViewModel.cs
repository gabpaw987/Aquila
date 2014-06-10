using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace TradingSoftware
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<ScrollViewer> _signalBoxes;

        public List<ScrollViewer> SignalBoxes
        {
            get
            {
                return _signalBoxes;
            }
            set
            {
                if (value != _signalBoxes)
                {
                    _signalBoxes = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("SignalBoxes"));
                }
            }
        }

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

        private List<WorkerViewModel> _workerViewModels;

        public List<WorkerViewModel> WorkerViewModels
        {
            get
            {
                return _workerViewModels;
            }
            set
            {
                if (value != _workerViewModels)
                {
                    _workerViewModels = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("WorkerViewModels"));
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

        private bool _creationShallIgnoreFirstSignal;

        public bool CreationShallIgnoreFirstSignal
        {
            get
            {
                return _creationShallIgnoreFirstSignal;
            }
            set
            {
                if (value != _creationShallIgnoreFirstSignal)
                {
                    _creationShallIgnoreFirstSignal = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationShallIgnoreFirstSignal"));
                }
            }
        }

        private bool _creationHasAlgorithmParameters;

        public bool CreationHasAlgorithmParameters
        {
            get
            {
                return _creationHasAlgorithmParameters;
            }
            set
            {
                if (value != _creationHasAlgorithmParameters)
                {
                    _creationHasAlgorithmParameters = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationHasAlgorithmParameters"));
                }
            }
        }

        private string _creationAlgorithmFilePath;

        public string CreationAlgorithmFilePath
        {
            get
            {
                return _creationAlgorithmFilePath;
            }
            set
            {
                if (value != _creationAlgorithmFilePath)
                {
                    _creationAlgorithmFilePath = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationAlgorithmFilePath"));
                }
            }
        }

        private string _creationAlgorithmParameters;

        public string CreationAlgorithmParameters
        {
            get
            {
                return _creationAlgorithmParameters;
            }
            set
            {
                if (value != _creationAlgorithmParameters)
                {
                    _creationAlgorithmParameters = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreationAlgorithmParameters"));
                }
            }
        }
    }
}