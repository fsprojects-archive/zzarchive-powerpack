// F#: * We never generate a real field, so all the tests are modified to check for the right property 
//     * Fixed missing codetypereference in access to static fields
//     * Also accessing protected static fields without "CodeTypeReference" is not correct in F#
//     * Reorganized class order 
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.VisualBasic;
using Microsoft.JScript;

public class DeclareField : CodeDomTestTree {

    public override string Comment
    {
        get { return "public static fields not allowed in F#"; }
    }		

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
            return "DeclareField";
        }
    }

    public override string Description {
        get {
            return "Tests declarations of fields.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        CodeNamespace nspace = new CodeNamespace("NSPC");
        nspace.Imports.Add(new CodeNamespaceImport("System"));
        cu.Namespaces.Add(nspace);

        CodeTypeDeclaration cd = new CodeTypeDeclaration("TestFields");
        cd.IsClass = true;
        nspace.Types.Add(cd);

        // GENERATES (C#):
        //        public static int UseStaticPublicField(int i) {
        //            ClassWithFields.StaticPublicField = i;
        //            return ClassWithFields.StaticPublicField;
        //
        CodeMemberMethod cmm;
        if (Supports(provider, GeneratorSupport.PublicStaticMembers))
        {
          AddScenario("CheckUseStaticPublicField");
          cmm = new CodeMemberMethod();
          cmm.Name = "UseStaticPublicField";
          cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
          cmm.ReturnType = new CodeTypeReference(typeof(int));
          cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(int)), "i"));
          cmm.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(
              new CodeTypeReferenceExpression("ClassWithFields"), "StaticPublicField"),
              new CodeArgumentReferenceExpression("i")));
          cmm.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new
              CodeTypeReferenceExpression("ClassWithFields"), "StaticPublicField")));
          cd.Members.Add(cmm);
        }

        // GENERATES (C#):
        //        public static int UseNonStaticPublicField(int i) {
        //            ClassWithFields variable = new ClassWithFields();
        //            variable.NonStaticPublicField = i;
        //            return variable.NonStaticPublicField;
        //        }
        AddScenario("CheckUseNonStaticPublicField");
        cmm = new CodeMemberMethod();
        cmm.Name = "UseNonStaticPublicField";
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        cmm.ReturnType = new CodeTypeReference(typeof(int));
        cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(int)), "i"));
        cmm.Statements.Add(new CodeVariableDeclarationStatement("ClassWithFields", "variable", new CodeObjectCreateExpression("ClassWithFields")));
        cmm.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(
            new CodeVariableReferenceExpression("variable"), "NonStaticPublicField"),
            new CodeArgumentReferenceExpression("i")));
        cmm.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new
            CodeVariableReferenceExpression("variable"), "NonStaticPublicField")));
        cd.Members.Add(cmm);

        // GENERATES (C#):
        //        public static int UseNonStaticInternalField(int i) {
        //            ClassWithFields variable = new ClassWithFields();
        //            variable.NonStaticInternalField = i;
        //            return variable.NonStaticInternalField;
        //        }          

        if (!(provider is JScriptCodeProvider) && !(provider is VBCodeProvider))
        {
          cmm = new CodeMemberMethod();
          AddScenario("CheckUseNonStaticInternalField");
          cmm.Name = "UseNonStaticInternalField";
          cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
          cmm.ReturnType = new CodeTypeReference(typeof(int));
          cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(int)), "i"));
          cmm.Statements.Add(new CodeVariableDeclarationStatement("ClassWithFields", "variable", new CodeObjectCreateExpression("ClassWithFields")));
          cmm.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(
              new CodeVariableReferenceExpression("variable"), "NonStaticInternalField"),
              new CodeArgumentReferenceExpression("i")));
          cmm.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new
              CodeVariableReferenceExpression("variable"), "NonStaticInternalField")));
          cd.Members.Add(cmm);
        }

        // GENERATES (C#):
        //        public static int UseInternalField(int i) {
        //            ClassWithFields.InternalField = i;
        //            return ClassWithFields.InternalField;
        //        }
        if (!(provider is JScriptCodeProvider) && !(provider is VBCodeProvider))
        {
          AddScenario("CheckUseInternalField");
          cmm = new CodeMemberMethod();
          cmm.Name = "UseInternalField";
          cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
          cmm.ReturnType = new CodeTypeReference(typeof(int));
          cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(int)), "i"));
          cmm.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new
              CodeTypeReferenceExpression("ClassWithFields"), "InternalField"),
              new CodeArgumentReferenceExpression("i")));
          cmm.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new
              CodeTypeReferenceExpression("ClassWithFields"), "InternalField")));
          cd.Members.Add(cmm);
        }

        // GENERATES (C#):
        //       public static int UseConstantField(int i) {
        //            return ClassWithFields.ConstantField;
        //        }
        AddScenario("CheckUseConstantField");
        cmm = new CodeMemberMethod();
        cmm.Name = "UseConstantField";
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        cmm.ReturnType = new CodeTypeReference(typeof(int));
        cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(int)), "i"));
        cmm.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new
            CodeTypeReferenceExpression("ClassWithFields"), "ConstantField")));
        cd.Members.Add(cmm);

        // code a new class to test that the protected field can be accessed by classes that inherit it
        // GENERATES (C#):
        //    public class TestProtectedField : ClassWithFields {
        //        public static int UseProtectedField(int i) {
        //            ProtectedField = i;
        //            return ProtectedField;
        //        }
        //    }
        cd = new CodeTypeDeclaration("TestProtectedField");
        cd.BaseTypes.Add(new CodeTypeReference("ClassWithFields"));
        cd.IsClass = true;
        nspace.Types.Add(cd);

        cmm = new CodeMemberMethod();
        AddScenario("CheckUseProtectedField");
        cmm.Name = "UseProtectedField";
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        cmm.ReturnType = new CodeTypeReference(typeof(int));
        cmm.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(int)), "i"));
        CodeFieldReferenceExpression reference = new CodeFieldReferenceExpression();
        reference.FieldName = "ProtectedField";
        reference.TargetObject = new CodeTypeReferenceExpression("ClassWithFields");
        cmm.Statements.Add(new CodeAssignStatement(reference,
            new CodeArgumentReferenceExpression("i")));
        cmm.Statements.Add(new CodeMethodReturnStatement(reference));
        cd.Members.Add(cmm);


        // declare class with fields
        //  GENERATES (C#):
        //            public class ClassWithFields {
        //                public static int StaticPublicField = 5;
        //                /*FamANDAssem*/ internal static int InternalField = 0;
        //                public const int ConstantField = 0;
        //                protected static int ProtectedField = 0;
        //                private static int PrivateField = 5;
        //                public int NonStaticPublicField = 5;
        //                /*FamANDAssem*/ internal int NonStaticInternalField = 0;
        //                public static int UsePrivateField(int i) {
        //                    PrivateField = i;
        //                    return PrivateField;
        //                }
        //            }
        cd = new CodeTypeDeclaration ("ClassWithFields");
        cd.IsClass = true;
        nspace.Types.Add (cd);

        CodeMemberField field;
        if (Supports (provider, GeneratorSupport.PublicStaticMembers)) {
            field = new CodeMemberField ();
            field.Name = "StaticPublicField";
            field.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            field.Type = new CodeTypeReference (typeof (int));
            field.InitExpression = new CodePrimitiveExpression (5);
            cd.Members.Add (field);
        }

        if (!(provider is JScriptCodeProvider) && !(provider is VBCodeProvider)) {
            field = new CodeMemberField ();
            field.Name = "InternalField";
            field.Attributes = MemberAttributes.FamilyOrAssembly | MemberAttributes.Static;
            field.Type = new CodeTypeReference (typeof (int));
            field.InitExpression = new CodePrimitiveExpression (0);
            cd.Members.Add (field);
        }

        field = new CodeMemberField ();
        field.Name = "ConstantField";
        field.Attributes = MemberAttributes.Public | MemberAttributes.Const;
        field.Type = new CodeTypeReference (typeof (int));
        field.InitExpression = new CodePrimitiveExpression (0);
        cd.Members.Add (field);

        field = new CodeMemberField ();
        field.Name = "ProtectedField";
        field.Attributes = MemberAttributes.Family | MemberAttributes.Static;
        field.Type = new CodeTypeReference (typeof (int));
        field.InitExpression = new CodePrimitiveExpression (0);
        cd.Members.Add (field);

        field = new CodeMemberField ();
        field.Name = "PrivateField";
        field.Attributes = MemberAttributes.Private | MemberAttributes.Static;
        field.Type = new CodeTypeReference (typeof (int));
        field.InitExpression = new CodePrimitiveExpression (5);
        cd.Members.Add (field);

        field = new CodeMemberField ();
        field.Name = "NonStaticPublicField";
        field.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        field.Type = new CodeTypeReference (typeof (int));
        field.InitExpression = new CodePrimitiveExpression (5);
        cd.Members.Add (field);

        if (!(provider is JScriptCodeProvider) && !(provider is VBCodeProvider)) {
            field = new CodeMemberField ();
            field.Name = "NonStaticInternalField";
            field.Attributes = MemberAttributes.FamilyOrAssembly | MemberAttributes.Final;
            field.Type = new CodeTypeReference (typeof (int));
            field.InitExpression = new CodePrimitiveExpression (0);
            cd.Members.Add (field);
        }

        // create a method to test access to private field
        AddScenario ("CheckUsePrivateField");
        cmm = new CodeMemberMethod ();
        cmm.Name = "UsePrivateField";
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
        CodeFieldReferenceExpression fieldref = new CodeFieldReferenceExpression ();
        fieldref.TargetObject = new CodeTypeReferenceExpression("ClassWithFields");
        fieldref.FieldName = "PrivateField";
        cmm.Statements.Add (new CodeAssignStatement (fieldref, new CodeArgumentReferenceExpression ("i")));
        cmm.Statements.Add (new CodeMethodReturnStatement (fieldref));
        cd.Members.Add (cmm);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateTestFields", "Find and instantiate TestFields.");
        if (!FindAndInstantiate ("NSPC.TestFields", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTestFields");

        if (VerifyMethod (genType, genObject, "UsePrivateField", new object[] {6}, 6)) {
            VerifyScenario ("CheckUsePrivateField");
        }
        if (VerifyMethod (genType, genObject, "UseConstantField", new object[] {6}, 0)) {
            VerifyScenario ("CheckUseConstantField");
        }
        if (VerifyMethod (genType, genObject, "UseNonStaticPublicField", new object[] {6}, 6)) {
            VerifyScenario ("CheckUseNonStaticPublicField");
        }

        if (!(provider is JScriptCodeProvider) && !(provider is VBCodeProvider)) {
            if (VerifyMethod (genType, genObject, "UseNonStaticInternalField", new object[] { 6 }, 6)) {
                VerifyScenario ("CheckUseNonStaticInternalField");
            }
        }

        if (!(provider is JScriptCodeProvider) && !(provider is VBCodeProvider)) {
            if (VerifyMethod (genType, genObject, "UseInternalField", new object[] { 6 }, 6)) {
                VerifyScenario ("CheckUseInternalField");
            }
        }

        if (Supports (provider, GeneratorSupport.PublicStaticMembers) && VerifyMethod (genType, genObject, "UseStaticPublicField",
                    new object [] { 6 }, 6)) {
            VerifyScenario ("CheckUseStaticPublicField");
        }

        AddScenario ("InstantiateTestProtectedField", "Find and instantiate TestProtectedFields.");
        if (!FindAndInstantiate ("NSPC.TestProtectedField", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTestProtectedField");

        if (VerifyMethod (genType, genObject, "UseProtectedField", new object[] {6}, 6)) {
            VerifyScenario ("CheckUseProtectedField");
        }

        AddScenario ("InstantiateClassWithFields", "Find and instantiate ClassWithFields.");
        if (!FindAndInstantiate ("NSPC.ClassWithFields", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateClassWithFields");

        if (VerifyMethod (genType, genObject, "UsePrivateField", new object[] {6}, 6)) {
            VerifyScenario ("CheckUsePrivateField");
        }
    }
}

