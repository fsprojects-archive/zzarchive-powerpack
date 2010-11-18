#light
namespace PersonalWebSite
#nowarn "49" // uppercase argument names required by ASP.NET data binding

open System
open System.Collections
open System.Collections.Generic
open System.Configuration
open System.Data
open System.Data.SqlClient
open System.Drawing
open System.Drawing.Drawing2D
open System.Drawing.Imaging
open System.IO
open System.Web
open System.Configuration

// Record types can be used for storing data from DB
type Album = { AlbumID:int; Count:int; Caption:string; IsPublic:bool }
type Photo = { PhotoID:int; PhotoAlbumID:int; PhotoCaption:string; }

type PhotoSize =
  | Small = 1     // 100px
  | Medium = 2    // 198px
  | Large = 3     // 600px
  | Original = 4  // Original Size

// Module with all web functionality used by databinding
module PhotoManager = 

  // Helper Functions
  let CalculateDimensions ((width, height) : int*int) (targetSize:int) =
    if (height > width) then
      (int32 ((float width) * ((float targetSize) / (float height))), targetSize)
    else
      (targetSize, int32 ((float height) * ((float targetSize) / (float width))))
    
  let ResizeImageFile (imageFile:byte[]) (targetSize:int) =
    use oldImage = System.Drawing.Image.FromStream(new MemoryStream(imageFile))
    let (nw, nh) = CalculateDimensions (oldImage.Width, oldImage.Height) targetSize
    use newImage = new Bitmap(nw, nh, PixelFormat.Format24bppRgb)
    use canvas = Graphics.FromImage(newImage)
    canvas.SmoothingMode <- SmoothingMode.AntiAlias
    canvas.InterpolationMode <- InterpolationMode.HighQualityBicubic
    canvas.PixelOffsetMode <- PixelOffsetMode.HighQuality
    canvas.DrawImage(oldImage, Rectangle(Point(0, 0), Size(nw, nh))) 
    let m = new MemoryStream()
    newImage.Save(m, ImageFormat.Jpeg)
    m.GetBuffer() 

  let ListUploadDirectory() =
    let d = new DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("~/Upload"))
    d.GetFileSystemInfos("*.jpg")
    
  // Photo-Related Methods
  let GetPhoto (photoid:int) (size:PhotoSize) = 
    let isize = (unbox (box size)) : int in
    use connection = new SqlConnection(ConfigurationManager.ConnectionStrings.get_Item("Personal").ConnectionString)
    use command = new SqlCommand("GetPhoto", connection)
    command.CommandType <- CommandType.StoredProcedure
    command.Parameters.Add(SqlParameter("@PhotoID", photoid)) |> ignore
    command.Parameters.Add(SqlParameter("@Size", isize)) |> ignore
    let filter = not (HttpContext.Current.User.IsInRole("Friends") || HttpContext.Current.User.IsInRole("Administrators"))
    command.Parameters.Add(SqlParameter("@IsPublic", filter)) |> ignore
    connection.Open()
    let result = command.ExecuteScalar()
    try 
      new MemoryStream(result :?> byte[])
    with _ -> 
      null 

  let GetDefaultPhoto (size:PhotoSize) =
    let path = HttpContext.Current.Server.MapPath("~/Images/")
    let ext = 
      match size with 
        | PhotoSize.Small  -> "placeholder-100.jpg"
        | PhotoSize.Medium -> "placeholder-200.jpg"
        | PhotoSize.Large  -> "placeholder-600.jpg"
        | _ ->                "placeholder-600.jpg"
    new FileStream(path + ext, FileMode.Open, FileAccess.Read, FileShare.Read)

  let GetFirstPhoto (albumid:int) (size:PhotoSize) =
    let isize = (unbox (box size)) : int in
    use connection = new SqlConnection(ConfigurationManager.ConnectionStrings.get_Item("Personal").ConnectionString)
    use command = new SqlCommand("GetFirstPhoto", connection)
    command.CommandType <- CommandType.StoredProcedure
    command.Parameters.Add(SqlParameter("@AlbumID", albumid)) |> ignore
    command.Parameters.Add(SqlParameter("@Size", isize)) |> ignore
    let filter = not (HttpContext.Current.User.IsInRole("Friends") || HttpContext.Current.User.IsInRole("Administrators"))
    command.Parameters.Add(SqlParameter("@IsPublic", filter)) |> ignore
    connection.Open()
    let result = command.ExecuteScalar()
    try
      new MemoryStream(result :?> byte[])
    with _ ->
      null
 
  let GetPhotos (albumID:int) =
    use connection = new SqlConnection(ConfigurationManager.ConnectionStrings.get_Item("Personal").ConnectionString)
    use command = new SqlCommand("GetPhotos", connection)
    command.CommandType <- CommandType.StoredProcedure
    command.Parameters.Add(SqlParameter("@AlbumID", albumID)) |> ignore
    let filter = not (HttpContext.Current.User.IsInRole("Friends") || HttpContext.Current.User.IsInRole("Administrators"))
    command.Parameters.Add(SqlParameter("@IsPublic", filter)) |> ignore
    connection.Open()
    let list = new List<Photo>()
    use reader = command.ExecuteReader()
    while (reader.Read()) do
      let temp = {PhotoID = unbox (reader.get_Item("PhotoID")); PhotoAlbumID = unbox (reader.get_Item("AlbumID")); PhotoCaption = unbox (reader.get_Item("Caption"))}
      list.Add(temp) 
    list 

  let GetRandomAlbumID() =
    use connection = new SqlConnection(ConfigurationManager.ConnectionStrings.get_Item("Personal").ConnectionString)
    use command = new SqlCommand("GetNonEmptyAlbums", connection)
    command.CommandType <- CommandType.StoredProcedure
    connection.Open()
    let list = new List<int>()
    use reader = command.ExecuteReader()
    while (reader.Read()) do
      list.Add(unbox (reader.get_Item("AlbumID"))) 
    try
      let r = Random()
      list.get_Item(r.Next(list.Count))
    with _ ->
      -1 
    
  let GetRandomPhotos () = GetPhotos (GetRandomAlbumID())
  
  let AddPhoto (AlbumID:int, PhotoCaption:string, BytesOriginal:byte[]) =
    use connection = new SqlConnection(ConfigurationManager.ConnectionStrings.get_Item("Personal").ConnectionString) 
    use command = new SqlCommand("AddPhoto", connection) 
    command.CommandType <- CommandType.StoredProcedure
    command.Parameters.Add(SqlParameter("@AlbumID", AlbumID)) |> ignore
    command.Parameters.Add(SqlParameter("@Caption", PhotoCaption)) |> ignore
    command.Parameters.Add(SqlParameter("@BytesOriginal", BytesOriginal)) |> ignore
    command.Parameters.Add(SqlParameter("@BytesFull", ResizeImageFile BytesOriginal 600)) |> ignore
    command.Parameters.Add(SqlParameter("@BytesPoster", ResizeImageFile BytesOriginal 198)) |> ignore
    command.Parameters.Add(SqlParameter("@BytesThumb", ResizeImageFile BytesOriginal 100)) |> ignore
    connection.Open()
    command.ExecuteNonQuery() 

  let RemovePhoto (PhotoID:int) =
    use connection = new SqlConnection(ConfigurationManager.ConnectionStrings.get_Item("Personal").ConnectionString)
    use command = new SqlCommand("RemovePhoto", connection)
    command.CommandType <- CommandType.StoredProcedure
    command.Parameters.Add(SqlParameter("@PhotoID", PhotoID)) |> ignore
    connection.Open()
    command.ExecuteNonQuery() 

  let EditPhoto (PhotoCaption:string, PhotoID:int) =
    use connection = new SqlConnection(ConfigurationManager.ConnectionStrings.get_Item("Personal").ConnectionString)
    use command = new SqlCommand("EditPhoto", connection)
    command.CommandType <- CommandType.StoredProcedure
    command.Parameters.Add(SqlParameter("@Caption", PhotoCaption)) |> ignore
    command.Parameters.Add(SqlParameter("@PhotoID", PhotoID)) |> ignore
    connection.Open()
    command.ExecuteNonQuery() 

  // Album-Related Methods

  let GetAlbums() =
    use connection = new SqlConnection(ConfigurationManager.ConnectionStrings.get_Item("Personal").ConnectionString)
    use command = new SqlCommand("GetAlbums", connection)
    command.CommandType <- CommandType.StoredProcedure
    let filter = not (HttpContext.Current.User.IsInRole("Friends") || HttpContext.Current.User.IsInRole("Administrators"))
    command.Parameters.Add(SqlParameter("@IsPublic", filter)) |> ignore
    connection.Open()
    let list = new List<Album>()
    use reader = command.ExecuteReader()
    while (reader.Read()) do
      let temp = { AlbumID = unbox (reader.get_Item("AlbumID")); Count = unbox (reader.get_Item("NumberOfPhotos"));
                   Caption = unbox (reader.get_Item("Caption")); IsPublic = unbox (reader.get_Item("IsPublic"))}
      list.Add(temp) 
    list 

  let AddAlbum (Caption:string, IsPublic:bool) =
    use connection = new SqlConnection(ConfigurationManager.ConnectionStrings.get_Item("Personal").ConnectionString)
    use command = new SqlCommand("AddAlbum", connection)
    command.CommandType <- CommandType.StoredProcedure
    command.Parameters.Add(SqlParameter("@Caption", Caption)) |> ignore
    command.Parameters.Add(SqlParameter("@IsPublic", IsPublic)) |> ignore
    connection.Open()
    command.ExecuteNonQuery() 

  let RemoveAlbum (AlbumID:int) =
    use connection = new SqlConnection(ConfigurationManager.ConnectionStrings.get_Item("Personal").ConnectionString)
    use command = new SqlCommand("RemoveAlbum", connection)
    command.CommandType <- CommandType.StoredProcedure
    command.Parameters.Add(SqlParameter("@AlbumID", AlbumID)) |> ignore
    connection.Open()
    command.ExecuteNonQuery()

  let EditAlbum (Caption:string) (IsPublic:bool) (AlbumID:int) =
    use connection = new SqlConnection(ConfigurationManager.ConnectionStrings.get_Item("Personal").ConnectionString)
    use command = new SqlCommand("EditAlbum", connection)
    command.CommandType <- CommandType.StoredProcedure
    command.Parameters.Add(SqlParameter("@Caption", Caption)) |> ignore
    command.Parameters.Add(SqlParameter("@IsPublic", IsPublic)) |> ignore
    command.Parameters.Add(SqlParameter("@AlbumID", AlbumID)) |> ignore
    connection.Open()
    command.ExecuteNonQuery() 

