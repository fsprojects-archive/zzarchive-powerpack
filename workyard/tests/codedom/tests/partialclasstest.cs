using System;
using System.IO;
using System.CodeDom;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.JScript;

public class PartialClassTest : CodeDomTestTree {

		public override string Comment
		{
			get
			{
				return "partial classes not supported";
			}
		}

    public override TestTypes TestType {
        get {
            return TestTypes.Whidbey;
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
            return "PartialClassTest";
        }
    }

    public override string Description {
        get {
            return "Tests partial class declaration.";
        }
    }

    public override CompilerParameters GetCompilerParameters (CodeDomProvider provider) {
        CompilerParameters parms = base.GetCompilerParameters (provider);
        parms.TreatWarningsAsErrors = false;
        return parms;
    }

    enum ClassTypes {
        Struct,
        Interface,
        Class
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {
#if WHIDBEY
        if (!(provider is JScriptCodeProvider)) {
            cu.ReferencedAssemblies.Add("System.dll");
            cu.UserData["AllowLateBound"] = true;

            CodeNamespace ns = new CodeNamespace("Namespace1");
            ns.Imports.Add(new CodeNamespaceImport("System"));
            cu.Namespaces.Add(ns);

            AddScenario ("FindPartialClass", "Attempt to find 'PartialClass'");
            BuildClassesIntoNamespace (provider, ns, "PartialClass", ClassTypes.Class, TypeAttributes.Public);
            AddScenario ("FindSealedPartialClass", "Attempt to find 'SealedPartialClass'");
            BuildClassesIntoNamespace (provider, ns, "SealedPartialClass", ClassTypes.Class, TypeAttributes.Sealed);
            BuildClassesIntoNamespace (provider, ns, "AbstractPartialClass", ClassTypes.Class, TypeAttributes.Abstract);
            BuildClassesIntoNamespace (provider, ns, "PrivatePartialClass", ClassTypes.Class, TypeAttributes.NotPublic);

            if (Supports (provider, GeneratorSupport.DeclareValueTypes)) {
                AddScenario ("FindSealedPartialStruct", "Attempt to find 'SealedPartialStruct'");
                BuildClassesIntoNamespace (provider, ns, "SealedPartialStruct", ClassTypes.Struct, TypeAttributes.Sealed);
                BuildClassesIntoNamespace (provider, ns, "AbstractPartialStruct", ClassTypes.Struct, TypeAttributes.Abstract);
            }
        }
#endif
    }

#if WHIDBEY
    void BuildClassesIntoNamespace (CodeDomProvider provider, CodeNamespace ns, string name,
            ClassTypes classType, TypeAttributes attributes) {

        CodeTypeDeclaration class1 = new CodeTypeDeclaration();
        class1.TypeAttributes = attributes;
        class1.Name = name;
        class1.IsPartial = true;
        if (classType == ClassTypes.Struct)
            class1.IsStruct = true;
        else if (classType == ClassTypes.Interface)
            class1.IsInterface = true;
        else
            class1.IsClass = true;

        ns.Types.Add(class1);

        class1.Members.Add (new CodeMemberField (typeof (int), "field1"));

        CodeMemberMethod fooMethod1 = new CodeMemberMethod();
        fooMethod1.Name = "Foo1";
        fooMethod1.Attributes = MemberAttributes.Public | MemberAttributes.Final ;
        fooMethod1.ReturnType = new CodeTypeReference(typeof(int));
        fooMethod1.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(1)));
        class1.Members.Add(fooMethod1);

        CodeMemberMethod methodMain =  new CodeMemberMethod ();
        methodMain.Name = "MainMethod";
        if (attributes != TypeAttributes.Abstract) { 
            methodMain.Statements.Add(
                new CodeVariableDeclarationStatement(
                new CodeTypeReference(name), "test1",
                new CodeObjectCreateExpression(new CodeTypeReference(name))));

            methodMain.Statements.Add( 
                new CodeExpressionStatement( 
                new CodeMethodInvokeExpression( 
                new CodeMethodReferenceExpression(
                new CodeVariableReferenceExpression ("test1"), "Foo1"), new CodeExpression[0])));

            methodMain.Statements.Add( 
                new CodeExpressionStatement( 
                new CodeMethodInvokeExpression( 
                new CodeMethodReferenceExpression(
                new CodeVariableReferenceExpression("test1"), "Foo2"), new CodeExpression[0])));
        }
        class1.Members.Add(methodMain);
        
        CodeTypeDeclaration class2 = new CodeTypeDeclaration();
        class2.TypeAttributes = attributes;
        class2.Name = name;
        class2.IsPartial = true; 	
        if (classType == ClassTypes.Struct)
            class2.IsStruct = true;
        else if (classType == ClassTypes.Interface)
            class2.IsInterface = true;
        else
            class2.IsClass = true;

        ns.Types.Add(class2);

        class2.Members.Add (new CodeMemberField (typeof (int), "field2"));

        CodeMemberMethod fooMethod2 = new CodeMemberMethod();
        fooMethod2.Name = "Foo2";
        fooMethod2.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        fooMethod2.ReturnType = new CodeTypeReference(typeof(int));
        fooMethod2.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(2)));
        class2.Members.Add(fooMethod2);
    }
#endif

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
#if WHIDBEY
        if (!(provider is JScriptCodeProvider)) {
            object genObject;
            Type genType;

            if (!FindAndInstantiate("Namespace1.PartialClass", asm, out genObject, out genType))
                return;
            VerifyScenario("FindPartialClass");

            if (!FindAndInstantiate("Namespace1.SealedPartialClass", asm, out genObject, out genType))
                return;
            VerifyScenario("FindSealedPartialClass");

            if (Supports(provider, GeneratorSupport.DeclareValueTypes))
            {
                if (!FindAndInstantiate("Namespace1.SealedPartialStruct", asm, out genObject, out genType))
                    return;
                VerifyScenario("FindSealedPartialStruct");
            }

            if (Supports(provider, GeneratorSupport.DeclareValueTypes))
            {
                if (!FindAndInstantiate("Namespace1.PartialInterface", asm, out genObject, out genType))
                    return;
                VerifyScenario("FindPartialInterface");
            }
        }
#endif
    }
}



