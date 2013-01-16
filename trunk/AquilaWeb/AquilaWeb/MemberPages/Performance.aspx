<%@ Page Title="Performance" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Performance.aspx.cs" Inherits="AquilaWeb.MemberPages.Performance" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="header" runat="server">
    <h1>PERFORMANCE</h1>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="content" runat="server">
    <h2>
        <asp:Label ID="lbl_perf_header" runat="server" Text="" />
    </h2>
    <asp:Label ID="test" runat="server" />
    <asp:Chart ID="Chart1" runat="server" ImageLocation="~/ChartImgs/ChartPic_#SEQ(300,3)">
        <Series>
        </Series>
        <ChartAreas>
            <asp:ChartArea Name="ChartArea1"></asp:ChartArea>
        </ChartAreas>
    </asp:Chart>
    <asp:Chart ID="Chart2" runat="server" ImageLocation="~/ChartImgs/ChartPic_#SEQ(300,3)">
        <Series>
        </Series>
        <ChartAreas>
            <asp:ChartArea Name="ChartArea2"></asp:ChartArea>
        </ChartAreas>
    </asp:Chart>
</asp:Content>
