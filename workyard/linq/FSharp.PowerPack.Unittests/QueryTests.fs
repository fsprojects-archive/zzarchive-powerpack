// Copyright (c) Microsoft Corporation 2005-2007.
// This sample code is provided "as is" without warranty of any kind. 
// We disclaim all warranties, either express or implied, including the 
// warranties of merchantability and fitness for a particular purpose. 
//

namespace FSharp.PowerPack.Unittests

open NUnit.Framework
open System.IO
open System.Windows.Forms
open Microsoft.FSharp.Linq
open Microsoft.FSharp.Linq.Query

#if ENTITIES 
open Entities.Northwind

[<AutoOpen>]
module Database = 
    let dbPath = __SOURCE_DIRECTORY__ + @"\NORTHWND.MDF"
    
    System.IO.File.SetAttributes(dbPath, System.IO.File.GetAttributes dbPath &&& ~~~System.IO.FileAttributes.ReadOnly )
    let sqlServerInstance = @".\SQLEXPRESS"
    let connString = @"AttachDBFileName='" + dbPath + "';Server='" + sqlServerInstance + "';user instance=true;Integrated Security=SSPI;Connection Timeout=30" 
    let entityConnString = "metadata=res://*/Northwind.csdl|res://*/Northwind.ssdl|res://*/Northwind.msl;provider=System.Data.SqlClient;provider connection string=\"" + connString + "\""

    let db = new Entities(entityConnString)
#else
open LinqToSql.Northwind

[<AutoOpen>]
module Database = 
    let dbPath = __SOURCE_DIRECTORY__ + @"\NORTHWND.MDF"
    
    System.IO.File.SetAttributes(dbPath, System.IO.File.GetAttributes dbPath &&& ~~~System.IO.FileAttributes.ReadOnly )
    let sqlServerInstance = @".\SQLEXPRESS"
    let connString = @"AttachDBFileName='" + dbPath + "';Server='" + sqlServerInstance + "';user instance=true;Integrated Security=SSPI;Connection Timeout=30" 

    let db = new NorthwindDataContext(connString)
    do db.Log <- System.Console.Out
#endif    

[<AutoOpen>]
module Macros = 

    [<ReflectedDefinition>]
    let queryCondition (c:Customer) = c.City = "London" 
            
    // Check we can use a macro as a subsequence
    [<ReflectedDefinition>]
    let subSequence (c:Customer) = seq { for e in db.Employees do if queryCondition  c then yield c }

    // Nullable manipulations
    [<ReflectedDefinition>]
    let (=?!) (x : System.Nullable<'a>) (y: 'a) = 
        x.HasValue && x.Value = y

#if ENTITIES
[<TestFixture>]
type public EntitiesQueryTests() =
#else
[<TestFixture>]
type public LinqToSqlQueryTests() =
#endif

    [<Test>]
    member this.``Join using nested 'for': With 'String.Concat' call``() = 
      <@ seq { for p in db.Products do 
                 for c in db.Categories do
                   if p.CategoryID.Value = c.CategoryID then
                     yield System.String.Concat(p.ProductName, c.CategoryName) } @>
      |> query |> checkContains "ChaiBeverages"

    [<Test>]
    member this.``Join using nested 'for': With string concatenation``() = 
      <@ seq { for p in db.Products do 
                 for c in db.Categories do
                   if p.CategoryID.Value = c.CategoryID then
                     yield p.ProductName + " (" + c.CategoryName + ")" } @>
      |> query |> checkContains "Chai (Beverages)"

    [<Test>]
    member this.``Nested query: Calling combinators``() = 
      <@ seq { for c in db.Categories do 
                 yield db.Products
                       |> Seq.filter (fun p -> p.CategoryID.Value = c.CategoryID) 
                       |> Seq.map (fun p -> p.UnitPrice.Value)
                       |> Seq.average } @>
      |> query |> Seq.length |> checkEquals 8

    [<Test>]
    member this.BasicTests1() = 

        check "vesnvew01" (query <@ seq {  for c in db.Customers do if true then yield c.ContactName }  |> Seq.length @>) 91
        check "vesnvew02" (query <@ seq { for c in db.Customers do if true then yield (c.ContactName,c.Address) } |> Seq.length @>) 91
        #if ENTITIES
        // Using arbitrary methods in projection is not supported in LINQ to Entities
        // (apart from creating tuples & records which is supported explicitly)
        #else
        check "vesnvew03" (query <@ seq { for c in db.Customers do yield [c.ContactName] } |> Seq.length @>) 91
        #endif
        check "vesnvew04" (query <@ seq { for c in db.Customers do if true then yield [c.ContactName;c.Address] } |> Seq.length @>) 91
        check "vesnvew05" (query <@ seq { for c in db.Customers do if false then yield c.ContactName } |> Seq.length @>) 0
        check "vesnvew06" (query <@ seq { for c in db.Customers do if 1 > 2 then yield c.ContactName } |> Seq.length @>) 0
        check "vesnvew07" (query <@ seq { for c in db.Customers do if 2 > 1 then yield c.ContactName } |> Seq.length @>) 91

        check "vesnvew08" ((query <@ seq { for c in db.Customers do yield c.ContactName } |> Seq.take 4 @>  ) |> Seq.length) 4
        check "vesnvew09" (query <@ seq { for c in db.Customers do yield c.ContactName } |> Seq.take 4 |> Seq.length @>  ) 4

        check "vesnvew0q" ((query <@ seq { for c in db.Customers do yield c.ContactName } |> Seq.truncate 4 @>  ) |> Seq.length) 4
        check "vesnvew0w" (query <@ seq { for c in db.Customers do yield c.ContactName } |> Seq.truncate 4 |> Seq.length @>  ) 4

        check "vesnvew0e" ((query <@ seq { for c in db.Customers do yield c.ContactName } |> Seq.take 4 @>  ) |> Seq.length) 4
        check "vesnvew0r" (query <@ seq { for c in db.Customers do yield c.ContactName } |> Seq.take 4 |> Seq.length @>  ) 4

        check "vesnvew0t" (query <@ seq { for c in db.Customers do yield c.ContactName } |> Seq.head @>) "Maria Anders"
        check "vesnvew0y" (query <@ seq { for c in db.Customers do yield c.ContactName } |> Seq.distinct |> Seq.length @>) 91
        check "vesnvew0u" (query <@ seq { for c in db.Customers do yield c.City } |> Seq.distinct |> Seq.length @>) 69
        check "vesnvew0i" (query <@ seq { for c in db.Customers do yield c.CustomerID } |> Seq.distinct |> Seq.length @>) 91

        check "vesnvew0o" ((query <@ seq { for c in db.Customers do yield c.Orders.Count }  @>) |> Seq.averageBy float |> int) 9
        
        check "vesnvew0p" ((query <@ seq { for c in db.Customers do yield c.Orders.Count }  @>) |> Seq.filter (fun x -> true) |> Seq.length) 91
        check "vesnvew0a" ((query <@ seq { for c in db.Customers do yield c.Orders.Count }  @>) |> Seq.filter (fun x -> false) |> Seq.length) 0
        check "vesnvew0s" ((query <@ seq { for c in db.Customers do yield c }  @>) |> Seq.filter (fun x -> x.City = "London") |> Seq.length) 6

    [<Test>]
    member this.BasicTests2() = 
        check "vesnvew0d" ((query <@ db.Customers  @>) |> Seq.length) 91
        check "vesnvew0f" ((query <@ db.Customers |> Seq.filter (fun x -> true) @>) |> Seq.length) 91
        check "vesnvew0g" ((query <@ seq { for c in db.Customers do yield c.Orders.Count }  @>) |> Seq.filter (fun x -> false) |> Seq.length) 0
        check "vesnvew0h" ((query <@ seq { for c in db.Customers do yield c }  @>) |> Seq.filter (fun x -> x.City = "London") |> Seq.length) 6


        check "vesnvew0j" ((query <@ let f (c:Customer) = c.City = "London" in seq { for c in db.Customers do if f c then yield c }  @>) |> Seq.length) 6

        check "vesnvew0k" ((query <@ seq { for c in db.Customers do if queryCondition  c then yield c }  @>) |> Seq.length) 6


        check "vesnvew0l" ((query <@ seq { for c in db.Customers do yield! subSequence c }  @>) |> Seq.length) 54



        check "vesnvew0z" (query <@ seq { for c in db.Customers do if c.Address.Contains("Jardim das rosas") then yield c.ContactName } |> Seq.length @>) 1

        check "vesnvew0x" (query <@ seq { for c in db.Customers do if c.Address.Length = 17 then yield c.ContactName } |> Seq.length @>) 6

        check "vesnvew0c" (query <@ seq { for c in db.Customers do for e in db.Employees do yield e.LastName } |> Seq.length @>) 819

        check "vesnvew0v" (query <@ seq { for c in db.Customers do for e in db.Employees do if true then yield (e.LastName,c.ContactName) } @> |> Seq.length)  819
        check "vesnvew0b" (query <@ seq { for c in db.Customers do if true then for e in db.Employees do yield (e.LastName,c.ContactName) } @> |> Seq.length)  819
        check "vesnvew0n" (query <@ seq { for c in db.Customers do if c.Country.Length = 6 then for e in db.Employees do yield (e.LastName,c.ContactName) } @> |> Seq.length)  288

    [<Test>]
    member this.EqualityAndComparison() =        
        let allEmployees = db.Employees |> Seq.toArray        
        check "terty01"  (query <@ seq { for c in db.Employees do if c.EmployeeID < 0 then yield c  } @> |> Seq.length) 0
        check "terty01a" (query <@ seq { for c in db.Employees do if c.EmployeeID > 0 then yield c  } @> |> Seq.length) (allEmployees.Length)
        check "terty01b" (query <@ seq { for c in db.Employees do if c.EmployeeID = 0 then yield c  } @> |> Seq.length) 0
        check "terty02" (query <@ seq { for c in db.Customers do if c.CompanyName = "Z"  then yield c  } @> |> Seq.length) 0
        check "terty03" (query <@ seq { for c in db.Employees do if c.EmployeeID <> allEmployees.[0].EmployeeID  then yield c  } @> |> Seq.length) (allEmployees.Length - 1)
        let jan1of2010 = System.DateTime(2010,1,1)
        check "terty04" 
            (query <@ seq { for c in db.Orders do if c.OrderDate.HasValue && c.OrderDate.Value <= jan1of2010  then yield c  } @> |> Seq.length) 
            (db.Orders |> Seq.filter(fun o -> o.OrderDate.HasValue) |> Seq.length)
        let jan1of1900 = System.DateTime(1900,1,1)
        check "terty05" 
            (query <@ seq { for c in db.Orders do if c.OrderDate.HasValue && c.OrderDate.Value >= jan1of1900  then yield c  } @> |> Seq.length) 
            (db.Orders |> Seq.filter(fun o -> o.OrderDate.HasValue) |> Seq.length)


    [<Test>]
    member this.BasicTests3() = 
        check "vesnvew0m" (query <@ seq { for c in db.Customers do 
                                             for e in db.Employees do 
                                                 if c.Address.Contains("Jardim") && 
                                                    c.Address.Contains("rosas") then 
                                                       yield (e.LastName,c.ContactName) } @> 
                                   |> Seq.length) 9
        
        check "vesnvew0QQ" (query <@ seq { for c in db.Customers do 
                                           for e in db.Employees do 
                                            if c.ContactName = e.LastName then 
                                             yield c.ContactName } @> 
                                   |> Seq.length) 0


        check "vesnvew0WW" (query <@ seq { for p in db.Products do
                                            for c in db.Categories do
                                             for s in db.Suppliers  do
                                              yield c.CategoryName, p.ProductName, s.CompanyName } 
                                   |> Seq.length @>) 17864


    [<Test>]
    member this.NullableTests1() = 

        check "vesnvew0EE" (query <@ seq { for p in db.Products do
                                           for c in db.Categories do
                                            for s in db.Suppliers  do
                                              if p.CategoryID =?! c.CategoryID &&
                                                 p.SupplierID =?! s.SupplierID then 
                                                yield c.CategoryName, p.ProductName, s.CompanyName } 
                                   |> Seq.length @>) 77

        check "vesnvew0RR" (query <@ seq { for p in db.Products do
                                             if p.CategoryID =?! 1 then 
                                                 yield p.ProductName }  |> Seq.length @>) 12
        
    [<Test>]
    member this.BasicTests4() = 

        // By design: Can't use Seq.groupBy
        check "vrejkner0TT" (try let _ = query <@ Seq.groupBy (fun (p:Product) -> p.CategoryID) db.Products @> in false with _ -> true) true

        check "vrejkner0YY" (try let _ = query <@ db.Products |> Seq.groupBy (fun p -> p.CategoryID) @> in false with _ -> true) true


        check "cnewnc081"  (query <@ Query.groupBy
                                       (fun (c:Customer) -> c.Address.Length) 
                                       (seq { for c in db.Customers do yield c }) 
                                    |> Seq.length @> ) 
                          22

    [<Test>]
    member this.BasicTests5() = 

        check "cnewnc082"  (query <@ Seq.sortBy
                                       (fun (c:Customer) -> c.Address.Length) 
                                       (seq { for c in db.Customers do yield c }) 
                                    |> Seq.length @> ) 
                          91

        check "cnewnc083"  (query <@ Seq.sort (seq { for c in db.Customers do yield c.Address.Length }) 
                                     |> Seq.length @> ) 
                          91


        check "cnewnc083"  (query <@ seq { for c in db.Customers do yield c.Address.Length }
                                     |> Seq.sort
                                     |> Seq.length @> ) 
                          91

        check "cnewnc094"  (query <@ seq { for c in db.Customers do yield c }
                                    |> Seq.sortBy (fun c -> c.Address.Length)                                    
                                    |> Seq.length @> ) 
                          91
        check "cnewnc094"  (query <@ Seq.length
                                       (Seq.sortBy (fun (c:Customer) -> c .Address.Length) 
                                         (seq { for c in db.Customers do yield c })) @> ) 
                          91

    [<Test>]
    member this.BasicTests6() = 

        check "cnewnc085"  (query <@ Seq.exists
                                       (fun (c:Customer) -> c.Address.Length > 10) 
                                       (seq { for c in db.Customers do yield c }) @> ) 
                          true


        check "cnewnc086"  (query <@ Seq.forall
                                       (fun (c:Customer) -> c.Address.Length <= 10) 
                                       (seq { for c in db.Customers do yield c }) @> ) 
                          false

        check "cnewnc087"  (query <@ Query.join 
                                       (seq { for e in db.Employees do yield e }) 
                                       (seq { for c in db.Customers do yield c }) 
                                       (fun e -> e.Country) 
                                       (fun c -> c.Country) 
                                       (fun e c -> (e,c)) 
                                    |> Seq.length  @> ) 
                          93

        check "cnewnc088"  (query <@ seq { for e in db.Employees do  
                                             for c in db.Customers do 
                                                 if e.Country = c.Country then 
                                                     yield (e,c) } 
                                    |> Seq.length  @> ) 
                      93


    [<Test>]
    member this.BasicTests7() = 
        check "cnewnc089"  (query <@ Linq.Query.groupJoin 
                                       (seq { for c in db.Employees do yield c }) 
                                       (seq { for c in db.Customers do yield c }) 
                                       (fun e -> e.Country) 
                                       (fun c -> c.Country) 
                                       (fun e cs -> (e,Seq.length cs)) 
                                    |> Seq.length  @> ) 
                          9



        check "we09j" 
              (query <@ seq { let grouping = db.Products |> Linq.Query.groupBy (fun p -> p.CategoryID)
                              yield! grouping } |> Seq.length   @>) 8

        check "we09j" 
              (query <@ seq { let grouping = db.Products |> Linq.Query.groupBy (fun p -> p.CategoryID)
                              for group in grouping  do
                                  yield group.Key }  |> Seq.length  @>) 8

        check "we09j" 
              (query <@ seq { let grouping = db.Products |> Linq.Query.groupBy (fun p -> p.CategoryID)
                              for group in grouping  do
                                          let lowest = group |> Linq.Query.minBy (fun x -> x.UnitPrice.Value)
                                          yield lowest } @> |> Seq.sum) 
              56.6500M
          
    [<Test>]
    member this.BasicTests8() = 
        check "we09j" 
              (query <@ seq { let grouping = db.Products |> Linq.Query.groupBy (fun p -> p.CategoryID)
                              for group in grouping  do
                                          let lowest = group |> Seq.minBy (fun x -> x.UnitPrice.GetValueOrDefault())
                                          let res = seq { for p in group do if p.UnitPrice = lowest.UnitPrice then yield p }
                                          yield group } |> Seq.length @>) 
              8

        check "we09j" 
              (query <@ db.Customers |> Seq.length @>)  91
        check "we09j" 
              (query <@ db.Orders |> Seq.length @>) 830

        check "we09j" 
              (((query <@ seq { for c in db.Order_Details -> c.Discount } |> Seq.sum @> - 121.04f) |> abs) < 0.001f) true
        check "we09j" 
              (query <@ seq { for c in db.Order_Details -> c.Discount } |> Seq.sumBy (fun d -> d + 1.0f) @> >= 2276.0f) true


(*
    check "vesnvew0" (query <@ [ for c in db.Customers do yield c.ContactName ] @>)
          ["Maria Anders"; "Ana Trujillo"; "Antonio Moreno"; "Thomas Hardy";
           "Christina Berglund"; "Hanna Moos"; "Frédérique Citeaux"; "Martín Sommer";
           "Laurence Lebihan"; "Elizabeth Lincoln"; "Victoria Ashworth";
           "Patricio Simpson"; "Francisco Chang"; "Yang Wang"; "Pedro Afonso";
           "Elizabeth Brown"; "Sven Ottlieb"; "Janine Labrune"; "Ann Devon";
           "Roland Mendel"; "Aria Cruz"; "Diego Roel"; "Martine Rancé"; "Maria Larsson";
           "Peter Franken"; "Carine Schmitt"; "Paolo Accorti"; "Lino Rodriguez";
           "Eduardo Saavedra"; "José Pedro Freyre"; "André Fonseca"; "Howard Snyder";
           "Manuel Pereira"; "Mario Pontes"; "Carlos Hernández"; "Yoshi Latimer";
           "Patricia McKenna"; "Helen Bennett"; "Philip Cramer"; "Daniel Tonini";
           "Annette Roulet"; "Yoshi Tannamuri"; "John Steel"; "Renate Messner";
           "Jaime Yorres"; "Carlos González"; "Felipe Izquierdo"; "Fran Wilson";
           "Giovanni Rovelli"; "Catherine Dewey"; "Jean Fresnière"; "Alexander Feuer";
           "Simon Crowther"; "Yvonne Moncada"; "Rene Phillips"; "Henriette Pfalzheim";
           "Marie Bertrand"; "Guillermo Fernández"; "Georg Pipps"; "Isabel de Castro";
           "Bernardo Batista"; "Lúcia Carvalho"; "Horst Kloss"; "Sergio Gutiérrez";
           "Paula Wilson"; "Maurizio Moroni"; "Janete Limeira"; "Michael Holz";
           "Alejandra Camino"; "Jonas Bergulfsen"; "Jose Pavarotti"; "Hari Kumar";
           "Jytte Petersen"; "Dominique Perrier"; "Art Braunschweiger"; "Pascale Cartrain";
           "Liz Nixon"; "Liu Wong"; "Karin Josephs"; "Miguel Angel Paolino";
           "Anabela Domingues"; "Helvetius Nagy"; "Palle Ibsen"; "Mary Saveley";
           "Paul Henriot"; "Rita Müller"; "Pirkko Koskitalo"; "Paula Parente";
           "Karl Jablonski"; "Matti Karttunen"; "Zbyszek Piestrzeniewicz"]
           
    check "vesnvew0" (query <@ [ for c in db.Customers do yield c.Address ] @>)
         ["Obere Str. 57"; "Avda. de la Constitución 2222"; "Mataderos  2312";
         "120 Hanover Sq."; "Berguvsvägen  8"; "Forsterstr. 57"; "24, place Kléber";
         "C/ Araquil, 67"; "12, rue des Bouchers"; "23 Tsawassen Blvd.";
         "Fauntleroy Circus"; "Cerrito 333"; "Sierras de Granada 9993"; "Hauptstr. 29";
         "Av. dos Lusíadas, 23"; "Berkeley Gardens 12  Brewery"; "Walserweg 21";
         "67, rue des Cinquante Otages"; "35 King George"; "Kirchgasse 6";
         "Rua Orós, 92"; "C/ Moralzarzal, 86"; "184, chaussée de Tournai";
         "Åkergatan 24"; "Berliner Platz 43"; "54, rue Royale"; "Via Monte Bianco 34";
         "Jardim das rosas n. 32"; "Rambla de Cataluña, 23"; "C/ Romero, 33";
         "Av. Brasil, 442"; "2732 Baker Blvd."; "5ª Ave. Los Palos Grandes";
         "Rua do Paço, 67"; "Carrera 22 con Ave. Carlos Soublette #8-35";
         "City Center Plaza 516 Main St."; "8 Johnstown Road";
         "Garden House Crowther Way"; "Maubelstr. 90"; "67, avenue de l'Europe";
         "1 rue Alsace-Lorraine"; "1900 Oak St."; "12 Orchestra Terrace"; "Magazinweg 7";
         "87 Polk St. Suite 5"; "Carrera 52 con Ave. Bolívar #65-98 Llano Largo";
         "Ave. 5 de Mayo Porlamar"; "89 Chiaroscuro Rd."; "Via Ludovico il Moro 22";
         "Rue Joseph-Bens 532"; "43 rue St. Laurent"; "Heerstr. 22";
         "South House 300 Queensbridge"; "Ing. Gustavo Moncada 8585 Piso 20-A";
         "2743 Bering St."; "Mehrheimerstr. 369"; "265, boulevard Charonne";
         "Calle Dr. Jorge Cash 321"; "Geislweg 14"; "Estrada da saúde n. 58";
         "Rua da Panificadora, 12"; "Alameda dos Canàrios, 891"; "Taucherstraße 10";
         "Av. del Libertador 900"; "2817 Milton Dr."; "Strada Provinciale 124";
         "Av. Copacabana, 267"; "Grenzacherweg 237"; "Gran Vía, 1";
         "Erling Skakkes gate 78"; "187 Suffolk Ln."; "90 Wadhurst Rd."; "Vinbæltet 34";
         "25, rue Lauriston"; "P.O. Box 555"; "Boulevard Tirou, 255";
         "89 Jefferson Way Suite 2"; "55 Grizzly Peak Rd."; "Luisenstr. 48";
         "Avda. Azteca 123"; "Av. Inês de Castro, 414"; "722 DaVinci Blvd.";
         "Smagsloget 45"; "2, rue du Commerce"; "59 rue de l'Abbaye";
         "Adenauerallee 900"; "Torikatu 38"; "Rua do Mercado, 12";
         "305 - 14th Ave. S. Suite 3B"; "Keskuskatu 45"; "ul. Filtrowa 68"]    
    
    check "vesnvew0" (query <@ [ for c in db.Employees do yield c.LastName ] @>)
       ["Buchanan"; "Callahan"; "Davolio"; "Dodsworth"; "Fuller"; "King"; "Leverling";"Peacock"; "Suyama"]

    check "vesnvew0" (query <@ [ for c in db.Customers do if true then yield c.ContactName ] @>)
        ["Maria Anders"; "Ana Trujillo"; "Antonio Moreno"; "Thomas Hardy";
         "Christina Berglund"; "Hanna Moos"; "Frédérique Citeaux"; "Martín Sommer";
         "Laurence Lebihan"; "Elizabeth Lincoln"; "Victoria Ashworth";
         "Patricio Simpson"; "Francisco Chang"; "Yang Wang"; "Pedro Afonso";
         "Elizabeth Brown"; "Sven Ottlieb"; "Janine Labrune"; "Ann Devon";
         "Roland Mendel"; "Aria Cruz"; "Diego Roel"; "Martine Rancé"; "Maria Larsson";
         "Peter Franken"; "Carine Schmitt"; "Paolo Accorti"; "Lino Rodriguez";
         "Eduardo Saavedra"; "José Pedro Freyre"; "André Fonseca"; "Howard Snyder";
         "Manuel Pereira"; "Mario Pontes"; "Carlos Hernández"; "Yoshi Latimer";
         "Patricia McKenna"; "Helen Bennett"; "Philip Cramer"; "Daniel Tonini";
         "Annette Roulet"; "Yoshi Tannamuri"; "John Steel"; "Renate Messner";
         "Jaime Yorres"; "Carlos González"; "Felipe Izquierdo"; "Fran Wilson";
         "Giovanni Rovelli"; "Catherine Dewey"; "Jean Fresnière"; "Alexander Feuer";
         "Simon Crowther"; "Yvonne Moncada"; "Rene Phillips"; "Henriette Pfalzheim";
         "Marie Bertrand"; "Guillermo Fernández"; "Georg Pipps"; "Isabel de Castro";
         "Bernardo Batista"; "Lúcia Carvalho"; "Horst Kloss"; "Sergio Gutiérrez";
         "Paula Wilson"; "Maurizio Moroni"; "Janete Limeira"; "Michael Holz";
         "Alejandra Camino"; "Jonas Bergulfsen"; "Jose Pavarotti"; "Hari Kumar";
         "Jytte Petersen"; "Dominique Perrier"; "Art Braunschweiger"; "Pascale Cartrain";
         "Liz Nixon"; "Liu Wong"; "Karin Josephs"; "Miguel Angel Paolino";
         "Anabela Domingues"; "Helvetius Nagy"; "Palle Ibsen"; "Mary Saveley";
         "Paul Henriot"; "Rita Müller"; "Pirkko Koskitalo"; "Paula Parente";
         "Karl Jablonski"; "Matti Karttunen"; "Zbyszek Piestrzeniewicz"]    
*)

module LocalType =
    open System.Linq
    type Foo() =                
        let source = [1;2;3;4;5] |> Queryable.AsQueryable
     
        let bar() =     
            <@ seq { for x in source -> x + 1 } @>
     
        let bar2() = <@ source @>
        member this.Bar() = bar()

        member this.Bar2() = bar2()

    [<TestFixture>]
    type Test4060() =
        [<Test>]
        member this.TestLocalField0() =
            try 
                (new Foo()).Bar2() |> query |> ignore
            with
            | :? System.NotSupportedException -> ()
            |   _ -> Assert.Fail("Should detect and report this case")


        [<Test>]
        member this.TestLocalField() =
            try 
                (new Foo()).Bar() |> query |> ignore
            with
            | :? System.NotSupportedException -> ()
            |   _ -> Assert.Fail("Should detect and report this case")
