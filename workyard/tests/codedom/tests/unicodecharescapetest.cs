using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.JScript;

public class UnicodeCharEscapeTest : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Everett;
        }
    }

    public override string Name {
        get {
            return "UnicodeCharEscapeTest";
        }
    }

    public override string Description {
        get {
            return "Test All the Unicode characters for escaping";
        }
    }

    public override bool ShouldVerify {
        get {
            return false;
        }
    }

    public override bool ShouldCompile {
        get {
            return true;
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        if (!(provider is JScriptCodeProvider)) {
            // GENERATES (C#):
            //
            //  namespace Namespace1 {
            //      
            //      public class TEST {
            //          
            //          public static void Main() {
            //              // the following is repeated Char.MaxValue times
            //              System.Console.WriteLine(/* character value goes here */);
            //          }
            //      }
            //  }
            CodeNamespace ns = new CodeNamespace ("Namespace1");
            ns.Imports.Add (new CodeNamespaceImport ("System"));
            cu.Namespaces.Add (ns);

            CodeTypeDeclaration cd = new CodeTypeDeclaration ("TEST");
            cd.IsClass = true;
            ns.Types.Add (cd);
            CodeEntryPointMethod methodMain = new CodeEntryPointMethod ();

            for (int i = 0; i < Char.MaxValue; i+=50)
                methodMain.Statements.Add (CDHelper.ConsoleWriteLineStatement (new CodePrimitiveExpression (System.Convert.ToChar (i))));

            cd.Members.Add (methodMain);
        }
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm)  {
    }
}

