using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class GoToTest : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Everett;
        }
    }

    public override string Name {
        get {
            return "GoToTest";
        }
    }

    public override string Description {
        get {
            return "Tests goto statements";
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

        if (Supports (provider, GeneratorSupport.GotoStatements))  {
            // create first method to test gotos that jump ahead to a defined label with statement
            //     GENERATE (C#):
            //            public static int FirstMethod(int i) {
            //                if ((i < 1)) {
            //                    goto comehere;
            //                }
            //                return 6;
            //            comehere:
            //                return 7;
            //            }
            AddScenario ("CheckFirstMethod");
            CodeMemberMethod cmm = new CodeMemberMethod ();
            cmm.Name = "FirstMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (typeof (int), "i");
            cmm.Parameters.Add (param);
            CodeConditionStatement condstmt = new CodeConditionStatement (new CodeBinaryOperatorExpression (
                new CodeArgumentReferenceExpression ("i"), CodeBinaryOperatorType.LessThan, new CodePrimitiveExpression (1)),
                new CodeGotoStatement ("comehere"));
            cmm.Statements.Add (condstmt);
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (6)));
            cmm.Statements.Add (new CodeLabeledStatement ("comehere",
                new CodeMethodReturnStatement (new CodePrimitiveExpression (7))));
            class1.Members.Add (cmm);

            // create second method to test gotos that jump ahead to a defined label without a statement attached to it
            //     GENERATE (C#):
            //            public static int SecondMethod(int i) {
            //                if ((i < 1)) {
            //                    goto comehere;
            //                    return 5;
            //                }
            //                return 6;
            //            comehere:
            //                return 7;
            //            }
            AddScenario ("CheckSecondMethod");
            cmm = new CodeMemberMethod ();
            cmm.Name = "SecondMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            param = new CodeParameterDeclarationExpression (typeof (int), "i");
            cmm.Parameters.Add (param);
            condstmt = new CodeConditionStatement (new CodeBinaryOperatorExpression (
                new CodeArgumentReferenceExpression ("i"), CodeBinaryOperatorType.LessThan, new CodePrimitiveExpression (1)),
                new CodeGotoStatement ("comehere"));
            cmm.Statements.Add (condstmt);
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (6)));
            cmm.Statements.Add (new CodeLabeledStatement ("comehere"));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (7)));
            class1.Members.Add (cmm);

            // create third method to test gotos that jump to a previously defined label
            //  GENERATE (C#):   
            //    public static int ThirdMethod(int i) {
            //    label:
            //        i = (i + 5);
            //        if ((i < 1)) {
            //            goto label;
            //        }
            //        return i;
            //    }
            AddScenario ("CheckThirdMethod");
            cmm = new CodeMemberMethod ();
            cmm.Name = "ThirdMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            param = new CodeParameterDeclarationExpression (typeof (int), "i");
            cmm.Parameters.Add (param);
            CodeAssignStatement assignmt = new CodeAssignStatement (new CodeArgumentReferenceExpression ("i"),
                new CodeBinaryOperatorExpression
                (new CodeArgumentReferenceExpression ("i"), CodeBinaryOperatorType.Add,
                new CodePrimitiveExpression (5)));
            cmm.Statements.Add (new CodeLabeledStatement ("label", assignmt));
            condstmt = new CodeConditionStatement (new CodeBinaryOperatorExpression (
                new CodeArgumentReferenceExpression ("i"), CodeBinaryOperatorType.LessThan, new CodePrimitiveExpression (1)),
                new CodeGotoStatement ("label"));
            cmm.Statements.Add (condstmt);
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("i")));
            class1.Members.Add (cmm);
        }
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {

        if (Supports (provider, GeneratorSupport.GotoStatements)) {
            object genObject;
            Type   genType;

            AddScenario ("InstantiateTest", "Find and instantiate Test.");
            if (!FindAndInstantiate ("NS.Test", asm, out genObject, out genType))
                return;
            VerifyScenario ("InstantiateTest");

            // verify goto which jumps ahead to label with statement 
            if (VerifyMethod (genType, genObject, "FirstMethod", new object[] {0}, 7) &&
                    VerifyMethod (genType, genObject, "FirstMethod", new object[] {2}, 6)) {
                VerifyScenario ("CheckFirstMethod");
            }

            // verify goto which jumps ahead to label without statement 
            if (VerifyMethod (genType, genObject, "SecondMethod", new object[] {0}, 7) &&
                    VerifyMethod (genType, genObject, "SecondMethod", new object[] {2}, 6)) {
                VerifyScenario ("CheckSecondMethod");
            }

            // verify goto which jumps to a previously defined label
            if (VerifyMethod (genType, genObject, "ThirdMethod", new object[] {-5}, 5) &&
                    VerifyMethod (genType, genObject, "ThirdMethod", new object[] {2}, 7)) {
                VerifyScenario ("CheckThirdMethod");
            }
        }
    }
}

