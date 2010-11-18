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

type Default_aspx = class
    inherit System.Web.UI.Page 

    val mutable ObjectDataSource1 : ObjectDataSource;
    val mutable FormView1 : FormView;
    val mutable LoginArea : LoginView
    
    new () = { ObjectDataSource1=null; FormView1=null; LoginArea=null; }
      
    member t.Randomize ((sender:obj),(e:EventArgs)) =
        let r = new Random();
        t.FormView1.PageIndex <- r.Next(t.FormView1.PageCount);
end