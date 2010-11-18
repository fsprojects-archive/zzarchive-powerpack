using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Samples.CodeDomTestSuite;

public class TypeOfTest : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Subset;
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
            return "TypeOfTest";
        }
    }

    public override string Description {
        get {
            return "Tests typeof statements.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {


        // GENERATES (C#):
        //
        //  namespace NSPC {
        //      using System;
        //      using System.Windows.Forms;
        //      
        //      
        //      public class ClassToTest {
        //          
        //          public string Primitives() {
        //              return typeof(int).ToString();
        //          }
        //          
        //          public string ArraysOfPrimitives() {
        //              return typeof(int[]).ToString();
        //          }
        //          
        //          public string NonPrimitives() {
        //              return typeof(System.ICloneable).ToString();
        //          }
        //          
        //          public string ArraysOfNonPrimitives() {
        //              return typeof(System.ICloneable[]).ToString();
        //          }
        //          
        //          public string GetSomeClass() {
        //              return typeof(SomeClass).ToString();
        //          }
        //      }
        //      
        //      public class SomeClass {
        //      }
        //  }

        CodeNamespace nspace = new CodeNamespace ("NSPC");
        nspace.Imports.Add (new CodeNamespaceImport ("System"));
        nspace.Imports.Add (new CodeNamespaceImport ("System.Windows.Forms"));
        cu.Namespaces.Add (nspace);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration ("ClassToTest");
        class1.IsClass = true;
        nspace.Types.Add (class1);

        AddScenario ("CheckPrimitives");
        CodeMemberMethod cmm = new CodeMemberMethod ();
        cmm.Name = "Primitives";
        cmm.ReturnType = new CodeTypeReference (typeof (string));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (
            new CodeTypeOfExpression (typeof (int)), "ToString")));
        class1.Members.Add (cmm);

        AddScenario ("CheckArraysOfPrimitives");
        cmm = new CodeMemberMethod ();
        cmm.Name = "ArraysOfPrimitives";
        cmm.ReturnType = new CodeTypeReference (typeof (string));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (
            new CodeTypeOfExpression (typeof (int[])), "ToString")));
        class1.Members.Add (cmm);

        AddScenario ("CheckNonPrimitives");
        cmm = new CodeMemberMethod ();
        cmm.Name = "NonPrimitives";
        cmm.ReturnType = new CodeTypeReference (typeof (string));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (
            new CodeTypeOfExpression (typeof (System.ICloneable)), "ToString")));
        class1.Members.Add (cmm);

        AddScenario ("CheckArraysOfNonPrimitives");
        cmm = new CodeMemberMethod ();
        cmm.Name = "ArraysOfNonPrimitives";
        cmm.ReturnType = new CodeTypeReference (typeof (string));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (
            new CodeTypeOfExpression (typeof (System.ICloneable[])), "ToString")));
        class1.Members.Add (cmm);

        AddScenario ("CheckGetSomeClass");
        cmm = new CodeMemberMethod ();
        cmm.Name = "GetSomeClass";
        cmm.ReturnType = new CodeTypeReference (typeof (string));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (
            new CodeTypeOfExpression ("SomeClass"), "ToString")));
        class1.Members.Add (cmm);

        CodeTypeDeclaration ce = new CodeTypeDeclaration ("SomeClass");
        ce.IsClass = true;
        ce.Attributes = MemberAttributes.Public;
        nspace.Types.Add (ce);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateClassToTest", "Find and instantiate ClassToTest.");
        if (!FindAndInstantiate ("NSPC.ClassToTest", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateClassToTest");

        if (VerifyMethod (genType, genObject, "Primitives", new object[] {}, typeof (int).ToString ())) {
            VerifyScenario ("CheckPrimitives");
        }
        if (VerifyMethod (genType, genObject, "ArraysOfPrimitives", new object[] {}, typeof (int[]).ToString ())) {
            VerifyScenario ("CheckArraysOfPrimitives");
        }
        if (VerifyMethod (genType, genObject, "NonPrimitives", new object[] {}, typeof (System.ICloneable).ToString ())) {
            VerifyScenario ("CheckNonPrimitives");
        }
        if (VerifyMethod (genType, genObject, "ArraysOfNonPrimitives", new object[] {}, typeof (System.ICloneable[]).ToString ())) {
            VerifyScenario ("CheckArraysOfNonPrimitives");
        }
        if (VerifyMethod (genType, genObject, "GetSomeClass", new object[] {}, "SomeClass") ||
                    VerifyMethod (genType, genObject, "GetSomeClass", new object[] {}, "NSPC.SomeClass")) {
            VerifyScenario ("CheckGetSomeClass");
        }
    }
}

