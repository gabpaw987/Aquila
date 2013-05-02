<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Performance.aspx.cs" Inherits="AquilaWeb.MemberPages.Performance" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="header" runat="server">
    <h1>PERFORMANCE</h1>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="content" runat="server">
    <div id="performance">
        <h2 id="performance_nr">Profit/Loss <span><asp:Label id="lbl_pl_ur" runat="server" Text="" /><asp:Label id="lbl_pl_r" runat="server" Text="" /></span></h2>
        <h2 id="performance_exp">Invested <span><asp:Label id="lbl_invested" runat="server" Text="" /></span></h2><br />
    </div>
    
    <div ID="instrument_charts" runat="server">
        <asp:Chart ID="Chart1" Width="500px" runat="server" ImageLocation="~/ChartImgs/ChartPic_#SEQ(300,3)">
            <Series>
            </Series>
            <ChartAreas>
                <asp:ChartArea Name="ChartArea1"></asp:ChartArea>
            </ChartAreas>
        </asp:Chart>
        <div>
            <table>
            <tr>
                <td><big>Profitable Trades</big></td><td><big><asp:Label id="lbl_pt" runat="server" Text="" /></big></td>
            </tr>
            <tr>
                <td><big>Unprofitable Trades</td><td><big><asp:Label id="lbl_upt" runat="server" Text="" /></big></td>
            </tr>
            <tr>
                <td><big>Ratio Profitable/Unprofitable</td><td><big><asp:Label id="lbl_ratio" runat="server" Text="" /></big></td>
            </tr>
            </table>
        </div>
    </div>
</asp:Content>
