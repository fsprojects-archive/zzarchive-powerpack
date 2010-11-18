using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class CallingFieldTest : CodeDomTestTree {

    public override string Comment
    {
        get
        {
            return "Static Fields with an InitExpression are Not Initialized Correctly";
        }
    }
    public override TestTypes TestType
    {
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
            return "CallingFieldTest";
        }
    }

    public override string Description {
        get {
            return "Tests calling fields.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        CodeNamespace nspace = new CodeNamespace ("NSPC");
        nspace.Imports.Add (new CodeNamespaceImport ("System"));
        cu.Namespaces.Add (nspace);


        // declare class with fields

        // GENERATES (C#):
        //    public class ClassWithFields {
        //        // only if provider supports GeneratorSupport.PublicStaticMembers
        //        public static string Microsoft = "hi";
        //        public static int StaticPublicField = 5;
        //        // ---
        //
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

        CodeMemberField field;
        if (Supports (provider, GeneratorSupport.PublicStaticMembers)) {

            field = new CodeMemberField ("System.String", "Microsoft");
            field.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            field.InitExpression = new CodePrimitiveExpression ("hi");
            cd.Members.Add (field);

            field = new CodeMemberField ();
            field.Name = "StaticPublicField";
            field.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            field.Type = new CodeTypeReference (typeof (int));
            field.InitExpression = new CodePrimitiveExpression (5);
            cd.Members.Add (field);
        }

        field = new CodeMemberField ();
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
        //        public static int UseFields(int i) {
        //            ClassWithFields number = new ClassWithFields();
        //            return ((number.NonStaticPublicField + number.UsePrivateField(i)) 
        //                        + ClassWithFields.StaticPublicField); // <-- only if supported
        //        }
        //    }
        cd = new CodeTypeDeclaration ("TestFields");
        cd.IsClass = true;
        nspace.Types.Add (cd);

        AddScenario ("CheckUseFields");
        cmm = new CodeMemberMethod ();
        cmm.Name = "UseFields";
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference ("ClassWithFields"), "number", new CodeObjectCreateExpression ("ClassWithFields")));
        CodeBinaryOperatorExpression binaryOpExpression = new CodeBinaryOperatorExpression (new CodeFieldReferenceExpression
            (new CodeVariableReferenceExpression ("number"), "NonStaticPublicField"), CodeBinaryOperatorType.Add,
            new CodeMethodInvokeExpression (
            new CodeVariableReferenceExpression ("number"), "UsePrivateField", new CodeArgumentReferenceExpression ("i")));

        if (Supports (provider, GeneratorSupport.PublicStaticMembers))
            binaryOpExpression = new CodeBinaryOperatorExpression (binaryOpExpression,
                    CodeBinaryOperatorType.Add, new CodeFieldReferenceExpression (
                        new CodeTypeReferenceExpression ("ClassWithFields"), "StaticPublicField"));

        cmm.Statements.Add (new CodeMethodReturnStatement (binaryOpExpression));
        cd.Members.Add (cmm);


    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateTestFields", "Find and instantiate TestFields class.");
        if (!FindAndInstantiate("NSPC.TestFields", asm, out genObject, out genType)) // F#: edit .. no nested classes
            return;
        VerifyScenario ("InstantiateTestFields");

        if (Supports (provider, GeneratorSupport.PublicStaticMembers)) {
            if (VerifyMethod (genType, genObject, "UseFields", new object[]{3}, 14))
                VerifyScenario ("CheckUseFields");
        } else {
            if (VerifyMethod (genType, genObject, "UseFields", new object[]{3}, 9))
                VerifyScenario ("CheckUseFields");
        }
    }
}

