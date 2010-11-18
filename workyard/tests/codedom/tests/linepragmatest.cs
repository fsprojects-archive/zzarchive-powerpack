using System;
using System.CodeDom;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.Samples.CodeDomTestSuite;

public class LinePragmaTest : CodeDomTestTree {


		public override TestTypes TestType {
        get {
            return TestTypes.Everett;
        }
    }

    public override string Name {
        get {
            return "LinePragmaTest";
        }
    }

    public override string Description {
        get {
            return "Tests CodeLinePragma.";
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

    public override bool ShouldSearch {
        get {
            return true;
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {
        CodeNamespace ns = new CodeNamespace ("Namespace1");
        cu.Namespaces.Add (ns);

        // GENERATES (C#):
        //
        //   namespace Namespace1 {
        //      public class Class1 {
        //          public int Method1 {
        //              #line 300 "LinedStatement"
        //              return 0;
        //
        //              #line default
        //              #line hidden
        //          }
        //      }
        //   }

        CodeTypeDeclaration class1 = new CodeTypeDeclaration ("Class1");
        class1.IsClass = true;
        class1.Attributes = MemberAttributes.Public;
        ns.Types.Add (class1);

        CodeMemberMethod method1 = new CodeMemberMethod ();
        method1.ReturnType = new CodeTypeReference (typeof (int));
        method1.Name = "Method1";
        class1.Members.Add (method1);

        AddScenario ("FindLinedStatement");
        CodeMethodReturnStatement ret = new CodeMethodReturnStatement (new CodePrimitiveExpression (0));
        ret.LinePragma = new CodeLinePragma ("LinedStatement", 300);
        method1.Statements.Add (ret);
    }

    public override void Search (CodeDomProvider provider, String output) {
        if (output.IndexOf ("LinedStatement") >= 0)
            VerifyScenario ("FindLinedStatement");
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
    }
}

