using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquilaWeb.App_Code
{
    public class DbParam
    {
        public string paramName;
        public NpgsqlDbType paramType;
        public Object paramValue;

        public DbParam(string paramName, NpgsqlDbType paramType, Object paramValue)
        {
            this.paramName = paramName;
            this.paramType = paramType;
            this.paramValue = paramValue;
        }
    }
}