<%@	Page Language="F#" MasterPageFile="~/Default.master" Title="Your Name Here | Photos"
	CodeFile="Photos.aspx.fs" Inherits="PersonalWebSite.Photos_aspx" %>

<asp:content contentplaceholderid="Main" runat="server">
	
	<div class="shim solid"></div> 
	  
	<div class="page" id="photos">
		<div class="buttonbar buttonbar-top">
			<a href="Albums.aspx"><asp:image runat="Server" skinid="gallery" /></a>
		</div>
		<asp:DataList ID="DataList1" runat="Server"	cssclass="view"	dataSourceID="ObjectDataSource1" 
			repeatColumns="4" repeatdirection="Horizontal" onitemdatabound="DataList1_ItemDataBound" EnableViewState="false">
			<ItemTemplate>
				<table border="0" cellpadding="0" cellspacing="0" class="photo-frame">
					<tr>
						<td class="topx--"></td>
						<td class="top-x-"></td>
						<td class="top--x"></td>
					</tr>
					<tr>
						<td class="midx--"></td>
						<td><a href='Details.aspx?AlbumID=<%# base.Eval("PhotoAlbumID") %>&Page=<%# box Container.ItemIndex %>'>
							<img src="Handler.ashx?PhotoID=<%# base.Eval("PhotoID") %>&Size=S" class="photo_198" style="border:4px solid white" alt='Thumbnail of Photo Number <%# base.Eval("PhotoID") %>' /></a></td>
						<td class="mid--x"></td>
					</tr>
					<tr>
						<td class="botx--"></td>
						<td class="bot-x-"></td>
						<td class="bot--x"></td>
					</tr>
				</table>
				<p><%# box (this.Server.HtmlEncode(base.Eval("PhotoCaption").ToString())) %></p>
			</ItemTemplate>
			<FooterTemplate>
			</FooterTemplate>
		</asp:DataList>
		<asp:panel id="Panel1" runat="server" visible="false" CssClass="nullpanel">There are currently no pictures in this album.</asp:panel>
		<div class="buttonbar">
			<a href="Albums.aspx"><asp:image id="gallery" runat="Server" skinid="gallery" /></a>
		</div>
	</div>
	
	<asp:ObjectDataSource ID="ObjectDataSource1" Runat="server" TypeName="PersonalWebSite.PhotoManager" 
		SelectMethod="GetPhotos">
		<SelectParameters>
			<asp:QueryStringParameter Name="AlbumID" Type="Int32" QueryStringField="albumID" DefaultValue="0"/>
		</SelectParameters>
	</asp:ObjectDataSource>
	
</asp:content>
