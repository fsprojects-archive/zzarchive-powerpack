<%@ Page Language="F#" AutoEventWireup="true" CodeFile="DataBinding.aspx.fs" Inherits="FSharpWeb.DataBinding" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <style type="text/css">
      body { font-family:calibri,verdana,sans-serif; }
    </style>
</head>
<body>
    <form runat="server">
    <div>
      <h1>ASP.NET F# Intro</h1>
      <p>Other samples: <a href="default.aspx">Factorial</a> | <a href="databinding.aspx">DataBinding</a></p>
      
      <h2>Displaying data</h2>
      <asp:Button id="btnTest" runat="server" text="Click me!" OnClick="ButtonClicked" /><br />
      <ul>
      <asp:Repeater id="rptData" runat="server">
        <itemtemplate>
          <li style="color:<%# this.Eval("Rgb") %>"><%# this.Eval("Name") %></li>
        </itemtemplate>
      </asp:Repeater>
      </ul>
    </div>
    </form>
</body>
</html>
