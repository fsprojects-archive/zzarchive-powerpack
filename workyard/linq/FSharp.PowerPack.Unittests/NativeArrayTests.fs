namespace FSharp.PowerPack.Unittests

open NUnit.Framework
open System.Collections.Generic
open System.Runtime.InteropServices
open System.Diagnostics
open System
open System.Windows.Forms
open System.Drawing
open Microsoft.FSharp.NativeInterop
open Microsoft.FSharp.Math

#nowarn "9"
#nowarn "51"

module PrimitiveBindings = 

    /// LAPACK/BLAS primitive matrix/matrix multiply routine
    [<DllImport(@"blas.dll",EntryPoint="dgemm_")>]
    extern void DoubleMatrixMultiply_(char* transa, char* transb, int* m, int* n, int *k,
                                      double* alpha, double* A, int* lda,double* B, int* ldb,
                                      double* beta,
                                      double* C, int* ldc);

    ///  C := alpha*op( A )*op( B ) + beta*C
    let DoubleMatrixMultiply trans alpha (A: FortranMatrix<double>) (B: FortranMatrix<double>) beta (C: FortranMatrix<double>) = 
        // Mutable is needed because F# only lets you take pointers to mutable values
        let mutable trans = trans  // nb. unchanged on exit 
        let mutable beta = beta 
        let mutable alpha = alpha 
        let mutable m = A.NumCols 
        let mutable n = B.NumRows 
        let mutable k = A.NumRows 
        let mutable lda = A.NumCols 
        // Call the BLAS/LAPACK routine
        DoubleMatrixMultiply_(&&trans, &&trans, &&m, &&n, &&k, &&alpha, A.Ptr, &&lda, B.Ptr, &&k, &&beta, C.Ptr, &&m)

    /// LAPACK/BLAS primitive matrix/vector multiply routine
    [<DllImport(@"blas.dll",EntryPoint="dgemv_")>]
    extern void DoubleMatrixVectorMultiply_(char* trans, int* m, int* n,
                                            double* alpha, double* A, int* lda,
                                            double* x, int* incx, double* beta,
                                            double* y, int* incy);

    let DoubleMatrixVectorMultiply trans alpha (A: FortranMatrix<double>) (B: NativeArray<double>) beta (C: NativeArray<double>) = 
        let mutable trans = trans
        let mutable beta = beta 
        let mutable alpha = alpha 
        let mutable m = A.NumCols 
        let mutable n = A.NumRows 
        let mutable i_one = 1
        // Call the BLAS/LAPACK routine
        DoubleMatrixVectorMultiply_(&&trans, &&m, &&n, &&alpha, A.Ptr, &&m, B.Ptr, &&i_one, &&beta, C.Ptr, &&i_one)


    [<DllImport(@"lapack.dll", EntryPoint="dgetrf_")>]
    extern void DoublePLUDecomposition_(int *m, int *n, double *a, int* lda, int *ipiv, int *info);

    let DoublePLUDecomposition (A : FortranMatrix<double>) (ipiv : NativeArray<int>) = 
        let mutable m = A.NumCols
        let mutable n = A.NumRows
        let mutable info = 0
        let mutable lda = A.NumCols
        DoublePLUDecomposition_(&&m, &&n, A.Ptr, &&lda, ipiv.Ptr, &&info);
        match info with 
        | -1 -> invalid_arg "m"
        | -2 -> invalid_arg "n"
        | -3 -> invalid_arg "A"
        | -4 -> invalid_arg "lda"
        | -5 -> invalid_arg "ipiv"
        | -6 -> invalid_arg "info"
        | 0 -> ()
        | n -> invalid_arg (sprintf "singular: U(%d,%d) is zero" n n)


    [<DllImport(@"lapack.dll", EntryPoint="dgetrs_")>]
    extern void DoubleSolveAfterPLUDecomposition_(char *trans, int *n, int *nrhs, double *a, int *lda, int *ipiv, double*b, int * ldb, int*info)

    let DoubleSolveAfterPLUDecomposition trans (A : FortranMatrix<double>) (B : FortranMatrix<double>) (ipiv : NativeArray<int>) = 
        let mutable trans = trans
        let mutable n = A.NumRows
        let mutable nrhs = B.NumCols
        let mutable lda = n
        let mutable ldb = n
        let mutable info = 0
        DoubleSolveAfterPLUDecomposition_(&&trans, &&n, &&nrhs, A.Ptr, &&lda, ipiv.Ptr, B.Ptr, &&ldb, &&info);
        match info with 
        | -1 -> invalid_arg "trans"
        | -2 -> invalid_arg "n"
        | -3 -> invalid_arg "nrhs"
        | -4 -> invalid_arg "A"
        | -5 -> invalid_arg "lda"
        | -6 -> invalid_arg "ipiv"
        | -7 -> invalid_arg "B"
        | -8 -> invalid_arg "ldb"
        | -9 -> invalid_arg "info"
        | _ -> ()

    [<DllImport(@"lapack.dll", EntryPoint="dgeev_")>]
    extern void DoubleComputeEigenValuesAndVectors_(char *jobvl, char *jobvr, int *n, double *a, int *lda, double *wr, double *wi, double *vl, 
                                                    int *ldvl, double *vr, int *ldvr, double*work, int *lwork, int*info);

    let DoubleComputeEigenValuesAndVectors jobvl jobvr (A : FortranMatrix<double>) (WR : NativeArray<double>) (WI : NativeArray<double>) (VL : FortranMatrix<double>) (VR : FortranMatrix<double>) (workspace : NativeArray<double>) = 
        let mutable jobvl = jobvl
        let mutable jobvr = jobvr
        let mutable lda = A.NumCols
        let mutable n = A.NumRows
        let mutable n = A.NumRows
        let mutable ldvl = VL.NumCols
        let mutable ldvr = VR.NumCols
        let mutable lwork = workspace.Length
        let mutable info = 0
        DoubleComputeEigenValuesAndVectors_(&&jobvl, &&jobvr, &&n, A.Ptr, &&lda, WR.Ptr, WI.Ptr, VL.Ptr, &&ldvl, VR.Ptr, &&ldvr,workspace.Ptr, &&lwork, &&info);
        match info with 
        | -1 -> invalid_arg "jobvl"
        | -2 -> invalid_arg "jobvr"
        | -3 -> invalid_arg "n"
        | -4 -> invalid_arg "A"
        | -5 -> invalid_arg "lda"
        | -6 -> invalid_arg "wr"
        | -7 -> invalid_arg "wi"
        | -8 -> invalid_arg "vl"
        | -9 -> invalid_arg "ldvl"
        | -10 -> invalid_arg "vr"
        | -11 -> invalid_arg "ldvr"
        | -12 -> invalid_arg "work"
        | -13 -> invalid_arg "lwork"
        | -14 -> invalid_arg "info"
        | _ -> ()

    let DoubleComputeEigenValuesAndVectorsWorkspaceSize jobvl jobvr (A : FortranMatrix<double>)  = 
        let mutable jobvl = jobvl
        let mutable jobvr = jobvr
        let mutable lda = A.NumCols
        let mutable n = A.NumRows
        let mutable ldvl = n
        let mutable ldvr = n
        let mutable lwork = -1
        let mutable workspaceSize = 0
        let mutable info = 0
        printf "DoubleComputeEigenValuesAndVectorsWorkspaceSize\n" ;
        DoubleComputeEigenValuesAndVectors_(&&jobvl, &&jobvr, &&n, A.Ptr, &&lda, A.Ptr, A.Ptr, A.Ptr, &&ldvl, A.Ptr, &&ldvr,NativePtr.ofNativeInt (NativePtr.toNativeInt (&&workspaceSize)), &&lwork, &&info);
        printf "workspaceSize = %d\n" workspaceSize;
        match info with 
        | -1 -> invalid_arg "jobvl"
        | -2 -> invalid_arg "jobvr"
        | -3 -> invalid_arg "n"
        | -4 -> invalid_arg "A"
        | -5 -> invalid_arg "lda"
        | -6 -> invalid_arg "wr"
        | -7 -> invalid_arg "wi"
        | -8 -> invalid_arg "vl"
        | -9 -> invalid_arg "ldvl"
        | -10 -> invalid_arg "vr"
        | -11 -> invalid_arg "ldvr"
        | -12 -> invalid_arg "work"
        | -13 -> invalid_arg "lwork"
        | -14 -> invalid_arg "info"
        | _ -> workspaceSize


//----------------------------------------------------------------------------
/// Tutorial Part 3. LAPACK accepts Fortran matrices, though often permits flags 
/// to view the input matrices a transposed way.  This is a pain. Here we
/// use some implicit transpose trickery and transpose settings to give 
/// a view of these operations oeprating over CMatrix values.  Note that no actual
/// copying of matrix data occurs.

module PrimitiveCMatrixBindings = 

     /// Here we builda  version that operates on row-major data
    let DoubleMatrixMultiply alpha (A: CMatrix<double>) (B: CMatrix<double>) beta (C: CMatrix<double>) = 
        Debug.Assert(A.NumCols = B.NumRows);
        // Lapack is column-major, so we give it the implicitly transposed matrices and reverse their order:
        // C <- A*B   ~~> C' <- (B'*A')
        PrimitiveBindings.DoubleMatrixMultiply 'n' alpha B.NativeTranspose A.NativeTranspose beta C.NativeTranspose

    let DoubleMatrixVectorMultiply alpha (A: CMatrix<double>) (B: NativeArray<double>) beta (C: NativeArray<double>) = 
        Debug.Assert(A.NumCols = B.Length);
        // Lapack is column-major, so we tell it that A is transposed. The 't' and the A.NativeTranspose effectively cancel.
        // C <- A*B   ~~> C <- (A''*B)
        PrimitiveBindings.DoubleMatrixVectorMultiply 't' alpha A.NativeTranspose B beta C

    let DoubleSolveAfterPLUDecomposition (A : CMatrix<double>) (B : FortranMatrix<double>) (ipiv : NativeArray<int>) = 
        Debug.Assert(A.NumRows = A.NumCols);
        Debug.Assert(A.NumCols = B.NumRows);
        Debug.Assert(ipiv.Length = A.NumRows);
        // Lapack is column-major, so we solve A' X = B.  
        PrimitiveBindings.DoubleSolveAfterPLUDecomposition 'T' A.NativeTranspose B ipiv
        
    let DoubleComputeEigenValues (A: CMatrix<double>) (WR: NativeArray<double>) (WI: NativeArray<double>) (workspace: NativeArray<double>) = 
        Debug.Assert(A.NumCols = A.NumRows);
        Debug.Assert(A.NumCols = WR.Length);
        Debug.Assert(A.NumCols = WI.Length);
        Debug.Assert(workspace.Length >= max 1 (3*A.NumCols));
        let dummy = A.NativeTranspose 
        // Lapack is column-major, but the eigen values of the transpose are the same as the eigen values of A
        // C <- A*B   ~~> C <- (A''*B)
        PrimitiveBindings.DoubleComputeEigenValuesAndVectors 'v' 'v' A.NativeTranspose WR WI dummy dummy workspace
      
    let DoubleComputeEigenValuesAndVectors (A: CMatrix<double>) (WR: NativeArray<double>) (WI: NativeArray<double>) (VR: FortranMatrix<double>) (workspace: NativeArray<double>) = 
        Debug.Assert(A.NumCols = A.NumRows);
        Debug.Assert(A.NumCols = WR.Length);
        Debug.Assert(A.NumCols = WI.Length);
        Debug.Assert(workspace.Length >= max 1 (3*A.NumCols));
        let dummy = A.NativeTranspose 
        // Lapack is column-major, but the eigen values of the transpose are the same as the eigen values of A
        // C <- A*B   ~~> C <- (A''*B)
        PrimitiveBindings.DoubleComputeEigenValuesAndVectors 'n' 'v' A.NativeTranspose WR WI dummy VR workspace
      
    let DoubleComputeEigenValuesWorkspace (A: CMatrix<double>) = 
        Debug.Assert(A.NumCols = A.NumRows);
        let dummy = A.NativeTranspose 
        // Lapack is column-major, but the eigen values of the transpose are the same as the eigen values of A
        // C <- A*B   ~~> C <- (A''*B)
        PrimitiveBindings.DoubleComputeEigenValuesAndVectorsWorkspaceSize 'n' 'n' A.NativeTranspose 
        

//----------------------------------------------------------------------------
/// Tutorial Part 4. To pass F# data structures to C and Fortran you need to
/// pin the underlying array objects.  This can be done entirely F# code.
///

module NativeUtilities = 
    let nativeArray_as_CMatrix_colvec (arr: 'a NativeArray) =
       new CMatrix<_>(arr.Ptr,arr.Length,1)
       
    let nativeArray_as_FortranMatrix_colvec (arr: 'a NativeArray) =
       new FortranMatrix<_>(arr.Ptr,arr.Length,1)
       
    (* Functions to pin and free arrays *)
    let pinM m = PinnedArray2.of_matrix(m)
    let pinV v = PinnedArray.of_vector(v)
    let pinA arr = PinnedArray.of_array(arr)
    let pinMV m1 v2 = pinM m1,pinV v2
    let pinVV v1 v2 = pinV v1,pinV v2
    let pinAA v1 v2 = pinA v1,pinA v2
    let pinMVV m1 v2 m3 = pinM m1,pinV v2,pinV m3
    let pinMM m1 m2  = pinM m1,pinM m2
    let pinMMM m1 m2 m3 = pinM m1,pinM m2,pinM m3
    let freeM (pA: PinnedArray2<'a>) = pA.Free()
    let freeV (pA: 'a PinnedArray) = pA.Free()
    let freeA (pA: 'a PinnedArray) = pA.Free()
    let freeMV ((pA: PinnedArray2<'a>),(pB : 'a PinnedArray)) = pA.Free(); pB.Free()
    let freeVV ((pA: 'a PinnedArray),(pB : 'a PinnedArray)) = pA.Free(); pB.Free()
    let freeAA ((pA: 'a PinnedArray),(pB : 'a PinnedArray)) = pA.Free(); pB.Free()
    let freeMM ((pA: PinnedArray2<'a>),(pB: PinnedArray2<'a>)) = pA.Free();pB.Free()
    let freeMMM ((pA: PinnedArray2<'a>),(pB: PinnedArray2<'a>),(pC: PinnedArray2<'a>)) = pA.Free();pB.Free();pC.Free()
    let freeMVV ((pA: PinnedArray2<'a>),(pB: 'a PinnedArray),(pC: 'a PinnedArray)) = pA.Free();pB.Free();pC.Free()


//----------------------------------------------------------------------------
/// Tutorial Part 5. Higher level-place bindings that operate mutatively over the F#
/// Matrix type by first pinning the data structures.  Be careful with these operations,
/// as they will write some results into your input matrices.
///

module MutableMatrixRoutines = 

    open NativeUtilities

    /// C <- A * B
    let mulMM (A:matrix) (B:matrix) (C:matrix) = 
        let (pA,pB,pC) as ptrs = pinMMM A B C
        try PrimitiveCMatrixBindings.DoubleMatrixMultiply 1.0 pA.NativeArray pB.NativeArray 0.0 pC.NativeArray
        finally
            freeMMM ptrs

    /// C <- A * V
    let mulMV (A:matrix) (B:vector) (C:vector) = 
        let pA,pB,pC as pin = pinMVV A B C
        try PrimitiveCMatrixBindings.DoubleMatrixVectorMultiply 1.0 pA.NativeArray pB.NativeArray 0.0 pC.NativeArray
        finally
            freeMVV pin

    /// B <- A \ B
    let solve (A : matrix) (B: vector) (ipiv: int[]) = 
        let pA = pinM A
        let pB = pinV B
        let pPivots = pinA ipiv
        try 
          PrimitiveBindings.DoublePLUDecomposition pA.NativeArray.NativeTranspose pPivots.NativeArray;
          PrimitiveCMatrixBindings.DoubleSolveAfterPLUDecomposition pA.NativeArray (nativeArray_as_FortranMatrix_colvec pB.NativeArray) pPivots.NativeArray
        finally
            freeM pA; freeV pB; freeA pPivots

    let computeEigenValues (A : matrix) (WR: double[]) (WI:double[]) (workspace:double[])  = 
        let pA = pinM A
        let pWR,pWI as pWRI = pinAA WR WI
        let pWorkspace = pinA workspace
        try 
          PrimitiveCMatrixBindings.DoubleComputeEigenValues pA.NativeArray pWR.NativeArray pWI.NativeArray pWorkspace.NativeArray 
        finally
            freeM pA; freeVV pWRI; freeA pWorkspace
            
    let computeEigenValuesAndVectors (A : matrix) (WR: double[]) (WI:double[]) (VR: matrix) (workspace:double[])  = 
        let pA,pVR as pMM = pinMM A VR
        let pWR,pWI as pWRI = pinAA WR WI
        let pWorkspace = pinA workspace
        try 
          PrimitiveCMatrixBindings.DoubleComputeEigenValuesAndVectors pA.NativeArray pWR.NativeArray pWI.NativeArray pVR.NativeArray.NativeTranspose pWorkspace.NativeArray 
        finally
            freeMM pMM; freeVV pWRI; freeA pWorkspace
            

//----------------------------------------------------------------------------
// Tutorial Part 6. Higher level bindings that defensively copy their input
//

module ImmutableMatrixRoutines = 

    open NativeUtilities

    /// Compute A * B
    let mulMM (A:matrix) (B:matrix) = 
        let C = Matrix.zero A.NumRows B.NumCols
        // C <- A * B
        MutableMatrixRoutines.mulMM A B C;
        C

    /// Compute A * v
    let mulMV (A:matrix) (v:vector) = 
        let C = Vector.zero A.NumRows
        // C <- A * v
        MutableMatrixRoutines.mulMV A v C;
        C

    /// Compute A \ v
    let solve (A : matrix) (v: vector) = 
        Debug.Assert(A.NumRows = A.NumCols);
        let A = Matrix.copy A // workspace (yuck) 
        let vX = Vector.copy v 
        let ipiv = Array.zeroCreate A.NumCols 
        // vX <- A \ v
        MutableMatrixRoutines.solve A vX ipiv;
        vX

    (* The underlying LAPACK routine raises an error when trying to estimate the workspace. *)
    (* Hence I've removed this from the sample just for now. *)
    
(*
    let computeEigenValuesWorkspace (A : matrix)   = 
        let pA = pinM A
        try 
          PrimitiveCMatrixBindings.DoubleComputeEigenValuesWorkspace pA.NativeArray 
        finally
            freeM pA
*)
            
    let computeEigenValues (A : matrix)  = 
        let At = A.Transpose
        let n = At.NumRows
        let WR = Array.zeroCreate n
        let WI = Array.zeroCreate n
        let workspace = Array.zeroCreate (5 * n (* computeEigenValuesWorkspace At *) ) 
        MutableMatrixRoutines.computeEigenValues At WR WI workspace;
        let W = Vector.Generic.init n (fun i -> Complex.mkPolar (WR.[i],WI.[i])) 
        W
             
    let computeEigenValuesAndVectors (A : matrix)  = 
        let At = A.Transpose
        let n = At.NumRows
        let WR = Array.zeroCreate n
        let WI = Array.zeroCreate n
        let VR = Matrix.zero n n
        let workspace = Array.zeroCreate (5 * n (* computeEigenValuesAndVectorsWorkspace A *) ) 
        MutableMatrixRoutines.computeEigenValuesAndVectors At WR WI VR workspace;
        let W = Vector.Generic.init n (fun i -> Complex.mkPolar (WR.[i],WI.[i])) 
        W,VR.Transpose

//----------------------------------------------------------------------------
// Tutorial Part 7. Reusing primitive bindings on other similarly shapped data structures
//
// Here we show that the same NativeArray/CMatrix/FortranMatrix primitive bindings can be used
// with other data structures where the underlying bits are ultimately stored as shape double[] and double[,],
// e.g. they can be used directly on arrays, or even on memory allocated C.

module MutableArrayRoutines = 
    open NativeUtilities
    let pinA2 arr2 = PinnedArray2.of_array2D(arr2)
    let pinA2AA m1 v2 m3 = pinA2 m1,pinA v2,pinA m3
    let freeA2A2A2 ((pA: PinnedArray2<'a>),(pB: PinnedArray2<'a>),(pC: PinnedArray2<'a>)) = pA.Free();pB.Free();pC.Free()
    let freeA2AA ((pA: PinnedArray2<'a>),(pB: 'a PinnedArray),(pC: 'a PinnedArray)) = pA.Free();pB.Free();pC.Free()
    let pinA2A2A2 m1 m2 m3 = pinA2 m1,pinA2 m2,pinA2 m3
    let freeA2 (pA: PinnedArray2<'a>) = pA.Free()

    let mulMM (A: double[,]) (B: double[,]) (C: double[,]) = 
        let (pA,pB,pC) as ptrs = pinA2A2A2 A B C
        try PrimitiveCMatrixBindings.DoubleMatrixMultiply 1.0 pA.NativeArray pB.NativeArray 0.0 pC.NativeArray
        finally
            freeMMM ptrs

    let mulMV (A: double[,]) (B: double[]) (C: double[]) = 
        let pA,pB,pC as pin = pinA2AA A B C
        try PrimitiveCMatrixBindings.DoubleMatrixVectorMultiply 1.0 pA.NativeArray pB.NativeArray 0.0 pC.NativeArray
        finally
            freeA2AA pin
    
    let solve (A : double[,]) (B: double[]) (ipiv: int[]) = 
        let pA = pinA2 A
        let pB = pinA B
        let pPivots = pinA ipiv
        try 
          PrimitiveBindings.DoublePLUDecomposition pA.NativeArray.NativeTranspose pPivots.NativeArray;
          PrimitiveCMatrixBindings.DoubleSolveAfterPLUDecomposition pA.NativeArray (nativeArray_as_FortranMatrix_colvec pB.NativeArray) pPivots.NativeArray
        finally
            freeA2 pA; freeA pB; freeA pPivots
    
module ImmutableArrayRoutines = 
    let mulMM (A:double[,]) (B:double[,]) = 
        let C = Array2D.zeroCreate (Array2D.length1 A) (Array2D.length2 B)
        MutableArrayRoutines.mulMM A B C;
        C

    let mulMV (A:double[,]) (B:double[]) = 
        let C = Array.zeroCreate (Array2D.length1 A)
        MutableArrayRoutines.mulMV A B C;
        C
    let solve (A : double[,]) (B: double[]) = 
        Debug.Assert(Array2D.length1 A = Array2D.length2 A);
        let A = Array2D.init (Array2D.length1 A) (Array2D.length2 A) (fun i j ->  A.[i,j]) 
        let BX = Array.copy B 
        let ipiv = Array.zeroCreate (Array2D.length1 A)
        MutableArrayRoutines.solve A BX ipiv;
        BX

[<TestFixture>]
type public NativeArrayTests() =
  
    [<Test>]
    member this.BasicTests1() = 


          // Ensure you have LAPACK.dll and BLAS.dll the sample directory or elsewhere
          // on your path. Here we set the current directory so we can find any local copies
          // of LAPACK.dll and BLAS.dll.
          System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__ 

          // Here are some simple matrix values.
          let onesM = matrix [ [ 1.0; 1.0];  
                                [1.0; 1.0] ]
          let onesA = [| 1.0; 1.0 |]
          let onesV = vector [ 1.0; 1.0]
          let onesv = matrix [ [ 1.0] ; 
                               [ 1.0] ]
          let twosM = matrix [ [ 2.0; 2.0]; 
                               [ 2.0; 2.0] ]
          let twosV = vector [ 2.0; 2.0 ]
          let twosv = matrix [ [ 2.0] ; 
                               [ 2.0] ]
          let iM = matrix [ [ 1.0; 0.0]; 
                            [ 0.0; 1.0] ]

          let miM = matrix [ [ -1.0; 0.0]; 
                             [  0.0; -1.0] ]

          matrix [ [ 0.0; 0.0]; 
                   [ 0.0; 0.0] ]  |> ignore

          Matrix.identity 2  |> ignore

          Matrix.identity 10  |> ignore

          let A = 1

          let show x = printf "%s\n" (sprintf "%A" x)
          printf " ------------------------\n" 

          let J  = matrix [ [ 2.0; 3.0]; 
                            [ 4.0; 5.0] ]
          let J2 = matrix [ [ 2.0; 3.0;4.0]; 
                            [ 2.0; 3.0;5.0]; 
                            [ 4.0; 5.0;6.0] ]

          // MATALB notation is M \ v, i.e. solve Mx=v for vector x.  Solving Mx=v
          // The notation we use here is M $ v
          let ( $ ) (M : matrix) v = ImmutableMatrixRoutines.solve M v
          let ( *. ) A B = ImmutableMatrixRoutines.mulMM A B
          let ( *%. ) A v = ImmutableMatrixRoutines.mulMV A v

          let Random = Matrix.create 200 200 1.0 |> Matrix.randomize
          let RandomV = Matrix.create 200 1 1.0 |> Matrix.toVector

          let time s f =
            let sw = new System.Diagnostics.Stopwatch() 
            sw.Start();
            let res = f()
            printf "%s, time: %d\n" s sw.ElapsedMilliseconds;
            ()
            
            
          time "Random * Random" (fun () -> Random * Random )
          time "Random *. Random" (fun () -> Random *. Random )
          time "Random $ Random" (fun () -> Random $ RandomV )
          time "Random $ Random, check with *" (fun () -> Vector.sum (Random * (Random $ RandomV) - RandomV) |> show )
          time "Random $ Random, check with *%." (fun () -> Vector.sum (Random *%. (Random $ RandomV) - RandomV) |> show )

          time "computeEigenValues Random" (fun () -> ImmutableMatrixRoutines.computeEigenValues Random)

          ImmutableMatrixRoutines.computeEigenValues (Matrix.identity 2) |> ignore
          ImmutableMatrixRoutines.computeEigenValuesAndVectors (Matrix.identity 2) |> ignore
          ImmutableMatrixRoutines.computeEigenValuesAndVectors (matrix [ [ 3.0 ] ]) |> ignore
          ImmutableMatrixRoutines.computeEigenValuesAndVectors (matrix [ [ 3.0; 0.0 ]; [ 0.0; 3.0] ]) |> ignore
          ImmutableMatrixRoutines.computeEigenValuesAndVectors (matrix [ [ 1.0; 0.0 ]; [ 0.0; -1.0] ]) |> ignore
          ImmutableMatrixRoutines.computeEigenValuesAndVectors (matrix [ [ -1.0; 0.0 ]; [ 0.0; -1.0] ]) |> ignore

          let trans = ref (fun (t:float) -> Matrix.identity 2)
          let points = ref [ (0.0, 0.0); (1.0,0.0); (1.0,1.0); (0.0,1.0); (0.0,0.0) ]

          let f = new System.Windows.Forms.Form()
          let paint (e:System.Windows.Forms.PaintEventArgs) = 
              let t = (System.DateTime.Now.Ticks |> Int64.to_float) / 10000000.0
              let trans = (!trans) t
              let of_vec (v:vector) = new Point(f.Width/2 + int (v.[0]*100.0), f.Height/2  - int (v.[1]*100.0)) 
              let origin = vector [0.0; 0.0]
              let to_vec (x,y) = vector [x;y]
              let draw_vec pen v1 v2 = e.Graphics.DrawLine(pen,of_vec v1,of_vec v2);
              let draw (p1,p2) = draw_vec Pens.Aqua (trans * to_vec p1) (trans * to_vec p2);
              let es,E = ImmutableMatrixRoutines.computeEigenValuesAndVectors trans.Transpose
              draw_vec Pens.Red origin (E.Column 0 * es.[0].r)
              draw_vec Pens.Red origin (E.Column 1 * es.[1].r)
              !points |> List.fold (fun acc p1 -> match acc with None -> Some p1 | Some p0 -> draw (p0,p1); Some p1) None |> ignore;
              f.Invalidate()
          let scaleM n k = Matrix.initDiagonal (Vector.create n k)
          trans := (fun t -> scaleM 2 (sin t)); f.Invalidate()
          let rotateM th = matrix [ [ cos th; -sin th ]; [ sin th; cos th ] ]
          let pi = System.Math.PI
          trans := (fun t -> rotateM (pi * t)); f.Invalidate()
          trans := (fun t -> matrix [ [ sin t; 1.0]; [cos (t/1.3); 1.0] ])

          ImmutableMatrixRoutines.computeEigenValuesAndVectors (rotateM 0.23)  |> ignore
          let M = matrix [ [ 0.5; 1.0]; [0.0; 1.0] ]
          trans := (fun _ -> M)
          let es,EM = ImmutableMatrixRoutines.computeEigenValuesAndVectors M.Transpose

          let e0 = es.[1]
          assert(e0.i = 0.0) 
          M.Transpose * EM.Column 0  |> ignore
          M.Transpose * EM.Column 1 * (1.0/es.[1].r)  |> ignore
          let disp x = sprintf "%A" x


          printf " ------------------------\n" 
          printf " %s $ %s = %s\n" (disp J) (disp onesV) (disp (J $ onesV))
          printf " ------------------------\n" 
          onesV  |> show
          printf " ------------------------\n" 
          J * (J $ vector [1.0;1.0])  |> show
          printf " ------------------------\n" 
          J2 * (J2 $ vector [1.0;2.0;3.0])  |> show
          printf " ------------------------\n" 
          printf " DONE\n" 


