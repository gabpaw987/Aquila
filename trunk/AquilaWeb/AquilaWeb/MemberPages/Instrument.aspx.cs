using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.DataVisualization.Charting;
using Npgsql;
using AquilaWeb.App_Code;
using NpgsqlTypes;
using System.Drawing;

namespace AquilaWeb.MemberPages
{
    public partial class InstrumentPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // change menu to display current page
            ((Main)Master).initMenu(1);

            // wrong symbol: back to portfolio overview
            if (!Portfolio.isValidSymbol(Request["symbol"]))
            {
                Session["msg_text"] = "An instrument with the symbol " + Request["symbol"] + " could not be found.";
                Session["msg_type"] = -1;
                Response.Redirect("Portfolio.aspx");
            }

            string symbol = Request["symbol"];

            // set h1 text (page heading)
            heading.Text = symbol;

            PriceSeries s = new PriceSeries();

            ///////////////
            // INSTRUMENT INFO
            ///////////////

            Instrument ins = new Instrument(symbol);

            info_currency.Text = ins.Currency;
            info_exchange.Text = ins.Exchange;

            Button btCalculate = new Button();
            btCalculate.ID = "btCalculate";
            btCalculate.CommandArgument = symbol;
            // already in portfolio
            if (Portfolio.isInPortfolio(symbol))
            {
                btCalculate.Click += (bt_sender, args) => RemoveInstrumentFromPortfolio(symbol);
                btCalculate.Text = "Remove from portfolio";
            }
            // not yet in portfolio
            else
            {
                btCalculate.Click += (bt_sender, args) => AddInstrumentToPortfolio(symbol);
                btCalculate.Text = "Add to portfolio";
            }
            info_portfolio_button.Controls.Add(btCalculate);

            ///////////////
            // CHARTS
            ///////////////

            // INTRADAY
            s.LoadPrices(Request["symbol"], PriceSeries.INTRADAY);
            // data available
            if (s.Prices.Count > 0)
            {
                ChartUtils.InitChart(Chart1, s, "Intraday Chart");
            }
            // no data available
            else
            {
                instrument_charts.Controls.Remove(Chart1);
                Label intradayLabel = new Label();
                intradayLabel.Text = "No intraday-data available!";
                instrument_charts.Controls.AddAt(0,intradayLabel);
            }

            // YTD
            s.LoadPrices(Request["symbol"], PriceSeries.YEAR);
            // data available
            if (s.Prices.Count > 0)
            {
                ChartUtils.InitChart(Chart2, s, "YTD Chart");
            }
            else
            {
                instrument_charts.Controls.Remove(Chart2);
                Label ytdLabel = new Label();
                ytdLabel.Text = "No ytd-data available!";
                instrument_charts.Controls.AddAt(2, ytdLabel);
            }
        }

        protected void AddInstrumentToPortfolio(string symbol){
        
        }

        protected void RemoveInstrumentFromPortfolio(string symbol)
        {

        }
    }
}