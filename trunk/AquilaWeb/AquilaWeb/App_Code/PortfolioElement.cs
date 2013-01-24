using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AquilaWeb.App_Code
{
    public class PortfolioElement
    {
        private string symbol;
        private decimal close;
        private int position;
        private decimal gain;
        private decimal maxinvest;
        private decimal cutloss;
        private decimal roi;
        private int decision;
        private bool auto;
        private bool active;

        public string Symbol
        {
            get { return this.symbol; }
            set { this.symbol = value; }
        }

        public decimal Close
        {
            get { return this.close; }
            set { this.close = value; }
        }

        public int Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        public decimal Gain
        {
            get { return this.gain; }
            set { this.gain = value; }
        }

        public decimal Maxinvest
        {
            get { return this.maxinvest; }
            set { this.maxinvest = value; }
        }

        public decimal Cutloss
        {
            get { return this.cutloss; }
            set { this.cutloss = value; }
        }

        public decimal Roi
        {
            get { return this.roi; }
            set { this.roi = value; }
        }

        public int Decision
        {
            get { return this.decision; }
            set { this.decision = value; }
        }

        public bool Auto
        {
            get { return this.auto; }
            set { this.auto = value; }
        }

        public bool Active
        {
            get { return this.active; }
            set { this.active = value; }
        }

        public PortfolioElement():this(null, 0, 0, 0, 0, 0, 0, 0, false, false)
        {

        }

        public PortfolioElement(string symbol,
                                decimal close,
                                int position,
                                decimal gain,
                                decimal maxinvest,
                                decimal cutloss,
                                decimal roi,
                                int decision,
                                bool auto,
                                bool active)
        {
            this.Symbol = symbol;
            this.Close = close;
            this.Position = position;
            this.Gain = gain;
            this.Maxinvest = maxinvest;
            this.Cutloss = cutloss;
            this.Roi = roi;
            this.Decision = decision;
            this.Auto = auto;
            this.Active = active;
        }
    }
}