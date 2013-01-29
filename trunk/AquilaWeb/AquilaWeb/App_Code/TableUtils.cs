using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace AquilaWeb.App_Code
{
    public class TableUtils
    {
        public static void addTextCell(TableRow tRow, string text)
        {
            addTextCell(tRow, text, String.Empty);
        }

        public static void addTextCell(TableRow tRow, string text, string cssclass)
        {
            addTextCell(tRow, text, cssclass, null);
        }

        public static void addTextCell(TableRow tRow, string text, string cssclass, string id)
        {
            addTextCell(tRow, text, cssclass, null, false);
        }

        public static void addTextCell(TableRow tRow, string text, string cssclass, string id, bool editCell)
        {
            TableCell tCell = new TableCell();
            if (id != null) tCell.ID = id;
            tCell.Text = text;
            if (cssclass != null) tCell.CssClass = cssclass;
            if (editCell) tCell.Attributes.Add("onclick", "edit_cell(this)");
            tRow.Cells.Add(tCell);
        }

        public static void addTextCell(TableRow tRow, string text, string cssclass, string id, bool editCell, string href)
        {
            TableCell tCell = new TableCell();
            if (id != null) tCell.ID = id;
            if (cssclass != null) tCell.CssClass = cssclass;
            if (editCell) tCell.Attributes.Add("onclick", "edit_cell(this)");

            if (href != null)
            {
                HyperLink a = new HyperLink();
                a.NavigateUrl = href;
                a.Text = text;
                tCell.Controls.Add(a);
            }
            else
            {
                tCell.Text = text;
            }
            tRow.Cells.Add(tCell);
        }
    }
}