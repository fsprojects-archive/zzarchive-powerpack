using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class SubsetFieldsTest : CodeDomTestTree {

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
            return "SubsetFieldsTest";
        }
    }

    public override string Description {
        get {
            return "Subset compatible test of calling fields.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        CodeNamespace nspace = new CodeNamespace ("NSPC");
        nspace.Imports.Add (new CodeNamespaceImport ("System"));
        cu.Namespaces.Add (nspace);


        // declare class with fields

        // GENERATES (C#):
        //    public class ClassWithFields {
        //        public int NonStaticPublicField = 6;
        //        private int PrivateField = 7;
        //        public int UsePrivateField(int i) {
        //            this.PrivateField = i;
        //            return this.PrivateField;
        //        }
        //    }
        CodeTypeDeclaration cd = new CodeTypeDeclaration ("ClassWithFields");
        cd.IsClass = true;
        nspace.Types.Add (cd);

        CodeMemberField field = new CodeMemberField ();
        field.Name = "NonStaticPublicField";
        field.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        field.Type = new CodeTypeReference (typeof (int));
        field.InitExpression = new CodePrimitiveExpression (6);
        cd.Members.Add (field);

        field = new CodeMemberField ();
        field.Name = "PrivateField";
        field.Attributes = MemberAttributes.Private | MemberAttributes.Final;
        field.Type = new CodeTypeReference (typeof (int));
        field.InitExpression = new CodePrimitiveExpression (7);
        cd.Members.Add (field);

        // create a method to test access to private field
        CodeMemberMethod cmm = new CodeMemberMethod ();
        cmm.Name = "UsePrivateField";
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
        cmm.Statements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (new CodeThisReferenceExpression (), "PrivateField"), new CodeArgumentReferenceExpression ("i")));
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (new CodeThisReferenceExpression (), "PrivateField")));
        cd.Members.Add (cmm);

        // GENERATES (C#):
        //    public class TestFields {
        //        public int UseFields(int i) {
        //            ClassWithFields number = new ClassWithFields();
        //            int someSum;
        //            int privateField;
        //            someSum = number.NonStaticPublicField;
        //            privateField = number.UsePrivateField (i);
        //            return someSum + privateField;
        //        }
        //    }
        cd = new CodeTypeDeclaration ("TestFields");
        cd.IsClass = true;
        nspace.Types.Add (cd);

        AddScenario ("CheckTestFields");
        cmm = new CodeMemberMethod ();
        cmm.Name = "UseFields";
        cmm.Attributes = MemberAttributes.Public;
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference ("ClassWithFields"), "number",
                    new CodeObjectCreateExpression ("ClassWithFields")));

        cmm.Statements.Add (new CodeVariableDeclarationStatement (typeof (int),
                    "someSum"));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (typeof (int),
                    "privateField"));

        cmm.Statements.Add (new CodeAssignStatement (new CodeVariableReferenceExpression ("someSum"),
                    CDHelper.CreateFieldRef ("number", "NonStaticPublicField")));

        cmm.Statements.Add (new CodeAssignStatement (new CodeVariableReferenceExpression ("privateField"),
                    CDHelper.CreateMethodInvoke (new CodeVariableReferenceExpression ("number"),
                        "UsePrivateField", new CodeArgumentReferenceExpression ("i"))));

        cmm.Statements.Add (new CodeMethodReturnStatement (CDHelper.CreateBinaryOperatorExpression ("someSum",
                        CodeBinaryOperatorType.Add, "privateField")));
        cd.Members.Add (cmm);


    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateTestFields", "Find and instantiate TestFields.");
        if (!FindAndInstantiate ("NSPC.TestFields", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTestFields");

        // verify method return value for method which references public, static field
        if (VerifyMethod (genType, genObject, "UseFields", new object[]{3}, 9))
            VerifyScenario ("CheckTestFields");
    }
}

