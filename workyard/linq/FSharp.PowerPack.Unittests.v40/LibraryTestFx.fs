
module FSharp.Core.Unittests.LibraryTestFx

open System
open System.Collections.Generic
open Microsoft.FSharp.Collections
open System.Linq

open NUnit.Framework

// Workaround for bug 3601, we are issuing an unnecessary warning
#nowarn "0004"

/// Check that the lamda throws an exception of the given type. Otherwise
/// calls Assert.Fail()
let private CheckThrowsExn<'a when 'a :> exn> (f : unit -> unit) =
    let funcThrowsAsExpected =
        try
            let _ = f ()
            Some "no exception" // Did not throw!
        with
        | :? 'a
            -> None // Thew null ref, OK
        | exn -> Some  (exn.ToString()) // Did now throw a null ref exception!
    match funcThrowsAsExpected with
    | None -> ()
    | Some s -> Assert.Fail(s)

let private CheckThrowsExn2<'a when 'a :> exn> s (f : unit -> unit) =
    let funcThrowsAsExpected =
        try
            let _ = f ()
            false // Did not throw!
        with
        | :? 'a
            -> true   // Thew null ref, OK
        | _ -> false  // Did now throw a null ref exception!
    if funcThrowsAsExpected
    then ()
    else Assert.Fail(s)

// Illegitimate exceptions. Once we've scrubbed the library, we should add an
// attribute to flag these exception's usage as a bug.
let CheckThrowsNullRefException      f = CheckThrowsExn<NullReferenceException>   f
let CheckThrowsIndexOutRangException f = CheckThrowsExn<IndexOutOfRangeException> f

// Legit exceptions
let CheckThrowsNotSupportedException f = CheckThrowsExn<NotSupportedException>    f
let CheckThrowsArgumentException     f = CheckThrowsExn<ArgumentException>        f
let CheckThrowsArgumentNullException f = CheckThrowsExn<ArgumentNullException>    f
let CheckThrowsArgumentNullException2 s f  = CheckThrowsExn2<ArgumentNullException>  s  f
let CheckThrowsKeyNotFoundException  f = CheckThrowsExn<KeyNotFoundException>     f
let CheckThrowsDivideByZeroException f = CheckThrowsExn<DivideByZeroException>    f
let CheckThrowsInvalidOperationExn   f = CheckThrowsExn<InvalidOperationException> f
let CheckThrowsFormatException       f = CheckThrowsExn<FormatException>           f
let CheckThrowsAggregateException    f = CheckThrowsExn<AggregateException>           f

// Verifies two sequences are equal (same length, equiv elements)
let VerifyPSeqsEqual seq1 seq2 =
    let len1 = PSeq.length seq1
    let len2 = PSeq.length seq2
    if len1 <> len2 then Assert.Fail(sprintf "seqs not equal length: %d and %d" len1 len2)
    let set1 = set seq1
    let set2 = set seq2
    if set1 = set2
    then ()
    else Assert.Fail(sprintf "contents not the same: %A %A" set1 set2)