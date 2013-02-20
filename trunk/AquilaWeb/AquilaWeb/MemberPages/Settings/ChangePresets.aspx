<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="ChangePresets.aspx.cs" Inherits="AquilaWeb.MemberPages.ChangePassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="header" runat="server">
    <h1>SETTINGS</h1>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="content" runat="server">
    <h2>Change Presets for new Securities</h2>
    <!-- Cutloss, MaxInvestment, Mode -->
    <asp:Table>
        <asp:TableRow>
            <asp:TableCell>Cutloss [%]:</asp:TableCell>
            <asp:TableCell>
                <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell>Maximum Investment [$]:</asp:TableCell>
            <asp:TableCell>
                <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell>Mode:</asp:TableCell>
            <asp:TableCell>
                <asp:DropDownList>
                    <asp:ListItem>Automatic</asp:ListItem>
                    <asp:ListItem>Manual</asp:ListItem>
                </asp:DropDownList>
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
</asp:Content>
