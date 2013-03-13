using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace AquilaWeb.App_Code
{
    public class Settings
    {
        // 1. Setting
        // 2. Symbol
        // 3. Wert

        // IsActive             manual/auto
        // Amount               Kapital bei 3.
        // BarSize (mBar, dBar)
        // BarType (Bid, Ask, Last/Trades, Midpoint)
        // PricePremiumPercentage
        // Running              Startet berechnung und Handel
        //public static 

        private string _user;
        private int _pfid;

        NpgsqlConnector _conn;

        public Settings() : this(Portfolio.PFID)
        {}

        public Settings(string username) : this(username, Portfolio.PFID)
        {}

        public Settings(int pfid) : 
            this(Membership.GetUser().ProviderUserKey.ToString(), pfid)
            //this(Membership.GetUser(HttpContext.Current.Request.LogonUserIdentity.Name).ProviderUserKey.ToString(), pfid)
        {}

        public Settings(string user, int pfid)
        {
            _user = user;
            _pfid = pfid;
            _conn = new NpgsqlConnector();
        }

        // negative -> error
        public int SetPresets(decimal cl, decimal mi, bool auto)
        {
            if (cl > 0 && cl < 100)
            {
                if (mi > 0)
                {
                    List<DbParam> pl = new List<DbParam>()
                    {
                        new DbParam("mi", NpgsqlDbType.Numeric, mi),
                        new DbParam("cl", NpgsqlDbType.Numeric, cl),
                        new DbParam("auto", NpgsqlDbType.Boolean, auto),
                        new DbParam("pfid", NpgsqlDbType.Integer, _pfid),
                    };
                    int rows = _conn.ExecuteDMLCommand("UPDATE pfcontrol SET maxinvest=:mi, cutloss=:cl, auto=:auto WHERE pfid=:pfid", pl);
                    if (rows == 1)
                    {
                        return 1;
                    }
                    else
                    {
                        return -3;
                    }
                }
                else
                {
                    // negative maximum investment
                    return -2;
                }
            }
            else
            {
                // invalid range for cut loss
                return -1;
            }
        }

        public decimal GetPresetCutLoss()
        {
            List<DbParam> pl = new List<DbParam>() 
            { 
                new DbParam("pfid", NpgsqlDbType.Integer, _pfid),
                new DbParam("user", NpgsqlDbType.Varchar, _user)
            };
            return _conn.SelectSingleValue<decimal>("SELECT cutloss FROM pfcontrol WHERE pfid=:pfid AND \"pId\"=:user", pl);
        }

        public decimal GetPresetMaxInvest()
        {
            List<DbParam> pl = new List<DbParam>() 
            { 
                new DbParam("pfid", NpgsqlDbType.Integer, _pfid),
                new DbParam("user", NpgsqlDbType.Varchar, _user)
            };
            return _conn.SelectSingleValue<decimal>("SELECT maxinvest FROM pfcontrol WHERE pfid=:pfid AND \"pId\"=:user", pl);
        }

        public bool GetPresetAuto()
        {
            List<DbParam> pl = new List<DbParam>() 
            { 
                new DbParam("pfid", NpgsqlDbType.Integer, _pfid),
                new DbParam("user", NpgsqlDbType.Varchar, _user)
            };
            return _conn.SelectSingleValue<bool>("SELECT auto FROM pfcontrol WHERE pfid=:pfid AND \"pId\"=:user", pl);
        }
    }
}