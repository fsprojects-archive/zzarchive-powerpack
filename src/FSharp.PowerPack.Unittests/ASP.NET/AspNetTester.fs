
namespace FSharp.PowerPack.Unittests

open NUnit.Framework
open System.IO
open System.Net


// DEBUGGING NOTES:
//
// Run this directly: 
//  "c:\Program Files\Common Files\microsoft shared\DevServer\9.0\WebDev.WebServer.EXE"
//
// e.g.
//
// "c:\Program Files\Common Files\microsoft shared\DevServer\9.0\WebDev.WebServer.EXE" /port:8082 /path:C:\fsharp\rc1\extras\fsppack\head\src\FSharp.PowerPack.Unittests\ASP.NET\AspNetIntro
//
// Then open up http://localhost:8082 and the appropriate page

[<AutoOpen>]
module AspNetUtilities = 

    let port = 8282

    let http (url: string) =
        let req = System.Net.WebRequest.Create url
        use resp = req.GetResponse()
        use stream = resp.GetResponseStream()
        use reader = new StreamReader(stream)
        let html = reader.ReadToEnd()
        html

    let httpHeaderSum (url: string) =
        let req = System.Net.WebRequest.Create url
        use resp = req.GetResponse()
        use stream = resp.GetResponseStream()
        use reader = new StreamReader(stream)
        let html = reader.ReadToEnd()
        let sum = int (resp.Headers.["Sum"])
        let ver = new System.Version(resp.Headers.["NetFxVer"])
        resp.Close()
        sum,ver

    let startWeb (port : int, webSitePath : string) =

        // Copy the relevant FSharp.Compiler.CodeDom DLL to the website
        let webSiteBinPath = Path.Combine(webSitePath, @"bin")
        let codeDomOriginalLocation = typeof<Microsoft.FSharp.Compiler.CodeDom.FSharpAspNetCodeProvider>.Assembly.Location 

        logMessage (sprintf "AspNet WebSite Bin directory: %s" webSiteBinPath)
        if not (Directory.Exists webSiteBinPath) then 
            Directory.CreateDirectory(webSiteBinPath) |> ignore
        logMessage (sprintf "AspNet CodeDom original location: %s" codeDomOriginalLocation)
        let codeDomRunLocation = Path.Combine(webSiteBinPath, "FSharp.Compiler.CodeDom.dll")

        File.Copy(codeDomOriginalLocation, codeDomRunLocation, true)
        logMessage (sprintf "AspNet CodeDom copied to: %s" codeDomRunLocation)
        // Start web server

        let progfile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles)
        let webserver90 = Path.Combine(progfile, @"Common Files\microsoft shared\DevServer\9.0\WebDev.WebServer.EXE")
        let webserver100v2 = Path.Combine(progfile, @"Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer20.EXE")
        let webserver100v4 = Path.Combine(progfile, @"Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer40.EXE")
        let webserver = 
            if File.Exists(webserver100v2) then 
                if System.Environment.Version.Major = 2 then webserver100v2
                else webserver100v4
            elif File.Exists(webserver90) then webserver90 
            else failwith "No ASP.NET dev web server found."
        
        let args = sprintf "/port:%d /path:\"%s\"" port webSitePath

        logMessage (sprintf "Starting web server '%s' args = %A" webserver args)
        logMessage (sprintf "To debug this test, run \n   \"%s\" %s /port:8082 /path:%s" webserver args webSitePath)

        let psi = new System.Diagnostics.ProcessStartInfo(webserver, args)
        psi.UseShellExecute <- true
        let p = System.Diagnostics.Process.Start(psi)

        logMessage (sprintf "AspNet Web Server started, PID = %d" p.Id)
        p.Id

    let killWeb (webServerPID : int) =
        try
            logMessage (sprintf "Killing AspNet Web Server, PID = %d" webServerPID)
            System.Diagnostics.Process.GetProcessById(webServerPID).Kill()
        with _ -> ()


[<TestFixture>]
type public AspNet_verySimple() =

    let mutable webserverPID = -1

    [<TestFixtureSetUp>]
    member this.Setup() = webserverPID <- startWeb(port, Path.Combine(__SOURCE_DIRECTORY__, @"VerySimple"))

    [<TestFixtureTearDown>]
    member this.TearDown() = if webserverPID <> -1 then killWeb(webserverPID)

  
    [<Test>]
    member this.LoadPagesTest() = 

        try
           logMessage (sprintf "sleeping...")
           System.Threading.Thread.Sleep 1000
           logMessage (sprintf "requesting...")
           let s, v = httpHeaderSum ("http://localhost:" + port.ToString() + "/")
           if s <> 153 then reportFailure (sprintf "Actual=%d, Expected=%d" s 153)
           if v <> System.Environment.Version then reportFailure (sprintf "Actual=%A, Expected=%A" v (System.Environment.Version))
           logMessage (sprintf "AspNet test passed: (%d,%A)" s v)
        with
        | e -> reportFailure (sprintf "AspNet test failed: %s" e.Message)



[<TestFixture>]
type public AspNet_Time() =

    let mutable webserverPID = -1

    [<TestFixtureSetUp>]
    member this.Setup() = 
        webserverPID <- startWeb(port, Path.Combine(__SOURCE_DIRECTORY__, @"Time"))
        logMessage (sprintf "sleeping...")
        System.Threading.Thread.Sleep 1000

    [<TestFixtureTearDown>]
    member this.TearDown() = if webserverPID <> -1 then killWeb(webserverPID)
  
    [<Test>]
    member this.LoadTimePage() = 

        try

           let html = http ("http://localhost:" + port.ToString() + "/Time.aspx")
           if not (html.Contains "The current time is") then reportFailure (sprintf "Couldn't find marker text in HTML returned, html = %s" html)
           logMessage (sprintf "AspNet test Time.aspx passed: (%s)" html)
        with
        | e -> reportFailure (sprintf "AspNet test failed: %s" e.Message)

    [<Test>]
    member this.LoadTime2Page() = 

        try

           let html = http ("http://localhost:" + port.ToString() + "/Time2.aspx")
           if not (html.Contains "The current time is") then reportFailure (sprintf "Couldn't find marker text in HTML returned, html = %s" html)
           logMessage (sprintf "AspNet test Time2.aspx passed: (%s)" html)

        with
        | e -> reportFailure (sprintf "AspNet test failed: %s" e.Message)


    [<Test>]
    member this.LoadAsyncTimePage() = 

        try

           let html = http ("http://localhost:" + port.ToString() + "/AsyncTime.aspx")
           if not (html.Contains "The current time is") then reportFailure (sprintf "Couldn't find marker text in HTML returned, html = %s" html)
           logMessage (sprintf "AspNet test AsyncTime.aspx passed: (%s)" html)

        with
        | e -> reportFailure (sprintf "AspNet test failed: %s" e.Message)

    [<Test>]
    member this.LoadAsyncTime2Page() = 

        try

           let html = http ("http://localhost:" + port.ToString() + "/AsyncTime2.aspx")
           if not (html.Contains "The current time is") then reportFailure (sprintf "Couldn't find marker text in HTML returned, html = %s" html)
           logMessage (sprintf "AspNet test AsyncTime2.aspx passed: (%s)" html)

        with
        | e -> reportFailure (sprintf "AspNet test failed: %s" e.Message)


[<TestFixture>]
type public AspNet_AspNetIntro() =

    let mutable webserverPID = -1

    [<TestFixtureSetUp>]
    member this.Setup() = 
        webserverPID <- startWeb(port, Path.Combine(__SOURCE_DIRECTORY__, @"AspNetIntro"))
        logMessage (sprintf "sleeping...")
        System.Threading.Thread.Sleep 1000

    [<TestFixtureTearDown>]
    member this.TearDown() = if webserverPID <> -1 then killWeb(webserverPID)
  
    [<Test>]
    member this.LoadDefaultPage() = 

        try
           logMessage (sprintf "requesting...")

           let html = http ("http://localhost:" + port.ToString() + "/default.aspx")
           if not (html.Contains "ASP.NET F# Intro") then reportFailure (sprintf "Couldn't find marker text in HTML returned, html = %s" html)
           logMessage (sprintf "AspNet test default.aspx passed: (%s)" html)


        with
        | e -> reportFailure (sprintf "AspNet test failed: %s" e.Message)


    [<Test>]
    member this.LoadDataBindingPage() = 
        try
           logMessage (sprintf "requesting...")

           let html = http ("http://localhost:" + port.ToString() + "/databinding.aspx")
           if not (html.Contains "Displaying data") then reportFailure (sprintf "Couldn't find marker text in HTML returned, html = %s" html)
           logMessage (sprintf "AspNet test databinding.aspx passed: (%s)" html)

        with
        | e -> reportFailure (sprintf "AspNet test failed: %s" e.Message)


[<TestFixture>]
type public AspNet_PersonalWebSite() =

    let mutable webserverPID = -1

    [<TestFixtureSetUp>]
    member this.Setup() = 
        // Make sure the database MDF files are writable
        let dbPath =  Path.Combine(__SOURCE_DIRECTORY__, @"PersonalWebSite\App_Data\PERSONAL.MDF")

        if File.Exists dbPath then 
           File.SetAttributes(dbPath, File.GetAttributes dbPath &&& ~~~FileAttributes.ReadOnly )

        let dbPath =  Path.Combine(__SOURCE_DIRECTORY__, @"PersonalWebSite\App_Data\ASPNETDB.MDF")

        if File.Exists dbPath then 
           File.SetAttributes(dbPath, File.GetAttributes dbPath &&& ~~~FileAttributes.ReadOnly )

        webserverPID <- startWeb(port, Path.Combine(__SOURCE_DIRECTORY__, @"PersonalWebSite"))

        logMessage (sprintf "sleeping...")
        System.Threading.Thread.Sleep 1000

    [<TestFixtureTearDown>]
    member this.TearDown() = if webserverPID <> -1 then killWeb(webserverPID)
  
    [<Test>]
    member this.LoadLinksPage() = 

        try
           logMessage (sprintf "requesting...")

           let html = http ("http://localhost:" + port.ToString() + "/Links.aspx")
           if not (html.Contains "About the Links") then reportFailure (sprintf "Couldn't find marker text in HTML returned, html = %s" html)
           logMessage (sprintf "AspNet test Links.aspx passed: (%s)" html)
        with
        | e -> reportFailure (sprintf "AspNet test failed: %s" e.Message)

    [<Test>]
    member this.LoadDefaultPage() = 

        try
           logMessage (sprintf "requesting...")

           let html = http ("http://localhost:" + port.ToString() + "/")
           if not (html.Contains "Your Name Here") then reportFailure (sprintf "Couldn't find marker text in HTML returned, html = %s" html)
           logMessage (sprintf "AspNet test default.aspx passed: (%s)" html)
        with
        | e -> reportFailure (sprintf "AspNet test failed: %s" e.Message)

    [<Test>]
    member this.LoadAlbumsPage() = 

        try
           logMessage (sprintf "requesting...")

           let html = http ("http://localhost:" + port.ToString() + "/Albums.aspx")
           if not (html.Contains "Welcome to My Photo Galleries") then reportFailure (sprintf "Couldn't find marker text in HTML returned, html = %s" html)
           logMessage (sprintf "AspNet test Albums.aspx passed: (%s)" html)
        with
        | e -> reportFailure (sprintf "AspNet test failed: %s" e.Message)

    [<Test>]
    member this.LoadResumePage() = 

        try
           logMessage (sprintf "requesting...")

           let html = http ("http://localhost:" + port.ToString() + "/Resume.aspx")
           if not (html.Contains "Your Name Here") then reportFailure (sprintf "Couldn't find marker text in HTML returned, html = %s" html)
           logMessage (sprintf "AspNet test Resume.aspx passed: (%s)" html)

        with
        | e -> reportFailure (sprintf "AspNet test failed: %s" e.Message)

