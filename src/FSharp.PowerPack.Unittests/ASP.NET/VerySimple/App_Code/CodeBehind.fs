module N.Module

type C(y:int) =  member this.Factorial (u) =
                    let rec f x = if x <= 1 then 1 else x * (f (x - 1))
                    f u

                 static member NetFxVersion = System.Environment.Version
                 
                 member this.TestList n = [1 .. n] |> List.map this.Factorial
                                                   |> List.sum
                                             
