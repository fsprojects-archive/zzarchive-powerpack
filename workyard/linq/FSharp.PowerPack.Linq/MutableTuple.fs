// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation 2005-2011.
// This sample code is provided "as is" without warranty of any kind. 
// We disclaim all warranties, either express or implied, including the 
// warranties of merchantability and fitness for a particular purpose. 
// ----------------------------------------------------------------------------

namespace Microsoft.FSharp.Linq.Runtime

// ----------------------------------------------------------------------------
// Mutable Tuples - used when translating queries that use F# tuples
// and records. We replace tuples/records with mutable tubples which 
// are handled correctly by LINQ to SQL/Entities
// ----------------------------------------------------------------------------

/// This type shouldn't be used directly from user code.
type MutableTuple<'T1>() =
  [<DefaultValue>]
  val mutable private item1 : 'T1
  member x.Item1 with get() = x.item1 and set(v) = x.item1 <- v


/// This type shouldn't be used directly from user code.
type MutableTuple<'T1, 'T2>() =
  [<DefaultValue>]
  val mutable private item1 : 'T1
  member x.Item1 with get() = x.item1 and set(v) = x.item1 <- v

  [<DefaultValue>]
  val mutable private item2 : 'T2
  member x.Item2 with get() = x.item2 and set(v) = x.item2 <- v


/// This type shouldn't be used directly from user code.
type MutableTuple<'T1, 'T2, 'T3>() =
  [<DefaultValue>]
  val mutable private item1 : 'T1
  member x.Item1 with get() = x.item1 and set(v) = x.item1 <- v

  [<DefaultValue>]
  val mutable private item2 : 'T2
  member x.Item2 with get() = x.item2 and set(v) = x.item2 <- v

  [<DefaultValue>]
  val mutable private item3 : 'T3
  member x.Item3 with get() = x.item3 and set(v) = x.item3 <- v


/// This type shouldn't be used directly from user code.
type MutableTuple<'T1, 'T2, 'T3, 'T4>() =
  [<DefaultValue>]
  val mutable private item1 : 'T1
  member x.Item1 with get() = x.item1 and set(v) = x.item1 <- v

  [<DefaultValue>]
  val mutable private item2 : 'T2
  member x.Item2 with get() = x.item2 and set(v) = x.item2 <- v

  [<DefaultValue>]
  val mutable private item3 : 'T3
  member x.Item3 with get() = x.item3 and set(v) = x.item3 <- v

  [<DefaultValue>]
  val mutable private item4 : 'T4
  member x.Item4 with get() = x.item4 and set(v) = x.item4 <- v


/// This type shouldn't be used directly from user code.
type MutableTuple<'T1, 'T2, 'T3, 'T4, 'T5>() =
  [<DefaultValue>]
  val mutable private item1 : 'T1
  member x.Item1 with get() = x.item1 and set(v) = x.item1 <- v

  [<DefaultValue>]
  val mutable private item2 : 'T2
  member x.Item2 with get() = x.item2 and set(v) = x.item2 <- v

  [<DefaultValue>]
  val mutable private item3 : 'T3
  member x.Item3 with get() = x.item3 and set(v) = x.item3 <- v

  [<DefaultValue>]
  val mutable private item4 : 'T4
  member x.Item4 with get() = x.item4 and set(v) = x.item4 <- v

  [<DefaultValue>]
  val mutable private item5 : 'T5
  member x.Item5 with get() = x.item5 and set(v) = x.item5 <- v


/// This type shouldn't be used directly from user code.
type MutableTuple<'T1, 'T2, 'T3, 'T4, 'T5, 'T6>() =
  [<DefaultValue>]
  val mutable private item1 : 'T1
  member x.Item1 with get() = x.item1 and set(v) = x.item1 <- v

  [<DefaultValue>]
  val mutable private item2 : 'T2
  member x.Item2 with get() = x.item2 and set(v) = x.item2 <- v

  [<DefaultValue>]
  val mutable private item3 : 'T3
  member x.Item3 with get() = x.item3 and set(v) = x.item3 <- v

  [<DefaultValue>]
  val mutable private item4 : 'T4
  member x.Item4 with get() = x.item4 and set(v) = x.item4 <- v

  [<DefaultValue>]
  val mutable private item5 : 'T5
  member x.Item5 with get() = x.item5 and set(v) = x.item5 <- v

  [<DefaultValue>]
  val mutable private item6 : 'T6
  member x.Item6 with get() = x.item6 and set(v) = x.item6 <- v


/// This type shouldn't be used directly from user code.
type MutableTuple<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7>() =
  [<DefaultValue>]
  val mutable private item1 : 'T1
  member x.Item1 with get() = x.item1 and set(v) = x.item1 <- v

  [<DefaultValue>]
  val mutable private item2 : 'T2
  member x.Item2 with get() = x.item2 and set(v) = x.item2 <- v

  [<DefaultValue>]
  val mutable private item3 : 'T3
  member x.Item3 with get() = x.item3 and set(v) = x.item3 <- v

  [<DefaultValue>]
  val mutable private item4 : 'T4
  member x.Item4 with get() = x.item4 and set(v) = x.item4 <- v

  [<DefaultValue>]
  val mutable private item5 : 'T5
  member x.Item5 with get() = x.item5 and set(v) = x.item5 <- v

  [<DefaultValue>]
  val mutable private item6 : 'T6
  member x.Item6 with get() = x.item6 and set(v) = x.item6 <- v

  [<DefaultValue>]
  val mutable private item7 : 'T7
  member x.Item7 with get() = x.item7 and set(v) = x.item7 <- v


/// This type shouldn't be used directly from user code.
type MutableTuple<'T1, 'T2, 'T3, 'T4, 'T5, 'T6, 'T7, 'T8>() =
  [<DefaultValue>]
  val mutable private item1 : 'T1
  member x.Item1 with get() = x.item1 and set(v) = x.item1 <- v

  [<DefaultValue>]
  val mutable private item2 : 'T2
  member x.Item2 with get() = x.item2 and set(v) = x.item2 <- v

  [<DefaultValue>]
  val mutable private item3 : 'T3
  member x.Item3 with get() = x.item3 and set(v) = x.item3 <- v

  [<DefaultValue>]
  val mutable private item4 : 'T4
  member x.Item4 with get() = x.item4 and set(v) = x.item4 <- v

  [<DefaultValue>]
  val mutable private item5 : 'T5
  member x.Item5 with get() = x.item5 and set(v) = x.item5 <- v

  [<DefaultValue>]
  val mutable private item6 : 'T6
  member x.Item6 with get() = x.item6 and set(v) = x.item6 <- v

  [<DefaultValue>]
  val mutable private item7 : 'T7
  member x.Item7 with get() = x.item7 and set(v) = x.item7 <- v

  [<DefaultValue>]
  val mutable private item8 : 'T8
  member x.Item8 with get() = x.item8 and set(v) = x.item8 <- v