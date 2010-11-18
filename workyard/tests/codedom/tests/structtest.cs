using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Drawing;
using Microsoft.Samples.CodeDomTestSuite;

public class StructTest : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Everett;
        }
    }

    public override string Name {
        get {
            return "StructTest";
        }
    }

    public override string Description {
        get {
            return "Tests structs";
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

        // create a namespace
        CodeNamespace ns = new CodeNamespace ("NS");
        ns.Imports.Add (new CodeNamespaceImport ("System"));
        ns.Imports.Add (new CodeNamespaceImport ("System.Drawing"));
        cu.Namespaces.Add (ns);

        cu.ReferencedAssemblies.Add ("System.Drawing.dll");

        // create a class
        CodeTypeDeclaration class1 = new CodeTypeDeclaration ();
        class1.Name = "Test";
        class1.IsClass = true;
        ns.Types.Add (class1);
        if (Supports (provider, GeneratorSupport.DeclareValueTypes)) {

            // create first struct to test nested structs
            //     GENERATE (C#):
            //	public struct structA {
            //		public structB innerStruct;
            //		public struct structB {
            //    		public int int1;
            //		}
            //	}
            CodeTypeDeclaration structA = new CodeTypeDeclaration ("structA");
            structA.IsStruct = true;

            CodeTypeDeclaration structB = new CodeTypeDeclaration ("structB");
            structB.Attributes = MemberAttributes.Public;
            structB.IsStruct = true;

            CodeMemberField firstInt = new CodeMemberField (typeof (int), "int1");
            firstInt.Attributes = MemberAttributes.Public;
            structB.Members.Add (firstInt);

            CodeMemberField innerStruct = new CodeMemberField ("structB", "innerStruct");
            innerStruct.Attributes = MemberAttributes.Public;

            structA.Members.Add (structB);
            structA.Members.Add (innerStruct);
            class1.Members.Add (structA);

            // create second struct to test tructs of non-primitive types
            //     GENERATE (C#):
            //         public struct structC {
            //              public Point pt1;
            //              public Point pt2;
            //         }
            CodeTypeDeclaration structC = new CodeTypeDeclaration ("structC");
            structC.IsStruct = true;

            CodeMemberField firstPt = new CodeMemberField ("Point", "pt1");
            firstPt.Attributes = MemberAttributes.Public;
            structC.Members.Add (firstPt);

            CodeMemberField secondPt = new CodeMemberField ("Point", "pt2");
            secondPt.Attributes = MemberAttributes.Public;
            structC.Members.Add (secondPt);
            class1.Members.Add (structC);

            // create method to test nested struct
            //     GENERATE (C#):
            //          public static int NestedStructMethod() {
            //               structA varStructA;
            //               varStructA.innerStruct.int1 = 3;
            //               return varStructA.innerStruct.int1;
            //          }
            AddScenario ("CheckNestedStructMethod");
            CodeMemberMethod nestedStructMethod = new CodeMemberMethod ();
            nestedStructMethod.Name = "NestedStructMethod";
            nestedStructMethod.ReturnType = new CodeTypeReference (typeof (int));
            nestedStructMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            CodeVariableDeclarationStatement varStructA = new CodeVariableDeclarationStatement ("structA", "varStructA");
            nestedStructMethod.Statements.Add (varStructA);
            nestedStructMethod.Statements.Add (
                new CodeAssignStatement (
                /* Expression1 */ new CodeFieldReferenceExpression (new CodeFieldReferenceExpression (new CodeVariableReferenceExpression ("varStructA"), "innerStruct"), "int1"),
                /* Expression1 */ new CodePrimitiveExpression (3))
                );
            nestedStructMethod.Statements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (new CodeFieldReferenceExpression (new CodeVariableReferenceExpression ("varStructA"), "innerStruct"), "int1")));
            class1.Members.Add (nestedStructMethod);


            // create method to test nested non primitive struct member
            //     GENERATE (C#):
            //          public static System.Drawing.Point NonPrimitiveStructMethod() {
            //               structC varStructC;
            //               varStructC.pt1 = new Point(1, -1);
            //               return varStructC.pt1;
            //          }
            AddScenario ("CheckNonPrimitiveStructMethod");
            CodeMemberMethod nonPrimitiveStructMethod = new CodeMemberMethod ();
            nonPrimitiveStructMethod.Name = "NonPrimitiveStructMethod";
            nonPrimitiveStructMethod.ReturnType = new CodeTypeReference (typeof (Point));
            nonPrimitiveStructMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            CodeVariableDeclarationStatement varStructC = new CodeVariableDeclarationStatement ("structC", "varStructC");
            nonPrimitiveStructMethod.Statements.Add (varStructC);
            nonPrimitiveStructMethod.Statements.Add (
                new CodeAssignStatement (
                /* Expression1 */ new CodeFieldReferenceExpression (
                new CodeVariableReferenceExpression ("varStructC"),
                "pt1"),
                /* Expression2 */ new CodeObjectCreateExpression ("Point", new CodeExpression[] {new CodePrimitiveExpression (1), new CodePrimitiveExpression (-1)})
                ));
            nonPrimitiveStructMethod.Statements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (new CodeVariableReferenceExpression ("varStructC"), "pt1")));
            class1.Members.Add (nonPrimitiveStructMethod);
        }
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        if (Supports (provider, GeneratorSupport.DeclareValueTypes)) {
            object genObject;
            Type   genType;

            AddScenario ("InstantiateTest", "Find and instantiate Test.");
            if (!FindAndInstantiate ("NS.Test", asm, out genObject, out genType))
                return;
            VerifyScenario ("InstantiateTest");

            // verify goto which jumps ahead to label with statement 
            if (VerifyMethod (genType, genObject, "NestedStructMethod", new object[] {}, 3)) {
                VerifyScenario ("CheckNestedStructMethod");
            }

            if (VerifyMethod (genType, genObject, "NonPrimitiveStructMethod", new object[] {}, new Point (1, -1))) {
                VerifyScenario ("CheckNonPrimitiveStructMethod");
            }
        }
    }
}

