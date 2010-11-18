<%@ WebHandler Language="F#" Class="PersonalWebSite.Handler" %>
#light
namespace PersonalWebSite

open System;
open System.IO;
open System.Web;
open PersonalWebSite

type Handler = class 
  new () = {}
  interface IHttpHandler with
    member this.IsReusable 
      with get() = true;

    member this.ProcessRequest (context:HttpContext) =
      // Set up the response settings
      context.Response.ContentType <- "image/jpeg";
      context.Response.Cache.SetCacheability(HttpCacheability.Public);
      context.Response.BufferOutput <- false;
            
      // Setup the Size Parameter
      let size = 
        match (context.Request.QueryString.get_Item("Size")) with
          | "S" -> PhotoSize.Small
          | "M" -> PhotoSize.Medium
          | "L" -> PhotoSize.Large
          | _ ->   PhotoSize.Original 

      // Setup the PhotoID Parameter
      let stream =
        if (context.Request.QueryString.get_Item("PhotoID") <> null && context.Request.QueryString.get_Item("PhotoID") <> "") then
          let id = Convert.ToInt32(context.Request.QueryString.get_Item("PhotoID")) in 
          ((PhotoManager.GetPhoto id size) :> Stream)
        else
          let id = Convert.ToInt32(context.Request.QueryString.get_Item("AlbumID")) in
          ((PhotoManager.GetFirstPhoto id size) :> Stream)
            
      // Get the photo from the database, if nothing is returned, get the default "placeholder" photo
      let stream = (if (stream = null) then ((PhotoManager.GetDefaultPhoto size) :> Stream) else stream)
            
      // Write image stream to the response stream
      let buffersize = 1024 * 16;
      let (buffer:byte[]) = Array.zero_create buffersize;
      let mutable count = stream.Read(buffer, 0, buffersize);
      while (count > 0) do
        context.Response.OutputStream.Write(buffer, 0, count)
        count <- stream.Read(buffer, 0, buffersize)
  end
end