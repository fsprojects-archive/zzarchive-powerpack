namespace FSharp.PowerPack.Unittests

open Microsoft.FSharp.Math
open NUnit.Framework

#nowarn "40"

[<TestFixture>]
type public PermutationTests() =
    [<Test>]
    member this.BasicTests() = 
        let p2 = Permutation.ofArray [| 0;2;1;3 |]
        do test "cwnewr91" (p2 0 = 0)
        do test "cwnewr92" (p2 1 = 2)
        do test "cwnewr93" (p2 2 = 1)
        do test "cwnewr94" (p2 3 = 3)
        let p3 = p2 >> p2
        do test "cwnewr95" (p3 0 = 0)
        do test "cwnewr96" (p3 1 = 1)
        do test "cwnewr97" (p3 2 = 2)
        do test "cwnewr98" (p3 3 = 3)
        let p4 = Permutation.rotation 4 1
        do test "cwnewr99" (p4 0 = 1)
        do test "cwnewr9a" (p4 1 = 2)
        do test "cwnewr9s" (p4 2 = 3)
        do test "cwnewr9d" (p4 3 = 0)
        let p5 = Permutation.rotation 4 -1
        do test "cwnewr9f" (p5 0 = 3)
        do test "cwnewr9g" (p5 1 = 0)
        do test "cwnewr9h" (p5 2 = 1)
        do test "cwnewr9j" (p5 3 = 2)
        let p6 = Permutation.swap 2 3
        do test "cwnewr9k" (p6 0 = 0)
        do test "cwnewr9l" (p6 1 = 1)
        do test "cwnewr9z" (p6 2 = 3)
        do test "cwnewr9x" (p6 3 = 2)

        do test "cwcoinwcq" (Array.permute (Permutation.rotation 4 1) [| 0;1;2;3 |] = [| 3;0;1;2 |])
        do test "cwcoinwcw" (Array.permute (Permutation.swap 1 2) [| 0;1;2;3 |] = [| 0;2;1;3 |])
        do test "cwcoinwce" (try let _ = Permutation.ofArray [| 0;0 |] in false with :? System.ArgumentException -> true)
        do test "cwcoinwcr" (try let _ = Permutation.ofArray [| 1;1 |] in false with :? System.ArgumentException -> true)
        do test "cwcoinwct" (try let _ = Permutation.ofArray [| 1;3 |] in false with :? System.ArgumentException -> true)
        ()

