using System;
using System.Globalization;
namespace TradingSoftware
{
    internal static class IBID
    {
        public static int ConnectionID = 1;
        public static int TickerID = 1;
        public static Object ConsoleTextLock = new Object();
        public static Object ConnectionLock = new Object();
        public static Object TickerLock = new Object();
        public static Object OrderLock = new Object();
        public static Object SoundLock = new Object();
        public static Object XMLReadLock = new Object();
        public static Object DataExportLock = new Object();
        
        public static int _orderId;
        public static int _noOfBarsGivenToAlgorithm;

        public static int OrderId
        {
            get
            {
                int newValue = int.Parse(XMLHandler.ReadValueFromXML(null, "orderId"), CultureInfo.InvariantCulture);

                if (!_orderId.Equals(newValue))
                {
                    _orderId = newValue;
                }

                return _orderId;
            }
            set
            {
                if (_orderId != value)
                {
                    _orderId = value;

                    XMLHandler.WriteValueToXML(null, "orderId", value.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        public static int NoOfBarsGivenToAlgorithm
        {
            get
            {
                int newValue = int.Parse(XMLHandler.ReadValueFromXML(null, "noOfBarsGivenToAlgorithm"), CultureInfo.InvariantCulture);

                if (!_noOfBarsGivenToAlgorithm.Equals(newValue))
                {
                    _noOfBarsGivenToAlgorithm = newValue;
                }

                return _noOfBarsGivenToAlgorithm;
            }
            set
            {
                if (_noOfBarsGivenToAlgorithm != value)
                {
                    _noOfBarsGivenToAlgorithm = value;

                    XMLHandler.WriteValueToXML(null, "noOfBarsGivenToAlgorithm", value.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
    }
}