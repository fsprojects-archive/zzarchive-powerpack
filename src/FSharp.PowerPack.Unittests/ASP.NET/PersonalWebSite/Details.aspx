<%@	Page Language="F#" MasterPageFile="~/Default.master" Title="Your Name Here | Picture Details"
	CodeFile="Details.aspx.fs" Inherits="PersonalWebSite.Details_aspx" %>

<asp:content id="Content1" contentplaceholderid="Main" runat="server">

	<div class="shim solid"></div>
	
	<div class="page" id="details">
		<asp:formview id="FormView1" runat="server"	datasourceid="ObjectDataSource1" cssclass="view"
			borderstyle="none" borderwidth="0" CellPadding="0" cellspacing="0" EnableViewState="false" AllowPaging="true">
			<itemtemplate>
			
				<div class="buttonbar buttonbar-top">
					<a href="Albums.aspx"><asp:image ID="Image1" runat="Server"	 skinid="gallery" /></a>
					&nbsp;&nbsp;&nbsp;&nbsp;
					<asp:ImageButton ID="ImageButton9" Runat="server" CommandName="Page" CommandArgument="First" skinid="first"/>
					<asp:ImageButton ID="ImageButton10"	Runat="server" CommandName="Page" CommandArgument="Prev" skinid="prev"/>
					<asp:ImageButton ID="ImageButton11"	Runat="server" CommandName="Page" CommandArgument="Next" skinid="next"/>
					<asp:ImageButton ID="ImageButton12"	Runat="server" CommandName="Page" CommandArgument="Last" skinid="last"/>
				</div>
				<p><%# box (this.Server.HtmlEncode(base.Eval("PhotoCaption").ToString())) %></p>
				<table border="0" cellpadding="0" cellspacing="0" class="photo-frame">
					<tr>
						<td class="topx--"></td>
						<td class="top-x-"></td>
						<td class="top--x"></td>
					</tr>
					<tr>
						<td class="midx--"></td>
						<td><img src="Handler.ashx?PhotoID=<%# base.Eval("PhotoID") %>&Size=L" class="photo_198" style="border:4px solid white" alt='Photo Number <%# base.Eval("PhotoID") %>' /></a></td>
						<td class="mid--x"></td>
					</tr>
					<tr>
						<td class="botx--"></td>
						<td class="bot-x-"></td>
						<td class="bot--x"></td>
					</tr>
				</table>
				<p><a href='Download.aspx?AlbumID=<%# base.Eval("PhotoAlbumID") %>&Page=<%# box Container.DataItemIndex %>'>
					<asp:image runat="Server" id="DownloadButton" AlternateText="download this photo" skinid="download" /></a></p>
				<div class="buttonbar">
					<a href="Albums.aspx"><asp:image ID="Image2" runat="Server"	 skinid="gallery" /></a>
					&nbsp;&nbsp;&nbsp;&nbsp;
					<asp:ImageButton ID="ImageButton1" Runat="server" CommandName="Page" CommandArgument="First" skinid="first"/>
					<asp:ImageButton ID="ImageButton2" Runat="server" CommandName="Page" CommandArgument="Prev" skinid="prev"/>
					<asp:ImageButton ID="ImageButton3" Runat="server" CommandName="Page" CommandArgument="Next" skinid="next"/>
					<asp:ImageButton ID="ImageButton4" Runat="server" CommandName="Page" CommandArgument="Last" skinid="last"/>
				</div>

			</itemtemplate>
		</asp:formview>
	</div>
	
	<asp:ObjectDataSource ID="ObjectDataSource1" Runat="server" TypeName="PersonalWebSite.PhotoManager" 
		SelectMethod="GetPhotos">
		<SelectParameters>
			<asp:QueryStringParameter Name="AlbumID" Type="Int32" QueryStringField="albumID" DefaultValue="0"/>
		</SelectParameters>
	</asp:ObjectDataSource>
	
</asp:content>
