using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.VisualBasic;

public class TypeTest : CodeDomTestTree {

    public override string Comment
    {
        get { return "public static fields not allowed in F#"; }
    }

    public override TestTypes TestType
    {
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
            return true;
        }
    }

    public override string Name {
        get {
            return "TypeTest";
        }
    }

    public override string Description {
        get {
            return "Tests generating and using objects of different types.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        CodeNamespace nspace = new CodeNamespace ("NSPC");
        cu.Namespaces.Add (nspace);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration ("ClassWithField");
        class1.IsClass = true;
        nspace.Types.Add (class1);

        // internal modifier
        //		public class ClassWithField {
        //			/*FamANDAssem*/ internal static int InternalField = 0;
        //		}
        CodeMemberField field = new CodeMemberField ();
        field.Name = "InternalField";
        field.Attributes = MemberAttributes.Assembly | MemberAttributes.Static;
        field.Type = new CodeTypeReference (typeof (int));
        field.InitExpression = new CodePrimitiveExpression (0);
        class1.Members.Add (field);

        class1 = new CodeTypeDeclaration ("ClassToTest");
        class1.IsClass = true;
        nspace.Types.Add (class1);

        // test internal field
        //      public static int UseInternalField(int i) {
        //			ClassWithField.InternalField = i;
        //			return ClassWithField.InternalField;
        //		}
        AddScenario ("CheckUseInternalField");
        CodeMemberMethod cmm = new CodeMemberMethod ();
        cmm.Name = "UseInternalField";
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
        cmm.Statements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (new
            CodeTypeReferenceExpression ("ClassWithField"), "InternalField"),
            new CodeArgumentReferenceExpression ("i")));
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (new
            CodeTypeReferenceExpression ("ClassWithField"), "InternalField")));
        class1.Members.Add (cmm);

        // nested array reference
        //        public int MoreArrayTests(int i) {
        //			int[][] arrayOfArrays = new int[][] {
        //					new int[] {
        //							3,
        //							4},
        //					new int[1],
        //					new int[0]};
        //			return (arrayOfArrays[0][1] + i);
        //		}
        // VB code provider doesn't support array of array initialization
        if (Supports (provider, GeneratorSupport.ArraysOfArrays) && !(provider is VBCodeProvider)) {
            AddScenario ("CheckMoreArrayTests");
            CodeMemberMethod secondMethod = new CodeMemberMethod ();
            secondMethod.Name = "MoreArrayTests";
            secondMethod.Attributes = (secondMethod.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            secondMethod.ReturnType = new CodeTypeReference (typeof (int));
            secondMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
            // array of arrays
            secondMethod.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference (typeof (int[][])),
                "arrayOfArrays", new CodeArrayCreateExpression (typeof (int[][]),
                new CodeArrayCreateExpression (typeof (int[]), new CodePrimitiveExpression (3), new CodePrimitiveExpression (4)),
                new CodeArrayCreateExpression (typeof (int[]), new CodePrimitiveExpression (1)), new CodeArrayCreateExpression (typeof (int[])))));
            secondMethod.Statements.Add (new CodeMethodReturnStatement (
                new CodeBinaryOperatorExpression (new CodeArrayIndexerExpression (
                new CodeArrayIndexerExpression (new CodeVariableReferenceExpression ("arrayOfArrays"), new CodePrimitiveExpression (0))
                , new CodePrimitiveExpression (1)),
                CodeBinaryOperatorType.Add, new CodeArgumentReferenceExpression ("i"))));
            class1.Members.Add (secondMethod);
        }
        // value and reference
        if (Supports (provider, GeneratorSupport.ReferenceParameters)) {
            //    GENERATE (C#):      
            //    static void Work(ref int i, out int j) {
            //             i = (i + 4);
            //             j = 5;
            //   }

            cmm = new CodeMemberMethod ();
            cmm.Name = "Work";
            cmm.ReturnType = new CodeTypeReference ("System.void");
            cmm.Attributes = MemberAttributes.Static;
            // add parameter with ref direction
            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (typeof (int), "i");
            param.Direction = FieldDirection.Ref;
            cmm.Parameters.Add (param);
            // add parameter with out direction
            param = new CodeParameterDeclarationExpression (typeof (int), "j");
            param.Direction = FieldDirection.Out;
            cmm.Parameters.Add (param);
            cmm.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("i"),
                new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"),
                CodeBinaryOperatorType.Add, new CodePrimitiveExpression (4))));
            cmm.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("j"),
                new CodePrimitiveExpression (5)));
            class1.Members.Add (cmm);

            // add a method that calls method work to verify that ref and out are working properly 
            // GENERATE (C#):
            //       public static int CallingWork(int a) {
            //          a = 10;
            //          int b;
            //          TEST.Work(ref a, out b);
            //          return (a + b);
            //       }
            AddScenario ("CheckCallingWork");
            cmm = new CodeMemberMethod ();
            cmm.Name = "CallingWork";
            cmm.Attributes = MemberAttributes.Public;
            CodeParameterDeclarationExpression parames = new CodeParameterDeclarationExpression (typeof (int), "a");
            cmm.Parameters.Add (parames);
            cmm.ReturnType = new CodeTypeReference ("System.Int32");
            cmm.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("a"),
                new CodePrimitiveExpression (10)));
            cmm.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "b"));
            // invoke the method called "work"
            CodeMethodInvokeExpression methodinvoked = new CodeMethodInvokeExpression (new CodeMethodReferenceExpression
                (new CodeTypeReferenceExpression ("ClassToTest"), "Work"));
            // add parameter with ref direction
            CodeDirectionExpression parameter = new CodeDirectionExpression (FieldDirection.Ref,
                new CodeArgumentReferenceExpression ("a"));
            methodinvoked.Parameters.Add (parameter);
            // add parameter with out direction
            parameter = new CodeDirectionExpression (FieldDirection.Out, new CodeVariableReferenceExpression ("b"));
            methodinvoked.Parameters.Add (parameter);
            cmm.Statements.Add (methodinvoked);
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression
                (new CodeArgumentReferenceExpression ("a"), CodeBinaryOperatorType.Add, new CodeVariableReferenceExpression ("b"))));
            class1.Members.Add (cmm);
        }
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;
        
        AddScenario ("InstantiateClassToTest", "Find and instantiate ClassToTest.");
        if (!FindAndInstantiate ("NSPC.ClassToTest", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateClassToTest");

        // verify internal field
        if (VerifyMethod (genType, genObject, "UseInternalField", new object[] {2}, 2)) {
            VerifyScenario ("CheckUseInternalField");
        }
        if (Supports (provider, GeneratorSupport.ReferenceParameters) &&
                VerifyMethod (genType, genObject, "CallingWork", new object[] {7}, 19)) {
            VerifyScenario ("CheckCallingWork");
        }
        if (Supports (provider, GeneratorSupport.ArraysOfArrays) && !(provider is VBCodeProvider) &&
                VerifyMethod (genType, genObject, "MoreArrayTests", new object[] {19}, 23)) {
            VerifyScenario ("CheckMoreArrayTests");
        }
    }
}

