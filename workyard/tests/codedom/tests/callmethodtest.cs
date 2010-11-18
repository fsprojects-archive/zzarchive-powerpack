// F#: added type casts from subtype
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class CallMethodTest : CodeDomTestTree {

		public override string Comment
		{
			get
			{
				return "F# doesn't support C# 'new' keyword";
			}
		}
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
            return "CallMethodTest";
        }
    }

    public override string Description {
        get {
            return "Tests calling methods.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {
        CodeNamespace nspace = new CodeNamespace ("NSPC");
        nspace.Imports.Add (new CodeNamespaceImport ("System"));
        cu.Namespaces.Add (nspace);

        CodeTypeDeclaration cd = new CodeTypeDeclaration ("TEST");
        cd.IsClass = true;
        nspace.Types.Add (cd);


        // GENERATES (C#):
        //        public int CallingOverrideScenario(int i) {
        //            ClassWVirtualMethod t = new ClassWOverrideMethod();
        //            return t.VirtualMethod(i);
        //        }
        AddScenario ("Check1CallingOverrideScenario", "Check an overridden method.");
        CodeMemberMethod cmm = new CodeMemberMethod ();
        cmm.Name = "CallingOverrideScenario";
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeVariableDeclarationStatement ("ClassWVirtualMethod", "t", new CodeCastExpression(new CodeTypeReference("ClassWVirtualMethod"), new CodeObjectCreateExpression ("ClassWOverrideMethod"))));
        CodeMethodInvokeExpression methodinvoke = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("t"), "VirtualMethod");
        methodinvoke.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
        cmm.Statements.Add (new CodeMethodReturnStatement (methodinvoke));
        cd.Members.Add (cmm);

        // declare a method without parameters
        cmm = new CodeMemberMethod ();
        cmm.Name = "NoParamsMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (16)));
        cd.Members.Add (cmm);

        // declare a method with multiple parameters
        cmm = new CodeMemberMethod ();
        cmm.Name = "MultipleParamsMethod";
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "b"));
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (new
            CodeArgumentReferenceExpression ("a"), CodeBinaryOperatorType.Add,
            new CodeArgumentReferenceExpression ("b"))));
        cd.Members.Add (cmm);

        // call method with no parameters, call a method with multiple parameters, 
        // and call a method from a method call
        //         public virtual int CallParamsMethods() {
        //              TEST t = new TEST();
        //              int val;
        //              val = t.NoParamsMethod ();
        //              return t.MultipleParamsMethod(78, val);
        //         }
        AddScenario ("CheckCallParamsMethod", "Check CheckCallParamsMethod.");
        cmm = new CodeMemberMethod ();
        cmm.Name = "CallParamsMethods";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference ("TEST"), "t", new CodeObjectCreateExpression ("TEST")));

        CodeVariableReferenceExpression cvre = new CodeVariableReferenceExpression (); //To increase code coverage
        cvre.VariableName = "t";

        CodeVariableReferenceExpression valCVRE = new CodeVariableReferenceExpression ();
        valCVRE.VariableName = "val";

        cmm.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "val"));
        cmm.Statements.Add (new CodeAssignStatement (valCVRE,
                    CDHelper.CreateMethodInvoke (new CodeVariableReferenceExpression ("t"), "NoParamsMethod")));

        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (cvre,
            "MultipleParamsMethod", new CodePrimitiveExpression (78), valCVRE)));
        cd.Members.Add (cmm);

        // method to test the 'new' scenario by calling the 'new' method
        // GENERATES (C#):
        //        public int CallingNewScenario(int i) {
        //            ClassWVirtualMethod t = new ClassWNewMethod();
        //            int x1;
        //            int x2;
        //            x1 = ((ClassWNewMethod)(t)).VirtualMethod(i);
        //            x2 = t.VirtualMethod(i);
        //            return (x1 - x2);
        //        }
        AddScenario ("CheckCallingNewScenario", "Check CheckCallingNewScenario.");
        cmm = new CodeMemberMethod ();
        cmm.Name = "CallingNewScenario";
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Attributes = MemberAttributes.Public;

        cmm.Statements.Add (new CodeVariableDeclarationStatement ("ClassWVirtualMethod", "t", new CodeCastExpression(new CodeTypeReference("ClassWVirtualMethod"), new CodeObjectCreateExpression ("ClassWNewMethod"))));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "x1"));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "x2"));


        methodinvoke = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("t"), "VirtualMethod");
        methodinvoke.Parameters.Add (new CodeArgumentReferenceExpression ("i"));

        CodeMethodInvokeExpression methodinvoke2 = new CodeMethodInvokeExpression (new CodeCastExpression ("ClassWNewMethod", new
            CodeVariableReferenceExpression ("t")), "VirtualMethod");
        methodinvoke2.Parameters.Add (new CodeArgumentReferenceExpression ("i"));

        cmm.Statements.Add (new CodeAssignStatement (new CodeVariableReferenceExpression ("x1"),
                    methodinvoke2));

        cmm.Statements.Add (new CodeAssignStatement (new CodeVariableReferenceExpression ("x2"),
                    methodinvoke));

        cmm.Statements.Add (new CodeMethodReturnStatement (
            CDHelper.CreateBinaryOperatorExpression ("x1", CodeBinaryOperatorType.Subtract, "x2")));

        cd.Members.Add (cmm);


        // ***************** declare method using new ******************
        // first declare a class with a virtual method in it 
        // GENERATES (C#):
        //         public class ClassWVirtualMethod {
        //                 public virtual int VirtualMethod(int a) {
        //                     return a;
        //                 }
        //         }    
        cd = new CodeTypeDeclaration ("ClassWVirtualMethod");
        cd.IsClass = true;
        nspace.Types.Add (cd);
        cmm = new CodeMemberMethod ();
        cmm.Name = "VirtualMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
        cd.Members.Add (cmm);

        // now declare a class that inherits from the previous class and has a 'new' method with the
        // name VirtualMethod
        // GENERATES (C#):
        //    public class ClassWNewMethod : ClassWVirtualMethod {
        //         public new virtual int VirtualMethod(int a) {
        //             return (2 * a);
        //         }
        //     }
        cd = new CodeTypeDeclaration ("ClassWNewMethod");
        cd.BaseTypes.Add (new CodeTypeReference ("ClassWVirtualMethod"));
        cd.IsClass = true;
        nspace.Types.Add (cd);
        cmm = new CodeMemberMethod ();
        cmm.Name = "VirtualMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.New;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
            new CodePrimitiveExpression (2), CodeBinaryOperatorType.Multiply
            , new CodeArgumentReferenceExpression ("a"))));
        cd.Members.Add (cmm);

        // now declare a class that inherits from the previous class and has a 'new' method with the
        // name VirtualMethod
        // GENERATES (C#):
        //            public class ClassWOverrideMethod : ClassWVirtualMethod {
        //                 public override int VirtualMethod(int a) {
        //                     return (2 * a);
        //                 }
        //             }
        cd = new CodeTypeDeclaration ("ClassWOverrideMethod");
        cd.BaseTypes.Add (new CodeTypeReference ("ClassWVirtualMethod"));
        cd.IsClass = true;
        nspace.Types.Add (cd);
        cmm = new CodeMemberMethod ();
        cmm.Name = "VirtualMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Override;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
            new CodePrimitiveExpression (2), CodeBinaryOperatorType.Multiply
            , new CodeArgumentReferenceExpression ("a"))));
        cd.Members.Add (cmm);

        //*************** overload member function ****************             
        // new class which will include both functions
        // GENERATES (C#):
        //    public class TEST7 {
        //         public int OverloadedMethod(int a) {
        //             return a;
        //         }
        //         public int OverloadedMethod(int a, int b) {
        //             return (b + a);
        //         }
        //         public int CallingOverloadedMethods(int i) {
        //             int one = OverloadedMethod(i, i);
        //             int two = OverloadedMethod(i);
        //             return (one - two);
        //         }
        //     }
        AddScenario ("CheckTEST7.CallingOverloadedMethods", "Check CallingOverloadedMethods()");
        cd = new CodeTypeDeclaration ("TEST7");
        cd.IsClass = true;
        nspace.Types.Add (cd);
        cmm = new CodeMemberMethod ();
        cmm.Name = "OverloadedMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
        cd.Members.Add (cmm);
        cmm = new CodeMemberMethod ();
        cmm.Name = "OverloadedMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "b"));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
            new CodeArgumentReferenceExpression ("b"), CodeBinaryOperatorType.Add,
            new CodeArgumentReferenceExpression ("a"))));
        cd.Members.Add (cmm);

        // declare a method that will call both OverloadedMethod functions
        cmm = new CodeMemberMethod ();
        cmm.Name = "CallingOverloadedMethods";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.Attributes = MemberAttributes.Public;
        CodeMethodReferenceExpression methodref = new CodeMethodReferenceExpression ();
        methodref.MethodName = "OverloadedMethod";

        cmm.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "one",
            new CodeMethodInvokeExpression (methodref, new
            CodeArgumentReferenceExpression ("i"), new CodeArgumentReferenceExpression ("i"))));

        cmm.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "two",
            new CodeMethodInvokeExpression (methodref, new
            CodeArgumentReferenceExpression ("i"))));

        cmm.Statements.Add (new CodeMethodReturnStatement (
                    CDHelper.CreateBinaryOperatorExpression ("one", CodeBinaryOperatorType.Subtract, "two")));
        cd.Members.Add (cmm);


        // GENERATES (C#):
        //
        //   namespace NSPC2 {
        //   
        //   
        //       public class TEST {
        //   
        //           public virtual int CallingOverrideScenario(int i) {
        //               NSPC.ClassWVirtualMethod t = new NSPC.ClassWOverrideMethod();
        //               return t.VirtualMethod(i);
        //           }
        //       }
        //   }
        
        nspace = new CodeNamespace ("NSPC2");
        cu.Namespaces.Add (nspace);

        cd = new CodeTypeDeclaration ("TEST");
        cd.IsClass = true;
        nspace.Types.Add (cd);

        AddScenario ("Check2CallingOverrideScenario", "Check CallingOverrideScenario()");
        cmm = new CodeMemberMethod ();
        cmm.Name = "CallingOverrideScenario";
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeVariableDeclarationStatement ("NSPC.ClassWVirtualMethod", "t", new CodeCastExpression(new CodeTypeReference("NSPC.ClassWVirtualMethod"), new CodeObjectCreateExpression ("NSPC.ClassWOverrideMethod"))));
        methodinvoke = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("t"), "VirtualMethod");
        methodinvoke.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
        cmm.Statements.Add (new CodeMethodReturnStatement (methodinvoke));
        cd.Members.Add (cmm);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object otest;
        object otest7;
        object onspc2test;
        Type   ttest;
        Type   ttest7;
        Type   tnspc2test;

        AddScenario ("InstantiateTEST", "Find and instantiate TEST.");
        if (!FindAndInstantiate ("NSPC.TEST", asm, out otest, out ttest))
            return;
        VerifyScenario ("InstantiateTEST");

        AddScenario ("InstantiateTEST7", "Find and instantiate TEST.");
        if (!FindAndInstantiate ("NSPC.TEST7", asm, out otest7, out ttest7))
            return;
        VerifyScenario ("InstantiateTEST7");

        AddScenario ("InstantiateNSPC2.TEST", "Find and instantiate TEST.");
        if (!FindAndInstantiate ("NSPC2.TEST", asm, out onspc2test, out tnspc2test))
            return;
        VerifyScenario ("InstantiateNSPC2.TEST");

        if (VerifyMethod (ttest, otest, "CallingOverrideScenario", new object[] {2}, 4)) {
            VerifyScenario ("Check1CallingOverrideScenario");
        }
        if (VerifyMethod (tnspc2test, onspc2test, "CallingOverrideScenario", new object[] {3}, 6)) {
            VerifyScenario ("Check2CallingOverrideScenario");
        }
        if (VerifyMethod (ttest, otest, "CallParamsMethods", null, 94)) {
            VerifyScenario ("CheckCallParamsMethod");
        }
        if (VerifyMethod (ttest7, otest7, "CallingOverloadedMethods", new object[] {22}, 22)) {
            VerifyScenario ("CheckTEST7.CallingOverloadedMethods");
        }
        if (VerifyMethod (ttest, otest, "CallingNewScenario", new object[] {5}, 5)) {
            VerifyScenario ("CheckCallingNewScenario");
        }
    }
}

