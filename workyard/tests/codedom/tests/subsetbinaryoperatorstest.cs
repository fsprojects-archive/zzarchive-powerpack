using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class SubsetBinaryOperatorsTest : CodeDomTestTree {

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
            return "SubsetBinaryOperatorsTest";
        }
    }

    public override string Description {
        get {
            return "Tests binary operators while staying within the subset spec.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        // GENERATES (C#):
        //  namespace Namespace1 {
        //      using System;
        //      
        //      
        //      public class Class1 : object {
        //          
        //          public int ReturnMethod(int intInput) {

        AddScenario ("ReturnMethod", "Verifies binary operators in a series of operations.");
        CodeNamespace ns = new CodeNamespace ("Namespace1");
        ns.Imports.Add (new CodeNamespaceImport ("System"));
        cu.Namespaces.Add (ns);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration ();
        class1.Name = "Class1";
        class1.BaseTypes.Add (new CodeTypeReference (typeof (object)));
        ns.Types.Add (class1);

        CodeMemberMethod retMethod = new CodeMemberMethod ();
        retMethod.Name = "ReturnMethod";
        retMethod.Attributes = (retMethod.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        retMethod.ReturnType = new CodeTypeReference (typeof (int));
        retMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "intInput"));

        // GENERATES (C#):
        //              int x1;
        //              x1 = 6 - 4;
        //              double x1d = x1;
        //              x1d = 18 / x1d;
        //              x1 = (int) x1d;
        //              x1 = x1 * intInput;

        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "x1"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("x1", 6, CodeBinaryOperatorType.Subtract, 4));
        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (double), "x1d", new CodeVariableReferenceExpression ("x1")));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("x1d", 18, CodeBinaryOperatorType.Divide, "x1d"));
        retMethod.Statements.Add (new CodeAssignStatement (new CodeVariableReferenceExpression ("x1"),
                    new CodeCastExpression (typeof (int), new CodeVariableReferenceExpression ("x1d"))));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("x1", "x1", CodeBinaryOperatorType.Multiply,
                    new CodeArgumentReferenceExpression ("intInput")));

        // GENERATES (C#):
        //              int x2;
        //              x2 = (19 % 8);
        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "x2"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("x2", 19, CodeBinaryOperatorType.Modulus, 8));

        // GENERATES (C#):
        //              int x3;
        //              x3 = 15 & 35;
        //              x3 = x3 | 129;
        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "x3"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("x3", 15, CodeBinaryOperatorType.BitwiseAnd, 35));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("x3", "x3", CodeBinaryOperatorType.BitwiseOr, 129));

        // GENERATES (C#):
        //              int x4 = 0;
        retMethod.Statements.Add (
            new CodeVariableDeclarationStatement (
            typeof (int),
            "x4",
            new CodePrimitiveExpression (0)));

        // GENERATES (C#):
        //              bool res1;
        //              res1 = x2 == 3;
        //              bool res2;
        //              res2 = x3 < 129;
        //              bool res3;
        //              res3 = res1 || res2;
        //              if (res3) {
        //                  x4 = (x4 + 1);
        //              }
        //              else {
        //                  x4 = (x4 + 2);
        //              }
        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (bool), "res1"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("res1", "x2", CodeBinaryOperatorType.ValueEquality, 3));

        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (bool), "res2"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("res2", "x3", CodeBinaryOperatorType.LessThan, 129));

        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (bool), "res3"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("res3", "res1", CodeBinaryOperatorType.BooleanOr, "res2"));

        retMethod.Statements.Add (
            new CodeConditionStatement (new CodeVariableReferenceExpression ("res3"),
            new CodeStatement [] { CDHelper.CreateIncrementByStatement ("x4", 1) },
            new CodeStatement [] { CDHelper.CreateIncrementByStatement ("x4", 2) }));

        // GENERATES (C#):
        //              bool res4;
        //              res4 = x2 > -1;
        //              bool res5;
        //              res5 = x3 > 5000;
        //              bool res6;
        //              res6 = res4 && res5;
        //              if (res6) {
        //                  x4 = (x4 + 4);
        //              }
        //              else {
        //                  x4 = (x4 + 8);
        //              }
        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (bool), "res4"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("res4", "x2", CodeBinaryOperatorType.GreaterThan, -1));

        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (bool), "res5"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("res5", "x3", CodeBinaryOperatorType.GreaterThanOrEqual, 5000));

        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (bool), "res6"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("res6", "res4", CodeBinaryOperatorType.BooleanAnd, "res5"));

        retMethod.Statements.Add (
            new CodeConditionStatement (
            new CodeVariableReferenceExpression ("res6"),
            new CodeStatement [] { CDHelper.CreateIncrementByStatement ("x4", 4) },
            new CodeStatement [] { CDHelper.CreateIncrementByStatement ("x4", 8) }));

        // GENERATES (C#):
        //              bool res7;
        //              res7 = x2 < 3;
        //              bool res8;
        //              res8 = x3 != 1;
        //              bool res9;
        //              res9 = res7 && res8;
        //              if (res9) {
        //                  x4 = (x4 + 16);
        //              }
        //              else {
        //                  x4 = (x4 + 32);
        //              }
        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (bool), "res7"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("res7", "x2", CodeBinaryOperatorType.LessThanOrEqual, 3));

        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (bool), "res8"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("res8", "x3", CodeBinaryOperatorType.IdentityInequality, 1));

        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (bool), "res9"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("res9", "res7", CodeBinaryOperatorType.BooleanAnd, "res8"));

        retMethod.Statements.Add (
            new CodeConditionStatement (
            new CodeVariableReferenceExpression ("res9"),
            new CodeStatement [] { CDHelper.CreateIncrementByStatement ("x4", 16) },
            new CodeStatement [] { CDHelper.CreateIncrementByStatement ("x4", 32) }));


        // GENERATES (C#):
        //              int theSum;
        //              theSum = x1 + x2;
        //              theSum = theSum + x3;
        //              theSum = theSum + x4;
        //              return theSum;
        //          }
        //
        retMethod.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "theSum"));
        retMethod.Statements.Add (CDHelper.CreateBinaryOperatorStatement ("theSum", "x1", CodeBinaryOperatorType.Add, "x2"));
        retMethod.Statements.Add (CDHelper.CreateIncrementByStatement ("theSum", "x3"));
        retMethod.Statements.Add (CDHelper.CreateIncrementByStatement ("theSum", "x4"));

        retMethod.Statements.Add (new CodeMethodReturnStatement (
                    new CodeVariableReferenceExpression ("theSum")));
        class1.Members.Add (retMethod);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateClass1", "Find and instantiate Namespace1.Class1.");
        if (!FindAndInstantiate ("Namespace1.Class1", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateClass1");

        // Verify Return value from function
        if (VerifyMethod (genType, genObject, "ReturnMethod", new object[] {5}, 204))
            VerifyScenario ("ReturnMethod");
    }
}

