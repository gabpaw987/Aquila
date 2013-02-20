<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="AquilaWeb.MemberPages.Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="header" runat="server">
    <h1>SETTINGS</h1>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="content" runat="server">
    <ul ID="SettingsList" runat="server">
        <asp:HyperLink ID="ChangePwLink" runat="server" Text="Change Password" NavigateUrl="~/MemberPages/Settings/ChangePassword.aspx" />
        <asp:HyperLink ID="ChangePresets" runat="server" Text="Change Presets" NavigateUrl="~/MemberPages/Settings/ChangePresets.aspx" />
    </ul>
</asp:Content>
