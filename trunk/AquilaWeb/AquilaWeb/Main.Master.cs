using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AquilaWeb
{
    public partial class Main : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /**
         * Changes HTML class of given menu item to "nav_selected"
         */
        public void initMenu(int i)
        {
            ((System.Web.UI.HtmlControls.HtmlGenericControl)this.FindControl("nav_i" + i)).Attributes["class"] = "nav_selected";
        }
    }
}