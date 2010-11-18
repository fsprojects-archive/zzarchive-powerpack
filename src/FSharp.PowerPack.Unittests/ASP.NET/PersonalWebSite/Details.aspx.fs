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

type Details_aspx = class
  inherit System.Web.UI.Page 
  
  val mutable FormView1 : FormView
  val mutable ObjectDataSource1 : ObjectDataSource
  new () = { FormView1 = null; ObjectDataSource1 = null; }
  
  member this.Page_Load (sender:obj, e:EventArgs) =
    this.Page.MaintainScrollPositionOnPostBack <- true;
    if (not this.IsPostBack) then
      let i = Convert.ToInt32(this.Request.QueryString.get_Item("Page"));
      if (i >= 0) then this.FormView1.PageIndex <- i;
end