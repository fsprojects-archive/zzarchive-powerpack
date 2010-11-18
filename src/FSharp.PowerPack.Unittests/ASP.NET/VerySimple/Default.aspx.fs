namespace N

open System
open System.Web
open System.Web.UI
open System.Web.UI.WebControls

type public Default() =
  inherit Page()
  member this.Page_Load(sender : obj, e : System.EventArgs) = 
    let o = new N.Module.C(10)
    let a1 = N.Module.C.NetFxVersion.ToString()
    let a2 = o.TestList(5).ToString()
    this.Response.AppendHeader("NetFxVer", a1)
    this.Response.AppendHeader("Sum", a2)
    this.Response.Write("<P>NetFxVer:" + a1)
    this.Response.Write("<P>Sum:" + a2)
