using AquilaWeb.App_Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AquilaWeb.MemberPages
{
    public partial class ChangePresets : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // change menu to display current page
            ((Main)Master).initMenu(3);

            if (!IsPostBack)
            {
                // set maxInvestment maximum value to total capital
                Portfolio pf = new Portfolio();
                RangeValidatorPresetMaxInvest.MaximumValue = (pf.GetCapital() - pf.GetSumMaxInvest()).ToString();

                // load current settings from DB
                FillInputsWithPresets();
            }
            else
            {
                
            }
        }

        private void FillInputsWithPresets()
        {
            AquilaWeb.App_Code.Settings s = new AquilaWeb.App_Code.Settings();

            PresetCutLoss.Text = Math.Round(s.GetPresetCutLoss(), 2).ToString();
            PresetMaxInvest.Text = Math.Round(s.GetPresetMaxInvest(), 2).ToString();
            PresetMode.SelectedIndex = (s.GetPresetAuto()) ? 0 : 1;
        }

        public void SubmitPresets(object sender, EventArgs e)
        {
            try
            { 
                // cut loss
                decimal cl = Decimal.Parse(PresetCutLoss.Text);
                // max invest
                decimal mi = Decimal.Parse(PresetMaxInvest.Text);
                // auto / manual
                bool auto = PresetMode.SelectedItem.Text == "Automatic" ? true : false;

                AquilaWeb.App_Code.Settings s = new AquilaWeb.App_Code.Settings();
                int status = s.SetPresets(cl, mi, auto);
            }
            catch(FormatException ex)
            {
                
            }
        }
    }
}