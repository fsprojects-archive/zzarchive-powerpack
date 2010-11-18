<%@ Page Language="F#" AutoEventWireup="true" CodeFile="Default.aspx.fs" Inherits="FSharpWeb.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
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
      
      <h2>Calculating factorial</h2>
      <asp:Button id="Button1" runat="server" text="Click me!" OnClick="ButtonClicked" /><br />
      <asp:Label id="Label1" runat="server" />
    </div>
    </form>
</body>
</html>
