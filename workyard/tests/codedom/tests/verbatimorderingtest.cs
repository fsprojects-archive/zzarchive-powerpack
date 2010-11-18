using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Samples.CodeDomTestSuite;

public class VerbatimOrderingTest : CodeDomTestTree {
		public override string Comment
		{
			get { return "order is irelevant - this test is odd"; }
		}		

    public override TestTypes TestType {
        get {
            return TestTypes.Whidbey;
        }
    }

    public override string Name {
        get {
            return "VerbatimOrderingTest";
        }
    }

    public override string Description {
        get {
            return "Tests if types and members are generated in the order they are specified in the tree.";
        }
    }

    public override bool ShouldSearch {
        get {
            return true;
        }
    }

    public override bool ShouldCompile {
        get {
            return true;
        }
    }

    public override bool ShouldVerify {
        get {
            return false;
        }
    }

    public override CodeGeneratorOptions GetGeneratorOptions (CodeDomProvider provider) {
        CodeGeneratorOptions generatorOptions = base.GetGeneratorOptions (provider);
#if WHIDBEY
        generatorOptions.VerbatimOrder = true;
#endif
        return generatorOptions;
    }

    public override CompilerParameters GetCompilerParameters (CodeDomProvider provider) {
        CompilerParameters parms = base.GetCompilerParameters (provider);
        parms.GenerateExecutable = true;
        return parms;
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {
#if WHIDBEY
        CodeNamespace ns = new CodeNamespace("Namespace1");

        cu.Namespaces.Add(ns);

        CodeTypeDeclaration cd = new CodeTypeDeclaration ("Class1");
        ns.Types.Add(cd);

        cd.Comments.Add(new CodeCommentStatement("Outer Type Comment"));

        CodeMemberField field1 = new CodeMemberField(typeof(String), "field1");
        CodeMemberField field2 = new CodeMemberField(typeof(String), "field2");
        field2.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Foo"));
        field2.EndDirectives.Add(new CodeRegionDirective (CodeRegionMode.End, String.Empty));
        field1.Comments.Add(new CodeCommentStatement("Field 1 Comment"));

        CodeMemberEvent evt1 = new CodeMemberEvent();
        evt1.Name = "Event1";
        evt1.Type = new CodeTypeReference(typeof(System.EventHandler));
        evt1.Attributes = (evt1.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;

        CodeMemberEvent evt2 = new CodeMemberEvent();
        evt2.Name = "Event2";
        evt2.Type = new CodeTypeReference(typeof(System.EventHandler));
        evt2.Attributes = (evt2.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;

        CodeMemberMethod method1 = new CodeMemberMethod();
        method1.Name = "Method1";
        method1.Attributes = (method1.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;            
        if (Supports(provider, GeneratorSupport.DeclareEvents)) {
            method1.Statements.Add(
                new CodeDelegateInvokeExpression(
                    new CodeEventReferenceExpression(new CodeThisReferenceExpression(), "Event1"), 
                    new CodeExpression[] {
                        new CodeThisReferenceExpression(),
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.EventArgs"), "Empty")
                    }));
        }


        CodeMemberMethod method2 = new CodeMemberMethod();
        method2.Name = "Method2";
        method2.Attributes = (method2.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        if (Supports(provider, GeneratorSupport.DeclareEvents)) {
            method2.Statements.Add(
                new CodeDelegateInvokeExpression(
                    new CodeEventReferenceExpression(new CodeThisReferenceExpression(), "Event2"), 
                    new CodeExpression[] {
                        new CodeThisReferenceExpression(),
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.EventArgs"), "Empty")
                    }));
        }
        method2.LinePragma = new CodeLinePragma("MethodLinePragma.txt", 500);            
        method2.Comments.Add(new CodeCommentStatement("Method 2 Comment"));

        CodeMemberProperty property1 = new CodeMemberProperty();
        property1.Name = "Property1";
        property1.Type = new CodeTypeReference(typeof(string));
        property1.Attributes = (property1.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        property1.GetStatements.Add(
            new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(),
                    "field1")));

        CodeMemberProperty property2 = new CodeMemberProperty();
        property2.Name = "Property2";
        property2.Type = new CodeTypeReference(typeof(string));
        property2.Attributes = (property2.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        property2.GetStatements.Add(
            new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(),
                    "field2")));


        CodeConstructor constructor1 = new CodeConstructor();
        constructor1.Attributes = (constructor1.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        constructor1.Statements.Add(
            new CodeAssignStatement(
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(),
                    "field1"),
                new CodePrimitiveExpression("value1")));
        constructor1.Statements.Add(
            new CodeAssignStatement(
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(),
                    "field2"),
                new CodePrimitiveExpression("value2")));

        CodeConstructor constructor2 = new CodeConstructor();
        constructor2.Attributes = (constructor2.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        constructor2.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "value1"));
        constructor2.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "value2"));                       

        CodeTypeConstructor typeConstructor2 = new CodeTypeConstructor();

        CodeEntryPointMethod methodMain =  new CodeEntryPointMethod();

        CodeTypeDeclaration nestedClass1 = new CodeTypeDeclaration ("NestedClass1");
        CodeTypeDeclaration nestedClass2 = new CodeTypeDeclaration ("NestedClass2");
        nestedClass2.LinePragma = new CodeLinePragma("NestedTypeLinePragma.txt", 400);
        nestedClass2.Comments.Add(new CodeCommentStatement("Nested Type Comment"));


        CodeTypeDelegate delegate1 = new CodeTypeDelegate();
        delegate1.Name = "nestedDelegate1";
        delegate1.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), "sender"));
        delegate1.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.EventArgs"), "e"));

        CodeTypeDelegate delegate2 = new CodeTypeDelegate();
        delegate2.Name = "nestedDelegate2";
        delegate2.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.Object"), "sender"));
        delegate2.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.EventArgs"), "e"));



        cd.Members.Add(field1);
        cd.Members.Add(method1);
        cd.Members.Add(constructor1);
        cd.Members.Add(property1);

        if (Supports (provider, GeneratorSupport.EntryPointMethod))
            cd.Members.Add(methodMain);

        if (Supports(provider, GeneratorSupport.DeclareEvents))
            cd.Members.Add(evt1);

        if (Supports(provider, GeneratorSupport.NestedTypes)) {
            cd.Members.Add(nestedClass1);
            if (Supports(provider, GeneratorSupport.DeclareDelegates)) {
                cd.Members.Add(delegate1);
            }
        }

        cd.Members.Add(field2);
        cd.Members.Add(method2);
        cd.Members.Add(constructor2);
        cd.Members.Add(property2);

        if (Supports(provider, GeneratorSupport.StaticConstructors)) {
            cd.Members.Add(typeConstructor2);
        }

        if (Supports(provider, GeneratorSupport.DeclareEvents)) {
            cd.Members.Add(evt2);
        }

        if (Supports(provider, GeneratorSupport.NestedTypes)) {
            cd.Members.Add(nestedClass2);
            if (Supports(provider, GeneratorSupport.DeclareDelegates)) {
                cd.Members.Add(delegate2);
            }
        }
#endif
    }

    public override void Search (CodeDomProvider provider, String strGeneratedCode)  {
#if WHIDBEY
        int searchIndex = 0; 

        string [] tokens = GetTokenOrder (provider);

        // load up scenarios
        for(int i = 0; i < tokens.Length; i++)
            if (String.CompareOrdinal (tokens[i], String.Empty) != 0)
                AddScenario ("Find" + i + ":" + tokens[i], "The first one to fail will make the rest fail.");

        // attempt to find them
        for(int i = 0; i < tokens.Length; i++) {
            searchIndex = strGeneratedCode.IndexOf (tokens[i], searchIndex);
            if(searchIndex > -1) {
                //LogMessage("Expected token doesn't exists...... Search Token number...:" + i.ToString()  + "Search token string...:" + strSearchTokens[i]);
                searchIndex += tokens[i].Length ;
                VerifyScenario ("Find" + i + ":" + tokens[i]);
            } else
                break;
        }
#endif
    }

    private string[] GetTokenOrder (CodeDomProvider provider) {
        ArrayList list = new ArrayList ();
        
        list.AddRange (new string[]{ "Namespace1",
                "Outer Type Comment", "Class1",
                "field1", "Method1" });

        // constructor
        if (provider is Microsoft.CSharp.CSharpCodeProvider)            
            list.Add ("Class1");
        else if (provider is Microsoft.VisualBasic.VBCodeProvider) 
            list.Add ("New");

        list.Add ("Property1");

        if (Supports (provider, GeneratorSupport.EntryPointMethod))
            list.Add ("Main");

        if (Supports (provider, GeneratorSupport.DeclareEvents))
            list.Add ("Event1");
       
        if (Supports (provider, GeneratorSupport.NestedTypes)) {
            list.Add ("NestedClass1");

            if (Supports (provider, GeneratorSupport.DeclareDelegates))
                list.Add ("nestedDelegate1");
        }

#if WHIDBEY
        // region directive
        if (provider is Microsoft.CSharp.CSharpCodeProvider ||
                provider is Microsoft.VisualBasic.VBCodeProvider)
            list.Add ("Foo");
#endif
        list.AddRange (new string [] {
            "field2", "Method 2 Comment",
            "MethodLinePragma.txt", "Method2" });

        // line pragma extras
        if (provider is Microsoft.CSharp.CSharpCodeProvider)            
            list.Add ("default");
        else if (provider is Microsoft.VisualBasic.VBCodeProvider) 
            list.Add ("ExternalSource");

        // constructor
        if (provider is Microsoft.CSharp.CSharpCodeProvider)            
            list.Add ("Class1");
        else if (provider is Microsoft.VisualBasic.VBCodeProvider) 
            list.Add ("New");

        list.Add ("Property2");

        // static constructor
        if (provider is Microsoft.CSharp.CSharpCodeProvider &&
                Supports (provider, GeneratorSupport.StaticConstructors))
            list.Add ("Class1");
        else if (provider is Microsoft.VisualBasic.VBCodeProvider &&
                Supports (provider, GeneratorSupport.StaticConstructors))
            list.Add ("New");

        if (Supports (provider, GeneratorSupport.DeclareEvents))
            list.Add ("Event2");
                    
        if (Supports (provider, GeneratorSupport.NestedTypes))
            list.AddRange (new string [] { "Nested Type Comment",
                "NestedTypeLinePragma.txt", "NestedClass2" });

        // line pragma extras
        if (provider is Microsoft.CSharp.CSharpCodeProvider)            
            list.Add ("default");
        else if (provider is Microsoft.VisualBasic.VBCodeProvider) 
            list.Add ("ExternalSource");

        if (Supports (provider, GeneratorSupport.DeclareDelegates))
            list.Add ("nestedDelegate2");

        return (string[]) list.ToArray (typeof (string));
    }
    
    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
    }
}	

