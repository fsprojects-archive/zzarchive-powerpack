using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class NamespaceTest : CodeDomTestTree {

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
            return "NamespaceTest";
        }
    }

    public override string Description {
        get {
            return "Tests namespaces.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        // GENERATES (C#):
        //  namespace NS {
        //  using System;
        //        public class Test {  
        //            public int NonStaticPublicField = 5;
        //        }
        //        public class Test2 {
        //            public int NonStaticPublicField = 5;
        //        }
        //    }  
        CodeNamespace ns = new CodeNamespace ("NS");
        ns.Imports.Add (new CodeNamespaceImport ("System"));
        cu.Namespaces.Add (ns);

        // create a class
        CodeTypeDeclaration class1 = new CodeTypeDeclaration ();
        class1.Name = "Test";
        class1.IsClass = true;
        ns.Types.Add (class1);

        CodeMemberField field = new CodeMemberField ();
        field.Name = "NonStaticPublicField";
        field.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        field.Type = new CodeTypeReference (typeof (int));
        field.InitExpression = new CodePrimitiveExpression (5);
        class1.Members.Add (field);

        class1 = new CodeTypeDeclaration ("Test2");
        class1.IsClass = true;
        ns.Types.Add (class1);

        field = new CodeMemberField ();
        field.Name = "NonStaticPublicField";
        field.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        field.Type = new CodeTypeReference (typeof (int));
        field.InitExpression = new CodePrimitiveExpression (5);
        class1.Members.Add (field);

        // GENERATES (C#):
        //    namespace NS2 {
        //        public class Test1 {   
        //            public static int TestingMethod(int i) {
        //                NS.Test temp1 = new NS.Test();
        //                NS.Test2 temp2 = new NS.Test2();
        //                temp1.NonStaticPublicField = i;
        //                temp2.NonStaticPublicField = (i * 10);
        //                int sum;
        //                sum = (temp1.NonStaticPublicField + temp2.NonStaticPublicField);
        //                return sum;
        //            }
        //        }
        //    }
        AddScenario ("CheckTestingMethod");
        ns = new CodeNamespace ("NS2");
        cu.Namespaces.Add (ns);

        class1 = new CodeTypeDeclaration ("Test1");
        class1.IsClass = true;
        ns.Types.Add (class1);

        CodeMemberMethod cmm = new CodeMemberMethod ();
        cmm.Name = "TestingMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        cmm.Statements.Add (new CodeVariableDeclarationStatement ("NS.Test", "temp1", new CodeObjectCreateExpression ("NS.Test")));
        cmm.Statements.Add (new CodeVariableDeclarationStatement ("NS.Test2", "temp2", new CodeObjectCreateExpression ("NS.Test2")));
        cmm.Statements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (new CodeVariableReferenceExpression ("temp1"), "NonStaticPublicField")
            , new CodeArgumentReferenceExpression ("i")));
        cmm.Statements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (new CodeVariableReferenceExpression ("temp2"), "NonStaticPublicField")
            , new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"), CodeBinaryOperatorType.Multiply, new CodePrimitiveExpression (10))));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "sum"));
        cmm.Statements.Add (new CodeAssignStatement (new CodeVariableReferenceExpression ("sum"),
            new CodeBinaryOperatorExpression (new CodeFieldReferenceExpression (new CodeVariableReferenceExpression ("temp1"), "NonStaticPublicField"),
            CodeBinaryOperatorType.Add, new CodeFieldReferenceExpression (new CodeVariableReferenceExpression ("temp2"), "NonStaticPublicField"))));
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("sum")));
        class1.Members.Add (cmm);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateTest1", "Find and instantiate Test1.");
        if (!FindAndInstantiate ("NS2.Test1", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTest1");

        if (VerifyMethod (genType, genObject, "TestingMethod", new object[] {12}, 132))
            VerifyScenario ("CheckTestingMethod");
    }
}

