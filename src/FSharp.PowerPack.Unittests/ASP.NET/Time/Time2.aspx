<!-- --------------------------
   Listing 14-4.
-->

<%@ Page Language="F#" 
         AutoEventWireup="true" 
         CodeFile="Time2.aspx.fs" 
         Inherits="FSharpWeb.Time2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
                      "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Current time</title>
    <style type="text/css">
        body { font-family:calibri,verdana,sans-serif; }
    </style>
</head>
<body>
    <form runat="server">
      The current time is:
      <asp:Label  runat="server" id="Time" />
      <asp:Button runat="server" id="Reload" text="Reload" OnClick="Reload_Click" />
    </form>
</body>
</html>
