<%@ Page Title="Performance" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Instrument.aspx.cs" Inherits="AquilaWeb.MemberPages.InstrumentPage" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="header" runat="server">
    <h1><asp:Literal runat="server" ID="heading" /></h1>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="content" runat="server">
    <h2>
        <asp:Label ID="lbl_perf_header" runat="server" Text="" />
    </h2>
    <asp:Table ID="instrument_info_table" runat="server">
        <asp:TableRow>
            <asp:TableCell>Currency:</asp:TableCell>
            <asp:TableCell>
                <asp:Label ID="info_currency" runat="server" Text="" />
            </asp:TableCell>
            <asp:TableCell ID="info_portfolio_button" RowSpan="2" />
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell>Exchange:</asp:TableCell>
            <asp:TableCell>
                <asp:Label ID="info_exchange" runat="server" Text="" />
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
    <div ID="instrument_charts" runat="server">
        <asp:Chart ID="Chart1" Width="420px" runat="server" ImageLocation="~/ChartImgs/ChartPic_#SEQ(300,3)">
            <Series>
            </Series>
            <ChartAreas>
                <asp:ChartArea Name="ChartArea1"></asp:ChartArea>
            </ChartAreas>
        </asp:Chart>
        <asp:Chart ID="Chart2" Width="420px" runat="server" ImageLocation="~/ChartImgs/ChartPic_#SEQ(300,3)">
            <Series>
            </Series>
            <ChartAreas>
                <asp:ChartArea Name="ChartArea2"></asp:ChartArea>
            </ChartAreas>
        </asp:Chart>
        <asp:XmlDataSource ID="XmlDataSource1" runat="server" DataFile="http://feeds.finance.yahoo.com/rss/2.0/headline?s=aapl&region=US&lang=en-US" XPath="rss/channel/item"></asp:XmlDataSource>
        <asp:Panel ID="Panel1" runat="server" Width="260px" CssClass="rss">
            <asp:DataList ID="DataList1" runat="server" DataSourceID="XmlDataSource1" CellSpacing="2" GridLines="Horizontal" CellPadding="2" ShowFooter="False" ShowHeader="False">
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# XPath("pubDate") %>' ForeColor="gray"></asp:Label><br />
                    <asp:HyperLink ID="HyperLink1" runat="server" Text='<%# XPath("title") %>' NavigateUrl='<%# XPath("link") %>' Target="_blank"></asp:HyperLink><br />
                
                </ItemTemplate>
            </asp:DataList>
        </asp:Panel>
    </div>
</asp:Content>
