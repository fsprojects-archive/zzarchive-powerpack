// ----------------------------
//  Listing 14-5.

namespace FSharpWeb

open System
open System.Web
open System.Web.UI
open System.Web.UI.WebControls
open Microsoft.FSharp.Control.WebExtensions

[<AutoOpen>]
module PageExtensions = 
    type System.Web.UI.Page with 
        member page.AddOnPreRenderCompleteAsync(p) = 
            let beginMeth, endMeth, cancelMeth = Async.AsBeginEnd p 

            page.AddOnPreRenderCompleteAsync
                ((fun obj ev cb state -> beginMeth(ev,cb,state)),
                 (fun iar -> endMeth iar));

        member page.RegisterAsyncTask(p) = 
            let beginMeth, endMeth, cancelMeth = p |> Async.AsBeginEnd

            page.RegisterAsyncTask
                (PageAsyncTask((fun obj ev cb state -> beginMeth(ev,cb,state)),
                               (fun iar -> endMeth iar),
                               (fun iar -> cancelMeth iar),
                               null));


type Time2() =
    inherit Page()
    
    [<DefaultValue>]
    val mutable Time : Label
    [<DefaultValue>]
    val mutable Reload : Button

    member page.Page_Load(sender: obj, e: EventArgs) =
        if not page.Page.IsPostBack then
            page.AddOnPreRenderCompleteAsync(fun e -> 
                // This is the asynchronous program that runs when during PreRender of the initial load
                async {

                    // Start the two children
                    let text1 = "<br> Process1 Started at " + DateTime.Now.ToLongTimeString(); 
                    let! p1 = Async.Sleep 5000 |> Async.StartChild
                    let text2 = "<br> Process2 Started at " + DateTime.Now.ToLongTimeString(); 
                    let! p2 = Async.Sleep 5000 |> Async.StartChild
                    // Wait for the two children
                    let! res1 = p1
                    let text3 = "<br> Process1 Responded at least before " + DateTime.Now.ToLongTimeString(); 
                    let! res2 = p2
                    let text4 = "<br> Process2 Responded at least before " + DateTime.Now.ToLongTimeString(); 
                    let text5 = "<br> All Done at " + DateTime.Now.ToLongTimeString();
                    // Update the display
                    page.Time.Text <- text1+text2+text3+text4+text5 })

        //if not page.Page.IsPostBack then
        //    page.Time.Text <- DateTime.Now.ToString()

    member page.Reload_Click(sender: obj, e: EventArgs) =
        page.RegisterAsyncTask(fun e -> 
            // This is the asynchronous program that runs in response to user action
            async { 

                // Start the two children
                let text1 = "<br> Process1 Started at " + DateTime.Now.ToLongTimeString(); 
                let! p1 = Async.Sleep 1000 |> Async.StartChild
                let text2 = "<br> Process2 Started at " + DateTime.Now.ToLongTimeString(); 
                let! p2 = Async.Sleep 1000 |> Async.StartChild
                // Wait for the two children
                let! res1 = p1
                let text3 = "<br> Process1 Responded at least before " + DateTime.Now.ToLongTimeString(); 
                let! res2 = p2
                let text4 = "<br> Process2 Responded at least before " + DateTime.Now.ToLongTimeString(); 
                let text5 = "<br> All Done at " + DateTime.Now.ToLongTimeString();
                // Update the display
                page.Time.Text <- "(Reload)" + text1 + text2 + text3 + text4 + text5  })

        //page.Time.Text <- "(R) " + DateTime.Now.ToString()

(*

    protected void Page_PreRenderComplete(object sender, EventArgs e) {
        Label1.Text = _ws1Result;
        Label2.Text = _ws2Result;
    }
*)
