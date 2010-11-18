#r "System.Core.dll"

#load "PSeq.fs"

open Microsoft.FSharp.Collections

let isPrime n = 
    let upperBound = int (sqrt (float n))
    let hasDivisor =     
        [2..upperBound]
        |> List.exists (fun i -> n % i = 0)
    not hasDivisor
        
let nums = [|1..500000|]

let finalDigitOfPrimes = 
    nums 
    |> PSeq.filter isPrime
    |> PSeq.groupBy (fun i -> i % 10)
    |> PSeq.map (fun (k, vs) -> (k, Seq.length vs))
    |> PSeq.toArray

let averageOfFinalDigit = 
    nums 
    |> PSeq.filter isPrime
    |> PSeq.groupBy (fun i -> i % 10)
    |> PSeq.map (fun (k, vs) -> (k, Seq.length vs))
    |> PSeq.averageBy (fun (k,n) -> float n)

let sumOfLastDigitsOfPrimes = 
    nums 
    |> PSeq.filter isPrime
    |> PSeq.sumBy (fun x -> x % 10)

open System
open System.Linq
let stringSeq1:seq<string> = seq ["1";"2"]
let stringSeq2:seq<string> = seq ["3";"4"]

let appendStringSeq = System.Linq.ParallelEnumerable.Concat(stringSeq1.AsParallel(), stringSeq2.AsParallel())

let expectedResultString = seq ["1";"2";"3";"4"]

let res = 
    System.Linq.ParallelEnumerable.All(System.Linq.ParallelEnumerable.Zip(expectedResultString.AsParallel(), 
                                                                          appendStringSeq.AsParallel(), 
                                                                          Func<_,_,_>(fun x y -> x,y)),
                                       Func<_,bool>(fun (x,y) -> x = y))

