namespace FSharp.PowerPack.Unittests
open NUnit.Framework

#nowarn "40"

[<TestFixture>]
type public HashtblTests() =

  
    [<Test>]
    member this.BasicTests() = 

            printf "I am running";
            let map1 = Hashtbl.create 5 in
            Hashtbl.add map1 5 5;
            Hashtbl.add map1 17 17;
            Hashtbl.add map1 94 94;
            let map2 = Hashtbl.copy map1 in
            Hashtbl.replace map1 5 7;
            Hashtbl.replace map1 17 19;
            check "21980d3" (Hashtbl.find map1 17) 19;    
            check "21980d3" (Hashtbl.find map1 5) 7;    
            check "21980d3" (Hashtbl.find map1 94) 94;    

            check "21980d3" (Hashtbl.find_all map1 17) [19];    
            check "21980d3" (Hashtbl.find_all map1 5) [7];    
            check "21980d3" (Hashtbl.find_all map1 94) [94];    

            check "21980d3" (Hashtbl.find map2 5) 5;    
            check "21980d3" (Hashtbl.find map2 17) 17;    
            check "21980d3" (Hashtbl.find map2 94) 94;    

            check "21980d3" (Hashtbl.find_all map2 5) [5];    
            check "21980d3" (Hashtbl.find_all map2 17) [17];    
            check "21980d3" (Hashtbl.find_all map2 94) [94];    
            printf "I am done"

    [<Test>]
    member this.NameResolutions() = 

        // check name resolutions
        let topEnvTable1 = new Microsoft.FSharp.Collections.HashSet<string>(10, HashIdentity.Structural)
        let topEnvTable2 = new Microsoft.FSharp.Collections.HashMultiMap<string,int>(10, HashIdentity.Structural)
        ()

    [<Test>]
    member this.MiscSamples() = 
        let SampleHashtbl1() =
            let tab = Microsoft.FSharp.Collections.HashMultiMap(30, HashIdentity.Structural) 
            let data = "The quick brown fox jumps over the lazy dog" 
            for i = 0 to data.Length - 1 do 
                let c = data.Chars(i) 
                match tab.TryFind(c) with 
                | None -> tab.Add(c,1)
                | Some v -> tab.Replace(c,v+1)
            tab |> Seq.iter (fun (KeyValue(c,v)) -> printf "Number of '%c' characters = %d\n" c v) 
          
        let SampleHashtbl1b() =
            let tab = Microsoft.FSharp.Collections.HashMultiMap<_,_>(30, HashIdentity.Structural) 
            let data = "The quick brown fox jumps over the lazy dog" 
            for i = 0 to data.Length - 1 do 
                let c = data.Chars(i) 
                match tab.TryFind(c) with 
                | None -> tab.Add(c,1)
                | Some v -> tab.Replace(c,v+1)
            tab |> Seq.iter (fun (KeyValue(c,v)) -> printf "Number of '%c' characters = %d\n" c v) 
          

        let SampleHashtbl2() =
            let tab = Hashtbl.create 30 
            let data = "The quick brown fox jumps over the lazy dog" 
            for i = 0 to data.Length - 1 do 
                let c = data.Chars(i) 
                match Hashtbl.tryfind tab c with 
                | None -> Hashtbl.add tab c 1
                | Some v -> Hashtbl.replace tab c (v+1)
            Hashtbl.iter (fun c v -> printf "Number of '%c' characters = %d\n" c v) tab
          
          
        let x1 = new HashMultiMap<int,int>(10, HashIdentity.Structural)
        let x2 = HashMultiMap<int,Set<int>>(10, HashIdentity.Structural)
        let x3 = HashMultiMap<int,Set<Set<int>>>(10, HashIdentity.Structural)
        let x4 = HashMultiMap<int,Set<Set<Set<int>>>>(10, HashIdentity.Structural)
        let x5 = HashMultiMap<int,Set<Set<Set<int>> >>(10, HashIdentity.Structural)
        let x6 = HashMultiMap<int,Set<Set<Set<int> > >>(10, HashIdentity.Structural)
        let x7 = HashMultiMap<int,Set<Set<Set<int> > > >(10, HashIdentity.Structural)
        let x8 = HashMultiMap<int,Set<Set<Set<int>>> >(10, HashIdentity.Structural)
        let x9 = HashMultiMap<Set<Set<int>>,int>(10, HashIdentity.Structural)
        let x10 = HashMultiMap<Set<Set<int>>,Set<int>>(10, HashIdentity.Structural)
        let x11 = HashMultiMap<Set<Set<int>>,Set<Set<int>>>(10, HashIdentity.Structural)
        ()
        

