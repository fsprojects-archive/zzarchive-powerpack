<%@ Page Language="F#" Async="true"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
                      "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script language="F#" runat="server" >
     /// F# scripts embedded in ASPX pages must be a set of 'member' declarations.
     /// ASP.NET inserts these into the code generated for the page object type.

     /// This member is invoked on the server when the page is loaded. It tests
     /// whether the page was loaded for the first time and updates the content of
     /// this.Time control.
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

    member page.Form1_Load(sender: obj, e: EventArgs) =
        if not page.Page.IsPostBack then
            page.AddOnPreRenderCompleteAsync(fun e -> 
                // This is the asynchronous program that runs when during PreRender of the initial load
                async { 

                    // Start the two children
                    let text1 = "<br> Process1 Started at " + DateTime.Now.ToLongTimeString(); 
                    let! p1 = Async.Sleep(500) |> Async.StartChild
                    let text2 = "<br> Process2 Started at " + DateTime.Now.ToLongTimeString(); 
                    let! p2 = Async.Sleep(500) |> Async.StartChild
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
                let! p1 = Async.Sleep(500) |> Async.StartChild
                let text2 = "<br> Process2 Started at " + DateTime.Now.ToLongTimeString(); 
                let! p2 = Async.Sleep(500) |> Async.StartChild
                // Wait for the two children
                let! res1 = p1
                let text3 = "<br> Process1 Responded at least before " + DateTime.Now.ToLongTimeString(); 
                let! res2 = p2
                let text4 = "<br> Process2 Responded at least before " + DateTime.Now.ToLongTimeString(); 
                let text5 = "<br> All Done at " + DateTime.Now.ToLongTimeString();
                // Update the display
                page.Time.Text <- "(Reload)" + text1+text2+text3+text4+text5 })

</script> 

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
   <title>Current time</title>
   <style type="text/css">
      body { font-family:calibri,verdana,sans-serif; }
   </style>
</head>
<body>
   <form id="Form1" runat="server" OnLoad="Form1_Load">
      The current time is:
      <asp:Label  runat="server" id="Time" />
      <asp:Button runat="server" id="Reload" text="Reload" OnClick="Reload_Click" />
   </form>
</body>
</html>

