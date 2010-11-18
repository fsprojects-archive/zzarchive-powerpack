using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class CreateObjectTest : CodeDomTestTree {

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
            return "CreateObjectTest";
        }
    }

    public override string Description {
        get {
            return "Tests object creation.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {
        // GENERATES (C#):
        //
        //  namespace NSPC {
        //      
        //      public class ClassWithMethod {
        //          
        //          public int Method() {
        //              return 4;
        //          }
        //      }
        //      
        //      public class ClassToTest {
        //          
        //          public int TestMethod() {
        //              ClassWithMethod tmp = new ClassWithMethod(2);
        //              ClassWithMethod temp = new ClassWithMethod();
        //              return temp.Method();
        //          }
        //          
        //          public int SecondTestMethod() {
        //              int a = 3;
        //              int b;
        //              b = 87;
        //              return (b - a);
        //          }
        //      }
        //  }

        CodeNamespace nspace = new CodeNamespace ("NSPC");
        cu.Namespaces.Add (nspace);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration ("ClassWithMethod");
        class1.IsClass = true;
        nspace.Types.Add (class1);

        // parameterless constructor
        CodeConstructor cons = new CodeConstructor ();
        cons.Attributes = MemberAttributes.Public;
        class1.Members.Add (cons);

        // parameterful constructor
        cons = new CodeConstructor ();
        cons.Attributes = MemberAttributes.Public;
        cons.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "myParameter"));
        cons.Statements.Add (new CodeVariableDeclarationStatement (
                    typeof (int), "noUse",
                    new CodeArgumentReferenceExpression ("myParameter")));
        class1.Members.Add (cons);

        CodeMemberMethod cmm = new CodeMemberMethod ();
        cmm.Name = "Method";
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (4)));
        class1.Members.Add (cmm);

        class1 = new CodeTypeDeclaration ("ClassToTest");
        class1.IsClass = true;
        nspace.Types.Add (class1);

        AddScenario ("CheckTestMethod", "Check the return value of TestMethod.");
        cmm = new CodeMemberMethod ();
        cmm.Name = "TestMethod";
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        cmm.ReturnType = new CodeTypeReference (typeof (int));

        // create a new class with the parameter
        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference ("ClassWithMethod"),
            "tmp", new CodeObjectCreateExpression (new CodeTypeReference ("ClassWithMethod"),
                new CodePrimitiveExpression (2))));

        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference ("ClassWithMethod"),
            "temp", new CodeObjectCreateExpression (new CodeTypeReference ("ClassWithMethod"))));
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (
            new CodeVariableReferenceExpression ("temp"), "Method")));
        class1.Members.Add (cmm);

        AddScenario ("CheckSecondTestMethod", "Check the return value of SecondTestMethod.");
        cmm = new CodeMemberMethod ();
        cmm.Name = "SecondTestMethod";
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference (typeof (int)), "a", new CodePrimitiveExpression (3)));
        cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference (typeof (int)), "b"));
        cmm.Statements.Add (new CodeAssignStatement (new CodeVariableReferenceExpression ("b"), new CodePrimitiveExpression (87)));
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (new
            CodeVariableReferenceExpression ("b"), CodeBinaryOperatorType.Subtract,
            new CodeVariableReferenceExpression ("a"))));
        class1.Members.Add (cmm);


    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateClassToTest", "Find and instantiate ClassToTest.");
        if (!FindAndInstantiate ("NSPC.ClassToTest", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateClassToTest");

        // verify method which has declaration of a non-primitive type
        if (VerifyMethod (genType, genObject, "TestMethod", new object[] {}, 4)) {
            VerifyScenario ("CheckTestMethod");
        }
        if (VerifyMethod (genType, genObject, "SecondTestMethod", new object[] {}, 84)) {
            VerifyScenario ("CheckSecondTestMethod");
        }
    }

}

