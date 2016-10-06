using System;
using System.Data;
using Npgsql;

namespace Aquila_Software
{
    internal class DatabaseHandler
    {
        private static string server = "localhost";
        private static string port = "5432";
        private static string userid = "postgres";
        private static string pw = "short";
        private static string dbname = "aquila";

        /// <summary>
        /// Executes the select.
        /// </summary>
        /// <param name="statement">The statement.</param>
        /// <returns></returns>
        public static DataTable executeSelect(string statement)
        {
            string connstring = "Server=" + server + ";Port=" + port + ";User Id=" + userid + ";Password=" + pw + ";Database=" + dbname + ";";
            NpgsqlConnection nc = new NpgsqlConnection(connstring);
            nc.Open();
            DataSet ds = new DataSet();
            try
            {
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(statement, nc);
                ds.Reset();
                da.Fill(ds);
            }
            finally
            {
                nc.Close();
                string[] splittedStatement = statement.Split(' ');
                try
                {
                    LogFileManager.WriteToLog("Selected from database: " + dbname + " and " + "read from: " + splittedStatement[2]);
                }
                catch (Exception) { }
            }
            return ds.Tables[0];
        }

        /// <summary>
        /// Executes the modify.
        /// </summary>
        /// <param name="statement">The statement.</param>
        /// <returns></returns>
        public static int executeModify(string statement)
        {
            string connstring = "Server=" + server + ";Port=" + port + ";User Id=" + userid + ";Password=" + pw + ";Database=" + dbname + ";";
            NpgsqlConnection nc = new NpgsqlConnection(connstring);
            int rowsaffected = 0;
            try
            {
                nc.Open();
                NpgsqlCommand command = new NpgsqlCommand(statement, nc);
                rowsaffected = command.ExecuteNonQuery();
            }
            catch (Exception)
            {
            }
            finally
            {
                nc.Close();
                string[] splittedStatement = statement.Split(' ');
                try
                {
                    LogFileManager.WriteToLog("Modified database: " + dbname + " and " + "changed in: " + splittedStatement[1]);
                }
                catch (Exception) { }
            }
            return rowsaffected;
        }
    }
}