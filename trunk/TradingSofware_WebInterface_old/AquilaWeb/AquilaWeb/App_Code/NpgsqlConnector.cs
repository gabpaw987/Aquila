using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using AquilaWeb.App_Code;

/// <summary>
/// Zusammenfassungsbeschreibung für NPGSQLConnector
/// </summary>
public class NpgsqlConnector
{
    // FIELDS //

    private NpgsqlConnection _conn;
    private bool _connected;
    private bool _closeAfterQuery;
    private NpgsqlTransaction _t;

    // PROPERTIES //

    public bool Connected
    {
        get { return this._connected; }
        set
        {
            // establish connection if true and currently closed
            if (value == true && this._connected == false) this._conn.Open();
            // close connection if false and currently open
            else if (value == false && this._connected == true) this._conn.Close();
            this._connected = value;
        }
    }

    public bool CloseAfterQuery
    {
        get { return this._closeAfterQuery; }
        set { this._closeAfterQuery = value; }
    }

    // CONSTRUCTORS //

    public NpgsqlConnector():this("aquila"){ }

    public NpgsqlConnector(string database) : this("127.0.0.1", "postgres", "short", database) { }

	public NpgsqlConnector(string server, string user, string pass, string database)
	{
        _connected = false;
        _closeAfterQuery = false;
        
        _conn = new NpgsqlConnection("Server="              + server + 
                                    ";Port=5432;User Id="  + user + 
                                    ";Password="           + pass + 
                                    ";Database="           + database + 
                                    ";SSL=true;Sslmode=prefer;Pooling=true");
	}

    public static NpgsqlConnection Connect()
    {
        return new NpgsqlConnection("Server=127.0.0.1;" +
                                    "Port=5432;User Id=postgres;" + 
                                    "Password=short;" + 
                                    "Database=aquila;" + 
                                    "SSL=true;Sslmode=prefer;Pooling=true");
    }

    public void StartTransaction()
    {
        if (Connected == false) Connected = true;
        // TODO: TransactionIsolationLevel
        _t = _conn.BeginTransaction();
    }

    public void Commit()
    {
        _t.Commit();
    }

    public void Rollback()
    {
        _t.Rollback();
    }

    public T SelectSingleValue<T>(string sql)
    {
        return SelectSingleValue<T>(sql, new List<DbParam>());
    }

    public T SelectSingleValue<T>(string sql, List<DbParam> p)
    {
        // Open connection if neccessary
        if (!Connected) Connected = true;

        using(NpgsqlCommand command = new NpgsqlCommand(sql, _conn))
        {
            foreach (DbParam e in p)
            {
                command.Parameters.Add(new NpgsqlParameter(e.paramName, e.paramType));
                command.Parameters[command.Parameters.Count-1].Value = e.paramValue;
            }

            Object scalar = new Object();
            try
            {
                scalar = command.ExecuteScalar();
                if (scalar == null || scalar is DBNull) scalar = default(T);
                return (T)scalar;
            }
            //catch (Exception e)
            //{
                //return default(T);
            //}
            finally
            {
                // Close db connection if closeAfterQuery is set
                if (CloseAfterQuery) Connected = false;
            }
        }
    }

    //public decimal SelectSingleDecimal(string sql)
    //{
    //    return SelectSingleValue<decimal>(sql, new List<DbParam>());
    //    // return (!(o is DBNull)) ? Convert.ToDecimal(o) : 0m;

    //    //if (!(o is DBNull))
    //    //{
    //    //    return Convert.ToDecimal(o);
    //    //}
    //    //else
    //    //{
    //    //    return 0m;
    //    //}
    //}

    //public string SelectSingleString(string sql)
    //{
    //    Object o = SelectSingleValue(sql);
    //    return !(o is DBNull) ? Convert.ToString(o) : null;
    //}

    public NpgsqlDataReader Select(string sql)
    {
        return Select(sql, new List<DbParam>());
    }

    public NpgsqlDataReader Select(string sql, List<DbParam> p)
    {
        // Open connection if neccessary
        if (!Connected) Connected = true;

        using (NpgsqlCommand command = new NpgsqlCommand(sql, _conn))
        {
            foreach (DbParam e in p)
            {
                command.Parameters.Add(new NpgsqlParameter(e.paramName, e.paramType));
                command.Parameters[command.Parameters.Count - 1].Value = e.paramValue;
            }

            NpgsqlDataReader dr;
            try
            {
                dr = command.ExecuteReader();
                return dr;
            }
            //catch
            //{ return null; }
            finally
            {

            }
        }
    }

    public int Insert(string sql)
    {
        return ExecuteDMLCommand(sql, new List<DbParam>());
    }

    public int Update(string sql)
    {
        return ExecuteDMLCommand(sql, new List<DbParam>());
    }

    public int ExecuteDMLCommand(string sql, List<DbParam> p)
    {
        // Open connection if neccessary
        if (!Connected) Connected = true;

        using (NpgsqlCommand command = new NpgsqlCommand(sql, _conn))
        {

            foreach (DbParam e in p)
            {
                command.Parameters.Add(new NpgsqlParameter(e.paramName, e.paramType));
                command.Parameters[command.Parameters.Count - 1].Value = e.paramValue;
            }

            try
            {
                return command.ExecuteNonQuery();
            }
            finally
            {
                // Close db connection if closeAfterQuery is set
                if (CloseAfterQuery) Connected = false;
            }
        }
    }
}