using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.Data;

namespace Aquila_Software
{
    internal class QueryHandler
    {
        static string server = "localhost";
        static string port = "5432";
        static string userid = "testuser";
        static string pw = "testpw";
        static string dbname = "aquila";

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
            if(DatabaseHandler.executeModify("INSERT INTO mBar VALUES('" + equtiySymbol + "','" + statement.Item1.ToString("YYYY-MM-DD HH:mm:ss zzz") + "'," + statement.Item2 + "," + statement.Item3 + "," + statement.Item4 + "," + statement.Item5 + "," + volume + ")") >=1)
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
