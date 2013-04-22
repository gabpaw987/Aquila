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
                    Type="Double" MinimumValue="0"
                    ErrorMessage="Capital must be positive" ForeColor="Red" Display="Dynamic"></asp:RangeValidator>
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
    <asp:Button ID="Submit" runat="server" Text="Change" OnClick="SubmitPortfolioSettings" />
</asp:Content>
