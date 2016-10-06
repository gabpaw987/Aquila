using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AquilaWeb.MemberPages
{
    public partial class ChangePortfolio : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // change menu to display current page
            ((Main)Master).initMenu(3);

            if (!IsPostBack)
            {
                // load current settings from DB
                FillInputsWithPresets();
            }
        }

        private void FillInputsWithPresets()
        {
            AquilaWeb.App_Code.Settings s = new AquilaWeb.App_Code.Settings();

            Capital.Text = Math.Round(s.GetPortfolioCapital(), 2).ToString();
            PricePremium.Text = Math.Round(s.GetPortfolioPPP(), 2).ToString();
            BarSize.SelectedIndex = (s.GetPortfolioBarSize().Equals("mBar")) ? 0 : 1;
            BarType.SelectedValue = s.GetPortfolioBarType();
        }

        protected void SubmitPortfolioSettings(object sender, EventArgs e)
        {
            try
            {
                // total investment capital
                decimal capital = Decimal.Parse(Capital.Text);
                // price premium percentage
                decimal ppp = Decimal.Parse(PricePremium.Text);
                // bar size
                string bsize = (BarSize.SelectedValue.Equals("Minute Bars")) ? "mBar" : "dBar";
                // bar type
                string btype = BarType.SelectedValue;

                AquilaWeb.App_Code.Settings s = new AquilaWeb.App_Code.Settings();
                s.SetPortfolioSettings(capital, ppp, bsize, btype);
            }
            catch (FormatException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }
    }
}