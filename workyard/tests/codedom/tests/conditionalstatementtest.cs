using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class ConditionalStatementTest : CodeDomTestTree {

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
            return "ConditionalStatementTest";
        }
    }

    public override string Description {
        get {
            return "Tests conditional statements.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        // GENERATES (C#):
        //
        //  namespace NSPC {
        //
        //      public class ClassWithMethod {
        //          
        //          public static int ReturnMethod(int intInput) {
        //              if (((intInput <= 3) 
        //                          && (intInput == 2))) {
        //                  intInput = (intInput + 16);
        //              }
        //              else {
        //                  intInput = (intInput + 1);
        //              }
        //              if ((intInput <= 10)) {
        //                  intInput = (intInput + 11);
        //              }
        //              return intInput;
        //          }
        //      }
        //  }

        AddScenario ("CheckReturnMethod1", "Check return value of ReturnMethod()");
        AddScenario ("CheckReturnMethod2", "Check return value of ReturnMethod()");
        CodeNamespace nspace = new CodeNamespace ("NSPC");
        cu.Namespaces.Add (nspace);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration ("ClassWithMethod");
        class1.IsClass = true;
        nspace.Types.Add (class1);

        CodeMemberMethod retMethod = new CodeMemberMethod ();
        retMethod.Name = "ReturnMethod";
        retMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        retMethod.ReturnType = new CodeTypeReference (typeof (int));
        retMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "intInput"));
        retMethod.Statements.Add (
            new CodeConditionStatement (
            new CodeBinaryOperatorExpression (
            new CodeBinaryOperatorExpression (
            new CodeArgumentReferenceExpression ("intInput"),
            CodeBinaryOperatorType.LessThanOrEqual,
            new CodePrimitiveExpression (3)),
            CodeBinaryOperatorType.BooleanAnd,
            new CodeBinaryOperatorExpression (
            new CodeArgumentReferenceExpression ("intInput"),
            CodeBinaryOperatorType.ValueEquality,
            new CodePrimitiveExpression (2))),
            CDHelper.CreateIncrementByStatement (new CodeArgumentReferenceExpression ("intInput"), 16),
            CDHelper.CreateIncrementByStatement (new CodeArgumentReferenceExpression ("intInput"), 1)));
        retMethod.Statements.Add (new CodeConditionStatement (
            new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("intInput"), CodeBinaryOperatorType.LessThanOrEqual,
            new CodePrimitiveExpression (10)),
            new CodeAssignStatement (new CodeArgumentReferenceExpression ("intInput"),
            new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("intInput"),
            CodeBinaryOperatorType.Add, new CodePrimitiveExpression (11)))));
        retMethod.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("intInput")));
        class1.Members.Add (retMethod);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;
        
        AddScenario ("InstantiateClassWithMethod", "Find and instantiate ClassWithMethod.");
        if (!FindAndInstantiate ("NSPC.ClassWithMethod", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateClassWithMethod");

        // Verify Return value from function
        if (VerifyMethod (genType, genObject, "ReturnMethod", new object[] {2}, 19)) {
            VerifyScenario ("CheckReturnMethod1");
        }
        if (VerifyMethod (genType, genObject, "ReturnMethod", new object[] {1}, 12)) {
            VerifyScenario ("CheckReturnMethod2");
        }
    }
}

