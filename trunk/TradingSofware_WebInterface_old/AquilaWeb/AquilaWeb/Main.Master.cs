using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace AquilaWeb
{
    public partial class Main : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // check session
            if (Session["msg_text"] != null && Session["msg_text"] is string)
            {
                int type = (Session["msg_type"] != null) ? (int)Session["msg_type"] : 0;
                setUserMessage((string)Session["msg_text"], type);

                Session.Remove("msg_text");
                Session.Remove("msg_type");
            }
        }

        /**
         * Changes HTML class of given menu item to "nav_selected"
         */
        public void initMenu(int i)
        {
            ((System.Web.UI.HtmlControls.HtmlGenericControl)this.FindControl("nav_i" + i)).Attributes["class"] = "nav_selected";
        }

        public void setUserMessage(string msg, int type)
        {
            usr_msg.Controls.Add(new LiteralControl(msg));

            switch (type)
            {
                case -1:    usr_msg.CssClass = "usr_error";
                    break;
                case 0:     usr_msg.CssClass = "usr_msg";
                    break;
                case 1:     usr_msg.CssClass = "usr_success";
                    break;
            }
        }
    }
}