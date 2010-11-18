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

type Albums_aspx = class
  inherit System.Web.UI.Page
  
  val mutable ObjectDataSource1 : ObjectDataSource
  val mutable DataList1 : DataList
  new () =  { ObjectDataSource1 = null; DataList1 = null; }
end