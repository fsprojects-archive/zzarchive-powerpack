
namespace MyNamespace

module M1 = 
    // This is f
    let f (x:int) = x

// This is type C
type C() = 
   /// This is P
   member x.MyProperty = 3
   /// This is M
   member x.MyMethod(y:int) = 3 + y
   [<CLIEvent>]
   member x.MyEvent = (new Event<int>()).Publish

module Nexted = 
    module M1 = 
        // This is f
        let f (x:int) = x

    // This is type C
    type C() = 
       /// This is P
       member x.MyProperty = 3
       /// This is M
       member x.MyMethod(y:int) = 3 + y
       [<CLIEvent>]
       member x.MyEvent = (new Event<int>()).Publish



   