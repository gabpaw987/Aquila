<%@ Page Title="Portfolio" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Portfolio.aspx.cs" Inherits="AquilaWeb.MemberPages.PortfolioPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
        
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="header" runat="server">
    <h1>PORTFOLIO</h1>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="content" runat="server">
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
        <Scripts>
            <asp:ScriptReference Path="~/Scripts/tables.js" />
        </Scripts>
    </asp:ScriptManagerProxy>

    <div id="del_dialog">
        <p class="message"></p>
    </div>

    <div id="performance">
        <h2 id="performance_nr">Profit/Loss <span><asp:Label id="lbl_pl_ur" runat="server" Text="" /><asp:Label id="lbl_pl_r" runat="server" Text="" /></span></h2>
        <h2 id="performance_exp">Invested <span><asp:Label id="lbl_invested" runat="server" Text="" /></span></h2>
    </div>
    <div id="table_wrapper">
        <asp:Table id="portfolio_table" class="stock_data_table" runat="server">
            <asp:TableHeaderRow TableSection="TableHeader">
                <asp:TableHeaderCell>Symbol</asp:TableHeaderCell>
                <asp:TableHeaderCell>Close</asp:TableHeaderCell>
                <asp:TableHeaderCell>Position</asp:TableHeaderCell>
                <asp:TableHeaderCell>Gain/Loss</asp:TableHeaderCell>
                <asp:TableHeaderCell>Max. Investment</asp:TableHeaderCell>
                <asp:TableHeaderCell>Cut-Loss</asp:TableHeaderCell>
                <asp:TableHeaderCell>Decision</asp:TableHeaderCell>
                <asp:TableHeaderCell>ROI</asp:TableHeaderCell>
                <asp:TableHeaderCell>Mode</asp:TableHeaderCell>
                <asp:TableHeaderCell>Status</asp:TableHeaderCell>
                <asp:TableHeaderCell></asp:TableHeaderCell>
            </asp:TableHeaderRow>
        </asp:Table>
    </div>
</asp:Content>
