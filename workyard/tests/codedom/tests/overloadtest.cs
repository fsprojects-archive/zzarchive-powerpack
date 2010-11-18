using System;
using System.CodeDom;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.Samples.CodeDomTestSuite;

public class OverloadTest : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Subset;
        }
    }

    public override string Name {
        get {
            return "OverloadTest";
        }
    }

    public override string Description {
        get {
            return "Tests method overloading.";
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

        // GENERATES (C#):
        //
        //  public class MyConverter : System.ComponentModel.TypeConverter {
        //      
        //      private void Foo() {
        //          this.Foo(null);
        //      }
        //      
        //      private void Foo(string s) {
        //      }
        //      
        //      public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType) {
        //          return base.CanConvertFrom(context, sourceType);
        //      }
        //  }

        CodeNamespace ns = new CodeNamespace ();
        cu.Namespaces.Add (ns);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration ();
        class1.Name = "MyConverter";
        class1.BaseTypes.Add (new CodeTypeReference (typeof (System.ComponentModel.TypeConverter)));
        ns.Types.Add (class1);

        CodeMemberMethod foo1 = new CodeMemberMethod ();
        foo1.Name = "Foo";
        foo1.Statements.Add (new CodeMethodInvokeExpression (new CodeThisReferenceExpression (), "Foo", new CodePrimitiveExpression (null)));
        class1.Members.Add (foo1);

        CodeMemberMethod foo2 = new CodeMemberMethod ();
        foo2.Name = "Foo";
        foo2.Parameters.Add (new CodeParameterDeclarationExpression (typeof (string), "s"));
        class1.Members.Add (foo2);

        CodeMemberMethod convert = new CodeMemberMethod ();
        convert.Name = "CanConvertFrom";
        convert.Attributes = MemberAttributes.Public | MemberAttributes.Override | MemberAttributes.Overloaded;
        convert.ReturnType = new CodeTypeReference (typeof (bool));
        convert.Parameters.Add (new CodeParameterDeclarationExpression (typeof (System.ComponentModel.ITypeDescriptorContext), "context"));
        convert.Parameters.Add (new CodeParameterDeclarationExpression (typeof (System.Type), "sourceType"));
        convert.Statements.Add (
            new CodeMethodReturnStatement (
            new CodeMethodInvokeExpression (
            new CodeBaseReferenceExpression (),
            "CanConvertFrom",
            new CodeArgumentReferenceExpression ("context"),
            new CodeArgumentReferenceExpression ("sourceType"))));
        class1.Members.Add (convert);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
    }
}

