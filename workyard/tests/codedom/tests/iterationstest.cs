using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class IterationsTest : CodeDomTestTree {

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
            return "IterationsTest";
        }
    }

    public override string Description {
        get {
            return "Tests iterations.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        // GENERATES (C#):
        //
        //  namespace NSPC {
        //      
        //      
        //      public class ClassWithMethod {
        //          
        //          public int TestBasicIterationStatement() {
        //              int i;
        //              for (i = 1; (i < 8); i = (i * 2)) {
        //              }
        //              return i;
        //          }
        //          
        //          public int TestComplexIterationStatement() {
        //              int i;
        //              int a = 7;
        //              int b;
        //              int c = 9;
        //              int d = 2;
        //              for (i = 0; (i < 2); i = (i + 1)) {
        //                  if ((a < 16)) {
        //                      for (b = 0; (b < 2); b = (b + 1)) {
        //                          if ((c < 10)) {
        //                              d = (d - 1);
        //                          }
        //                          d = (d * 2);
        //                      }
        //                  }
        //              }
        //              return d;
        //          }
        //      }
        //  }

        CodeNamespace nspace = new CodeNamespace ("NSPC");
        cu.Namespaces.Add (nspace);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration ("ClassWithMethod");
        class1.IsClass = true;
        nspace.Types.Add (class1);

        AddScenario ("CheckTestBasicIterationStatement");
        CodeMemberMethod cmm = new CodeMemberMethod ();
        cmm.Name = "TestBasicIterationStatement";
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference (typeof (int)), "i"));
        cmm.Statements.Add (new CodeIterationStatement (new CodeAssignStatement (new
            CodeVariableReferenceExpression ("i"), new CodePrimitiveExpression (1)),
            new CodeBinaryOperatorExpression (new CodeVariableReferenceExpression ("i"),
            CodeBinaryOperatorType.LessThan, new CodePrimitiveExpression (8)),
            new CodeAssignStatement (new CodeVariableReferenceExpression ("i"),
            new CodeBinaryOperatorExpression (new CodeVariableReferenceExpression ("i"), CodeBinaryOperatorType.Multiply,
            new CodePrimitiveExpression (2)))));
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("i")));
        class1.Members.Add (cmm);

        AddScenario ("CheckTestComplexIterationStatement");
        cmm = new CodeMemberMethod ();
        cmm.Name = "TestComplexIterationStatement";
        cmm.Attributes = MemberAttributes.Public;
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference (typeof (int)), "i"));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference (typeof (int)), "a", new CodePrimitiveExpression (7)));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference (typeof (int)), "b"));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference (typeof (int)), "c", new CodePrimitiveExpression (9)));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference (typeof (int)), "d", new CodePrimitiveExpression (2)));
        CodeIterationStatement iteration = new CodeIterationStatement ();
        iteration.IncrementStatement = new CodeAssignStatement (new CodeVariableReferenceExpression ("i")
            , new CodeBinaryOperatorExpression (new CodeVariableReferenceExpression ("i"), CodeBinaryOperatorType.Add,
            new CodePrimitiveExpression (1)));
        iteration.InitStatement = new CodeAssignStatement (new CodeVariableReferenceExpression ("i"), new
            CodePrimitiveExpression (0));
        iteration.TestExpression = (new CodeBinaryOperatorExpression (new CodeVariableReferenceExpression ("i"),
            CodeBinaryOperatorType.LessThan, new CodePrimitiveExpression (2)));
        CodeConditionStatement secondIf = new CodeConditionStatement (new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("c"), CodeBinaryOperatorType.LessThan, new CodePrimitiveExpression (10)),
            new CodeAssignStatement (new CodeVariableReferenceExpression ("d"), new CodeBinaryOperatorExpression (
            new CodeVariableReferenceExpression ("d"), CodeBinaryOperatorType.Subtract, new CodePrimitiveExpression (1))));

        CodeIterationStatement secondFor = new CodeIterationStatement ();
        secondFor.Statements.Add (secondIf);
        secondFor.IncrementStatement = new CodeAssignStatement (new CodeVariableReferenceExpression ("b")
            , new CodeBinaryOperatorExpression (new CodeVariableReferenceExpression ("b"), CodeBinaryOperatorType.Add,
            new CodePrimitiveExpression (1)));
        secondFor.InitStatement = new CodeAssignStatement (new CodeVariableReferenceExpression ("b"), new
            CodePrimitiveExpression (0));
        secondFor.TestExpression = (new CodeBinaryOperatorExpression (new CodeVariableReferenceExpression ("b"),
            CodeBinaryOperatorType.LessThan, new CodePrimitiveExpression (2)));
        secondFor.Statements.Add (new CodeAssignStatement (new CodeVariableReferenceExpression ("d"),
            new CodeBinaryOperatorExpression (new CodeVariableReferenceExpression ("d"), CodeBinaryOperatorType.Multiply,
            new CodePrimitiveExpression (2))));

        CodeConditionStatement firstIf = new CodeConditionStatement ();
        firstIf.Condition = new CodeBinaryOperatorExpression (new CodeVariableReferenceExpression ("a"), CodeBinaryOperatorType.LessThan,
            new CodePrimitiveExpression (16));
        firstIf.TrueStatements.Add (secondFor);


        iteration.Statements.Add (firstIf);
        cmm.Statements.Add (iteration);
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("d")));
        class1.Members.Add (cmm);

    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateClassWithMethod", "Find and instantiate ClassWithMethod.");
        if (!FindAndInstantiate ("NSPC.ClassWithMethod", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateClassWithMethod");


        if (VerifyMethod (genType, genObject, "TestBasicIterationStatement", new object[] {}, 8)) {
            VerifyScenario ("CheckTestBasicIterationStatement");
        }
        if (VerifyMethod (genType, genObject, "TestComplexIterationStatement", new object[] {}, 2)) {
            VerifyScenario ("CheckTestComplexIterationStatement");
        }
    }
}

