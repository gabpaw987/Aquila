<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Test_Search.aspx.cs" Inherits="AquilaWeb.Test_Search" %>

<%@ Register Src="~/Controls/SearchBox.ascx" TagPrefix="uc1" TagName="SearchBox" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <uc1:SearchBox runat="server" ID="SearchBox" />
    </div>
    </form>
</body>
</html>
