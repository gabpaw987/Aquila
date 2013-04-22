<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="AquilaWeb.MemberPages.SettingsPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="header" runat="server">
    <h1>SETTINGS</h1>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="content" runat="server">
    <ul ID="SettingsList" runat="server">
        <li><asp:HyperLink ID="ChangePwLink" runat="server" Text="Change Password" NavigateUrl="~/MemberPages/Settings/ChangePassword.aspx" /></li>
        <li><asp:HyperLink ID="ChangePortfolioLink" runat="server" Text="Portfolio Settings" NavigateUrl="~/MemberPages/Settings/ChangePortfolio.aspx" /></li>
        <li><asp:HyperLink ID="ChangePresetsLink" runat="server" Text="Change Presets" NavigateUrl="~/MemberPages/Settings/ChangePresets.aspx" /></li>
    </ul>
</asp:Content>
