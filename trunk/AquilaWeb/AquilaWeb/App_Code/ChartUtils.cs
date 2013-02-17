using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI.DataVisualization.Charting;

namespace AquilaWeb.App_Code
{
    public class ChartUtils
    {
        public static void InitChart(Chart c, PriceSeries s)
        {
            InitChart(c, s, null);
        }

        public static void InitChart(Chart c, PriceSeries s, string title)
        {
            Series price = new Series("price");
            c.Series.Add(price);

            // Set series chart type
            c.Series["price"].ChartType = SeriesChartType.Candlestick;

            // Set the style of the open-close marks
            c.Series["price"]["OpenCloseStyle"] = "Triangle";

            // Show both open and close marks
            c.Series["price"]["ShowOpenClose"] = "Both";

            // Set point width
            c.Series["price"]["PointWidth"] = "1.0";

            // Set colors bars
            c.Series["price"]["PriceUpColor"] = "Green";
            c.Series["price"]["PriceDownColor"] = "Red";

            for (int i = 0; i < s.Prices.Count; i++)
            {
                // adding date and high
                c.Series["price"].Points.AddXY(s.Prices[i].Time, s.Prices[i].High);
                // adding low
                c.Series["price"].Points[i].YValues[1] = Convert.ToDouble(s.Prices[i].Low);
                //adding open
                c.Series["price"].Points[i].YValues[2] = Convert.ToDouble(s.Prices[i].Open);
                // adding close
                c.Series["price"].Points[i].YValues[3] = Convert.ToDouble(s.Prices[i].Close);
            }

            if (s.BarType == PriceSeries.INTRADAY)
            {
                c.Series["price"].XValueMember = "Time";
                c.Series["price"].IsXValueIndexed = true;
            }
            else if (s.BarType == PriceSeries.YEAR)
            {
                c.Series["price"].XValueMember = "Day";
            }

            c.Series["price"].YValueMembers = "HighPrice, LowPrice, OpenPrice, ClosePrice";

            // Title
            c.ChartAreas[0].AxisX.Title = title;

            // Scale
            decimal minPrice = s.MinPrice();
            decimal maxPrice = s.MaxPrice();
            c.ChartAreas[0].AxisY.Minimum = Convert.ToDouble(Math.Round(minPrice - (maxPrice - minPrice) / 10));
            c.ChartAreas[0].AxisY.Maximum = Convert.ToDouble(Math.Round(maxPrice + (maxPrice - minPrice) / 10));

            // Grid Color
            c.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            c.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            // Colors
            //c.Series["price"].BorderColor = System.Drawing.Color.Black;
            c.Series["price"].Color = System.Drawing.Color.Black;
            c.Series["price"].CustomProperties = "PriceDownColor=Green, PriceUpColor=Red";
            c.Series["price"].XValueType = ChartValueType.Time;

            Series ma = new Series("ma");
            c.Series.Add(ma);
            c.Series[1].ChartType = SeriesChartType.FastLine;
            //Chart1.DataManipulator.FinancialFormula(FinancialFormula.MovingAverage, "2", "price:Y3", "ma");

            c.DataBind();
        }
    }
}