namespace FSharp.PowerPack.Unittests
open NUnit.Framework

module MG = Microsoft.FSharp.Math.Matrix.Generic
module MDouble  = Math.Matrix
module VGeneric = Math.Vector.Generic
module VDouble  = Math.Vector

#nowarn "44" // This construct is deprecated. 

module Notation = 

    type matrix = Matrix<float>
    type vector = Vector<float>
    type rowvec = RowVector<float>
    type complex = Complex
  
    let gmatrix ll = Matrix.Generic.ofSeq ll
    let gvector l = Vector.Generic.ofSeq  l
    let growvec l = RowVector.Generic.ofSeq l
      
    let S x = Matrix.Generic.ofScalar x
    let RV x = Matrix.Generic.ofRowVector x 
    let V x  = Matrix.Generic.ofVector x
    let M2V x = Matrix.Generic.toVector x
    let M2RV x = Matrix.Generic.toRowVector x
    let M2S x = Matrix.Generic.toScalar x

    let complex x y = Complex.mkRect (x,y)
    // Notational conveniences
    let matrix ll = Matrix.ofSeq ll
    let vector l = Vector.ofSeq l
    let rowvec l = RowVector.ofSeq l

// This code used to be in the power pack but is now part of these tests
module LinearAlgebra = 
        let inline sumfR f (a,b) =
            let mutable res = 0.0 in
            for i = a to b do
                res <- res + f i;
            done;
            res

        let isSymmetric a = a |> Matrix.foralli (fun i j aij -> aij = a.[j,i]) 
        let isLowerTriangular a = a |> Matrix.foralli (fun i j aij -> i>=j || aij=0.0)

        let choleskyFactor (a: matrix) =
          let nA,mA = a.Dimensions in
          if nA<>mA              then invalid_arg "choleskyFactor: not square";
          if not (isSymmetric a) then invalid_arg "choleskyFactor: not symmetric";
          let lres = Matrix.zero nA nA  in(* nA=mA *)
          for j=0 to nA-1 do
            for i=j to nA-1 do (* j <= i *)
              let psum = sumfR (fun k -> lres.[i,k] * lres.[j,k]) (0,j-1) in
              let a_ij = a.[i,j] in
              if i=j then
                lres.[i,i] <- (System.Math.Sqrt (a_ij - psum))
              else
                lres.[i,j] <- ((a_ij - psum) / lres.[j,j])
          lres

        let lowerTriangularInverse (l: matrix) =
          let nA,mA = l.Dimensions in
          let res = Matrix.zero nA nA  in (* nA=mA *)
          for j=0 to nA-1 do
            for i=j to nA-1 do (* j <= i *)
              let psum   = sumfR (fun k -> l.[i,k] * res.[k,j]) (0,i-1) in
              let id_ij  = if i=j then 1.0 else 0.0 in
              let res_ij = (id_ij - psum) / l.[i,i] in
              res.[i, j] <- res_ij
          res

        let symmetricInverse a =
          let l     = choleskyFactor a         in
          let l_t   = l.Transpose                in 
          let l_inv = lowerTriangularInverse l in
          let a_inv = l_inv.Transpose * l_inv in
          a_inv

        let determinant (a: matrix) =
          let rec det js ks =
            match ks with
              | []    -> 1.0
              | k::ks -> 
                let rec split sign (preJs,js) =
                  match js with
                  | []    -> 0.0
                  | j::js -> sign * a.[j,k] * det (List.rev preJs @ js) ks
                             +
                             split (0.0 - sign) (j::preJs,js) in
                split 1.0 ([],js) in
          let nA,mA = a.Dimensions in
          if nA<>mA then invalid_arg "determinant: not square";
          det [0..nA-1] [0..nA-1]

        let cholesky a =
          let l    = choleskyFactor         a in
          let lInv = lowerTriangularInverse l in
          l,lInv

open Notation
open LinearAlgebra

[<TestFixture>]
type public MatrixVectorTests() =
  
    [<Test>]
    member this.VectorSlicingTests() = 

            let v1 = vector [| 1.0;2.0;3.0;4.0;5.0;6.0 |]
            test "vslice1923" (v1.[*] = v1)
            test "vslice1923" (v1.[0..] = v1)
            test "vslice1924" (v1.[1..] = vector [| 2.0;3.0;4.0;5.0;6.0 |])
            test "vslice1925" (v1.[2..] = vector [| 3.0;4.0;5.0;6.0 |])
            test "vslice1926" (v1.[5..] = vector [| 6.0 |])
            test "vslice1927" (v1.[6..] = vector [| |])
            test "vslice1928" (try v1.[7..] |> ignore; false with _ -> true)
            test "vslice1929" (try v1.[-1 ..] |> ignore; false with _ -> true)
            test "vslice1917" (v1.[..0] = vector [| 1.0 |])
            test "vslice1911" (v1.[..1] = vector [| 1.0;2.0|])
            test "vslice1912" (v1.[..2] = vector [| 1.0;2.0;3.0 |])
            test "vslice1913" (v1.[..3] = vector [| 1.0;2.0;3.0;4.0|])
            test "vslice1914" (v1.[..4] = vector [| 1.0;2.0;3.0;4.0;5.0 |])
            test "vslice1915" (v1.[..5] = vector [| 1.0;2.0;3.0;4.0;5.0;6.0 |])
            test "vslice1918" (try v1.[..6] |> ignore; false with _ -> true)
            test "vslice1817" (v1.[1..0] = vector [|  |])
            test "vslice1811" (v1.[1..1] = vector [| 2.0 |])
            test "vslice1812" (v1.[1..2] = vector [| 2.0;3.0 |])
            test "vslice1813" (v1.[1..3] = vector [| 2.0;3.0;4.0|])
            test "vslice1814" (v1.[1..4] = vector [| 2.0;3.0;4.0;5.0|])
            test "vslice1815" (v1.[1 ..5] = vector [| 2.0;3.0;4.0;5.0;6.0|])
            test "vslice1818" (try v1.[1..6] |> ignore; false with _ -> true)

    [<Test>]
    member this.MatrixSlicingTests() = 

        let m1 = matrix [| [| 1.0;2.0;3.0;4.0;5.0;6.0 |];
                           [| 10.0;20.0;30.0;40.0;50.0;60.0 |]  |]
        test "mslice1923" (m1.[*,*] = m1)
        test "mslice1924" (m1.[0..,*] = matrix [| [| 1.0;2.0;3.0;4.0;5.0;6.0 |];
                                                  [| 10.0;20.0;30.0;40.0;50.0;60.0 |]  |])
        test "mslice1924" (m1.[1..,*] = matrix [| //[| 1.0;2.0;3.0;4.0;5.0;6.0 |];
                                                  [| 10.0;20.0;30.0;40.0;50.0;60.0 |]  |])
        test "mslice1924" (m1.[..0,*] = matrix [| [| 1.0;2.0;3.0;4.0;5.0;6.0 |];
                                                  //[| 10.0;20.0;30.0;40.0;50.0;60.0 |]  
                                                |])
        test "mslice1924" (m1.[*,0..] = matrix [| [| 1.0;2.0;3.0;4.0;5.0;6.0 |];
                                                  [| 10.0;20.0;30.0;40.0;50.0;60.0 |]  
                                                |])
        test "mslice1924" (m1.[*,1..] = matrix [| [| 2.0;3.0;4.0;5.0;6.0 |];
                                                  [| 20.0;30.0;40.0;50.0;60.0 |]  
                                                |])
        test "mslice1924" (m1.[*,2..] = matrix [| [| 3.0;4.0;5.0;6.0 |];
                                                  [| 30.0;40.0;50.0;60.0 |]  
                                                |])
        test "mslice1924" (m1.[*,3..] = matrix [| [| 4.0;5.0;6.0 |];
                                                  [| 40.0;50.0;60.0 |]  
                                                |])
        test "mslice1924" (m1.[*,4..] = matrix [| [| 5.0;6.0 |];
                                                  [| 50.0;60.0 |]  
                                                |])
        test "mslice1924" (m1.[*,5..] = matrix [| [| 6.0 |];
                                                  [| 60.0 |]  
                                                |])

    [<Test>]
    member this.SomeSlicingTests() = 

        let M = matrix [[ 0.0; 1.0; 2.0];[ 0.0; 1.0; 2.0];[ 0.0; 1.0; 2.0]]

        check "ce0cew9js" M.[0..,0..] M
        M.[1..2,0..1] <- matrix [[ 10.0;11.0];[ 12.0;13.0]]
        check "ce0cew9js" M.[0..,0..] (matrix [[ 0.0; 1.0; 2.0];[ 10.0; 11.0; 2.0];[ 12.0; 13.0; 2.0]])

        
    [<Test>]
    member this.ComplexOpsTest() = 

        do test "989w42rra1" ((complex 1.0 2.0).RealPart = 1.0)
        do test "989w42rra2" ((complex 1.0 2.0).ImaginaryPart = 2.0)
        do test "989w42rra3" ((complex 1.0 2.0).i = 2.0)
        do test "989w42rra4" ((complex 1.0 2.0).r = 1.0)
        do test "989w42rra5" ((complex 3.0 4.0).Magnitude = 5.0)
        do test "989w42rra6" ((complex 1.0 0.0).Phase = 0.0)
        do test "989w42rra7" ((complex 1.0 0.0).Conjugate = (complex 1.0 0.0))
        do test "989w42rra8" (Math.Complex.Create(1.0,2.0) = (complex 1.0 2.0))
        do test "989w42rra9" (Math.Complex.Zero = complex 0.0 0.0)
        do test "989w42rra0" (Math.Complex.Zero = Math.Complex.CreatePolar(0.0,1.0))
        do test "989w42rra11" (Math.Complex.One = complex 1.0 0.0)
        do test "989w42rra12" (Math.Complex.OneI = complex 0.0 1.0)
        do test "989w42rra13" ((complex 2.0 0.0) = Math.Complex.CreatePolar(2.0,0.0) )
        do test "989w42rra14" (complex 1.0 2.0 + complex (-1.0) (-2.0) = complex 0.0 0.0)
        do test "989w42rra15" (complex 1.0 2.0 - (-(complex (-1.0) (-2.0))) = complex 0.0 0.0)
        do test "989w42rra16" ((complex 1.0 0.0) * complex (-1.0) 0.0 = complex (-1.0) 0.0)
        do test "989w42rra17" ((complex 1.0 0.0) * 2.0 = complex 2.0 0.0)
        do test "989w42rra18" (2.0 * (complex 1.0 0.0) = complex 2.0 0.0)

        do test "vrkhwe3r4y19" ((complex 1.0 1.0).ToString("g", System.Globalization.CultureInfo.InvariantCulture) = "1r+1i")
        do test "vrkhwe3r4y20" ((complex 1.0 (-1.0)).ToString("g", System.Globalization.CultureInfo.InvariantCulture) = "1r-1i")
        do test "vrkhwe3r4y21" ((complex (-1.0) (-1.0)).ToString("g", System.Globalization.CultureInfo.InvariantCulture) = "-1r-1i")
        do test "vrkhwe3r4y22" ((complex (-1.0) 1.0).ToString("g", System.Globalization.CultureInfo.InvariantCulture) = "-1r+1i")
        do test "vrkhwe3r4y23" ((complex (-1.0) 0.0).ToString("g", System.Globalization.CultureInfo.InvariantCulture) = "-1r+0i")
        do test "vrkhwe3r4y24" ((complex 0.0 0.0).ToString("g", System.Globalization.CultureInfo.InvariantCulture) = "0r+0i")

    [<Test>]
    member this.MatrixOpsTest() = 

        test "989w42rra" (Matrix.get (matrix [[1.0;2.0;1.0]]) 0 0 = 1.0)
        test "989w42rrb" (Matrix.get (Matrix.zero 1 2) 0 0 = 0.0)
        test "989w42rrc" (Matrix.get (Matrix.zero 1 2) 0 1 = 0.0)
        test "989w42rrd" (Matrix.get (Matrix.create 1 2 1.1) 0 0 = 1.1)
        test "989w42rre" (Matrix.get (Matrix.create 1 2 1.1) 0 1 = 1.1)
        test "989w42rrf" (Matrix.get (Matrix.constDiag 2 1.1) 0 0 = 1.1)
        test "989w42rrg" (Matrix.get (Matrix.constDiag 2 1.1) 0 1 = 0.0)
        test "989w42rrh" (Matrix.get (Matrix.constDiag 2 1.1) 1 0 = 0.0)
        test "989w42rri" (Matrix.get (Matrix.constDiag 2 1.1) 1 1 = 1.1)
        test "989w42rrj" ((matrix [[1.0;2.0;1.0]]).Item(0,0) = 1.0)
        test "989w42rrk" ((matrix [[1.0;2.0;1.0]]).Item(0,1) = 2.0)
        test "989w42rrl" ((matrix [[1.0;2.0;1.0]]).Item(0,2) = 1.0)
        test "989w42rr1" ((matrix [[1.0;2.0;1.0]]).[0,0] = 1.0)
        test "989w42rr2" ((matrix [[1.0;2.0;1.0]]).[0,1] = 2.0)
        test "989w42rr3" ((matrix [[1.0;2.0;1.0]]).[0,2] = 1.0)
        test "989h42rr" ((matrix [[1.0;2.0;1.0]]).Dimensions = (1,3))
        test "8f3fo3ij" ((matrix [[1.0];[2.0];[1.0]]).Dimensions = (3,1))
        test "989hf2f2a" (Matrix.toScalar (Matrix.mul (matrix [[1.0;2.0;1.0]]) (matrix [[1.0];[2.0];[1.0]])) = 6.0)

        test "989hf2f2b" ((let m = matrix [[1.0;2.0;1.0]] in 
                           Matrix.inplace_add m (matrix [[1.0;2.0;3.0]]);
                           m) = matrix [[2.0;4.0;4.0]])

        test "989hf2f2c" (Matrix.toScalar (matrix [[1.0;2.0;1.0]] * matrix [[1.0];[2.0];[1.0]]) = 6.0)
        test "989hf2f2d" (Matrix.toScalar (matrix [[1.0;2.0]; [3.0;4.0]] * 
                              matrix [[1.0;2.0];[1.0;2.0]]) = 3.0)
        test "989hf2f2e" (compare (matrix [[1.0]]) (matrix [[1.0]]) = 0)
        test "989hf2f2f" (compare (matrix [[1.0]]) (matrix [[2.0]]) = -1)
        test "989hf2f2g" (compare (matrix [[2.0]]) (matrix [[1.0]]) = 1)
        test "989hf2f2h" (-(matrix [[2.0]]) = matrix [[-2.0]])
        test "989hf2f2i" (-(matrix [[2.0; -1.0]]) = matrix [[-2.0; 1.0]])
        test "989hf2f2j" (matrix [[1.0;2.0]; [3.0;4.0]] * matrix [[1.0;2.0]; [1.0;2.0]] = matrix [[3.0;6.0]; [7.0; 14.0]])

        test "989hf2f2q" (Matrix.getCol (matrix [[1.0;2.0]; 
                                                 [3.0;4.0]]) 0 =
                                 vector [1.0;
                                         3.0])

        test "989hf2f3q1" (Matrix.foldByCol (fun x y -> x@[y]) (RowVector.Generic.of_list []) (matrix [[]; []])   = (RowVector.Generic.of_list []))
        test "989hf2f3q1" (Matrix.foldByCol (fun x y -> x+y) (RowVector.Generic.of_list []) (matrix [[]; []])   = (RowVector.Generic.of_list []))

        test "989hf2f3q2" (Matrix.foldByCol (fun x y -> x@[y]) (RowVector.Generic.of_list [[];[]]) (matrix [[1.0;2.0]; [3.0;4.0]])   = (RowVector.Generic.of_list [[1.0;3.0];[2.0;4.0]]))
        test "989hf2f3q2" (Matrix.foldByCol (fun x y -> x+y) (RowVector.Generic.of_list [0.0;0.0]) (matrix [[1.0;2.0]; [3.0;4.0]])   = (RowVector.Generic.of_list [4.0;6.0]))

        test "989hf2f3q3" (Matrix.foldByCol (fun x y -> x@[y]) (RowVector.Generic.of_list [[];[]]) (matrix [[1.0;2.0]; [3.0;4.0]; [5.0;6.0]])   = (RowVector.Generic.of_list [[1.0;3.0;5.0];[2.0;4.0;6.0]]))
        test "989hf2f3q3" (Matrix.foldByCol (fun x y -> x+y) (RowVector.Generic.of_list [0.0;0.0]) (matrix [[1.0;2.0]; [3.0;4.0]; [5.0;6.0]])   = (RowVector.Generic.of_list [9.0;12.0]))

        test "989hf2f3r4" (Matrix.foldByRow (fun x y -> x@[y]) (Vector.Generic.of_list [[];[]]) (matrix [[1.0;2.0]; [3.0;4.0]])   = (Vector.Generic.of_list [[1.0;2.0];[3.0;4.0]]))
        test "989hf2f3r4" (Matrix.foldByRow (fun x y -> x+y) (Vector.Generic.of_list [0.0;0.0]) (matrix [[1.0;2.0]; [3.0;4.0]])   = (Vector.Generic.of_list [3.0;7.0]))

        test "989hf2f3r5" (Matrix.foldByRow (fun x y -> x@[y]) (Vector.Generic.of_list [[];[];[]]) (matrix [[1.0;2.0]; [3.0;4.0]; [5.0;6.0]])   = (Vector.Generic.of_list [[1.0;2.0];[3.0;4.0]; [5.0;6.0];]))
        test "989hf2f3r5" (Matrix.foldByRow (fun x y -> x+y) (Vector.Generic.of_list [0.0;0.0;0.0]) (matrix [[1.0;2.0]; [3.0;4.0]; [5.0;6.0]])   = (Vector.Generic.of_list [3.0;7.0; 11.0;]))

        test "989hf2f3r4" (Matrix.foldRow (fun x y -> x@[y]) [] (matrix [[1.0;2.0]; [3.0;4.0]]) 0   = [1.0;2.0])
        test "989hf2f3r4" (Matrix.foldCol (fun x y -> x@[y]) [] (matrix [[1.0;2.0]; [3.0;4.0]]) 0   = [1.0;3.0])
        test "989hf2f3r4" (Matrix.foldRow (fun x y -> x@[y]) [] (matrix [[1.0;2.0]; [3.0;4.0]]) 1   = [3.0;4.0])
        test "989hf2f3r4" (Matrix.foldCol (fun x y -> x@[y]) [] (matrix [[1.0;2.0]; [3.0;4.0]]) 1   = [2.0;4.0])


        test "989hf2f2r1" (Matrix.getRow (matrix [[1.0;2.0]; 
                                                 [3.0;4.0]]) 0 =
                                 rowvec [1.0;2.0])

        test "989hf2f2r2" (Matrix.getDiag (matrix [[1.0;2.0]; 
                                                  [3.0;4.0]]) =
                                 vector [1.0;4.0])

        test "989hf2f2r3" (Matrix.getDiagN (matrix [[1.0;2.0]; 
                                                   [3.0;4.0]]) 0 =
                                 vector [1.0;4.0])

        test "989hf2f2r4" (Matrix.getDiagN (matrix [[1.0;2.0]; 
                                                   [3.0;4.0]]) 1 =
                                 vector [2.0])

        test "989hf2f2r5" (Matrix.getDiagN (matrix [[1.0;2.0]; 
                                                   [3.0;4.0]]) 2 =
                                 vector [])

        test "989hf2f2r6" (Matrix.getDiagN (matrix [[1.0;2.0]; 
                                                   [3.0;4.0]]) 3 =
                                 vector [])

        test "989hf2f2r7" (Matrix.getDiagN (matrix [[1.0;2.0]; 
                                                   [3.0;4.0]]) (-1) =
                                 vector [3.0])

        test "989hf2f2r8" (Matrix.getDiagN (matrix [[1.0;2.0]; 
                                                   [3.0;4.0]]) (-2) =
                                 vector [])

        test "989hf2f2s9" (Matrix.getRow (matrix [[1.0;2.0]; 
                                                 [3.0;4.0]]) 1 =
                                 rowvec [3.0;4.0])

        test "989hf2f2tq" (Matrix.getRows (matrix [[1.0;2.0]; 
                                                  [3.0;4.0]]) 1 1 =
                                  (Matrix.ofRowVector (rowvec [3.0;4.0])))

        test "989hf2f2u" (Matrix.getCols (matrix [[1.0;2.0]; 
                                                  [3.0;4.0]]) 0 1 =
                                  (Matrix.ofVector (vector [1.0;3.0])))
        test "989hf2f2v" (Matrix.getRegion (matrix [[1.0;2.0]; 
                                                    [3.0;4.0]]) 0 1 1 1 =
                                  (matrix[[2.0]]))


        test "989w42rrw" (Vector.range 0 3 = vector [0.0; 1.0; 2.0; 3.0])
        test "989w42rrx" (Matrix.diag (vector [1.0; 2.0]) = matrix [[1.0;0.0]; 
                                                             [0.0;2.0]])
        test "989w42rry" (Vector.rangef 0.0 1.0 3.0 = vector [0.0; 1.0; 2.0; 3.0])
        test "989w42rrz" (Vector.rangef 1.0 1.0 3.0 = vector [1.0; 2.0; 3.0])
        test "989w42rra" (Vector.rangef 0.0 2.0 3.0 = vector [0.0; 2.0; 3.0])
        test "989w42rrb" (determinant (matrix [[1.0]]) = 1.0)
        test "989w42rrc" (determinant (matrix [[1.0;2.0];[3.0;4.0]]) = -2.0)

        let a  = matrix [[1.0;2.0;1.0]; 
                         [2.0;13.0;2.0]; 
                         [1.0;2.0;5.0]]
        let l  = choleskyFactor a
        test "caeirj20" (l = matrix [[1.0; 0.0; 0.0];
                                     [2.0; 3.0; 0.0];
                                     [1.0; 0.0; 2.0]])
        let lt = Matrix.transpose l
        test "caeirj20" (l |> Matrix.transpose |> Matrix.transpose = l)
        let a2 = l * lt
        let zz = a2 - a
        test "caeirj20" (zz = Matrix.zero 3 3)
        let li = lowerTriangularInverse l
        let uu1 = l * li  
        let uu2 = li * l
          
        test "cdwioeu" (uu1 = matrix [[1.;0.;0.];[0.;1.;0.];[0.;0.;1.]])
        test "cdwioeu" (uu2 = matrix [[1.;0.;0.];[0.;1.;0.];[0.;0.;1.]])

        let m = matrix [[1.0;2.0;1.0];
                        [3.0;8.0;5.0];
                        [4.0;2.0;1.0]]
        let d = determinant m
        test "caeirj20" (d = 6.0)

        
        for i = 0 to 100000 do
          ignore(determinant m)
        done

        for i = 0 to 100000 do
          ignore(m .* m)
        done

        for i = 0 to 100000 do
          ignore(m * m)
        done
        


        let aa = Matrix.constDiag 2 2.0
        let bb = Matrix.constDiag 2 1.5
        let cc = aa + bb 
        let dd = aa * cc

        let xx = (Matrix.create 4 4 1.0) + (Matrix.constDiag 4 1.0)
        let ll  = choleskyFactor xx
        let llT = Matrix.transpose ll
        let yy = ll * llT

        let llInv = lowerTriangularInverse ll
        let uu = ll * llInv
        let aaa = Matrix.constDiag  4   1.0
        let v   = Matrix.create 4 1 2.0
        let w   = aaa * v

        let zzz = aa + bb
        let zz2 = aa * bb
        let zz3 = 3.0 * bb
        let zz4 = bb * 3.0
        //let zz5 = bb * 3.0

        zzz.Item(1,1) <- 3.0
        test "ehoihoi" (zzz.Item(1,1) = 3.0)

        zzz.[1,1] <- 4.0
        test "ehoihoi" (zzz.[1,1] = 4.0)

        
    [<Test>]
    member this.MiscRalfTest() = 

        (*----------------------------------------------------------------------------
        !* Ralf's test
         *--------------------------------------------------------------------------*)
            
        let n = 30
        let b = Matrix.randomize (Matrix.create n n 1.2)
            
        let testCholesky (b:matrix) =
          let C = b * b.Transpose in
          let d = choleskyFactor C in
          let diff = d * d.Transpose - C in
          let mm = Matrix.fold max 0.0 diff in
          mm
        

        printf "minimum result = %O\n" (testCholesky b)
        test "check very small" (testCholesky b < 0.000000001)

    [<Test>]
    member this.PermuteColumns() =
        let u = matrix [ [1.;2.]; [3.;4.]; [5.;6.] ] 
        let u' = u.PermuteColumns (fun j -> j)
        Assert.AreEqual(u, u')

        let u1 = matrix [ [1.;2.;3.]; [4.;5.;6.]; ] 
        let u1' = u1.PermuteColumns (fun j -> 2 - j)
        Assert.AreEqual(matrix [[3.;2.;1.]; [6.;5.;4.]], u1')

        let u1transposed = u1.Transpose
        Assert.AreEqual(matrix [[1.;4.];[2.;5.];[3.;6.]], u1transposed)

       
    [<Test>]
    member this.PerfTest() = 
        // sums over all elements of a vector

        //-----------------------------------------------------
        // Matrix Loop Perf: Example sumM1 (recommended, though sumM3 more explicitly better)
        //
        // This is simple code.  The indirect call in "Matrix.fold" may turn out to be a little
        // costly, and either expanding by hand or using "inline" on Matrix.fold will
        // help a lot (see sumM3)
        let fold f z (A: matrix) = 
          let mutable res = z in
          for i = 0 to A.NumRows-1 do
            for j = 0 to A.NumCols-1 do
              res <- f res (A.Item(i, j));
            done;
          done;
          res

        let sumM1 (m:matrix) = fold (fun acc x -> acc + x) 0.0 m


        //-----------------------------------------------------
        // Matrix Loop Perf: Example sumM1a (recommended, though sumM3 more explicitly better)
        //
        // This is simple code.  The indirect call in "Matrix.fold" may turn out to be a little
        // costly, and either expanding by hand or using "inline" on Matrix.fold will
        // help a lot (see sumM3)
        let folda f z (A: matrix) = 
          let mutable res = z in
          for i = 0 to A.NumRows-1 do
            for j = 0 to A.NumCols-1 do
              res <- f res (A.[i, j]);
            done;
          done;
          res

        let sumM1a (m:matrix) = folda (fun acc x -> acc + x) 0.0 m

        //-----------------------------------------------------
        // Matrix Loop Perf: Example sumM2 (not recommended)
        //
        // This is simple code and gives a reusable operator that 
        // may come in handy. The tail recursive functions go1 and go2  in foldM2 will be turned 
        // into loops and will run pretty fast, but variables 'd1' and 'd2' are
        // captured as a free variables within the inner functions
        // and no guarantees are made that the information necessary for
        // array-bounds-check elimination will be propagated down.
        let foldM2 f z (A: matrix) = 
          let d1 = A.NumRows-1 in 
          let d2 = A.NumCols-1 in 
          let rec go2 acc i j = if j < d2 then f acc (A.Item(i,j)) else acc in 
          let rec go1 acc i = if i < d1 then go1 (go2 acc i 0) (i+1) else acc in 
          go1 z 0

        let sumM2 (m:matrix) = foldM2 (fun acc x -> acc + x) 0.0 m


        //-----------------------------------------------------
        // Matrix Loop Perf: Example sumM3 (recommended)
        //
        // This is about as good as it gets.  My only performance
        // concern here is that the array is hidden by an abstraction
        // boundary and so the calls to A.NumRows, A.NumCols, A.Item etc.
        // may not be optimizaed to act directly on the array.  Given
        // the relative simplicity of the implementations of those
        // functions that would be considered a bug in F#.
        let sumM3 (A: matrix) = 
          let mutable res = 0.0 in
          for i = 0 to A.NumRows-1 do
            for j = 0 to A.NumCols-1 do
              res <-  res + (A.Item(i, j));
            done;
          done;
          res

        //-----------------------------------------------------
        // Matrix Loop Perf: Example sumM4 (not recommended)
        //
        // This is sumM2 expanded by hand.
        // Again the tail recursive functions go1 and go2 will be turned 
        // into loops and will run pretty fast, but variables 'd1' and 'd2' are
        // captured as a free variables within the inner functions
        // functions, and so no guarantees are made that the information necessary for
        // arry-bounds-check elimination will be propagated down
        let sumM4 (A: matrix) = 
          let d1 = A.NumRows-1 in 
          let d2 = A.NumCols-1 in 
          let rec go2 acc i j = if j < d2 then acc + (A.Item(i,j)) else acc in 
          let rec go1 acc i = if i < d1 then go1 (go2 acc i 0) (i+1) else acc in 
          go1 0.0 0
        ()


    [<Test>]
    member this.GMatrixOpsAtFloatTest() = 


        test "989w42rr" (MG.get (matrix [[1.0;2.0;1.0]]) 0 0 = 1.0)
        test "989h42rr" ((matrix [[1.0;2.0;1.0]]).Dimensions = (1,3))
        test "8f3fo3ij" ((matrix [[1.0];[2.0];[1.0]]).Dimensions = (3,1))
        test "989hf2f2a" (Matrix.toScalar (MG.mul (matrix [[1.0;2.0;1.0]]) (matrix [[1.0];[2.0];[1.0]])) = 6.0)
        test "989hf2f2q" (Matrix.toScalar (matrix [[1.0;2.0;1.0]] * matrix [[1.0];[2.0];[1.0]]) = 6.0)
        test "989hf2f2w" (Matrix.toScalar (matrix [[1.0;2.0]; [3.0;4.0]] * 
                              matrix [[1.0;2.0];[1.0;2.0]]) = 3.0)
        test "989hf2f2e" (compare (matrix [[1.0]]) (matrix [[1.0]]) = 0)
        test "989hf2f2rw" (compare (matrix [[1.0]]) (matrix [[2.0]]) = -1)
        test "989hf2f2t" (compare (matrix [[2.0]]) (matrix [[1.0]]) = 1)
        test "989hf2f2y" (-(matrix [[2.0; -1.0]]) = matrix [[-2.0; 1.0]])
        test "989hf2f2u" (matrix [[1.0;2.0]; [3.0;4.0]] *  matrix [[1.0;2.0]; [1.0;2.0]] = matrix [[3.0;6.0]; [7.0; 14.0]])
        test "989hf2f2g" (MG.getCol (matrix [[1.0;2.0]; 
                                          [3.0;4.0]]) 0 =
                                 vector [1.0;
                                         3.0])

        test "989hf2f2h" (MG.getRow (matrix [[1.0;2.0]; 
                                          [3.0;4.0]]) 0 =
                                 rowvec [1.0;2.0])

        test "989hf2f2j" (MG.getRow (matrix [[1.0;2.0]; 
                                          [3.0;4.0]]) 1 =
                                 rowvec [3.0;4.0])

        test "989hf2f2k" (MG.getRows (matrix [[1.0;2.0]; 
                                          [3.0;4.0]]) 1 1 =
                                  (MG.ofRowVector (rowvec [3.0;4.0])))

        test "989hf2f2l" (MG.getCols (matrix [[1.0;2.0]; 
                                          [3.0;4.0]]) 0 1 =
                                  (V (vector [1.0;3.0])))
        test "989hf2f2z" (MG.getRegion (matrix [[1.0;2.0]; 
                                          [3.0;4.0]]) 0 1 1 1 =
                                  (matrix[[2.0]]))


        //test "989w42rr" (rangeVG 0 3 = vector [0.0; 1.0; 2.0; 3.0])
        test "989w42rr" (MG.diag (vector [1.0; 2.0]) = matrix [[1.0;0.0]; 
                                                               [0.0;2.0]])



    [<Test>]
    member this.GMatrixOpsAtFloat32Test() = 

        test "989w42rr" (MG.get (gmatrix [[1.0f;2.0f;1.0f]]) 0 0 = 1.0f)
        test "989h42rr" ((gmatrix [[1.0f;2.0f;1.0f]]).Dimensions = (1,3))
        test "8f3fo3ij" ((gmatrix [[1.0f];[2.0f];[1.0f]]).Dimensions = (3,1))
        test "98439hf2f2" (MG.toScalar (MG.mul (gmatrix [[1.0f;2.0f;1.0f]]) (gmatrix [[1.0f];[2.0f];[1.0f]])) = 6.0f)
        test "98439hf2f2" (MG.toScalar (gmatrix [[1.0f;2.0f;1.0f]] * gmatrix [[1.0f];[2.0f];[1.0f]]) = 6.0f)
        test "98439hf2f2" (MG.toScalar (gmatrix [[1.0f;2.0f]; [3.0f;4.0f]] * 
                              gmatrix [[1.0f;2.0f];[1.0f;2.0f]]) = 3.0f)
        test "98439hf2f2" (compare (gmatrix [[1.0f]]) (gmatrix [[1.0f]]) = 0)
        test "98439hf2f2" (compare (gmatrix [[1.0f]]) (gmatrix [[2.0f]]) = -1)
        test "98439hf2f2" (compare (gmatrix [[2.0f]]) (gmatrix [[1.0f]]) = 1)
        test "98439hf2f2" (-(gmatrix [[2.0f; -1.0f]]) = gmatrix [[-2.0f; 1.0f]])
        test "98439hf2f2" (gmatrix [[1.0f;2.0f]; [3.0f;4.0f]] * gmatrix [[1.0f;2.0f]; [1.0f;2.0f]] = gmatrix [[3.0f;6.0f]; [7.0f; 14.0f]])
        test "98439hf2f2" (MG.getCol (gmatrix [[1.0f;2.0f]; 
                                          [3.0f;4.0f]]) 0 =
                                 gvector [1.0f;3.0f])

        test "98439hf2f2" (MG.getRow (gmatrix [[1.0f;2.0f]; 
                                          [3.0f;4.0f]]) 0 =
                                 growvec [1.0f;2.0f])

        test "98439hf2f2" (MG.getRow (gmatrix [[1.0f;2.0f]; 
                                          [3.0f;4.0f]]) 1 =
                                 growvec [3.0f;4.0f])

        test "98439hf2f2" (MG.getRows (gmatrix [[1.0f;2.0f]; 
                                          [3.0f;4.0f]]) 1 1 =
                                  (MG.ofRowVector (growvec [3.0f;4.0f])))

        test "98439hf2f2" (MG.getCols (gmatrix [[1.0f;2.0f]; 
                                          [3.0f;4.0f]])  0 1 =
                                  (V (gvector [1.0f;3.0f])))
        test "98439hf2f2" (MG.getRegion (gmatrix [[1.0f;2.0f]; 
                                          [3.0f;4.0f]]) 0 1 1 1 =
                                  (gmatrix[[2.0f]]))



    [<Test>]
    member this.GMatrixOpsAtBigNumTest() = 

        test "989w42rr" (MG.get (gmatrix [[1N;2N;1N]]) 0 0 = 1N)
        test "989h42rr" ((gmatrix [[1N;2N;1N]]).Dimensions = (1,3))
        test "8f3fo3ij" ((gmatrix [[1N];[2N];[1N]]).Dimensions = (3,1))
        test "68439hf2f2a" (MG.toScalar (MG.mul (gmatrix [[1N;2N;1N]]) (gmatrix [[1N];[2N];[1N]])) = 6N)
        test "68439hf2f2s" (MG.toScalar (gmatrix [[1N;2N;1N]] * gmatrix [[1N];[2N];[1N]]) = 6N)
        test "68439hf2f2d" (MG.toScalar (gmatrix [[1N;2N]; [3N;4N]] * 
                              gmatrix [[1N;2N];[1N;2N]]) = 3N)
        test "68439hf2f2f" (compare (gmatrix [[1N]]) (gmatrix [[1N]]) = 0)
        test "68439hf2f2g" (compare (gmatrix [[1N]]) (gmatrix [[2N]]) = -1)
        test "68439hf2f2h" (compare (gmatrix [[2N]]) (gmatrix [[1N]]) = 1)
        test "68439hf2f2j" (-(gmatrix [[2N; -1N]]) = gmatrix [[-2N; 1N]])
        test "68439hf2f2k" (gmatrix [[1N;2N]; [3N;4N]] * gmatrix [[1N;2N]; [1N;2N]] = gmatrix [[3N;6N]; [7N; 14N]])
        test "68439hf2f2ppp" (1N = 1N)
        test "68439hf2f2ppp" ([| 1N; 3N |] = [| 1N ; 3N |])

        test "68439hf2f2m" (MG.getCol (gmatrix [[1N;2N]; 
                                          [3N;4N]]) 0 =
                                 gvector [1N;
                                         3N])

        test "68439hf2f2q" (MG.getRow (gmatrix [[1N;2N]; 
                                          [3N;4N]]) 0 =
                                 growvec [1N;2N])

        test "68439hf2f2w" (MG.getRow (gmatrix [[1N;2N]; 
                                          [3N;4N]]) 1 =
                                 growvec [3N;4N])

        test "68439hf2f2e" (MG.getRows (gmatrix [[1N;2N]; 
                                          [3N;4N]]) 1 1 =
                                  (MG.ofRowVector (growvec [3N;4N])))

        test "68439hf2f2r" (MG.getCols (gmatrix [[1N;2N]; 
                                                 [3N;4N]]) 0 1 =
                                  (V (gvector [1N;3N])))
        test "68439hf2f2t" (MG.getRegion (gmatrix [[1N;2N]; 
                                                   [3N;4N]]) 0 1 1 1 =
                                  (gmatrix[[2N]]))


    [<Test>]
    member this.FloatMatrixPerfTest() = 

        let m = matrix [[1.0;2.0;1.0];
                        [3.0;8.0;5.0];
                        [4.0;2.0;1.0]]

        let d = determinant m
        test "caeirj20" (d = 6.0)

        for i = 0 to 100000 do
          ignore(determinant m)
        done
        for i = 0 to 100000 do
          ignore(m .* m)
        done

        for i = 0 to 100000 do
          ignore(m * m)
        done

        for i = 0 to 100000 do
          ignore(m + m)
        done

        for i = 0 to 100000 do
          ignore(m * m * m)
        done

    [<Test>]
    member this.Float32MatrixPerf() = 

        let m = gmatrix [[1.0f;2.0f;1.0f];
                        [3.0f;8.0f;5.0f];
                        [4.0f;2.0f;1.0f]]


        for i = 0 to 100000 do
          ignore(m .* m)
        done

        for i = 0 to 100000 do
          ignore(m * m)
        done
    
    [<Test>]
    member this.GMatrixOpsAtComplexTest() = 

        let R r = complex r 0.0
        let I i = complex 0.0 i
        test "989w42rr" (MG.get (gmatrix [[R 1.0;R 2.0;R 1.0]]) 0 0 = R 1.0)
        test "989h42rr" ((gmatrix [[R 1.0;R 2.0;R 1.0]]).Dimensions = (1,3))
        test "8f3fo3ij" ((gmatrix [[R 1.0];[R 2.0];[R 1.0]]).Dimensions = (3,1))
        test "28439hf2f2" (MG.toScalar (MG.mul (gmatrix [[R 1.0;R 2.0;R 1.0]]) (gmatrix [[R 1.0];[R 2.0];[R 1.0]])) = R 6.0)
        test "28439hf2f2" (MG.toScalar (gmatrix [[R 1.0;R 2.0;R 1.0]] * gmatrix [[R 1.0];[R 2.0];[R 1.0]]) = R 6.0)
        test "28439hf2f2" (MG.toScalar (gmatrix [[R 1.0;R 2.0]; [R 3.0;R 4.0]] * 
                              gmatrix [[R 1.0;R 2.0];[R 1.0;R 2.0]]) = R 3.0)
        test "28439hf2f2" (compare (gmatrix [[R 1.0]]) (gmatrix [[R 1.0]]) = 0)
        test "28439hf2f2" (compare (gmatrix [[R 1.0]]) (gmatrix [[R 2.0]]) = -1)
        test "28439hf2f2" (compare (gmatrix [[R 2.0]]) (gmatrix [[R 1.0]]) = 1)
        test "28439hf2f2" (-(gmatrix [[R 2.0; -R 1.0]]) = gmatrix [[-R 2.0; R 1.0]])
        test "28439hf2f2" (gmatrix [[R 1.0;R 2.0]; [R 3.0;R 4.0]] * gmatrix [[R 1.0;R 2.0]; [R 1.0;R 2.0]] = gmatrix [[R 3.0;R 6.0]; [R 7.0; R 14.0]])
        test "28439hf2f2" (MG.getCol (gmatrix [[R 1.0;R 2.0]; 
                                          [R 3.0;R 4.0]]) 0 =
                                 gvector [R 1.0;
                                         R 3.0])

        test "28439hf2f2" (MG.getRow (gmatrix [[R 1.0;R 2.0]; 
                                          [R 3.0;R 4.0]]) 0 =
                                 growvec [R 1.0;R 2.0])

        test "28439hf2f2" (MG.getRow (gmatrix [[R 1.0;R 2.0]; 
                                          [R 3.0;R 4.0]]) 1 =
                                 growvec [R 3.0;R 4.0])

        test "28439hf2f2" (MG.getRows (gmatrix [[R 1.0;R 2.0]; 
                                          [R 3.0;R 4.0]]) 1 1 =
                                  (MG.ofRowVector (growvec [R 3.0;R 4.0])))

        test "28439hf2f2" (MG.getCols (gmatrix [[R 1.0;R 2.0]; 
                                          [R 3.0;R 4.0]]) 0 1 =
                                  (V (gvector [R 1.0;R 3.0])))
        test "28439hf2f2" ((gmatrix [[R 1.0;R 2.0]; 
                                     [R 3.0;R 4.0]]).Region(0,1,1,1) =
                                  (gmatrix[[R 2.0]]))

        (* Test norm functions on gmatrix, note, there are 2 norm functions on gmatrix.
         * One fast-path that assumes float elt type, one assuming generic element type.
         * Similarly, two Vector norms to test.
         * No RowVector norm.
         *)

        test "norm-gmatrix-generic-int32"   (MG.norm (MG.init 2 2 (fun _ _ -> 11))    = 22.0)
        test "norm-gmatrix-generic-int64"   (MG.norm (MG.init 2 2 (fun _ _ -> 11L))   = 22.0)
        test "norm-gmatrix-generic-float"   (MG.norm (MG.init 2 2 (fun _ _ -> 11.0))  = 22.0)
        test "norm-gmatrix-generic-float32" (MG.norm (MG.init 2 2 (fun _ _ -> 11.0f)) = 22.0)
        test "norm-gmatrix-generic-bigint"  (MG.norm (MG.init 2 2 (fun _ _ -> 11I))   = 22.0)
        test "norm-gmatrix-generic-bignum"  (MG.norm (MG.init 2 2 (fun _ _ -> 11N))   = 22.0)
        test "norm-gmatrix-float"           (MDouble .norm (MDouble .init 2 2 (fun _ _ -> 11.0))  = 22.0)

        test "norm-vector-generic-int32"   (VGeneric.norm (VGeneric.init 4 (fun _ -> 11))    = 22.0)
        test "norm-vector-generic-int64"   (VGeneric.norm (VGeneric.init 4 (fun _ -> 11L))   = 22.0)
        test "norm-vector-generic-float"   (VGeneric.norm (VGeneric.init 4 (fun _ -> 11.0))  = 22.0)
        test "norm-vector-generic-float32" (VGeneric.norm (VGeneric.init 4 (fun _ -> 11.0f)) = 22.0)
        test "norm-vector-generic-bigint"  (VGeneric.norm (VGeneric.init 4 (fun _ -> 11I))   = 22.0)
        test "norm-vector-generic-bignum"  (VGeneric.norm (VGeneric.init 4 (fun _ -> 11N))   = 22.0)
        test "norm-vector-float"           (VDouble .norm (VDouble .init 4 (fun _ -> 11.0))  = 22.0)

        (* norm is not provided on RowVector...
        module RGeneric = Math.RowVector.Generic
        module RDouble  = Math.RowVector
        test "norm-rowvector-generic-float" (RGeneric.norm (RGeneric.init 4 (fun _ -> 1.1)) = 2.2)
        test "norm-rowvector-float"         (RDouble .norm (RDouble .init 4 (fun _ -> 1.1)) = 2.2)
        *)   

    [<Test>]
    member this.SizeOfFormattedMatrix() = 

        //check the number of characters in this display is < 100000. In reality it displays 100 x 100, around 74000 characters
        test "vrkomvrwe0" ((sprintf "%A" (matrix [ for i in 0 .. 1000 -> [ for j in 0 .. 1000 -> float (i+j) ] ])).Length < 100000)
        //check the number of characters in this display is < 10000. In reality it displays 50 x 50, around 7000 characters
        test "vrkomvrwe0" (((matrix [ for i in 0 .. 1000 -> [ for j in 0 .. 1000 -> float (i+j) ] ]).ToString()).Length < 10000)

(* Regression 3716: ensure the members/properties mentioned in matrix Obsolete redirect messages exist *)
module M3716 = begin
  open Microsoft.FSharp.Math
  let check_Dimensions     (m:matrix) = m.Dimensions     : int * int
  let check_NumRows        (m:matrix) = m.NumRows        : int
  let check_NumCols        (m:matrix) = m.NumCols        : int
  let check_NonZeroEntries (m:matrix) = m.NonZeroEntries : seq<int * int * float> 
  let check_Column         (m:matrix) = m.Column         : int -> Vector<float>
  let check_Row            (m:matrix) = m.Row            : int -> RowVector<float>
  let check_Columns        (m:matrix) = m.Columns        : int  * int -> Matrix<float>
  let check_Rows           (m:matrix) = m.Rows           : int  * int -> Matrix<float>
  let check_Region         (m:matrix) = m.Region         : int  * int * int * int -> Matrix<float>
  let check_Diagonal       (m:matrix) = m.Diagonal       : Vector<float>
  let check_ElementOps     (m:matrix) = m.ElementOps     : INumeric<float>
end

module Numeric = 

    open System

    open Microsoft.FSharp.Math
    module LinearAlgebra = begin

        open Microsoft.FSharp.Core
        open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
        open Microsoft.FSharp.Core.Operators
        open Microsoft.FSharp.Collections
        open Microsoft.FSharp.Math

        let inline sumfR f (a,b) =
            let mutable res = 0.0 in
            for i = a to b do
                res <- res + f i;
            done;
            res

        (*----------------------------------------------------------------------------
        !* predicates
         *--------------------------------------------------------------------------*)

        let isSymmetric a = a |> Matrix.foralli (fun i j aij -> aij = a.[j,i]) 
        let isLowerTriangular a = a |> Matrix.foralli (fun i j aij -> i>=j || aij=0.0)

        (*----------------------------------------------------------------------------
        !* choleskyFactor
         *--------------------------------------------------------------------------*)

        let choleskyFactor (a: matrix) =
          let nA,mA = a.Dimensions in
          if nA<>mA              then invalid_arg "choleskyFactor: not square";
          if not (isSymmetric a) then invalid_arg "choleskyFactor: not symmetric";
          let lres = Matrix.zero nA nA  in(* nA=mA *)
          for j=0 to nA-1 do
            for i=j to nA-1 do (* j <= i *)
              (* Consider a_ij = sum(k=0...n-1)  (lres_ik . lresT_kj)
               *               = sum(k=0...n-1)  (lres_ik . lres_jk)
               *               = sum(k=0...j-1)  (lres_ik . lres_jk) + lres_ij . lres_jj + (0 when k>j)
               *               = psum                                + lres_ij . lres_jj
               * This determines lres_ij terms of preceding terms.
               * lres_ij depends on lres_ik and lres_jk (and maybe lres_ii) for k<i
               *)
              let psum = sumfR (fun k -> lres.[i,k] * lres.[j,k]) (0,j-1) in
              let a_ij = a.[i,j] in
              if i=j then
                lres.[i,i] <- (System.Math.Sqrt (a_ij - psum))
              else
                lres.[i,j] <- ((a_ij - psum) / lres.[j,j])
            done
          done;
          // if not (isLowerTriangular lres) then failwith "choleskyFactor: not lower triangular result";
          lres

        (*----------------------------------------------------------------------------
        !* lowerTriangularInverse
         *--------------------------------------------------------------------------*)
            
        let lowerTriangularInverse (l: matrix) =
          (* Given l s.t. LT: l is lower_triangular (l_ij = 0 when i<j).
           * Finds res s.t. l.res = id *)
          let nA,mA = l.Dimensions in
          let res = Matrix.zero nA nA  in (* nA=mA *)
          for j=0 to nA-1 do
            for i=j to nA-1 do (* j <= i *)
              (* Consider id_ij = sum(k=0...n-1)  (l_ik . res_kj)
               *                = sum(k=0...i-1)  (l_ik . res_kj) + l_ii . res_ij + (0 when k>i by LT)
               *                = psum                            + l_ii . res_ij
               * Have res_ij terms of res_kj for k<i and l_??.
               *)
              let psum   = sumfR (fun k -> l.[i,k] * res.[k,j]) (0,i-1) in
              let id_ij  = if i=j then 1.0 else 0.0 in
              let res_ij = (id_ij - psum) / l.[i,i] in
              res.[i, j] <- res_ij
            done
          done;
          res


        (*----------------------------------------------------------------------------
        !* symmetricInverse
         *--------------------------------------------------------------------------*)

        let symmetricInverse a =
          (* Given a symmetric matix.
           * Have l s.t. a = l . transpose(l)  where l is lowerTriangular.
           * Have l_inv s.t. l.l_inv = id
           * Have a_inv = transpose(l_inv).l_inv
           *)
          let l     = choleskyFactor a         in
          let l_t   = l.Transpose                in 
          let l_inv = lowerTriangularInverse l in
          let a_inv = l_inv.Transpose * l_inv in
          a_inv


        (*----------------------------------------------------------------------------
        !* determinant 
         *--------------------------------------------------------------------------*)


        let determinant (a: matrix) =
          (* Allocates lists to manage walk over permutations.
           * Iterating through permutations a mutable array may help GC.
           *)
          let rec det js ks =
            match ks with
              | []    -> 1.0
              | k::ks -> 
                let rec split sign (preJs,js) =
                  match js with
                  | []    -> 0.0
                  | j::js -> sign * a.[j,k] * det (List.rev preJs @ js) ks
                             +
                             split (0.0 - sign) (j::preJs,js) in
                split 1.0 ([],js) in
          let nA,mA = a.Dimensions in
          if nA<>mA then invalid_arg "determinant: not square";
          det [0..nA-1] [0..nA-1]

        let cholesky a =
          let l    = choleskyFactor         a in
          let lInv = lowerTriangularInverse l in
          l,lInv

    end
    module Notation = begin

        type matrix = Matrix<float>
        type vector = Vector<float>
        type rowvec = RowVector<float>
        type complex = Complex
      
        module Generic = begin
            type 'a matrix = Matrix<'a>
            type 'a vector = Vector<'a>
            type 'a rowvec = RowVector<'a>
            type complex = Complex
          
            let complex x y = Complex.mkRect (x,y)
            let matrix ll = Matrix.Generic.ofSeq ll
            let vector l = Vector.Generic.ofSeq  l
            let rowvec l = RowVector.Generic.ofSeq l
            let S x = Matrix.Generic.ofScalar x
            let RV x = Matrix.Generic.ofRowVector x 
            let V x  = Matrix.Generic.ofVector x
            let M2V x = Matrix.Generic.toVector x
            let M2RV x = Matrix.Generic.toRowVector x
            let M2S x = Matrix.Generic.toScalar x
        end    

        let complex x y = Complex.mkRect (x,y)
        // Notational conveniences
        let matrix ll = Matrix.ofSeq ll
        let vector l = Vector.ofSeq l
        let rowvec l = RowVector.ofSeq l
        let S x = Matrix.ofScalar x
        let RV x = Matrix.ofRowVector x 
        let V x  = Matrix.ofVector x
        let M2V x = Matrix.toVector x
        let M2RV x = Matrix.toRowVector x
        let M2S x = Matrix.toScalar x
    end


    open LinearAlgebra
    open Notation 

    let abs x = if x<0. then -. x   else x
    let neg x = -. x

    let poly x cs =
      (* eval a poly expansion - could take a precision and terminate early... *)
      let rec eval (* i *) acc cs xi = 
        match cs with
        | []    -> acc
        | c::cs -> eval (* i+1 *) (acc + c * xi) cs (x * xi)
      in
      eval (* i=0 *) 0.0 cs 1.

    let powerseries x cs =
      (* eval a poly expansion - and divide by factorials along the way *)
      let rec eval i acc cs xi ifact = 
        match cs with
        | []    -> acc
        | c::cs -> eval (i+1) (acc + (c * xi) / ifact) cs (x * xi) (ifact * (float (i+1)))
      in
      eval 0 0.0 cs 1. 1. 

    (*check*)
    let e   = powerseries 1. [1.; 1.;1.; 1.;1.; 1.;1.; 1.;1.; 1.;1.; 1.;1.; 1.;]
    let pi  = 2. * asin 1.0      


    (*----------------------------------------------------------------------------
    !* erfc - complementary error function and inverse
     *--------------------------------------------------------------------------*)

    (* NOTE: got sign wrong in poly factorisation *)  
    let erfc x =
      if x = neg_infinity then 2.0 else
      if x = infinity then 0.0 else
      let z = abs x in
      let t = 1.0 / (1.0 + 0.5 * z) in
      let y = t * exp (-. z*z 
                       +. poly t [ -1.26551223;  1.00002368; 0.37409196;  0.09678418; -0.18628806;
                                    0.27886807; -1.13520398; 1.48851587; -0.82215223;  0.17087277])
      in
      if x >= 0.0 then y else 2.0 - y

    let erfcinv y =
      if (y < 0. || y > 2.) then failwith "Inverse complementary function not defined outside [0,2]." else
      if (y = 0.) then infinity else
      if (y = 2.) then neg_infinity else
      let central y =
        let q = y - 1.0 in
        let r = q * q in
          poly r   [ - 0.8862269264526915; 2.710410832036097; -3.057303267970988;
                     1.524304069216834; -0.3051415712357203; 0.01370600482778535 ]
          * q
          / poly r [ 1.0                ; -3.320170388221430;  4.175081992982483;
                     - 2.432796560310728; 0.6311946752267222; -0.05319931523264068 ]
      in
      let lower y =
        (* Rational approximation lower region *)
          let q = sqrt (-2.0 * log (y / 2.0)) in
          poly q [-2.077595676404383; -3.093354679843504;  1.802933168781950;
                   1.697592457770869;  0.2279687217114118; 0.005504751339936943;] /
          poly q [ 1.0;                3.754408661907416;  2.445134137142996;
                   0.3224671290700398; 0.007784695709041462]
      in
      let upper y =
        let  q = sqrt (-2.0 * log (1. - y / 2.0)) in
        -.
          poly q [ -2.077595676404383; -3.093354679843504;  1.802933168781950;
                   1.697592457770869;  0.2279687217114118; 0.005504751339936943] /
          poly q [ 1.0;                 3.754408661907416;  2.445134137142996;
                    0.3224671290700398;  0.007784695709041462;
                 ]
      in
      let halleys x = (* One iteration of Halley's rational method (third order) gives full machine precision. *)
        let u = (erfc (x) - y) / (-2.0 / sqrt pi * exp (-. x * x)) in
        x - u / (1.0 + x * u)
      in
     (* compute x according to region *)
      let x =
        if (y  >= 0.0485 && y <= 1.9515) then central y else
        if (y < 0.0485) then lower y else
        (* y > 1.9515 *) upper y
      in
      (* iterate halleys once *)
      halleys x


    (*----------------------------------------------------------------------------
    !* phi and phi_inv - complementary error function 
     *--------------------------------------------------------------------------*)
      
    let root2 = sqrt 2.0
    let phi     t = erfc(-. t / root2) / 2.
    let phi_inv p = -. root2 * erfcinv (2. * p)

    let Phi t = phi t
    let Phi_Inv p = phi_inv p


    (*----------------------------------------------------------------------------
    !* normalpdf, v3, w3
     *--------------------------------------------------------------------------*)
      
    /// <summary>
    /// Computes the normal density at a specified point of interest.
    /// </summary>
    /// <param name="t">The point of interest.</param>
    /// <returns>The normal density at the point of interest.</returns>
    let dSqrt2Pi = 2.5066282746310002 (* not checked *)
    let normalpdf t = 1.0 / dSqrt2Pi * exp (-. t*t / 2.0)

    /// <summary>
    /// Computes the additive correction of a single-sided truncated Gaussian with unit variance.
    /// </summary>
    /// <param name="t">The mean of the non-truncated Gaussian.</param>
    /// <param name="epsilon">The truncation point.</param>
    /// <returns>The additive correction.</returns>
    let doubleEpsilon = System.Double.Epsilon
    let v t epsilon =
      let dNumerator = normalpdf (t - epsilon) in
      let dDenominator = phi (t - epsilon) in
      if dDenominator < sqrt doubleEpsilon then
        0.0-t + epsilon
      else
       dNumerator / dDenominator

    /// <summary>
    /// Computes the additive correction of a general double-sided truncated Gaussian with unit variance.
    /// </summary>
    /// <param name="t">The mean of the non-truncated Gaussian.</param>
    /// <param name="l">The lower truncation point.</param>
    /// <param name="u">The upper truncation point.</param>
    /// <returns>The additive correction.</returns>
    /// <remarks>This routine has not been tested in all regimes of t for a given l and u.</remarks>
    /// <exception cref="ArithmeticException">Thrown if the computation is not numerically stable.</exception>
    let v3 t l u =
      let dNumerator = normalpdf (t - l) - normalpdf (t - u) in
      let dDenominator = phi (u - t) - phi (l - t) in
      if (dDenominator < sqrt (doubleEpsilon)) then
        failwith "Unsafe v3 call" (* raise (new System.ArithmeticException("Unsafe computation of v"))*)
        (* failwith "Unsafe computation of v" // was arithmetic exception *)
      else
        if t<0.0 then (* MatLab version only, not C# *)
          (0.0 - dNumerator) / dDenominator
        else
          dNumerator / dDenominator

    /// <summary>
    /// Computes the multiplicative correction of a general double-sided truncated Gaussian with unit variance.
    /// </summary>
    /// <param name="t">The mean of the non-truncated Gaussian.</param>
    /// <param name="l">The lower truncation point.</param>
    /// <param name="u">The upper truncation point.</param>
    /// <returns>The multiplicative correction.</returns>
    /// <remarks>This routine has not been tested in all regimes of t for a given l and u.</remarks>
    /// <exception cref="ArithmeticException">Thrown if the computation is not numerically stable.</exception>
    let w3 t l u =
      let dNumerator = (u - t) * normalpdf (u - t) - (l - t) * normalpdf (l - t) in
      let dDenominator = phi (u - t) - phi (l - t) in
      if (dDenominator < sqrt (doubleEpsilon)) then
        failwith "unsafe w3 call" (* raise (new ArithmeticException ("Unsafe computation of v")) *)
      else
        let z = v3 t l u in
        z * z + dNumerator / dDenominator


    (*----------------------------------------------------------------------------
    !* streams
     *--------------------------------------------------------------------------*)

    type 'a stream = SCONS of (unit -> ('a * 'a stream))  (* fully suspended *)
    let rec constS x = SCONS (fun () -> (x,constS x))
    let rec unfoldS   f z = SCONS (fun () -> let z,x = f z in x,unfoldS f z)
    let rec generateS f   = SCONS(fun () -> f (),generateS f)
    let pullS (SCONS xtailf) = xtailf()
    let listS xs =
      let pull = function (x::xs) -> xs,x | [] -> failwith "listS: exhausted points" in
      unfoldS pull xs
    let rec takeS n s = if n=0 then [] else let x,s = pullS s in x::takeS (n-1) s


    (*----------------------------------------------------------------------------
    !* streams - random, neiderreiter
     *--------------------------------------------------------------------------*)
    let randomM m n = Matrix.randomize (Matrix.zero m n)       
    let randomPts dim = generateS (fun () -> randomM dim 1)

    (* Neiderreiter series:
     * Definition extracted from C# code.
     * To generate npts random points of dimension d.
     * 
     * For k = 0 to npts-1   // over pts 
     *  For i = 0 to ndims-1 // over dimension
     *  
     *   p(k).[i] = let t = (k+1).Neiderreiter(i)
     *              in  abs(2*fract(t) - 1)
     *
     * where fract(t) = t - floor(t)
     * where Neiderreiter(i) = 2 ^ ( (i+1)/(ndims+1) )
     *
     * So, Neiderreiter(i) falls inside [1,2].
     *)
    let neiderreiterS dim npts =
      let fract x = x - floor x in
      let neiderreiter  = Matrix.init dim 1 (fun i _ -> 2. ** (float (i + 1) / float (dim + 1))) in
      let mkPoint k = Matrix.init dim 1 (fun i _ -> let t = float(k + 1) * Matrix.get neiderreiter i 0 in
                                                    let x = abs (2.0 * fract t - 1.0) in
                                                    assert (0.0 <= x && x <= 1.0);
                                                    x)
      in
      let step k = if k<npts then k+1,mkPoint k
                             else failwith "neiderreiterS: exhausted points"
      in
      unfoldS step 0

    let dim,npts = 3,100
    let pts = takeS npts (neiderreiterS dim npts)


    (*----------------------------------------------------------------------------
    !* unitIntegrals
     *--------------------------------------------------------------------------*)

    let unitIntegralOverPts pts nPts f =
      (* unit integral of f by averaging nPts samples of f at pt where pt drawn from pts *)
      let rec sum i total f pts =
        if i=nPts then total
                  else let pt,pts = pullS pts in
                       let total = total + f pt in
                       sum (i + 1) total f pts
      in
      sum 0 0. f pts / float nPts

    let unitIntegralOverRandomPoints dim nPts f =
      (* unit integral of f by averaging nPts samples of f at a random pt *)  
      let eval () = let w = randomM dim 1 in
                    f w
      in
      let sum = seq { for i in 0 .. nPts-1 -> eval() } |> Seq.fold (+) 0.0 in 
      let res = sum / float nPts in
      res

    let f v = let x = Matrix.get v 0 0 in x*x  (* [1/3 x^3] 0,1 = 1/3 *)
    let x1 = unitIntegralOverRandomPoints 1             1000 f   (* random pts *)
    let x2 = unitIntegralOverPts (randomPts 1)          1000 f   (* random pt sequence *)
    let x3 = unitIntegralOverPts (neiderreiterS 1 1000) 1000 f   (* pseudo random pt sequence *)


    (*----------------------------------------------------------------------------
    !* weightedGaussian - alg 9
     *--------------------------------------------------------------------------*)

    (* weightedGaussian: implements algorithm 8 *)
    let weightedGaussian unitIntegrate g (mu:Matrix<_>,sigma:Matrix<_>) (alpha:Matrix<_>,beta:Matrix<_>) =
      printf "dim(sigma) = %d,%d\n" sigma.NumRows sigma.NumCols;
      printf "dim(mu) = %d,%d\n" mu.NumRows mu.NumCols;
      printf "dim(alpha) = %d,%d\n" alpha.NumRows alpha.NumCols;
      printf "dim(beta) = %d,%d\n" beta.NumRows beta.NumCols;
      let l     = choleskyFactor sigma in
      printf "dim(l) = %d,%d\n" l.NumRows l.NumCols;
      let l_inv = lowerTriangularInverse l in
      printf "dim(l_inv) = %d,%d\n" l_inv.NumRows l_inv.NumCols;
      (* apply mu subst and L_inv subst *)
      let alpha'  = l_inv * (alpha - mu) in
      let beta'   = l_inv * (beta  - mu) in
      printf "dim(alpha') = %d,%d\n" alpha'.NumRows alpha'.NumCols;
      printf "dim(beta') = %d,%d\n" beta'.NumRows beta'.NumCols;

      (* apply phi_inv subst *)
      let alpha''     = Matrix.map phi alpha' in
      let beta''      = Matrix.map phi beta'  in
      printf "dim(alpha'') = %d,%d\n" alpha''.NumRows alpha''.NumCols;
      printf "dim(beta'') = %d,%d\n" beta''.NumRows beta''.NumCols;
      let alphabeta'' = beta'' - alpha'' in
      (* reduced to unit integral *)
      let eval w =
        let v = alpha'' + (w .* alphabeta'') in
        let z = Matrix.map phi_inv v in
        let x = (l * z) + mu in
        g x
      in
      unitIntegrate eval

    let g v = let x = Matrix.get v 0 0 in
              let y = Matrix.get v 1 0 in
              x*y*y

    (*	    
    let integrate3 = weightedGaussian
    let res1 = integrate3 (unitIntegralOverRandomPoints 3 100)                g (mu,sigma) (alpha,beta)  
    let res2 = integrate3 (unitIntegralOverRandomPoints 3 1000)               g (mu,sigma) (alpha,beta)  
    let res3 = integrate3 (unitIntegralOverRandomPoints 3 10000)              g (mu,sigma) (alpha,beta)
    let res4 = integrate3 (unitIntegralOverPts (neiderreiterS 3 100) 100)     g (mu,sigma) (alpha,beta)
    let res5 = integrate3 (unitIntegralOverPts (neiderreiterS 3 1000) 1000)   g (mu,sigma) (alpha,beta)
    let res6 = integrate3 (unitIntegralOverPts (neiderreiterS 3 10000) 10000) g (mu,sigma) (alpha,beta)
    *)

    let res1 = 
      let mu    = Matrix.create 3 1 0.2 in
      let sigma = Matrix.add (Matrix.create 3 3 0.2) (Matrix.init 3 3 (fun _ _ -> 2.0)) in
      let alpha = Matrix.create 3 1 0.1 in
      let beta  = Matrix.create 3 1 0.3 in
      weightedGaussian (unitIntegralOverRandomPoints 3 100) g (mu,sigma) (alpha,beta)


    (*----------------------------------------------------------------------------
    !* expectationPropagation - alg 8
     *--------------------------------------------------------------------------*)
      
    let sqr x  : float  = x*x
    let inv x = 1.0/x

    let expectationPropagation (mu,sigma:matrix) m (a:matrix[],z:_[],alpha: _[],beta: _[]) nIters =

      let mu0 = mu in

      let mutable muHat    = mu0   in     
      let mutable sigmaHat = sigma in     
      let mu = Vector.create m 0.0 in  // nb: rebinds mu 
      let pi = Vector.create m 0.0 in
      let s  = Vector.create m 1.0 in

      (* pre-compute *)
      let aT = a |> Array.map (fun ai ->    printf "dim(ai) = %d,%d\n" ai.NumRows ai.NumCols; ai.Transpose)  in
      Printf.printf "sigmaHat=\n%O\n\n" sigmaHat;

      for i = 1 to nIters do
       for j = 0 to m-1 do
        Printf.printf "i=%d,j=%d,sigmaHat=\n%O\n\n" i j sigmaHat;

        (* Pre-computations for jth factor *)    
        let u_j      = sigmaHat * a.[j] in
        Printf.printf "i=%d,j=%d,u_j=\n%O\n\n" i j u_j;

        let c_j      = aT.[j] * u_j   |> Matrix.toScalar in
        Printf.printf "i=%d,j=%d,c_j=\n%O\n\n" i j c_j;
        let m_j      = aT.[j] * muHat |> Matrix.toScalar in
        Printf.printf "i=%d,j=%d,m_j=\n%O\n\n" i j m_j;
        let d_j      = pi.[j] * c_j in
        Printf.printf "i=%d,j=%d,d_j=\n%O\n\n" i j d_j;
        let phi_j    = m_j + d_j / (1.0 - d_j) * (m_j - mu.[j]) in
        Printf.printf "i=%d,j=%d,phi_j=\n%O\n\n" i j phi_j;
        let psi_j    = c_j / (1.0 - d_j) in
        Printf.printf "i=%d,j=%d,psi_j=\n%O\n\n" i j psi_j;
        let alpha_j  = alpha.[j] phi_j psi_j in
        Printf.printf "i=%d,j=%d,alpha_j=\n%O\n\n" i j alpha_j;
        let beta_j   = beta.[j]  phi_j psi_j in
        Printf.printf "i=%d,j=%d,beta_j=\n%O\n\n" i j beta_j;
       (* ADF update *)
        muHat    <- (let factor = (pi.[j] * (m_j - mu.[j]) + alpha_j)  /  (1.0 - d_j) in muHat + (factor * u_j))
        Printf.printf "i=%d,j=%d,muHat=\n%O\n\n" i j muHat;
        sigmaHat <- (let factor = (pi.[j] * (1.0 - d_j) - beta_j)  /  (1.0 - d_j) ** 2.0 in sigmaHat + (factor * (u_j * u_j.Transpose)));
        Printf.printf "i=%d,j=%d,ie1=\n%O\n\n" i j ((1.0 - d_j) ** 2.0);
        Printf.printf "i=%d,j=%d,ie2=\n%O\n\n" i j u_j.Transpose;
        Printf.printf "i=%d,j=%d,ie3=\n%O\n\n" i j ((pi.[j] * (1.0 - d_j) - beta_j));
        Printf.printf "i=%d,j=%d,ie4=\n%O\n\n" i j ((pi.[j] * (1.0 - d_j) - beta_j)  /  (1.0 - d_j) ** 2.0);
        Printf.printf "i=%d,j=%d,ie5=\n%O\n\n" i j (let factor = (pi.[j] * (1.0 - d_j) - beta_j)  /  (1.0 - d_j) ** 2.0 in factor * (u_j * u_j.Transpose));
        Printf.printf "i=%d,j=%d,sigmaHat=\n%O\n\n" i j sigmaHat;
        (* Factor update *)
        pi.[j] <- 1.0 / (inv beta_j - psi_j);
        mu.[j] <- alpha_j / beta_j + phi_j;
        s.[j]  <- z.[j] phi_j psi_j * exp ( sqr(alpha_j) / (2.0 * beta_j)) / sqrt (1.0 - psi_j * beta_j);
       done;
      done;


      (* result *)
      let sigmaInv    = symmetricInverse sigma     in
      let sigmaHatInv = symmetricInverse sigmaHat in (* invertible? *)

      let zHat =
        Vector.prod s *
        sqrt (determinant (sigmaHat * sigmaInv)) *
        exp (-0.5 * ( Vector.dot pi (mu .* mu)
                + Matrix.toScalar (mu0.Transpose  *  sigmaInv * mu0)
                - Matrix.toScalar (muHat.Transpose * sigmaHatInv * muHat)))
      in
      muHat, sigmaHat, zHat



    type floatf = float -> float -> float

    let expectationPropagationB 
         (mu,sigma:matrix) 
         m 
         (a : matrix[]) 
         (z : floatf[])
         (alphas: floatf [])
         (betas : floatf [])
         nIters =

      let mu0 = mu in

      let mutable muHat    = mu0   in     
      let mutable sigmaHat = sigma in     
      let mu = Vector.create m 0.0 in  // nb: rebinds mu 
      let pi = Vector.create m 0.0 in
      let s  = Vector.create m 1.0 in

      // pre-compute 
      let aT = a |> Array.map (fun ai ->  ai.Transpose)  in

      for i = 1 to nIters do
       for j = 0 to m-1 do

        // Pre-computations for jth factor 
        let u      = sigmaHat * a.[j] in

        let c      = aT.[j] * u   |> M2S in
        let m      = aT.[j] * muHat |> M2S in
        let d      = pi.[j] * c in
        let phi    = m + d / (1.0 - d) * (m - mu.[j]) in
        let psi    = c / (1.0 - d) in
        let alpha  = alphas.[j] phi psi in
        let beta   = betas.[j]  phi psi in
       // ADF update 
        muHat    <- (let factor = (pi.[j] * (m - mu.[j]) + alpha)  /  (1.0 - d) in
                     muHat + (factor * u))
        sigmaHat <- (let factor = (pi.[j] * (1.0 - d) - beta)  /  (1.0 - d) ** 2.0 in
                     sigmaHat + (factor * (u * u.Transpose)))
        // Factor update 
        pi.[j] <- 1.0 / (inv beta - psi);
        mu.[j] <- alpha / beta + phi;
        s.[j]  <- z.[j] phi psi * exp ( sqr(alpha) / (2.0 * beta)) / sqrt (1.0 - psi * beta);
       done;
      done;


      (* result *)
      let sigmaInv    = symmetricInverse sigma     in
      let sigmaHatInv = symmetricInverse sigmaHat in (* invertible? *)

      let zHat =
        Vector.prod s *
        sqrt (determinant (sigmaHat * sigmaInv)) *
        exp (-0.5 * ( Vector.dot pi (mu .* mu)
                 + M2S (mu0.Transpose  *  sigmaInv * mu0)
                 - M2S (muHat.Transpose * sigmaHatInv * muHat)))
      in
      muHat, sigmaHat, zHat

    (*----------------------------------------------------------------------------
    !* The MatLab version with hardwired functions
     *--------------------------------------------------------------------------*)

    let getV m i   = Matrix.get m i 0
    let setV m i x = Matrix.set m i 0 x

    (* TODO: syntax for matrix indexing and assignment *)
    (* Recoded MatLab version *)
    let zeros d = Matrix.create d 1 0.0
    let expectationPropagationMatLab (mu,sigma) m (l,u) noIterations =
      let siteMu            = zeros m in
      let sitePi            = zeros m in
      let siteS             = zeros m in
      let newMu          = ref mu      in
      let newSigma        = ref sigma   in

      for i = 0 to noIterations-1 do
        for j = 0 to m-1 do 
          // prepare computation    
          let t      = Matrix.init m 1 (fun i _ -> Matrix.get !newSigma i j) in
          let d  = getV sitePi j * Matrix.get !newSigma j j in
          let e  = 1.0 - d in
          let phi  = getV !newMu j + d * (getV !newMu j - getV siteMu j) / e in
          let psi  = Matrix.get !newSigma j j / e in
          let sPsi  = sqrt psi in
          let alpha  = v3 (phi / sPsi) (getV l j / sPsi) (getV u j / sPsi) / sPsi in
          let beta  = w3 (phi / sPsi) (getV l j / sPsi) (getV u j / sPsi) / psi in
          
          // GDF update
          newMu    := !newMu + (((getV sitePi j * (getV !newMu j - getV siteMu j) + alpha) / e) * t) ;
          newSigma := !newSigma + (((getV sitePi j * e - beta) / (e * e)) * (t * Matrix.transpose t)) ;
             
          // Factor update
          setV sitePi j (1.0 / (1.0/beta - psi));
          setV siteMu j (alpha / beta + phi);
          setV siteS  j ( (Phi ((getV u j - phi) / sPsi) - Phi ((getV l j - phi) / sPsi)) 
                      * exp (alpha * alpha / (2.0 * beta)) / sqrt (1.0 - psi * beta))
        done
      done;
      // compute the normalisation constant
      let SigmaInv    = symmetricInverse sigma    in
      let newSigmaInv = symmetricInverse !newSigma in
      let Z = 
         Matrix.prod siteS
         * sqrt (determinant !newSigma / determinant sigma)
         * exp (-0.5 * ( Matrix.dot sitePi (siteMu .* siteMu)
                        + Matrix.toScalar (Matrix.transpose mu * SigmaInv * mu)
                        - Matrix.toScalar (Matrix.transpose !newMu * newSigmaInv * !newMu)))
      in
      !newMu, !newSigma, Z


    (*----------------------------------------------------------------------------
    !* expectationPropagation - test code
     *--------------------------------------------------------------------------*)

    let mA = 1
    let aA     = [| Matrix.create 3 1 0.2   |]
    let zA     = [| fun (a:float) (b:float) -> a+b |]
    let alphaA = [| fun (a:float) (b:float) -> 0.1 |]
    let betaA  = [| fun (a:float) (b:float) -> 0.9 |]

    let muA    = Matrix.create 3 1 0.2
    let sigma' = matrix [[10.0; 2.0;-3.0];
                         [ 2.0; 4.0; 5.0];
                         [-3.0; 5.0;40.0]]
    let sigmaA = Matrix.map (fun k -> k * 0.1) sigma'
    let sInv  = symmetricInverse sigmaA (* check |R inverse exists *)


    let nItersA = 8
    let muHatA,sigmaHatA,zHatA = expectationPropagation (muA,sigmaA) mA (aA,zA,alphaA,betaA) nItersA

    let dumpM str m = Printf.printf "%s:\n%O\n\n" str m
    let dumpR str x = Printf.printf "%s = %f\n\n" str x
    let _ = dumpM "muHat"    muHatA
    let _ = dumpM "sigmaHat" sigmaHatA
    let _ = dumpR "zHat"     zHatA


    (*----------------------------------------------------------------------------
    !* expectationPropagation - test code MatLab values
     *--------------------------------------------------------------------------*)

    (* Matlab:
    >> [newMu, newSigma, evidence] = EP ([1;2],[[1 0];[0 2]], [0;0], [2; 3], 20)
    newMu =
        1.0000
        1.6599
    newSigma =
        0.2911         0
             0    0.6306
    evidence =
        0.4653
    >>
    >> help EP

      EP		The EP algorithm for computing Gaussian approximation to a
 		    truncated Gaussian.
     
 	    [newMu, newSigma, Z] = EP (mu, sigma, l, u, noIterations)
     
 		    mu		mean of the untruncated Gaussian
 		    sigma		covariance of the untruncated Gaussian
 		    l		lower integration boundaries
 		    u		upper integration boundaries
 		    noIterations	number of iterations (default: 20)
 		    newMu		mean of the Gaussian approximation
 		    newSigma	covariance of the Gaussian approximation
 		    Z		normalisation constant of the untruncated Gaussian
     
      2005 written by Ralf Herbrich
      Microsoft Research Ltd.
    *)

    let m = 2
    let mu    = matrix [[1.0];[2.0]]
    let sigma = matrix [[1.0; 0.0]; [0.0; 2.0]]
    let l     = matrix [[0.0];[0.0]]
    let u     = matrix [[2.0];[3.0]]
    let nIters = 20

    let x,y = l.Dimensions
    let _ = assert(x=m)
    let _ = assert(y=1)

    let a     = [| matrix [[ 1.0]; [0.0 ]];
                   matrix [[ 0.0]; [1.0 ]] |]
                
    (* factor l',u' *)
    let zf j phi_j psi_j =
      let k = inv(sqrt(psi_j)) in
      let l_j' = Matrix.get l j 0 * k in
      let u_j' = Matrix.get u j 0 * k in
      phi (u_j' - phi_j * k) - phi (l_j' - phi_j * k)

    let z = [| zf 0;zf 1 |]
               
    (* there is common code, compute both *)
    let alpha i phi_i psi_i =
      let root_psi_i = sqrt(psi_i) in
      v3 (phi_i/root_psi_i) (Matrix.get l i 0 / root_psi_i) (Matrix.get u i 0 / root_psi_i) / root_psi_i

    let beta i phi_i psi_i =
      let root_psi_i = sqrt(psi_i) in
      w3 (phi_i/root_psi_i) (Matrix.get l i 0 / root_psi_i) (Matrix.get u i 0 / root_psi_i) / psi_i

        
    let alphas = Array.init 2 (fun i -> alpha i)
    let betas  = Array.init 2 (fun i -> beta  i)

    let muHat ,sigmaHat ,zHat  = expectationPropagation (mu,sigma) m (a,z,alphas,betas) nIters

    let _ = dumpM "muHat"    muHat 
    let _ = dumpM "sigmaHat" sigmaHat   
    let _ = dumpR "zHat"     zHat  



    let muHatC,sigmaHatC,zHatC = expectationPropagationMatLab (mu,sigma) m (l,u) nIters

    let _ = dumpM "muHatC"    muHatC 
    let _ = dumpM "sigmaHatC" sigmaHatC   
    let _ = dumpR "zHatC"     zHatC  

    (*----------------------------------------------------------------------------
    !* misc tests
     *--------------------------------------------------------------------------*)


    let _ = (box 1N).GetHashCode()
    let _ = (box 1I).GetHashCode()
    let _ = hash 10000000000000000000N
    let _ = hash 1N
    let _ = hash (-1N)
    let _ = hash 1I
    let _ = hash 1000000000000000000I
    let _ = hash (-1I)
    let _ = hash (-10000000000000000000000000000000000000000I)


    (*----------------------------------------------------------------------------
    !* comparison code
     *--------------------------------------------------------------------------*)

    
    (*

    // specific alpha and gamma functions
    let alpha i phi psi (lower:matrix) (upper:matrix) = 
	    let spsi = sqrt (psi) in 
	    let u = upper.Item (i, 0)/spsi in
	    if (System.Double.IsPositiveInfinity (u)) then
		    1.0 / spsi * GaussianApproximations.v (phi/spsi, lower.Item (i, 0)/spsi)
	    else
		    1.0 / spsi * GaussianApproximations.v (phi/spsi, lower.Item (i, 0)/spsi, u)
    	
    let gamma i phi psi (lower:matrix) (upper:matrix) = 
	    let spsi = sqrt (psi) in 
	    let u = upper.Item (i, 0)/spsi in
	    if (System.Double.IsPositiveInfinity (u)) then
		    1.0 / psi * GaussianApproximations.w (phi/spsi, lower.Item (i, 0)/spsi)
	    else
		    1.0 / psi * GaussianApproximations.w (phi/spsi, lower.Item (i, 0)/spsi, u)
    	
    *)


    (* FRAGS: test code

    let n = 150
    let mu = new Matrix (n, 1)
    let sigma = new Matrix (n, n)
    let A = new Matrix (n, n)
    let lower = new Matrix (n, 1)
    let upper = new Matrix (n, 1)

    do
      for i = 0 to n-1 do
        mu.Item (i, 0) <- 1.0;
        sigma.Item (i, i) <- 1.0 + (float) i;
        A.Item (i, i) <- 1.0;
        lower.Item (i, 0) <- 0.0;
        upper.Item (i, 0) <- System.Double.PositiveInfinity;
      done  

    expectationPropagationMatLab (vnoc mu,vnoc sigma) n (vnoc lower,vnoc upper) 10

    *)

// These are really type checking tests
module BasicOverloadTests = 

    let f1 (a:matrix) (b:matrix) = a * b
    let f2 (a:matrix) (b:vector) = a * b

    let f12 (x1: Matrix<'a>) (x2: Matrix<'a>) = x1 * x2
    let f13 (x1: Matrix<'a>) (x2: Vector<'a>) = x1 * x2
    let f14 (x1: RowVector<'a>) (x2: Matrix<'a>) = x1 * x2

    // Traditionally we've had a hard time supporting both 
    //    'a * Matrix<'a>  -> Matrix<'a>
    //    Matrix<'a> * 'a  -> Matrix<'a>
    // overloads for multiplication. However these now work correctly
    let f15 (x1: Matrix<float>) (x2: float) = x1 * x2
    let f16 (x1: float) (x2: Matrix<float>) = x1 * x2
    // This is an interesting case. Strictly speaking it seems there is not enough information to resolve the
    // overload.
    // let f23 (x1: Matrix<_>) (x2: Matrix<_>) = x1 * x2

    let f24 (x1: Matrix<int>) (x2: Matrix<_>) = x1 * x2

// These are really type checking tests
module BasicGeneralizationTests = 
    let MFMV = Microsoft.FSharp.Math.Vector.init 3 (fun i -> float(i))
    let _ = MFMV.[1]
    let _ = MFMV.[1] <- 3.13

    // check we have generalized 
    let MFMV_generic_function (k: 'a) = 
      let MFMV = Microsoft.FSharp.Math.Vector.Generic.init 3 (fun i -> k) in
      let _ = MFMV.[1] in
      let _ =MFMV.[1] <- k in
      ()

    // check we have generalized 
    do MFMV_generic_function 1
    do MFMV_generic_function "3"

    let MFMM = Microsoft.FSharp.Math.Matrix.init 3 3 (fun i j -> float(i+j))
    let _ = MFMM.[1,1]
    let _ = MFMM.[1,1] <- 3.13

    // check we have generalized 
    let MFMM_generic_function (k: 'a) = 
      let MFMM = Microsoft.FSharp.Math.Matrix.Generic.init 3 3 (fun i j -> k) in
      let _ = MFMM.[1,1] in
      let _ =MFMM.[1,1] <- k in
      ()

    // check we have generalized 
    do MFMM_generic_function 1
    do MFMM_generic_function "3"

