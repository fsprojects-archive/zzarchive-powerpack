#light
namespace PersonalWebSite

open PersonalWebSite

open System;
open System.Configuration;
open System.Web;
open System.Web.Security;
open System.Web.UI;
open System.Web.UI.WebControls;
open System.Web.UI.WebControls.WebParts;
open System.Web.UI.HtmlControls;
open System.Data;
open System.Data.OleDb;
open System.IO;

type Admin_Photos_aspx = class
  inherit System.Web.UI.Page
  
  val mutable ObjectDataSource1 : ObjectDataSource;
  val mutable ObjectDataSource2 : ObjectDataSource;
  val mutable FormView1 : FormView
  val mutable GridView1 : GridView
  
  new () = { ObjectDataSource2=null; ObjectDataSource1=null; FormView1=null; GridView1=null; }

  member this.FormView1_ItemInserting(sender:obj, e:FormViewInsertEventArgs) =
    if ((e.Values.get_Item("BytesOriginal") :?> byte[]).Length = 0) then e.Cancel <- true

  member this.Button1_Click(sender:obj, e:ImageClickEventArgs) =
    let d = new DirectoryInfo(this.Server.MapPath("~/Upload"));
    for f in d.GetFiles("*.jpg") do
      using (f.OpenRead()) (fun sr ->
        let siz = int (sr.Length);
        let buffer = ((Array.zeroCreate siz):byte[])
        sr.Read(buffer, 0, siz) |> ignore
        PhotoManager.AddPhoto (Convert.ToInt32(this.Request.QueryString.get_Item("AlbumID")), f.Name, buffer) )
    this.GridView1.DataBind();
end