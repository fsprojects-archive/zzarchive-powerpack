using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class BinaryOperatorsTest : CodeDomTestTree {

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
            return "BinaryOperatorsTest";
        }
    }

    public override string Description {
        get {
            return "Tests binary operators.";
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

        CodeNamespace ns = new CodeNamespace ("Namespace1");
        ns.Imports.Add (new CodeNamespaceImport ("System"));
        cu.Namespaces.Add (ns);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration ();
        class1.Name = "Class1";
        class1.BaseTypes.Add (new CodeTypeReference (typeof (object)));
        ns.Types.Add (class1);

        AddScenario ("CheckReturnMethod", "Tests varying operators.");
        CodeMemberMethod retMethod = new CodeMemberMethod ();
        retMethod.Name = "ReturnMethod";
        retMethod.Attributes = (retMethod.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        retMethod.ReturnType = new CodeTypeReference (typeof (int));
        retMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "intInput"));

        // GENERATES (C#):
        //              int x1 = ((18 
        //                          / (6 - 4)) 
        //                          * intInput);
        //
        // in VB (a = a/a) generates an warning if a is integer. Cast the expression just for VB language. 
        CodeBinaryOperatorExpression cboExpression = new CodeBinaryOperatorExpression (
            new CodeBinaryOperatorExpression (
            new CodePrimitiveExpression (18),
            CodeBinaryOperatorType.Divide,
            new CodeBinaryOperatorExpression (
            new CodePrimitiveExpression (6),
            CodeBinaryOperatorType.Subtract,
            new CodePrimitiveExpression (4))),
            CodeBinaryOperatorType.Multiply,
            new CodeArgumentReferenceExpression ("intInput"));

        CodeVariableDeclarationStatement variableDeclaration = null;
        if (provider is Microsoft.VisualBasic.VBCodeProvider)
            variableDeclaration = new CodeVariableDeclarationStatement (typeof (int), "x1", new CodeCastExpression (typeof (int), cboExpression));
        else
            variableDeclaration = new CodeVariableDeclarationStatement (typeof (int), "x1", cboExpression);

        retMethod.Statements.Add (variableDeclaration);

        // GENERATES (C#):
        //              int x2 = (19 % 8);
        retMethod.Statements.Add (
            new CodeVariableDeclarationStatement (
            typeof (int),
            "x2",
            new CodeBinaryOperatorExpression (
            new CodePrimitiveExpression (19),
            CodeBinaryOperatorType.Modulus,
            new CodePrimitiveExpression (8))));

        // GENERATES (C#):
        //              int x3 = ((15 & 35) 
        //                          | 129);
        retMethod.Statements.Add (
            new CodeVariableDeclarationStatement (
            typeof (int),
            "x3",
            new CodeBinaryOperatorExpression (
            new CodeBinaryOperatorExpression (
            new CodePrimitiveExpression (15),
            CodeBinaryOperatorType.BitwiseAnd,
            new CodePrimitiveExpression (35)),
            CodeBinaryOperatorType.BitwiseOr,
            new CodePrimitiveExpression (129))));

        // GENERATES (C#):
        //              int x4 = 0;
        retMethod.Statements.Add (
            new CodeVariableDeclarationStatement (
            typeof (int),
            "x4",
            new CodePrimitiveExpression (0)));

        // GENERATES (C#):
        //              if (((x2 == 3) 
        //                          || (x3 < 129))) {
        //                  x4 = (x4 + 1);
        //              }
        //              else {
        //                  x4 = (x4 + 2);
        //              }

        retMethod.Statements.Add (
            new CodeConditionStatement (
            new CodeBinaryOperatorExpression (
            new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("x2"),
            CodeBinaryOperatorType.ValueEquality,
            new CodePrimitiveExpression (3)),
            CodeBinaryOperatorType.BooleanOr,
            new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("x3"),
            CodeBinaryOperatorType.LessThan,
            new CodePrimitiveExpression (129))),
            CDHelper.CreateIncrementByStatement ("x4", 1),
            CDHelper.CreateIncrementByStatement ("x4", 2)));

        // GENERATES (C#):
        //              if (((x2 > -1) 
        //                          && (x3 >= 5000))) {
        //                  x4 = (x4 + 4);
        //              }
        //              else {
        //                  x4 = (x4 + 8);
        //              }
        retMethod.Statements.Add (
            new CodeConditionStatement (
            new CodeBinaryOperatorExpression (
            new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("x2"),
            CodeBinaryOperatorType.GreaterThan,
            new CodePrimitiveExpression (-1)),
            CodeBinaryOperatorType.BooleanAnd,
            new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("x3"),
            CodeBinaryOperatorType.GreaterThanOrEqual,
            new CodePrimitiveExpression (5000))),
            CDHelper.CreateIncrementByStatement ("x4", 4),
            CDHelper.CreateIncrementByStatement ("x4", 8)));

        // GENERATES (C#):
        //              if (((x2 <= 3) 
        //                          && (x3 != 1))) {
        //                  x4 = (x4 + 16);
        //              }
        //              else {
        //                  x4 = (x4 + 32);
        //              }
        retMethod.Statements.Add (
            new CodeConditionStatement (
            new CodeBinaryOperatorExpression (
            new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("x2"),
            CodeBinaryOperatorType.LessThanOrEqual,
            new CodePrimitiveExpression (3)),
            CodeBinaryOperatorType.BooleanAnd,
            new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("x3"),
            CodeBinaryOperatorType.IdentityInequality,
            new CodePrimitiveExpression (1))),
            CDHelper.CreateIncrementByStatement ("x4", 16),
            CDHelper.CreateIncrementByStatement ("x4", 32)));


        // GENERATES (C#):
        //              return (x1 
        //                          + (x2 
        //                          + (x3 + x4)));
        //          }
        retMethod.Statements.Add (
            new CodeMethodReturnStatement (
            new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("x1"),
            CodeBinaryOperatorType.Add,
            new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("x2"),
            CodeBinaryOperatorType.Add,
            new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("x3"),
            CodeBinaryOperatorType.Add,
            new CodeVariableReferenceExpression ("x4"))))));
        class1.Members.Add (retMethod);

        //          
        // GENERATES (C#):
        //          public int SecondReturnMethod(int intInput) {
        //              // To test CodeBinaryOperatorType.IdentityEquality operator
        //              if ((((Object)(intInput)) == ((Object)(5)))) {
        //                  return 5;
        //              }
        //              else {
        //                  return 4;
        //              }
        //          }
        //      }
        AddScenario ("CheckSecondReturnMethod", "Tests identity equality.");
        retMethod = new CodeMemberMethod ();
        retMethod.Name = "SecondReturnMethod";
        retMethod.Attributes = (retMethod.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        retMethod.ReturnType = new CodeTypeReference (typeof (int));
        retMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "intInput"));

        retMethod.Statements.Add (new CodeCommentStatement ("To test CodeBinaryOperatorType.IdentiEquality operator"));
        retMethod.Statements.Add (
            new CodeConditionStatement (
            new CodeBinaryOperatorExpression (new CodeCastExpression ("Object",
            new CodeArgumentReferenceExpression ("intInput")),
            CodeBinaryOperatorType.IdentityEquality, new CodeCastExpression ("Object",
            new CodePrimitiveExpression (5))),
            new CodeStatement[] {new CodeMethodReturnStatement (new
                CodePrimitiveExpression (5))}, new CodeStatement[] {new
                CodeMethodReturnStatement (new CodePrimitiveExpression (4))}));
        class1.Members.Add (retMethod);

        // GENERATES (C#):
        //      public class Class2 : object {
        //      }
        //  }

        /*class1 = new CodeTypeDeclaration ();
        class1.Name = "Class2";
        class1.BaseTypes.Add (new CodeTypeReference (typeof (object)));
        ns.Types.Add (class1);*/
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateClass1", "Find and instantiate Class1 from the assembly.");
        if (!FindAndInstantiate ("Namespace1.Class1", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateClass1");

        // Verify Return value from function
        if (VerifyMethod (genType, genObject, "ReturnMethod", new object[] {5}, 230)) {
            VerifyScenario ("CheckReturnMethod");
        }

        if (VerifyMethod (genType, genObject, "SecondReturnMethod", new object[] {5}, 4) &&
                VerifyMethod (genType, genObject, "SecondReturnMethod", new object[] {8}, 4)) {
            VerifyScenario ("CheckSecondReturnMethod");
        }
    }
}

