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

type Admin_Albums_aspx = class
  inherit System.Web.UI.Page
  val mutable ObjectDataSource1 : ObjectDataSource
  val mutable GridView1 : GridView
  new () = { ObjectDataSource1=null; GridView1=null; }
end