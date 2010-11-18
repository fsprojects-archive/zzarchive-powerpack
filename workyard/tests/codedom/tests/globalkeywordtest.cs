using System;
using System.IO;
using System.CodeDom;
using System.Reflection;
using System.Collections;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.JScript;

public class GlobalKeywordTest : CodeDomTestTree {

		public override TestTypes TestType {
        get {
            return TestTypes.Whidbey;
        }
    }

    public override string Name {
        get {
            return "GlobalKeywordTest";
        }
    }

    public override string Description {
        get {
            return "Test the global keyword used to differentiate global namespaces from local ones.";
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
        if (!(provider is JScriptCodeProvider)) {
            // GENERATES (C#):
            //   namespace Foo {
            //       public class Foo {
            //
            //           public int _verifyGlobalGeneration1 = 2147483647;
            //
            //           public int System {
            //              get { return 42; }
            //           }
            //   
            //           public int Property {
            //               get { return 2147483647; }
            //           }
            //
            //           public int GlobalTestProperty1 {
            //               get {
            //                   return _verifyGlobalGeneration1;
            //               }
            //               set {
            //                   _verifyGlobalGeneration1 = value;
            //               }
            //           }
            //         
            //           public global::System.Nullable<int> GlobalTestProperty2 {
            //               get {
            //                   return _verifyGlobalGeneration2;
            //               }
            //               set {
            //                   _verifyGlobalGeneration2 = value;
            //               }
            //           }
            //           
            //   
            //           public int TestMethod02() {
            //               int iReturn;
            //               iReturn = global::Foo.Foo.Property;
            //               return iReturn;
            //           }
            //   
            //           public int TestMethod03() {
            //               int iReturn;
            //               iReturn = global::System.Math.Abs(-1);
            //               return iReturn;
            //           }
            //
            //           public int TestMethod04() {
            //               int iReturn;
            //               iReturn = System;
            //               return iReturn;
            //       }
            //   }

            CodeNamespace ns = new CodeNamespace ("Foo");
            ns.Comments.Add (new CodeCommentStatement ("Foo namespace"));

            cu.Namespaces.Add (ns);

            CodeTypeDeclaration cd = new CodeTypeDeclaration ("Foo");
            ns.Types.Add (cd);

            CodeMemberProperty property = new CodeMemberProperty ();
            property.Name = "System";
            property.Attributes = MemberAttributes.Public;
            property.Attributes = (property.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Private | MemberAttributes.Static;
            property.Type = new CodeTypeReference (typeof (int));
            property.GetStatements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (42)));
            cd.Members.Add (property);

            property = new CodeMemberProperty ();
            property.Name = "Property";
            property.Attributes = (property.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Private | MemberAttributes.Static;
            property.Type = new CodeTypeReference (typeof (int));
            property.GetStatements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (Int32.MaxValue)));
            cd.Members.Add (property);

            AddScenario ("CallTestMethod02", "Call Foo.Foo.TestMethod02.");
            CodeMemberMethod method2 = new CodeMemberMethod ();
            method2.Name = "TestMethod02";
            method2.Attributes = (method2.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            method2.ReturnType = new CodeTypeReference (typeof (int));
            method2.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "iReturn"));

            CodePropertyReferenceExpression cpr = new CodePropertyReferenceExpression (
                                              new CodeTypeReferenceExpression (new CodeTypeReference ("Foo.Foo",
                                                      CodeTypeReferenceOptions.GlobalReference)), "Property");

            CodeAssignStatement cas = new CodeAssignStatement (new CodeVariableReferenceExpression ("iReturn"), cpr);
            method2.Statements.Add (cas);
            method2.Statements.Add (new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("iReturn")));
            cd.Members.Add (method2);

            AddScenario ("CallTestMethod03", "Call Foo.Foo.TestMethod02.");
            CodeMemberMethod method3 = new CodeMemberMethod ();
            method3.Name = "TestMethod03";
            method3.Attributes = (method3.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            method3.ReturnType = new CodeTypeReference (typeof (int));
            method3.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "iReturn"));
            CodeTypeReferenceOptions ctro = CodeTypeReferenceOptions.GlobalReference;
            CodeTypeReference ctr = new CodeTypeReference (typeof (Math), ctro);
            CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression (
                                              new CodeMethodReferenceExpression (
                                              new CodeTypeReferenceExpression (ctr), "Abs"), new CodeExpression[] { new CodePrimitiveExpression (-1) });
            cas = new CodeAssignStatement (new CodeVariableReferenceExpression ("iReturn"), cmie);
            method3.Statements.Add (cas);
            method3.Statements.Add (new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("iReturn")));
            cd.Members.Add (method3);

            AddScenario ("CallTestMethod04", "Call Foo.Foo.TestMethod04.");
            CodeMemberMethod method4 = new CodeMemberMethod ();
            method4.Name = "TestMethod04";
            method4.Attributes = (method4.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            method4.ReturnType = new CodeTypeReference (typeof (int));
            method4.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "iReturn"));

            cpr = new CodePropertyReferenceExpression (null, "System");

            cas = new CodeAssignStatement (new CodeVariableReferenceExpression ("iReturn"), cpr);
            method4.Statements.Add (cas);
            method4.Statements.Add (new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("iReturn")));
            cd.Members.Add (method4);

            // Verify that what CodeTypeReferenceOptions are correctly set. 
            // Basically this check gives the code coverage for the get property
            AddScenario ("CTR_GetGlobalRefCheck", "Check that CodeTypeReference.Options gives the proper value.");
            if (ctr.Options == CodeTypeReferenceOptions.GlobalReference)
                VerifyScenario ("CTR_GetGlobalRefCheck");

            // one-off generate statements
            StringWriter sw = new StringWriter ();

            // global shouldn't be generated in this instance
            CodeTypeReference variableType = new CodeTypeReference (typeof (System.String), CodeTypeReferenceOptions.GlobalReference);
            CodeVariableDeclarationStatement variable = new CodeVariableDeclarationStatement (variableType, "myVariable");
            provider.GenerateCodeFromStatement (variable, sw, null);

            // global should be generated in this instance
            CodeTypeReference variableType2 = new CodeTypeReference (typeof (System.Array), CodeTypeReferenceOptions.GlobalReference);
            CodeVariableDeclarationStatement variable2 = new CodeVariableDeclarationStatement (variableType2, "myVariable2");
            provider.GenerateCodeFromStatement (variable2, sw, null);

            AddScenario ("GlobalKeywordShouldExist", "When an array is referred to, a global qualifier should be generated on it.");
            if (sw.ToString ().IndexOf ("global") != -1 && sw.ToString ().IndexOf ("Global") != -1) {
                LogMessage ("Global keyword does not exist in statement: " + sw.ToString ());
            }
            else
                VerifyScenario ("GlobalKeywordShouldExist");
        }
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        if (!(provider is JScriptCodeProvider)) {
            object genObject;
            Type genType;

            AddScenario ("FindAndInstantiateFoo", "Find and instantiate the object to test");
            if (!FindAndInstantiate ("Foo.Foo", asm, out genObject, out genType))
                return;
            VerifyScenario ("FindAndInstantiateFoo");

            // verify scenario with 'new' attribute
            if (VerifyMethod (genType, genObject, "TestMethod02", new object[]{} , Int32.MaxValue))
                VerifyScenario ("CallTestMethod02");

            // verify scenario with 'new' attribute
            if (VerifyMethod (genType, genObject, "TestMethod03", new object[]{} , 1))
                VerifyScenario ("CallTestMethod03");

            // verify scenario with 'new' attribute
            if (VerifyMethod (genType, genObject, "TestMethod04", new object[]{} , 42))
                VerifyScenario ("CallTestMethod04");
        }
    }
}                                                            

