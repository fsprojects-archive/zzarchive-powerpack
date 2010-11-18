using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Globalization;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class CodeSnippetTest : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Everett;
        }
    }

    public override bool ShouldCompile {
        get {
            return false;
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

    public override string Name {
        get {
            return "CodeSnippetTest";
        }
    }

    public override string Description {
        get {
            return "Tests code snippets.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        // GENERATES (C#):
        //
        //  namespace NSPC {
        //      
        //      public class ClassWithMethod {
        //          
        //          public int MethodName() {
        //              This is a CODE SNIPPET #*$*@;
        //              return 3;
        //          }
        //      }
        //  }
        AddScenario ("FindSnippet", "Find code snippet in the code.");
        CodeNamespace nspace = new CodeNamespace ("NSPC");
        cu.Namespaces.Add (nspace);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration ("ClassWithMethod");
        class1.IsClass = true;
        nspace.Types.Add (class1);

        CodeMemberMethod cmm = new CodeMemberMethod ();
        cmm.Name = "MethodName";
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Statements.Add (new CodeExpressionStatement (new CodeSnippetExpression ("This is a CODE SNIPPET #*$*@")));
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (3)));
        class1.Members.Add (cmm);
    }

    public override void Search (CodeDomProvider provider, String output) {
        int index;

        // find the snippet
        String str = "This is a CODE SNIPPET #*$*@";
        index = output.IndexOf (str);
        if (index >= 0 &&
                String.Compare (str, 0, output, index, str.Length, false, CultureInfo.InvariantCulture) == 0)
            VerifyScenario ("FindSnippet");
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        // only search
    }
}

