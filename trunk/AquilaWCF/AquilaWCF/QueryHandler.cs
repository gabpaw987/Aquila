using System;
using System.Globalization;
using Krs.Ats.IBNet;

namespace Aquila_Software
{
    public class QueryHandler
    {
        /// <summary>
        /// Inserts the bar.
        /// </summary>
        /// <param name="equtiySymbol">The equtiy symbol.</param>
        /// <param name="bartype">The bartype.</param>
        /// <param name="volume">The volume.</param>
        /// <param name="statement">The statement.</param>
        /// <returns></returns>
        public static bool insertBar(string equtiySymbol, string bartype, int volume, Tuple<DateTime, decimal, decimal, decimal, decimal> statement)
        {
            if (DatabaseHandler.executeModify("INSERT INTO mBar VALUES('" + equtiySymbol + "','" + statement.Item1.ToString("yyyy-MM-dd HH:mm:ss zzz") + "'," + statement.Item2.ToString(CultureInfo.InvariantCulture) + "," + statement.Item3.ToString(CultureInfo.InvariantCulture) + "," + statement.Item4.ToString(CultureInfo.InvariantCulture) + "," + statement.Item5.ToString(CultureInfo.InvariantCulture) + "," + volume + ")") >= 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Inserts the signal.
        /// </summary>
        /// <param name="equtiySymbol">The equtiy symbol.</param>
        /// <param name="statement">The statement.</param>
        /// <param name="when">The when.</param>
        /// <returns></returns>
        public static bool insertSignal(string equtiySymbol, int statement, DateTime when)
        {
            if (DatabaseHandler.executeModify("INSERT INTO signal VALUES('" + equtiySymbol + "','" + when + "'," + statement + ")") >= 1)
            {
                return true;
            }
            return false;
        }

        public static bool insertOrder(string equtiySymbol, DateTime ten, DateTime tex, decimal pen, decimal pex, int size)
        {
            if (DatabaseHandler.executeModify("INSERT INTO sorder VALUES(1,'" + equtiySymbol + "','" + ten + "','" + tex + "','limit'," + pen.ToString(CultureInfo.InvariantCulture) + "," + pex.ToString(CultureInfo.InvariantCulture) + "," + size + "," + "true," + "1.0" + ")") >= 1)
            {
                return true;
            }
            return false;
        }

        public static bool insertIndicator(string iname, BarSize barsize)
        {
            string interval = "";

            if (barsize.Equals(BarSize.OneMinute))
            {
                interval = "1 minute";
            }
            else if (barsize.Equals(BarSize.OneDay))
            {
                interval = "1 day";
            }
            else
            {
                return false;
            }

            if (DatabaseHandler.executeModify("INSERT INTO indicator VALUES('" + iname + "','" + interval + "')") >= 1)
            {
                return true;
            }
            return false;
        }

        public static bool insertIndicatorValue(string equtiySymbol, string iname, DateTime t, decimal value)
        {
            if (DatabaseHandler.executeModify("INSERT INTO indicatorval VALUES('" + equtiySymbol + "','" + iname + "','" + t + "'," + value.ToString(CultureInfo.InvariantCulture) + ")") >= 1)
            {
                return true;
            }
            return false;
        }

        //DONE: insert indicator implementieren
        //Microsoft.FSharp.Collections.FSharpMap<string, decimal[]>
        //http://stackoverflow.com/questions/8522511/accessing-an-f-map-from-inside-c-sharp-code
    }
}