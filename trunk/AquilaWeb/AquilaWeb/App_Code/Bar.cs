using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquilaWeb.App_Code
{
    public class Bar
    {
        public DateTime Time;
        public decimal Open;
        public decimal High;
        public decimal Low;
        public decimal Close;

        public Bar(DateTime t, decimal o, decimal h, decimal l, decimal c)
        {
            Time = t;
            Open = o;
            High = h;
            Low = l;
            Close = c;
        }
    }
}