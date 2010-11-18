using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class CallMethodWDirect : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Everett;
        }
    }

    public override string Name {
        get {
            return "CallMethodWDirect";
        }
    }

    public override string Description {
        get {
            return "Call a method with a direction";
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
        CodeNamespace nspace = new CodeNamespace ("NSPC");
        nspace.Imports.Add (new CodeNamespaceImport ("System"));
        cu.Namespaces.Add (nspace);

        CodeTypeDeclaration cd = new CodeTypeDeclaration ("TEST");
        cd.IsClass = true;
        nspace.Types.Add (cd);
        if (Supports (provider, GeneratorSupport.ReferenceParameters)) {
            //    GENERATE (C#):      
            //    void Work(ref int i, out int j) {
            //             i = (i + 4);
            //             j = 5;
            //   }

            CodeMemberMethod cmm = new CodeMemberMethod ();
            cmm.Name = "Work";
            cmm.ReturnType = new CodeTypeReference ("System.Void");
            // add parameter with ref direction
            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (typeof (int), "i");
            param.Direction = FieldDirection.Ref;
            cmm.Parameters.Add (param);
            // add parameter with out direction
            param = new CodeParameterDeclarationExpression (typeof (int), "j");
            param.Direction = FieldDirection.Out;
            cmm.Parameters.Add (param);
            cmm.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("i"),
                new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"),
                CodeBinaryOperatorType.Add, new CodePrimitiveExpression (4))));
            cmm.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("j"),
                new CodePrimitiveExpression (5)));
            cd.Members.Add (cmm);

            // add a method that calls method work to verify that ref and out are working properly 
            // GENERATE (C#):
            //       public int CallingWork(int a) {
            //        a = 10;
            //        int b;
            //        Work(ref a, out b);
            //        return (a + b);
            //        }
            AddScenario ("CheckCallingWork", "Check the return value of CallingWork().");
            cmm = new CodeMemberMethod ();
            cmm.Name = "CallingWork";
            cmm.Attributes = MemberAttributes.Public;
            CodeParameterDeclarationExpression parames = new CodeParameterDeclarationExpression (typeof (int), "a");
            cmm.Parameters.Add (parames);
            cmm.ReturnType = new CodeTypeReference ("System.Int32");
            cmm.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("a"),
                new CodePrimitiveExpression (10)));
            cmm.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "b"));
            // invoke the method called "work"
            CodeMethodInvokeExpression methodinvoked = new CodeMethodInvokeExpression (new CodeMethodReferenceExpression (null,
                        "Work"));
            // add parameter with ref direction
            CodeDirectionExpression parameter = new CodeDirectionExpression (FieldDirection.Ref,
                new CodeArgumentReferenceExpression ("a"));
            methodinvoked.Parameters.Add (parameter);
            // add parameter with out direction
            parameter = new CodeDirectionExpression (FieldDirection.Out, new CodeVariableReferenceExpression ("b"));
            methodinvoked.Parameters.Add (parameter);
            cmm.Statements.Add (methodinvoked);
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression
                (new CodeArgumentReferenceExpression ("a"), CodeBinaryOperatorType.Add, new CodeVariableReferenceExpression ("b"))));
            cd.Members.Add (cmm);
        }
    }


    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        if (Supports (provider, GeneratorSupport.ReferenceParameters)) {
            AddScenario ("InstantiateTEST", "Find and instantiate CLASSNAME.");
            if (!FindAndInstantiate ("NSPC.TEST", asm, out genObject, out genType))
                return;
            VerifyScenario ("InstantiateTEST");

            // verify method return value, verify that ref and out worked accordingly
            if (VerifyMethod (genType, genObject, "CallingWork", new object[] {5}, 19)) {
                VerifyScenario ("CheckCallingWork");
            }
        }
    }
}

