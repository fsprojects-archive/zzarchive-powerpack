<%@ Page Language="F#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
                      "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script language="F#" runat="server">
     /// F# scripts embedded in ASPX pages must be a set of 'member' declarations.
     /// ASP.NET inserts these into the code generated for the page object type.

     /// This member is invoked on the server when the page is loaded. It tests
     /// whether the page was loaded for the first time and updates the content of
     /// this.Time control.
     member this.Form1_Load(sender: obj, e: EventArgs) =
         if not this.Page.IsPostBack then
             this.Time.Text <- DateTime.Now.ToString()

     /// This member is invoked on the server when the Reload button is clicked.
     member this.Reload_Click(sender: obj, e: EventArgs) =
         this.Time.Text <- "(R) " + DateTime.Now.ToString()
</script> 

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
   <title>Current time</title>
   <style type="text/css">
      body { font-family:calibri,verdana,sans-serif; }
   </style>
</head>
<body>
   <form id="Form1" runat="server" OnLoad="Form1_Load">
      The current time is:
      <asp:Label  runat="server" id="Time" />
      <asp:Button runat="server" id="Reload" text="Reload" OnClick="Reload_Click" />
   </form>
</body>
</html>

