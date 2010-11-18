// F#: test originally generated: (a:int[]).[0] + (b:int64[]).[0] 
//     which is incorrect, so I changed all numerical types to int

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class SubsetArrayTest : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Subset;
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
            return "SubsetArrayTest";
        }
    }

    public override string Description {
        get {
            return "Tests arrays while conforming to the subset.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        CodeNamespace ns = new CodeNamespace ("Namespace1");
        cu.Namespaces.Add (ns);

        // Generate Class1
        CodeTypeDeclaration class1 = new CodeTypeDeclaration ("Class1");
        class1.IsClass = true;
        ns.Types.Add (class1);

        // GENERATES (C#):
        //       public int ArrayMethod(int parameter) {
        //                int arraySize = 3;
        //                int[] array1;
        //                int[] array2 = new int[3];
        //                int[] array3 = new int[] {1,
        //                        4,
        //                        9};
        //                array1 = new int[arraySize];
        //                int retValue = 0;
        //                int i;
        //                for (i = 0; (i < array1.Length); i = (i + 1)) {
        //                    array1[i] = (i * i);
        //                    array2[i] = (array1[i] - i);
        //                    retValue = retValue + array1[i];
        //                    retValue = retValue + array2[i];
        //                    retValue = retValue + array3[i];
        //                }
        //                return retValue;
        //            }
        AddScenario ("ArrayMethod", "Tests sized arrays, initialized arrays, standard arrays of small size");
        CodeMemberMethod arrayMethod = new CodeMemberMethod ();
        arrayMethod.Name = "ArrayMethod";
        arrayMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "parameter"));
        arrayMethod.Attributes = (arrayMethod.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        arrayMethod.ReturnType = new CodeTypeReference (typeof (System.Int32));
        arrayMethod.Statements.Add (
            new CodeVariableDeclarationStatement (typeof (int), "arraySize", new CodePrimitiveExpression (3)));

        arrayMethod.Statements.Add (
            new CodeVariableDeclarationStatement (typeof (int[]), "array1"));

        arrayMethod.Statements.Add (
            new CodeVariableDeclarationStatement (
            new CodeTypeReference ("System.Int32", 1),
            "array2",
            new CodeArrayCreateExpression (typeof (int[]), new CodePrimitiveExpression (3))));

        arrayMethod.Statements.Add (
            new CodeVariableDeclarationStatement (
            new CodeTypeReference ("System.Int32", 1),
            "array3",
            new CodeArrayCreateExpression (
            new CodeTypeReference ("System.Int32", 1),
            new CodeExpression[] {
                new CodePrimitiveExpression (1),
                new CodePrimitiveExpression (4),
                new CodePrimitiveExpression (9)})));

        arrayMethod.Statements.Add (
            new CodeAssignStatement (
            new CodeVariableReferenceExpression ("array1"),
            new CodeArrayCreateExpression (typeof (int[]), new CodeVariableReferenceExpression ("arraySize"))));

        arrayMethod.Statements.Add (
            new CodeVariableDeclarationStatement (typeof (System.Int32), "retValue", new CodePrimitiveExpression (0)));

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
            CDHelper.CreateIncrementByStatement ("retValue",
                    CDHelper.CreateArrayRef ("array1", "i")),
            CDHelper.CreateIncrementByStatement ("retValue",
                    CDHelper.CreateArrayRef ("array2", "i")),
            CDHelper.CreateIncrementByStatement ("retValue",
                    CDHelper.CreateArrayRef ("array3", "i"))));

        arrayMethod.Statements.Add (
            new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("retValue")));
        class1.Members.Add (arrayMethod);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateClass1", "Make sure Class1 was instantiated before doing anything.");
        if (!FindAndInstantiate ("Namespace1.Class1", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateClass1");

        // Verify Array Operations
        if (VerifyMethod (genType, genObject, "ArrayMethod", new object[] {0}, 21))
            VerifyScenario ("ArrayMethod");
    }
}

