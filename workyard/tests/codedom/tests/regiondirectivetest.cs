// F#: No regions in F#

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.JScript;


public class RegionDirectiveTest : CodeDomTestTree {

		public override string Comment
		{
			get { return "#region isn't supported by F#"; }
		}		

    public override TestTypes TestType {
        get {
            return TestTypes.Whidbey;
        }
    }

    public override string Name {
        get {
            return "RegionDirectiveTest";
        }
    }

    public override string Description {
        get {
            return "Tests region directives on various code constructs.";
        }
    }

    public override bool ShouldCompile {
        get {
            return true;
        }
    }

    public override bool ShouldSearch {
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
        if (!(provider is JScriptCodeProvider)) {
            // GENERATES (C#):
            //
            //  #region Compile Unit Region
            //  
            //  namespace Namespace1 {
            //      
            //      
            //      #region Outer Type Region
            //      // Outer Type Comment
            //      public class Class1 {
            //          
            //          // Field 1 Comment
            //          private string field1;
            //          
            //          public void Method1() {
            //              this.Event1(this, System.EventArgs.Empty);
            //          }
            //          
            //          #region Constructor Region
            //          public Class1() {
            //              #region Statements Region
            //              this.field1 = "value1";
            //              this.field2 = "value2";
            //              #endregion
            //          }
            //          #endregion
            //          
            //          public string Property1 {
            //              get {
            //                  return this.field1;
            //              }
            //          }
            //          
            //          public static void Main() {
            //          }
            //          
            //          public event System.EventHandler Event1;
            //          
            //          public class NestedClass1 {
            //          }
            //          
            //          public delegate void nestedDelegate1(object sender, System.EventArgs e);
            //          
            //  
            //          
            //          #region Field Region
            //          private string field2;
            //          #endregion
            //          
            //          #region Method Region
            //          // Method 2 Comment
            //          
            //          #line 500 "MethodLinePragma.txt"
            //          public void Method2() {
            //              this.Event2(this, System.EventArgs.Empty);
            //          }
            //          
            //          #line default
            //          #line hidden
            //          #endregion
            //          
            //          public Class1(string value1, string value2) {
            //          }
            //          
            //          #region Property Region
            //          public string Property2 {
            //              get {
            //                  return this.field2;
            //              }
            //          }
            //          #endregion
            //          
            //          #region Type Constructor Region
            //          static Class1() {
            //          }
            //          #endregion
            //          
            //          #region Event Region
            //          public event System.EventHandler Event2;
            //          #endregion
            //          
            //          #region Nested Type Region
            //          // Nested Type Comment
            //          
            //          #line 400 "NestedTypeLinePragma.txt"
            //          public class NestedClass2 {
            //          }
            //          
            //          #line default
            //          #line hidden
            //          #endregion
            //          
            //          #region Delegate Region
            //          public delegate void nestedDelegate2(object sender, System.EventArgs e);
            //          #endregion
            //          
            //          #region Snippet Region
            //  
            //          #endregion
            //      }
            //      #endregion
            //  }
            //  #endregion

            CodeNamespace ns = new CodeNamespace ("Namespace1");

            cu.StartDirectives.Add (new CodeRegionDirective (CodeRegionMode.Start, "Compile Unit Region"));
            cu.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, string.Empty));

            cu.Namespaces.Add (ns);

            CodeTypeDeclaration cd = new CodeTypeDeclaration ("Class1");
            ns.Types.Add (cd);

            cd.StartDirectives.Add (new CodeRegionDirective (CodeRegionMode.Start, "Outer Type Region"));
            cd.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, string.Empty));

            cd.Comments.Add (new CodeCommentStatement ("Outer Type Comment"));

            CodeMemberField field1 = new CodeMemberField (typeof (String), "field1");
            CodeMemberField field2 = new CodeMemberField (typeof (String), "field2");
            field1.Comments.Add (new CodeCommentStatement ("Field 1 Comment"));
            field2.StartDirectives.Add (new CodeRegionDirective (CodeRegionMode.Start, "Field Region"));
            field2.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, string.Empty));

            CodeMemberEvent evt1 = new CodeMemberEvent ();
            evt1.Name = "Event1";
            evt1.Type = new CodeTypeReference (typeof (System.EventHandler));
            evt1.Attributes = (evt1.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;

            CodeMemberEvent evt2 = new CodeMemberEvent ();
            evt2.Name = "Event2";
            evt2.Type = new CodeTypeReference (typeof (System.EventHandler));
            evt2.Attributes = (evt2.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;

            evt2.StartDirectives.Add (new CodeRegionDirective (CodeRegionMode.Start, "Event Region"));
            evt2.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, string.Empty));


            CodeMemberMethod method1 = new CodeMemberMethod ();
            method1.Name = "Method1";
            method1.Attributes = (method1.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            if (provider.Supports (GeneratorSupport.DeclareEvents)) {
                method1.Statements.Add (
                    new CodeDelegateInvokeExpression (
                        new CodeEventReferenceExpression (new CodeThisReferenceExpression (), "Event1"),
                        new CodeExpression[] {
                        new CodeThisReferenceExpression(),
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.EventArgs"), "Empty")
                    }));
            }


            CodeMemberMethod method2 = new CodeMemberMethod ();
            method2.Name = "Method2";
            method2.Attributes = (method2.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            if (provider.Supports (GeneratorSupport.DeclareEvents)) {
                method2.Statements.Add (
                    new CodeDelegateInvokeExpression (
                        new CodeEventReferenceExpression (new CodeThisReferenceExpression (), "Event2"),
                        new CodeExpression[] {
                        new CodeThisReferenceExpression(),
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("System.EventArgs"), "Empty")
                    }));
            }
            method2.LinePragma = new CodeLinePragma ("MethodLinePragma.txt", 500);
            method2.Comments.Add (new CodeCommentStatement ("Method 2 Comment"));

            method2.StartDirectives.Add (new CodeRegionDirective (CodeRegionMode.Start, "Method Region"));
            method2.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, string.Empty));


            CodeMemberProperty property1 = new CodeMemberProperty ();
            property1.Name = "Property1";
            property1.Type = new CodeTypeReference (typeof (string));
            property1.Attributes = (property1.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            property1.GetStatements.Add (
                new CodeMethodReturnStatement (
                    new CodeFieldReferenceExpression (
                        new CodeThisReferenceExpression (),
                        "field1")));

            CodeMemberProperty property2 = new CodeMemberProperty ();
            property2.Name = "Property2";
            property2.Type = new CodeTypeReference (typeof (string));
            property2.Attributes = (property2.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            property2.GetStatements.Add (
                new CodeMethodReturnStatement (
                    new CodeFieldReferenceExpression (
                        new CodeThisReferenceExpression (),
                        "field2")));

            property2.StartDirectives.Add (new CodeRegionDirective (CodeRegionMode.Start, "Property Region"));
            property2.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, string.Empty));


            CodeConstructor constructor1 = new CodeConstructor ();
            constructor1.Attributes = (constructor1.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            CodeStatement conState1 = new CodeAssignStatement (
                                        new CodeFieldReferenceExpression (
                                            new CodeThisReferenceExpression (),
                                            "field1"),
                                        new CodePrimitiveExpression ("value1"));
            conState1.StartDirectives.Add (new CodeRegionDirective (CodeRegionMode.Start, "Statements Region"));
            constructor1.Statements.Add (conState1);
            CodeStatement conState2 = new CodeAssignStatement (
                                        new CodeFieldReferenceExpression (
                                            new CodeThisReferenceExpression (),
                                            "field2"),
                                        new CodePrimitiveExpression ("value2"));
            conState2.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, string.Empty));
            constructor1.Statements.Add (conState2);

            constructor1.StartDirectives.Add (new CodeRegionDirective (CodeRegionMode.Start, "Constructor Region"));
            constructor1.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, string.Empty));

            CodeConstructor constructor2 = new CodeConstructor ();
            constructor2.Attributes = (constructor2.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            constructor2.Parameters.Add (new CodeParameterDeclarationExpression (typeof (string), "value1"));
            constructor2.Parameters.Add (new CodeParameterDeclarationExpression (typeof (string), "value2"));

            CodeTypeConstructor typeConstructor2 = new CodeTypeConstructor ();

            typeConstructor2.StartDirectives.Add (new CodeRegionDirective (CodeRegionMode.Start, "Type Constructor Region"));
            typeConstructor2.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, string.Empty));


            CodeEntryPointMethod methodMain = new CodeEntryPointMethod ();

            CodeTypeDeclaration nestedClass1 = new CodeTypeDeclaration ("NestedClass1");
            CodeTypeDeclaration nestedClass2 = new CodeTypeDeclaration ("NestedClass2");
            nestedClass2.LinePragma = new CodeLinePragma ("NestedTypeLinePragma.txt", 400);
            nestedClass2.Comments.Add (new CodeCommentStatement ("Nested Type Comment"));

            nestedClass2.StartDirectives.Add (new CodeRegionDirective (CodeRegionMode.Start, "Nested Type Region"));
            nestedClass2.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, string.Empty));



            CodeTypeDelegate delegate1 = new CodeTypeDelegate ();
            delegate1.Name = "nestedDelegate1";
            delegate1.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference ("System.Object"), "sender"));
            delegate1.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference ("System.EventArgs"), "e"));

            CodeTypeDelegate delegate2 = new CodeTypeDelegate ();
            delegate2.Name = "nestedDelegate2";
            delegate2.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference ("System.Object"), "sender"));
            delegate2.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference ("System.EventArgs"), "e"));

            delegate2.StartDirectives.Add (new CodeRegionDirective (CodeRegionMode.Start, "Delegate Region"));
            delegate2.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, string.Empty));


            CodeSnippetTypeMember snippet1 = new CodeSnippetTypeMember ();
            CodeSnippetTypeMember snippet2 = new CodeSnippetTypeMember ();

            CodeRegionDirective regionStart = new CodeRegionDirective (CodeRegionMode.End, "");
            regionStart.RegionText = "Snippet Region";
            regionStart.RegionMode = CodeRegionMode.Start;
            snippet2.StartDirectives.Add (regionStart);
            snippet2.EndDirectives.Add (new CodeRegionDirective (CodeRegionMode.End, string.Empty));


            cd.Members.Add (field1);
            cd.Members.Add (method1);
            cd.Members.Add (constructor1);
            cd.Members.Add (property1);
            cd.Members.Add (methodMain);

            if (Supports (provider, GeneratorSupport.DeclareEvents)) {
                cd.Members.Add (evt1);
            }

            if (Supports (provider, GeneratorSupport.NestedTypes)) {
                cd.Members.Add (nestedClass1);
                if (Supports (provider, GeneratorSupport.DeclareDelegates)) {
                    cd.Members.Add (delegate1);
                }
            }

            cd.Members.Add (snippet1);

            cd.Members.Add (field2);
            cd.Members.Add (method2);
            cd.Members.Add (constructor2);
            cd.Members.Add (property2);


            if (Supports (provider, GeneratorSupport.StaticConstructors)) {
                cd.Members.Add (typeConstructor2);
            }

            if (Supports (provider, GeneratorSupport.DeclareEvents)) {
                cd.Members.Add (evt2);
            }
            if (Supports (provider, GeneratorSupport.NestedTypes)) {
                cd.Members.Add (nestedClass2);
                if (Supports (provider, GeneratorSupport.DeclareDelegates)) {
                    cd.Members.Add (delegate2);
                }
            }
            cd.Members.Add (snippet2);
        }
#endif
    }

    private string[] GetScenarios (CodeDomProvider provider, bool verbatimOrder) {
        ArrayList arrList = new ArrayList();

        if(verbatimOrder) {
            arrList.Add("FindCompileUnitRegion");
            AddScenario("FindCompileUnitRegion");
            arrList.Add("FindOuterTypeRegion");
            AddScenario("FindOuterTypeRegion");
            arrList.Add("FindConstructorRegion");
            AddScenario("FindConstructorRegion");
            if (!(provider is Microsoft.VisualBasic.VBCodeProvider)) { 
                arrList.Add("FindStatementsRegion");
                AddScenario("FindStatementsRegion");
                arrList.Add("FindEndRegion1");
                AddScenario("FindEndRegion1");
            }
            arrList.Add("FindEndRegion2");
            AddScenario("FindEndRegion2");
            arrList.Add("FindFieldRegion");
            AddScenario("FindFieldRegion");
            arrList.Add("FindEndRegion3");
            AddScenario("FindEndRegion3");
            arrList.Add("FindMethodRegion");
            AddScenario("FindMethodRegion");
            arrList.Add("FindEndRegion4");
            AddScenario("FindEndRegion4");
            arrList.Add("FindPropertyRegion");
            AddScenario("FindPropertyRegion");
            arrList.Add("FindEndRegion5");
            AddScenario("FindEndRegion5");
            arrList.Add("FindTypeConstructorRegion");
            AddScenario("FindTypeConstructorRegion");
            arrList.Add("FindEndRegion6");
            AddScenario("FindEndRegion6");
            arrList.Add("FindEventRegion");
            AddScenario("FindEventRegion");
            arrList.Add("FindEndRegion7");
            AddScenario("FindEndRegion7");
            arrList.Add("FindNestedTypeRegion");
            AddScenario("FindNestedTypeRegion");
            arrList.Add("FindEndRegion8");
            AddScenario("FindEndRegion8");
            arrList.Add("FindDelegateRegion");
            AddScenario("FindDelegateRegion");
            arrList.Add("FindEndRegion9");
            AddScenario("FindEndRegion9");
            arrList.Add("FindSnippetRegion");
            AddScenario("FindSnippetRegion");
            arrList.Add("FindEndRegion10");
            AddScenario("FindEndRegion10");
            arrList.Add("FindEndRegion11");
            AddScenario("FindEndRegion11");
            arrList.Add("FindEndRegion12");
            AddScenario("FindEndRegion12");
        } else {
            arrList.Add("FindCompileUnitRegion");
            AddScenario("FindCompileUnitRegion");
            arrList.Add("FindOuterTypeRegion");
            AddScenario("FindOuterTypeRegion");
            arrList.Add("FindFieldRegion");
            AddScenario("FindFieldRegion");
            arrList.Add("FindEndRegion1");
            AddScenario("FindEndRegion1");
            arrList.Add("FindSnippetRegion");
            AddScenario("FindSnippetRegion");
            arrList.Add("FindEndRegion2");
            AddScenario("FindEndRegion2");
            arrList.Add("FindTypeConstructorRegion");
            AddScenario("FindTypeConstructorRegion");
            arrList.Add("FindEndRegion3");
            AddScenario("FindEndRegion3");
            arrList.Add("FindConstructorRegion");
            AddScenario("FindConstructorRegion");
            if (!(provider is Microsoft.VisualBasic.VBCodeProvider)) { 
                arrList.Add("FindStatementsRegion");
                AddScenario("FindStatementsRegion");
                arrList.Add("FindEndRegion4");
                AddScenario("FindEndRegion4");
            }
            arrList.Add("FindEndRegion5");
            AddScenario("FindEndRegion5");
            arrList.Add("FindPropertyRegion");
            AddScenario("FindPropertyRegion");
            arrList.Add("FindEndRegion6");
            AddScenario("FindEndRegion6");
            arrList.Add("FindEventRegion");
            AddScenario("FindEventRegion");
            arrList.Add("FindEndRegion7");
            AddScenario("FindEndRegion7");
            arrList.Add("FindMethodRegion");
            AddScenario("FindMethodRegion");
            arrList.Add("FindEndRegion8");
            AddScenario("FindEndRegion8");
            arrList.Add("FindNestedTypeRegion");
            AddScenario("FindNestedTypeRegion");
            arrList.Add("FindEndRegion9");
            AddScenario("FindEndRegion9");
            arrList.Add("FindDelegateRegion");
            AddScenario("FindDelegateRegion");
            arrList.Add("FindEndRegion10");
            AddScenario("FindEndRegion10");
            arrList.Add("FindEndRegion11");
            AddScenario("FindEndRegion11");
        }
        return(string[]) arrList.ToArray(typeof(string)) ;
    }

    private string[] GetTokenOrder (CodeDomProvider provider, bool verbatimOrder) {
        ArrayList arrList = new ArrayList();

        if (verbatimOrder) {
            arrList.Add(AddBeginRegion (provider, "Compile Unit Region"));
            arrList.Add(AddBeginRegion (provider, "Outer Type Region"));
            arrList.Add(AddBeginRegion (provider, "Constructor Region"));
            if (!(provider is Microsoft.VisualBasic.VBCodeProvider)) { 
                arrList.Add(AddBeginRegion (provider, "Statements Region"));
                arrList.Add(AddEndRegion (provider));
            }
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Field Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Method Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Property Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Type Constructor Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Event Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Nested Type Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Delegate Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Snippet Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddEndRegion (provider));
        } else {
            arrList.Add(AddBeginRegion (provider, "Compile Unit Region"));
            arrList.Add(AddBeginRegion (provider, "Outer Type Region"));
            arrList.Add(AddBeginRegion (provider, "Field Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Snippet Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Type Constructor Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Constructor Region"));
            if (!(provider is Microsoft.VisualBasic.VBCodeProvider)) { 
                arrList.Add(AddBeginRegion (provider, "Statements Region"));
                arrList.Add(AddEndRegion (provider));
            }
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Property Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Event Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Method Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Nested Type Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddBeginRegion (provider, "Delegate Region"));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddEndRegion (provider));
            arrList.Add(AddEndRegion (provider));
        }
        return (string[]) arrList.ToArray(typeof(string)) ;
    }
    
    public string AddBeginRegion (CodeDomProvider provider, string regionText)  {
        string regionFind = String.Empty;

        if (provider is Microsoft.CSharp.CSharpCodeProvider)
            regionFind = String.Format ("#region {0}", regionText);
        else if (provider is Microsoft.VisualBasic.VBCodeProvider) 
            regionFind = String.Format ("#Region \"{0}\"", regionText);
        else
            regionFind = regionText;

        return regionFind; 
    }

    public string AddEndRegion (CodeDomProvider provider)  {
        string regionFind = String.Empty;

        if (provider is Microsoft.CSharp.CSharpCodeProvider)            
            regionFind = "#endregion";
        else if (provider is Microsoft.VisualBasic.VBCodeProvider) 
            regionFind = "#End Region";

        return regionFind; 
    }

    public override void Search (CodeDomProvider provider, string strGeneratedCode)  {
#if WHIDBEY
        if (!(provider is JScriptCodeProvider)) {
            int searchIndex = 0;

            string[] tokens = GetTokenOrder (provider, true);
            string[] scenarios = GetScenarios (provider, true);

            for (int i = 0; i < tokens.Length; i++) {
                searchIndex = strGeneratedCode.IndexOf (tokens[i], searchIndex);
                if (searchIndex > -1) {
                    searchIndex += tokens[i].Length;
                    VerifyScenario (scenarios[i]);
                }
                else
                    break;
            }
        }
#endif
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
    }
}	