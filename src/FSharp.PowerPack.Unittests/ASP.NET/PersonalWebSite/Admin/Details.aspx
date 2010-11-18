<%@ Page Language="F#" MasterPageFile="~/Default.master" Title="Your Name Here | Admin"
    CodeFile="Details.aspx.fs" Inherits="PersonalWebSite.Admin_Details_aspx" %>

<asp:content id="Content1" contentplaceholderid="Main" runat="server">

    <div class="shim gradient"></div>

	<div class="page" id="admin-details">
		<asp:formview id="FormView1" runat="server" datasourceid="ObjectDataSource1" cssclass="view"
			borderstyle="none" borderwidth="0" CellPadding="0" cellspacing="0" EnableViewState="false">
			<itemtemplate>
				<p><%# box (this.Server.HtmlEncode(base.Eval("PhotoCaption").ToString())) %></p>
				<table border="0" cellpadding="0" cellspacing="0" class="photo-frame">
					<tr>
						<td class="topx--"></td>
						<td class="top-x-"></td>
						<td class="top--x"></td>
					</tr>
					<tr>
						<td class="midx--"></td>
						<td><img src="../Handler.ashx?PhotoID=<%# base.Eval("PhotoID") %>&Size=L" class="photo_198" style="border:4px solid white" alt='Photo Number <%# base.Eval("PhotoID") %>' /></a></td>
						<td class="mid--x"></td>
					</tr>
					<tr>
						<td class="botx--"></td>
						<td class="bot-x-"></td>
						<td class="bot--x"></td>
					</tr>
				</table>
				<p>&nbsp;</p>
			</itemtemplate>
		</asp:formview>
	</div>

	<asp:ObjectDataSource ID="ObjectDataSource1" Runat="server" TypeName="PersonalWebSite.PhotoManager" 
		SelectMethod="GetPhotos">
		<SelectParameters>
			<asp:QueryStringParameter Name="AlbumID" Type="Int32" QueryStringField="AlbumID" DefaultValue="0"/>
		</SelectParameters>
	</asp:ObjectDataSource>

</asp:content>