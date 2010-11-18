#light
namespace PersonalWebSite

open System;
open System.Data;
open System.Configuration;
open System.Web;
open System.Web.Security;
open System.Web.UI;
open System.Web.UI.WebControls;
open System.Web.UI.WebControls.WebParts;
open System.Web.UI.HtmlControls;

type Photos_aspx = class
  inherit System.Web.UI.Page 
 
  val mutable gallery : Image  
  val mutable DataList1 : DataList
  val mutable Panel1 : Panel
  val mutable ObjectDataSource1 : ObjectDataSource
  
  new () = { DataList1 = null; Panel1 = null; ObjectDataSource1 = null; gallery = null; }
  
  member this.DataList1_ItemDataBound (sender:obj, e:DataListItemEventArgs) =
    if (e.Item.ItemType = ListItemType.Footer) then
      if (this.DataList1.Items.Count = 0) then this.Panel1.Visible <- true;
      
end      