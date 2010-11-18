<%@ Page Language="F#" MasterPageFile="" Title="Your Name Here | Download" 
    CodeFile="Download.aspx.fs" Inherits="PersonalWebSite.Download_aspx" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <style type="text/css">
    body {
        margin:0;
        padding:0;
        }
    p {
        font:11px verdana;
        padding:15px 0 0 6px;
        }
    </style>
</head>
<body>
    <form runat="server">
    <div>

		<p>Right-click and select "Save Picture As.." to download the picture.</p>
		<asp:formview id="FormView1" runat="server" datasourceid="ObjectDataSource1" borderstyle="none" borderwidth="0" CellPadding="0" cellspacing="0">
			<itemtemplate>
				<img src="Handler.ashx?PhotoID=<%# base.Eval("PhotoID") %>" alt='Photo Number <%# base.Eval("PhotoID") %>' /></itemtemplate>
		</asp:formview>

		<asp:ObjectDataSource ID="ObjectDataSource1" Runat="server" TypeName="PersonalWebSite.PhotoManager" 
			SelectMethod="GetPhotos">
			<SelectParameters>
				<asp:QueryStringParameter Name="AlbumID" Type="Int32" QueryStringField="AlbumID" DefaultValue="0"/>
			</SelectParameters>
		</asp:ObjectDataSource>

    </div>
    </form>
</body>
</html>
