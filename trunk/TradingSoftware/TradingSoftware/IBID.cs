using System;
namespace TradingSoftware
{
    internal static class IBID
    {
        public static int ConnectionID = 1;
        public static int OrderID = 1;
        public static int TickerID = 1;
        public static Object ConsoleTextLock = new Object();
        public static Object ConnectionLock = new Object();
        public static Object TickerLock = new Object();
        public static Object OrderLock = new Object();
        public static Object SoundLock = new Object();
    }
}