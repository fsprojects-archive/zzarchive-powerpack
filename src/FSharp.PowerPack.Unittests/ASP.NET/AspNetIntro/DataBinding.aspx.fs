#light
namespace FSharpWeb

open System
open System.Web
open System.Web.UI
open System.Web.UI.WebControls

open FSharpWeb

type DataBinding() =
  inherit Page()
  
  [<DefaultValue>]
  val mutable btnTest : Button
  [<DefaultValue>]
  val mutable rptData : Repeater
  
  member x.ButtonClicked(sender, e) =
    x.rptData.DataSource <- Logic.loadData();
    x.rptData.DataBind();
    