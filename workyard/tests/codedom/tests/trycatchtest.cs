using System;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class TryCatchTest : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Everett;
        }
    }

    public override bool ShouldVerify {
        get {
            return true;
        }
    }

    public override bool ShouldCompile {
        get {
            return true;
        }
    }


    public override string Name {
        get {
            return "TryCatchTest";
        }
    }

    public override string Description {
        get {
            return "Tests try/catch/finally statements.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {
        // create a namespace
        CodeNamespace ns = new CodeNamespace ("NS");
        ns.Imports.Add (new CodeNamespaceImport ("System"));
        cu.Namespaces.Add (ns);

        // create a class
        CodeTypeDeclaration class1 = new CodeTypeDeclaration ();
        class1.Name = "Test";
        class1.IsClass = true;
        ns.Types.Add (class1);

        if (Supports (provider, GeneratorSupport.TryCatchStatements)) {

            // try catch statement with just finally
            // GENERATE (C#):
            //       public static int FirstScenario(int a) {
            //            try {
            //            }
            //            finally {
            //                a = (a + 5);
            //            }
            //            return a;
            //        }
            AddScenario ("CheckFirstScenario");
            CodeMemberMethod cmm = new CodeMemberMethod ();
            cmm.Name = "FirstScenario";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (typeof (int), "a");
            cmm.Parameters.Add (param);

            CodeTryCatchFinallyStatement tcfstmt = new CodeTryCatchFinallyStatement ();
            tcfstmt.FinallyStatements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("a"), new
                CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("a"), CodeBinaryOperatorType.Add,
                new CodePrimitiveExpression (5))));
            cmm.Statements.Add (tcfstmt);
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
            class1.Members.Add (cmm);

            // in VB (a = a/a) generates an warning if a is integer. Cast the expression just for VB language. 
            CodeBinaryOperatorExpression cboExpression   = new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("a"), CodeBinaryOperatorType.Divide, new CodeArgumentReferenceExpression ("a"));
            CodeAssignStatement          assignStatement = null;
            if (provider is Microsoft.VisualBasic.VBCodeProvider)
                assignStatement = new CodeAssignStatement (new CodeArgumentReferenceExpression ("a"), new CodeCastExpression (typeof (int), cboExpression));
            else
                assignStatement = new CodeAssignStatement (new CodeArgumentReferenceExpression ("a"), cboExpression);

            // try catch statement with just catch
            // GENERATE (C#):
            //        public static int SecondScenario(int a, string exceptionMessage) {
            //            try {
            //                a = (a / a);
            //            }
            //            catch (System.Exception e) {
            //                a = 3;
            //                exceptionMessage = e.ToString();
            //            }
            //            finally {
            //                a = (a + 1);
            //            }
            //            return a;
            //        }
            AddScenario ("CheckSecondScenario");
            cmm = new CodeMemberMethod ();
            cmm.Name = "SecondScenario";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            param = new CodeParameterDeclarationExpression (typeof (int), "a");
            cmm.Parameters.Add (param);
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (String), "exceptionMessage"));

            tcfstmt = new CodeTryCatchFinallyStatement ();
            CodeCatchClause catchClause = new CodeCatchClause ("e");
            tcfstmt.TryStatements.Add (assignStatement);
            catchClause.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("a"),
                new CodePrimitiveExpression (3)));
            catchClause.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("exceptionMessage"),
                new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("e"), "ToString")));
            tcfstmt.CatchClauses.Add (catchClause);
            tcfstmt.FinallyStatements.Add (CDHelper.CreateIncrementByStatement (new CodeArgumentReferenceExpression ("a"), 1));

            cmm.Statements.Add (tcfstmt);
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));

            class1.Members.Add (cmm);

            // try catch statement with multiple catches
            // GENERATE (C#):
            //        public static int ThirdScenario(int a, string exceptionMessage) {
            //            try {
            //                a = (a / a);
            //            }
            //            catch (System.ArgumentNullException e) {
            //                a = 10;
            //                exceptionMessage = e.ToString();
            //            }
            //            catch (System.DivideByZeroException f) {
            //                exceptionMessage = f.ToString();
            //                a = 9;
            //            }
            //            return a;
            //        }
            AddScenario ("CheckThirdScenario");
            cmm = new CodeMemberMethod ();
            cmm.Name = "ThirdScenario";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            param = new CodeParameterDeclarationExpression (typeof (int), "a");
            cmm.Parameters.Add (param);
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (String), "exceptionMessage"));

            tcfstmt = new CodeTryCatchFinallyStatement ();
            catchClause = new CodeCatchClause ("e", new CodeTypeReference (typeof (ArgumentNullException)));
            tcfstmt.TryStatements.Add (assignStatement);
            catchClause.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("a"),
                new CodePrimitiveExpression (9)));
            catchClause.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("exceptionMessage"),
                new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("e"), "ToString")));
            tcfstmt.CatchClauses.Add (catchClause);

            // add a second catch clause
            catchClause = new CodeCatchClause ("f", new CodeTypeReference (typeof (Exception)));
            catchClause.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("exceptionMessage"),
                new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("f"), "ToString")));
            catchClause.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("a"),
                new CodePrimitiveExpression (9)));
            tcfstmt.CatchClauses.Add (catchClause);

            cmm.Statements.Add (tcfstmt);
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
            class1.Members.Add (cmm);

            // catch throws exception
            // GENERATE (C#):
            //        public static int FourthScenario(int a) {
            //            try {
            //                a = (a / a);
            //            }
            //            catch (System.Exception e) {
            //                // Error handling
            //                throw e;
            //            }
            //            return a;
            //        }
            AddScenario ("CheckFourthScenario");
            cmm = new CodeMemberMethod ();
            cmm.Name = "FourthScenario";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            param = new CodeParameterDeclarationExpression (typeof (int), "a");
            cmm.Parameters.Add (param);

            tcfstmt = new CodeTryCatchFinallyStatement ();
            catchClause = new CodeCatchClause ("e");
            tcfstmt.TryStatements.Add (assignStatement);
            catchClause.Statements.Add (new CodeCommentStatement ("Error handling"));
            catchClause.Statements.Add (new CodeThrowExceptionStatement (new CodeArgumentReferenceExpression ("e")));
            tcfstmt.CatchClauses.Add (catchClause);
            cmm.Statements.Add (tcfstmt);
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
            class1.Members.Add (cmm);
        }
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        if (Supports (provider, GeneratorSupport.TryCatchStatements)) {
            object genObject;
            Type   genType;

            AddScenario ("InstantiateTest", "Find and instantiate Test.");
            if (!FindAndInstantiate ("NS.Test", asm, out genObject, out genType))
                return;
            VerifyScenario ("InstantiateTest");

            // verify method return value
            // verifying first scenario 
            if (VerifyMethod (genType, genObject, "FirstScenario", new object[]{1}, 6) &&
                    VerifyMethod (genType, genObject, "FirstScenario", new object[]{10}, 15)) {
                VerifyScenario ("CheckFirstScenario");
            }

            // verify second scenario return method
            if (VerifyMethod (genType, genObject, "SecondScenario", new object[]{0, "hi"}, 4) &&
                    VerifyMethod (genType, genObject, "SecondScenario", new object[]{10, "hi"}, 2)) {
                VerifyScenario ("CheckSecondScenario");
            }

            // verify third scenario return method
            if (VerifyMethod (genType, genObject, "ThirdScenario", new object[]{0, "hi"}, 9)) {
                VerifyScenario ("CheckThirdScenario");
            }

            // verify fourth scenario return method
            if (VerifyException (genType, genObject, "FourthScenario", new object[]{0},
                        new System.Reflection.TargetInvocationException (new DivideByZeroException ())) &&
                    VerifyMethod (genType, genObject, "FourthScenario", new object[]{10}, 1)) {
                VerifyScenario ("CheckFourthScenario");
            }
        }
    }
}

