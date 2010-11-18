// F#: test originally generated: (a:int[]).[0] + (b:int64[]).[0] 
//     which is incorrect, so I changed all numerical types to int

using System;
using System.IO;
using System.CodeDom;
using System.Reflection;
using System.Collections;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.VisualBasic;

public class ArrayTest : CodeDomTestTree {
    public override TestTypes TestType {
        get {
            return TestTypes.Everett;
        }
    }

    public override bool ShouldCompile {
        get {
            return true;
        }
    }

    public override bool ShouldVerify {
        get {
            return true;
        }
    }

    public override string Name {
        get {
            return "ArrayTest";
        }
    }

    public override string Description {
        get {
            return "Tests arrays.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {
        CodeNamespace ns = new CodeNamespace ("Namespace1");
        cu.Namespaces.Add (ns);

        // GENERATES (C#):
        //    public class Class2 {
        //            public int number;
        //        }
        CodeTypeDeclaration class1 = new CodeTypeDeclaration ("Class2");
        class1.IsClass = true;
        ns.Types.Add (class1);

        CodeMemberField field = new CodeMemberField (typeof (int), "number");
        field.Attributes = MemberAttributes.Final | MemberAttributes.Public;
        class1.Members.Add (field);

        class1 = new CodeTypeDeclaration ();
        class1.Name = "Class1";
        class1.BaseTypes.Add (new CodeTypeReference (typeof (object)));
        ns.Types.Add (class1);


        // the following method tests sized arrays, initialized arrays, standard arrays of small size
        // GENERATES (C#):
        //       public long ArrayMethod(int parameter) {
        //                int arraySize = 3;
        //                int[] array1;
        //                int[] array2 = new int[3];
        //                int[] array3 = new int[] {1,
        //                        4,
        //                        9};
        //                array1 = new int[arraySize];
        //                long retValue = 0;
        //                int i;
        //                for (i = 0; (i < array1.Length); i = (i + 1)) {
        //                    array1[i] = (i * i);
        //                    array2[i] = (array1[i] - i);
        //                    retValue = (retValue 
        //                                + (array1[i] 
        //                                + (array2[i] + array3[i])));
        //                }
        //                return retValue;
        //            }

        AddScenario ("CallArrayMethod", "Call and check the return value of ArrayMethod()");
        CodeMemberMethod arrayMethod = new CodeMemberMethod ();
        arrayMethod.Name = "ArrayMethod";
        arrayMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "parameter"));
        arrayMethod.Attributes = (arrayMethod.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        arrayMethod.ReturnType = new CodeTypeReference (typeof (System.Int32));

        arrayMethod.Statements.Add (
            new CodeVariableDeclarationStatement (typeof (int), "arraySize", new CodePrimitiveExpression (3)));

        // dummy array creates
        arrayMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (int[]),
                    "dummyArray1", new CodeArrayCreateExpression (typeof (int[]), 10)));
        arrayMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (int[]),
                    "dummyArray2", new CodeArrayCreateExpression (typeof (int[]),
                        new CodeVariableReferenceExpression ("arraySize"))));

        arrayMethod.Statements.Add (
            new CodeVariableDeclarationStatement (typeof (int[]), "array1"));

        CodeArrayCreateExpression creatExp = new CodeArrayCreateExpression ();
        creatExp.CreateType = new CodeTypeReference (typeof (int[]));
        creatExp.Size = 3;

        arrayMethod.Statements.Add (
            new CodeVariableDeclarationStatement (
            new CodeTypeReference ("System.Int32", 1),
            "array2",
            creatExp));

        arrayMethod.Statements.Add (
            new CodeVariableDeclarationStatement (
            new CodeTypeReference ("System.Int32", 1),
            "array3",
            new CodeArrayCreateExpression (
            new CodeTypeReference ("System.Int32", 1),
            new CodeExpression[] {
                new CodePrimitiveExpression ((int)1), 
                new CodePrimitiveExpression ((int)4),
                new CodePrimitiveExpression ((int)9), })));

        creatExp = new CodeArrayCreateExpression ();
        creatExp.CreateType = new CodeTypeReference (typeof (int[]));
        creatExp.SizeExpression = new CodeVariableReferenceExpression ("arraySize");

        arrayMethod.Statements.Add (new CodeAssignStatement (new CodeVariableReferenceExpression ("array1"), creatExp));
        
        arrayMethod.Statements.Add (
            new CodeVariableDeclarationStatement (typeof (System.Int32), "retValue", new CodePrimitiveExpression ((int)0))); 
        arrayMethod.Statements.Add (
            new CodeVariableDeclarationStatement (typeof (int), "i"));
        arrayMethod.Statements.Add (
            new CodeIterationStatement (
            new CodeAssignStatement (new CodeVariableReferenceExpression ("i"), new CodePrimitiveExpression (0)),
            new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("i"),
            CodeBinaryOperatorType.LessThan,
            new CodePropertyReferenceExpression (
            new CodeVariableReferenceExpression ("array1"),
            "Length")),
            new CodeAssignStatement (
            new CodeVariableReferenceExpression ("i"),
            new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("i"),
            CodeBinaryOperatorType.Add,
            new CodePrimitiveExpression (1))),
            new CodeAssignStatement (
            new CodeArrayIndexerExpression (
            new CodeVariableReferenceExpression ("array1"),
            new CodeVariableReferenceExpression ("i")),
            new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("i"),
            CodeBinaryOperatorType.Multiply,
            new CodeVariableReferenceExpression ("i"))),
            new CodeAssignStatement (
            new CodeArrayIndexerExpression (
            new CodeVariableReferenceExpression ("array2"),
            new CodeVariableReferenceExpression ("i")),
            new CodeBinaryOperatorExpression (
            new CodeArrayIndexerExpression (
            new CodeVariableReferenceExpression ("array1"),
            new CodeVariableReferenceExpression ("i")),
            CodeBinaryOperatorType.Subtract,
            new CodeVariableReferenceExpression ("i"))),
            new CodeAssignStatement(
            new CodeVariableReferenceExpression("retValue"),
            new CodeBinaryOperatorExpression(
            new CodeVariableReferenceExpression("retValue"),
            CodeBinaryOperatorType.Add,
            new CodeBinaryOperatorExpression(
            new CodeArrayIndexerExpression(
            new CodeVariableReferenceExpression("array1"),
            new CodeVariableReferenceExpression("i")),
            CodeBinaryOperatorType.Add,
            new CodeBinaryOperatorExpression(
            new CodeArrayIndexerExpression(
            new CodeVariableReferenceExpression("array2"),
            new CodeVariableReferenceExpression("i")),
            CodeBinaryOperatorType.Add,
            new CodeArrayIndexerExpression(
            new CodeVariableReferenceExpression("array3"),
            new CodeVariableReferenceExpression("i"))))))));
        arrayMethod.Statements.Add(
            new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("retValue")));
        class1.Members.Add (arrayMethod);


        // GENERATES (C#):
        //        public int MoreArrayTests(int i) {
        //                int[][] arrayOfArrays = new int[][] {new int[] {3,
        //                                4},
        //                        new int[1],
        //                        new int[0]};
        //                int[] array2 = new int[0];
        //                Class2[] arrayType = new Class2[2];
        //                arrayType[1] = new Class2();
        //                arrayType[1].number = (arrayOfArrays[0][1] + i);
        //                return arrayType[1].number;
        //            }

        AddScenario ("CallMoreArrayTests", "Call and check the return value of MoreArrayTests()");
        CodeMemberMethod secondMethod = new CodeMemberMethod ();
        secondMethod.Name = "MoreArrayTests";
        secondMethod.Attributes = (arrayMethod.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        secondMethod.ReturnType = new CodeTypeReference (typeof (int));
        secondMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));

        // array of arrays
        // in Everett, VB doesn't support array of array initialization
        if (Supports (provider, GeneratorSupport.ArraysOfArrays) && !(provider is VBCodeProvider)) {
            secondMethod.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference (typeof (int[][])),
                "arrayOfArrays", new CodeArrayCreateExpression (typeof (int[][]),
                new CodeArrayCreateExpression (typeof (int[]), new CodePrimitiveExpression (3), new CodePrimitiveExpression (4)),
                new CodeArrayCreateExpression (typeof (int[]), new CodePrimitiveExpression (1)), new CodeArrayCreateExpression (typeof (int[])))));
        }

        //empty array
        secondMethod.Statements.Add (new CodeVariableDeclarationStatement (
            new CodeTypeReference ("System.Int32", 1), "array2",
            new CodeArrayCreateExpression (typeof (int[]), new CodePrimitiveExpression (0))));

        // array of nonprimitive type
        secondMethod.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference ("Class2", 1),
            "arrayType", new CodeArrayCreateExpression (new CodeTypeReference ("Class2", 1), new CodePrimitiveExpression (2))));
        secondMethod.Statements.Add (new CodeAssignStatement (new CodeArrayIndexerExpression (new CodeVariableReferenceExpression ("arrayType"),
            new CodePrimitiveExpression (1)), new CodeObjectCreateExpression (new CodeTypeReference ("Class2"))));

        // in Everett, VB doesn't support array of array initialization
        if (Supports (provider, GeneratorSupport.ArraysOfArrays) && !(provider is VBCodeProvider)) {
            secondMethod.Statements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression
                (new CodeArrayIndexerExpression (new CodeVariableReferenceExpression ("arrayType"),
                new CodePrimitiveExpression (1)), "number"),
                new CodeBinaryOperatorExpression (new CodeArrayIndexerExpression (
                new CodeArrayIndexerExpression (new CodeVariableReferenceExpression ("arrayOfArrays"), new CodePrimitiveExpression (0))
                , new CodePrimitiveExpression (1)),
                CodeBinaryOperatorType.Add, new CodeArgumentReferenceExpression ("i"))));
        }
        else {
            // If the code provider doesn't support ArrayOfArrays this is generated:
            //   arrayType[1].number = i;
            secondMethod.Statements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression
                (new CodeArrayIndexerExpression (new CodeVariableReferenceExpression ("arrayType"),
                new CodePrimitiveExpression (1)), "number"),
                new CodeArgumentReferenceExpression ("i")));

        }
        secondMethod.Statements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression
            (new CodeArrayIndexerExpression (new CodeVariableReferenceExpression ("arrayType"),
            new CodePrimitiveExpression (1)), "number")));

        class1.Members.Add (secondMethod);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateClass1", "Find and instantiate Class1");
        if (!FindAndInstantiate ("Namespace1.Class1", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateClass1");

        // Verify Array Operations
        if (VerifyMethod (genType, genObject, "ArrayMethod", new object[] {0}, 21)) {
            VerifyScenario ("CallArrayMethod");
        }

        // in Everett, VB doesn't support array of array initialization
        if (Supports (provider, GeneratorSupport.ArraysOfArrays) && !(provider is VBCodeProvider)) {
            if (VerifyMethod (genType, genObject, "MoreArrayTests", new object[] {43}, 47))
                VerifyScenario ("CallMoreArrayTests");
        }
        else {
            if (VerifyMethod (genType, genObject, "MoreArrayTests", new object[] {43}, 43))
                VerifyScenario ("CallMoreArrayTests");
        }
    }
}

