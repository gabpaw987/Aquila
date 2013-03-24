﻿using System;
using System.Globalization;

namespace Aquila_Software
{
    internal class QueryHandler
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
    }
}