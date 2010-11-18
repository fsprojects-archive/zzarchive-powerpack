using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class CodePrimitiveExpressionTest : CodeDomTestTree {

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
            return false;
        }
    }

    public override string Name {
        get {
            return "CodePrimitiveExpressionTest";
        }
    }

    public override string Description {
        get {
            return "Tests coding primitive expressions.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {
        CodeNamespace ns = new CodeNamespace ();
        ns.Name = "MyNamespace";
        cu.Namespaces.Add (ns);

        // GENERATES (C#):
        //
        // namespace MyNamespace {
        //     public class MyClass {
        //         private void PrimitiveTest() {
        //             char var1 = 'a';
        //             char var2 = '\0';
        //             string var3 = "foo\0bar\0baz\0";
        //             object var4 = null;
        //             int var5 = 42;
        //             double var6 = 3.14;
        //             System.Console.Write(var1);
        //             System.Console.Write(var2);
        //             System.Console.Write(var3);
        //             System.Console.Write(var4);
        //             System.Console.Write(var5);
        //             System.Console.Write(var6);
        //         }
        //     }
        // }
        
        CodeTypeDeclaration class1 = new CodeTypeDeclaration ();
        class1.Name = "MyClass";
        class1.IsClass = true;
        class1.Attributes = MemberAttributes.Public;
        ns.Types.Add (class1);


        CodeMemberMethod method = new CodeMemberMethod ();
        method.Name = "PrimitiveTest";
        method.Statements.Add (new CodeVariableDeclarationStatement (typeof (char), "var1", new CodePrimitiveExpression ('a')));
        method.Statements.Add (new CodeVariableDeclarationStatement (typeof (char), "var2", new CodePrimitiveExpression ('\0')));
        method.Statements.Add (new CodeVariableDeclarationStatement (typeof (string), "var3", new CodePrimitiveExpression ("foo\0bar\0baz\0")));
        method.Statements.Add (new CodeVariableDeclarationStatement (typeof (Object), "var4", new CodePrimitiveExpression (null)));
        method.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "var5", new CodePrimitiveExpression (42)));
        method.Statements.Add (new CodeVariableDeclarationStatement (typeof (double), "var6", new CodePrimitiveExpression (3.14)));
        method.Statements.Add (new CodeMethodInvokeExpression (new CodeTypeReferenceExpression (typeof (Console)), "Write", new CodeVariableReferenceExpression ("var1")));
        method.Statements.Add (new CodeMethodInvokeExpression (new CodeTypeReferenceExpression (typeof (Console)), "Write", new CodeVariableReferenceExpression ("var2")));
        method.Statements.Add (new CodeMethodInvokeExpression (new CodeTypeReferenceExpression (typeof (Console)), "Write", new CodeVariableReferenceExpression ("var3")));
        method.Statements.Add (new CodeMethodInvokeExpression (new CodeTypeReferenceExpression (typeof (Console)), "Write", new CodeVariableReferenceExpression ("var4")));
        method.Statements.Add (new CodeMethodInvokeExpression (new CodeTypeReferenceExpression (typeof (Console)), "Write", new CodeVariableReferenceExpression ("var5")));
        method.Statements.Add (new CodeMethodInvokeExpression (new CodeTypeReferenceExpression (typeof (Console)), "Write", new CodeVariableReferenceExpression ("var6")));
        class1.Members.Add (method);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
    }
}

