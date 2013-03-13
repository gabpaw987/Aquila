<%@ Page Title="Change Presets" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="ChangePresets.aspx.cs" Inherits="AquilaWeb.MemberPages.ChangePresets" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="header" runat="server">
    <h1>SETTINGS</h1>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="content" runat="server">
    <h2>Change Presets for new Securities</h2>
    <!-- Cutloss, MaxInvestment, Mode -->
    <asp:Table runat="server">

        <asp:TableRow runat="server">
            <asp:TableCell runat="server">Cutloss [%]:</asp:TableCell>
            <asp:TableCell runat="server">
                <asp:TextBox ID="PresetCutLoss" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldPresetCutLoss" runat="server" 
                    ErrorMessage="No cut-loss entered" ControlToValidate="PresetCutLoss" ForeColor="Red"  Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidatorPresetCutLoss" runat="server" ControlToValidate="PresetCutLoss"
                    Type="Double" MinimumValue="0" MaximumValue="100"
                    ErrorMessage="Cut-loss must be a percentage" ForeColor="Red" Display="Dynamic"></asp:RangeValidator>
            </asp:TableCell>
        </asp:TableRow>

        <asp:TableRow runat="server">
            <asp:TableCell runat="server">Maximum Investment [$]:</asp:TableCell>
            <asp:TableCell runat="server">
                <asp:TextBox ID="PresetMaxInvest" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldPresetMaxInvest" runat="server" 
                    ErrorMessage="No maximum investment entered" ControlToValidate="PresetMaxInvest" ForeColor="Red"  Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:RangeValidator ID="RangeValidatorPresetMaxInvest" runat="server" ControlToValidate="PresetMaxInvest"
                    Type="Double" MinimumValue="0" ErrorMessage="Maximum Investment not valid" ForeColor="Red" Display="Dynamic"></asp:RangeValidator>
            </asp:TableCell>
        </asp:TableRow>

        <asp:TableRow runat="server">
            <asp:TableCell runat="server">Mode:</asp:TableCell>
            <asp:TableCell runat="server">
                <asp:DropDownList ID="PresetMode" runat="server">
                    <asp:ListItem runat="server">Automatic</asp:ListItem>
                    <asp:ListItem runat="server">Manual</asp:ListItem>
                </asp:DropDownList>
            </asp:TableCell>
        </asp:TableRow>

    </asp:Table>
    <asp:Button ID="Submit" runat="server" Text="Change" OnClick="SubmitPresets" />
</asp:Content>
