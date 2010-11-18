#light
namespace FSharpWeb

open System
open System.Web
open System.Web.UI
open System.Web.UI.WebControls

//open FSharpWeb

type Default() =
  inherit Page()
  
  [<DefaultValue>]
  val mutable Button1 : Button

  [<DefaultValue>]
  val mutable Label1 : Label

  member this.ButtonClicked(sender, e) =
    this.Label1.Text <- 
      (sprintf "Factorial of 5 is: %d" (FSharpWeb.Logic.factorial 5))


(*
  // initialize all controls to a null value
  new() = 
    let init() = (Array.zeroCreate 1).[0]
    { btnTest = init();
      Label1 = init(); 
    }
*)
