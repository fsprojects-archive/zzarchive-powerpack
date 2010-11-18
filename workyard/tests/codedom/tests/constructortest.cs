using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.VisualBasic;
using Microsoft.JScript;

public class ConstructorTest : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Everett;
        }
    }

    public override bool ShouldVerify {
        get {
            return true;
        }
    }

    public override bool ShouldCompile {
        get {
            return true;
        }
    }


    public override string Name {
        get {
            return "ConstructorTest";
        }
    }

    public override string Description {
        get {
            return "Tests constructors.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        // create a namespace
        CodeNamespace ns = new CodeNamespace ("NS");
        ns.Imports.Add (new CodeNamespaceImport ("System"));
        cu.Namespaces.Add (ns);

        // create a class that will be used to test the constructors of other classes
        CodeTypeDeclaration class1 = new CodeTypeDeclaration ();
        class1.Name = "Test";
        class1.IsClass = true;
        ns.Types.Add (class1);

        CodeMemberMethod cmm;

        // construct a method to test class with chained public constructors
        // GENERATES (C#):
        //        public static string TestingMethod1() {
        //                Test2 t = new Test2();
        //                return t.accessStringField;
        //            }
        if (Supports (provider, GeneratorSupport.ChainedConstructorArguments)) {
            AddScenario ("CheckTestingMethod01");
            cmm = new CodeMemberMethod ();
            cmm.Name = "TestingMethod1";
            cmm.Attributes = MemberAttributes.Public;
            cmm.ReturnType = new CodeTypeReference (typeof (String));
            // utilize constructor
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("Test2", "t", new CodeObjectCreateExpression ("Test2")));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodePropertyReferenceExpression (
                new CodeVariableReferenceExpression ("t"), "accessStringField")));
            class1.Members.Add (cmm);
        }

        // construct a method to test class with base public constructor
        // GENERATES (C#):
        //        public static string TestingMethod2() {
        //                Test3 t = new Test3();
        //                return t.accessStringField;
        //            }
        AddScenario ("CheckTestingMethod02");
        cmm = new CodeMemberMethod ();
        cmm.Name = "TestingMethod2";
        cmm.Attributes = MemberAttributes.Public;
        cmm.ReturnType = new CodeTypeReference (typeof (String));
        // utilize constructor
        cmm.Statements.Add (new CodeVariableDeclarationStatement ("Test3", "t", new CodeObjectCreateExpression ("Test3")));
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodePropertyReferenceExpression (
            new CodeVariableReferenceExpression ("t"), "accessStringField")));

        class1.Members.Add (cmm);
        // construct a method to test class with internal constructor
        // GENERATES (C#):
        //        public static int TestInternalConstruct(int a) {
        //                ClassWInternalConstruct t = new ClassWInternalConstruct();
        //                t.i = a;
        //                return t.i;
        //            }

        CodeParameterDeclarationExpression param = null;

#if !WHIDBEY
        // Everett VB compiler doesn't like this construct
        if (!(provider is VBCodeProvider)) {
#endif
            AddScenario ("CheckTestInternalConstruct");
            cmm = new CodeMemberMethod ();
            cmm.Name = "TestInternalConstruct";
            cmm.Attributes = MemberAttributes.Public;
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            param = new CodeParameterDeclarationExpression (typeof (int), "a");
            cmm.Parameters.Add (param);
            // utilize constructor
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("ClassWInternalConstruct", "t", new CodeObjectCreateExpression ("ClassWInternalConstruct")));
            // set then get number
            cmm.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeVariableReferenceExpression ("t"), "i")
                , new CodeArgumentReferenceExpression ("a")));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodePropertyReferenceExpression (
                new CodeVariableReferenceExpression ("t"), "i")));
            class1.Members.Add (cmm);
#if !WHIDBEY
        }
#endif



        // construct a method to test class with static constructor
        // GENERATES (C#):
        //        public static int TestStaticConstructor(int a) {
        //                Test4 t = new Test4();
        //                t.i = a;
        //                return t.i;
        //            }
        if (Supports (provider, GeneratorSupport.StaticConstructors)) {
            AddScenario ("CheckTestStaticConstructor");
            cmm = new CodeMemberMethod ();
            cmm.Name = "TestStaticConstructor";
            cmm.Attributes = MemberAttributes.Public;
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            param = new CodeParameterDeclarationExpression (typeof (int), "a");
            cmm.Parameters.Add (param);
            // utilize constructor
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("Test4", "t", new CodeObjectCreateExpression ("Test4")));
            // set then get number
            cmm.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeVariableReferenceExpression ("t"), "i")
                , new CodeArgumentReferenceExpression ("a")));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodePropertyReferenceExpression (
                new CodeVariableReferenceExpression ("t"), "i")));
            class1.Members.Add (cmm);
        }


        //  *** second class, tests chained, public constructors ***
        // GENERATES (C#):
        //        public class Test2 { 
        //            private string stringField;
        //            public Test2() : 
        //                    this("testingString", null, null) {
        //            }
        //            public Test2(String p1, String p2, String p3) {
        //                this.stringField = p1;
        //            }
        //            public string accessStringField {
        //                get {
        //                    return this.stringField;
        //                }
        //                set {
        //                    this.stringField = value;
        //                }
        //            }
        //        } 
        class1 = new CodeTypeDeclaration ();
        class1.Name = "Test2";
        class1.IsClass = true;
        ns.Types.Add (class1);

        class1.Members.Add (new CodeMemberField (new CodeTypeReference (typeof (String)), "stringField"));
        CodeMemberProperty prop = new CodeMemberProperty ();
        prop.Name = "accessStringField";
        prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        prop.Type = new CodeTypeReference (typeof (String));
        prop.GetStatements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (new CodeThisReferenceExpression (),
            "stringField")));
        prop.SetStatements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (new
            CodeThisReferenceExpression (), "stringField"),
            new CodePropertySetValueReferenceExpression ()));
        class1.Members.Add (prop);

        CodeConstructor cctor;
        if (Supports (provider, GeneratorSupport.ChainedConstructorArguments)) {
            cctor = new CodeConstructor ();
            cctor.Attributes = MemberAttributes.Public;
            cctor.ChainedConstructorArgs.Add (new CodePrimitiveExpression ("testingString"));
            cctor.ChainedConstructorArgs.Add (new CodePrimitiveExpression (null));
            cctor.ChainedConstructorArgs.Add (new CodePrimitiveExpression (null));
            class1.Members.Add (cctor);
        }

        CodeConstructor cc = new CodeConstructor ();
        cc.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded;
        cc.Parameters.Add (new CodeParameterDeclarationExpression ("String", "p1"));
        cc.Parameters.Add (new CodeParameterDeclarationExpression ("String", "p2"));
        cc.Parameters.Add (new CodeParameterDeclarationExpression ("String", "p3"));
        cc.Statements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (new CodeThisReferenceExpression ()
            , "stringField"), new CodeArgumentReferenceExpression ("p1")));
        class1.Members.Add (cc);


        // **** third class tests base constructors ****
        // GENERATES (C#):
        //    public class Test3 : Test2 {
        //            public Test3() : 
        //                    base("testingString", null, null) {
        //            }
        //        }
        class1 = new CodeTypeDeclaration ();
        class1.Name = "Test3";
        class1.IsClass = true;
        class1.BaseTypes.Add (new CodeTypeReference ("Test2"));
        ns.Types.Add (class1);

        cctor = new CodeConstructor ();
        cctor.Attributes = MemberAttributes.Public;
        cctor.BaseConstructorArgs.Add (new CodePrimitiveExpression ("testingString"));
        cctor.BaseConstructorArgs.Add (new CodePrimitiveExpression (null));
        cctor.BaseConstructorArgs.Add (new CodePrimitiveExpression (null));
        class1.Members.Add (cctor);


        if (Supports (provider, GeneratorSupport.StaticConstructors)) {
            // *** fourth class tests static constructors ****
            // GENERATES (C#):
            //    public class Test4 {
            //            private int number;
            //            static Test4() {
            //            }
            //            public int i {
            //                get {
            //                    return number;
            //                }
            //                set {
            //                    number = value;
            //                }
            //            }
            //        }
            class1 = new CodeTypeDeclaration ();
            class1.Name = "Test4";
            class1.IsClass = true;
            ns.Types.Add (class1);

            class1.Members.Add (new CodeMemberField (new CodeTypeReference (typeof (int)), "number"));
            prop = new CodeMemberProperty ();
            prop.Name = "i";
            prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            prop.Type = new CodeTypeReference (typeof (int));
            prop.GetStatements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (null, "number")));
            prop.SetStatements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (null, "number"),
                new CodePropertySetValueReferenceExpression ()));
            class1.Members.Add (prop);


            CodeTypeConstructor ctc = new CodeTypeConstructor ();
            class1.Members.Add (ctc);
        }

        // *******  class tests internal constructors **********
        // GENERATES (C#):
        //    public class ClassWInternalConstruct {
        //            private int number;
        //            /*FamANDAssem*/ internal ClassWInternalConstruct() {
        //            }
        //            public int i {
        //                get {
        //                    return number;
        //                }
        //                set {
        //                    number = value;
        //                }
        //            }
        //        }
        class1 = new CodeTypeDeclaration ();
        class1.Name = "ClassWInternalConstruct";
        class1.IsClass = true;
        ns.Types.Add (class1);

        class1.Members.Add (new CodeMemberField (new CodeTypeReference (typeof (int)), "number"));
        prop = new CodeMemberProperty ();
        prop.Name = "i";
        prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        prop.Type = new CodeTypeReference (typeof (int));
        prop.GetStatements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (null, "number")));
        prop.SetStatements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (null, "number"),
            new CodePropertySetValueReferenceExpression ()));
        class1.Members.Add (prop);

        if (!(provider is JScriptCodeProvider)) {
            cctor = new CodeConstructor ();
            cctor.Attributes = MemberAttributes.FamilyOrAssembly;
            class1.Members.Add (cctor);
        }

        // ******** class tests private constructors **********
        // GENERATES (C#):
        //    public class ClassWPrivateConstruct {
        //            static int number;
        //            private ClassWPrivateConstruct() {
        //            }
        //        }
        class1 = new CodeTypeDeclaration ();
        class1.Name = "ClassWPrivateConstruct";
        class1.IsClass = true;
        ns.Types.Add (class1);

        cctor = new CodeConstructor ();
        cctor.Attributes = MemberAttributes.Private;
        class1.Members.Add (cctor);


        // ******* class tests protected constructors **************
        // GENERATES (C#):
        //    public class ClassWProtectedConstruct {
        //            protected ClassWProtectedConstruct() {
        //            }
        //        }
        class1 = new CodeTypeDeclaration ();
        class1.Name = "ClassWProtectedConstruct";
        class1.IsClass = true;
        ns.Types.Add (class1);

        cctor = new CodeConstructor ();
        cctor.Attributes = MemberAttributes.Family;
        class1.Members.Add (cctor);

        // class that inherits protected constructor
        // GENERATES (C#):
        //    public class InheritsProtectedConstruct : ClassWProtectedConstruct {
        //    }
        class1 = new CodeTypeDeclaration ();
        class1.Name = "InheritsProtectedConstruct";
        class1.IsClass = true;
        class1.BaseTypes.Add (new CodeTypeReference ("ClassWProtectedConstruct"));
        ns.Types.Add (class1);


    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateTest", "Find and instantiate Test.");
        if (!FindAndInstantiate ("NS.Test", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTest");

        if (Supports (provider, GeneratorSupport.ChainedConstructorArguments) &&
                VerifyMethod (genType, genObject, "TestingMethod1", new object[] {}, "testingString")) {
            VerifyScenario ("CheckTestingMethod01");
        }

        if (VerifyMethod (genType, genObject, "TestingMethod2", new object[] {}, "testingString")) {
            VerifyScenario ("CheckTestingMethod02");
        }

        if (Supports (provider, GeneratorSupport.StaticConstructors)) {
            if (VerifyMethod (genType, genObject, "TestStaticConstructor", new object[] {7}, 7)) {
                VerifyScenario ("CheckTestStaticConstructor");
            }
        }

#if !WHIDBEY
        if (!(provider is VBCodeProvider)) {
#endif
            if (VerifyMethod (genType, genObject, "TestInternalConstruct", new object[] {7}, 7)) {
               VerifyScenario ("CheckTestInternalConstruct");
            }
#if !WHIDBEY
        }
#endif

        if (FindType ("ClassWPrivateConstruct", asm, out genType)) {
            // should be empty
            if (genType.GetConstructors ().Length <= 0)
                VerifyScenario ("ClassWPrivateConstruct");
        }

        if (FindType ("ClassWProtectedConstruct", asm, out genType)) {
            // should be empty
            if (genType.GetConstructors ().Length <= 0)
                VerifyScenario ("ClassWProtectedConstruct");
        }
    }
}

