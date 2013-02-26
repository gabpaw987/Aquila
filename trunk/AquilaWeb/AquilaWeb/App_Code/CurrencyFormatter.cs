using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace AquilaWeb.App_Code
{
    public sealed class CurrencyFormatter
    {
        public readonly static CultureInfo dollar;
        public readonly static CultureInfo euro;

        static CurrencyFormatter()
        { 
            CurrencyFormatter.dollar = CurrencyFormatter.CreateCultureInfo("$");
            CurrencyFormatter.euro = CurrencyFormatter.CreateCultureInfo("€");
        }

        private static CultureInfo CreateCultureInfo(string curr)
        {
            CultureInfo ci = new CultureInfo("en-US");
            ci = (CultureInfo)ci.Clone();
            ci.NumberFormat.CurrencySymbol = curr;
            return ci;
        }

        public static CultureInfo getCurrencyFormatter(string currency)
        {
            switch (currency)
            {
                case "EUR": return CurrencyFormatter.euro;
                case "USD": return CurrencyFormatter.dollar;
            }
            return CultureInfo.CurrentCulture;
        }
    }
}