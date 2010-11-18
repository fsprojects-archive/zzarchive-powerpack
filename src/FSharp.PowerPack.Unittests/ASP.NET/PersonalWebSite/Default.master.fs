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

type Default_master = class
  inherit System.Web.UI.MasterPage
  val mutable SiteMapDataSource1 : SiteMapDataSource
  val mutable Main : ContentPlaceHolder
  new () = { SiteMapDataSource1 = null; Main = null }
end