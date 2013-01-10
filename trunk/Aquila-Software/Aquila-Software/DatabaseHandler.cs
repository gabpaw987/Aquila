using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.Data;

namespace Aquila_Software
{
    internal class DatabaseHandler
    {
        static string server = "localhost";
        static string port = "5432";
        static string userid = "testuser";
        static string pw = "testpw";
        static string dbname = "aquila";

        public static DataTable executeSelect(string statement)
        {
            string connstring = "Server="+server+";Port="+port+";User Id="+userid+";Password="+pw+";Database="+dbname+";";
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
                LogFileManager.WriteToLog("Selected from database: "+dbname+" and "+"read from: "+splittedStatement[2]);
            }
            return ds.Tables[0];
        }

        public static int executeModify(string statement)
        {
            string connstring = "Server=" + server + ";Port=" + port + ";User Id=" + userid + ";Password=" + pw + ";Database=" + dbname + ";";
            NpgsqlConnection nc = new NpgsqlConnection(connstring);
            nc.Open();
            NpgsqlCommand command = new NpgsqlCommand(statement, nc);
            int rowsaffected;
            try
            {
                rowsaffected = command.ExecuteNonQuery();
            }
            finally
            {
                nc.Close();
                string[] splittedStatement = statement.Split(' ');
                LogFileManager.WriteToLog("Modified database: " + dbname + " and " + "changed in: " + splittedStatement[1]);
            }
            return rowsaffected;
        }
    }
}
