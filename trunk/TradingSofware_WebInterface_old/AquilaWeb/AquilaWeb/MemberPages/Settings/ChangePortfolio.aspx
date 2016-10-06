<%@ Page Title="Portfolio Settings" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="ChangePortfolio.aspx.cs" Inherits="AquilaWeb.MemberPages.ChangePortfolio" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="header" runat="server">
    <h1>SETTINGS</h1>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="content" runat="server">
    <h2>Portfolio Settings</h2>
    <!-- Cutloss, MaxInvestment, Mode -->
    <asp:Table ID="Table1" runat="server">

        <asp:TableRow ID="TableRow1" runat="server">
            <asp:TableCell ID="TableCell1" runat="server">Total Investment Capital [$]:</asp:TableCell>
            <asp:TableCell ID="TableCell2" runat="server">
                <asp:TextBox ID="Capital" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldCapital" runat="server" 
                    ErrorMessage="No capital entered" ControlToValidate="Capital" ForeColor="Red"  Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidatorCapital" runat="server" ControlToValidate="Capital"
                    Type="Double" MinimumValue="0" MaximumValue="100000000000000000"
                    ErrorMessage="Capital must be positive" ForeColor="Red" Display="Dynamic"></asp:RangeValidator>
            </asp:TableCell>
        </asp:TableRow>

        <asp:TableRow ID="TableRow2" runat="server">
            <asp:TableCell ID="TableCell3" runat="server">Price Premium Percentage [%]:</asp:TableCell>
            <asp:TableCell ID="TableCell4" runat="server">
                <asp:TextBox ID="PricePremium" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldPricePremium" runat="server" 
                    ErrorMessage="No price premium entered" ControlToValidate="PricePremium" ForeColor="Red"  Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidatorPricePremium" runat="server" ControlToValidate="PricePremium"
                    Type="Double" MinimumValue="0" MaximumValue="100"
                    ErrorMessage="Price Premium must be a percentage" ForeColor="Red" Display="Dynamic"></asp:RangeValidator>
            </asp:TableCell>
        </asp:TableRow>

        <asp:TableRow ID="TableRow3" runat="server">
            <asp:TableCell ID="TableCell5" runat="server">Bar Size:</asp:TableCell>
            <asp:TableCell ID="TableCell6" runat="server">
                <asp:DropDownList ID="BarSize" runat="server">
                    <asp:ListItem runat="server">Minute Bars</asp:ListItem>
                    <asp:ListItem runat="server">Daily Bars</asp:ListItem>
                </asp:DropDownList>
            </asp:TableCell>
        </asp:TableRow>

        <asp:TableRow ID="TableRow4" runat="server">
            <asp:TableCell ID="TableCell7" runat="server">Bar Type:</asp:TableCell>
            <asp:TableCell ID="TableCell8" runat="server">
                <asp:DropDownList ID="BarType" runat="server">
                    <asp:ListItem runat="server">Ask</asp:ListItem>
                    <asp:ListItem runat="server">Last</asp:ListItem>
                    <asp:ListItem runat="server">Bid</asp:ListItem>
                    <asp:ListItem runat="server">Midpoint</asp:ListItem>
                </asp:DropDownList>
            </asp:TableCell>
        </asp:TableRow>

    </asp:Table>
    <asp:Button ID="Submit" runat="server" Text="Change" OnClick="SubmitPortfolioSettings" />
</asp:Content>
