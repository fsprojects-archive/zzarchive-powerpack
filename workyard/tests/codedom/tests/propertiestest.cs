using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Samples.CodeDomTestSuite;

public class PropertiesTest : CodeDomTestTree {



    public override TestTypes TestType {
        get {
            return TestTypes.Subset;
        }
    }

    public override string Name {
        get {
            return "PropertiesTest";
        }
    }

    public override string Description {
        get {
            return "Tests properties";
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
        cu.Namespaces.Add (ns);

        // create a class to inherit from, this will be used to test
        // overridden properties
        CodeTypeDeclaration decl = new CodeTypeDeclaration ();
        decl.Name = "BaseClass";
        decl.IsClass = true;
        ns.Types.Add (decl);

        decl.Members.Add (new CodeMemberField (typeof (string), "backing"));

        CodeMemberProperty textProp = new CodeMemberProperty ();
        textProp.Name = "Text";
        textProp.Attributes = MemberAttributes.Public;
        textProp.Type = new CodeTypeReference (typeof (string));
        textProp.GetStatements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression ("VooCar")));
        textProp.SetStatements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (null, "backing"),
                    new CodePropertySetValueReferenceExpression ()));
        decl.Members.Add (textProp);

        // create a class
        CodeTypeDeclaration class1 = new CodeTypeDeclaration ();
        class1.Name = "Test";
        class1.IsClass = true;
        class1.BaseTypes.Add (new CodeTypeReference ("BaseClass"));
        ns.Types.Add (class1);

        CodeMemberField int1 = new CodeMemberField (typeof (int), "int1");
        class1.Members.Add (int1);

        CodeMemberField tempString = new CodeMemberField (typeof (string), "tempString");
        class1.Members.Add (tempString);
        // Property that add 1 on a get
        //     GENERATE (C#):
        //        public virtual int prop1 {
        //            get {
        //                return (int1 + 1);
        //            }
        //            set {
        //                int1 = value;
        //            }
        //        }
        AddScenario ("Checkprop1");
        CodeMemberProperty prop1 = new CodeMemberProperty ();
        prop1.Name = "prop1";
        prop1.Type = new CodeTypeReference (typeof (int));
        prop1.Attributes = MemberAttributes.Public;
        prop1.HasGet = true;
        prop1.HasSet = true;
        prop1.GetStatements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (new CodeFieldReferenceExpression (null, "int1"),
                        CodeBinaryOperatorType.Add, new CodePrimitiveExpression (1))));
        prop1.SetStatements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (null, "int1"), new CodePropertySetValueReferenceExpression ()));
        class1.Members.Add (prop1);

        // override Property
        AddScenario ("CheckText");
        CodeMemberProperty overrideProp = new CodeMemberProperty ();
        overrideProp.Name = "Text";
        overrideProp.Type = new CodeTypeReference (typeof (string));
        overrideProp.Attributes = MemberAttributes.Public | MemberAttributes.Override;
        overrideProp.HasGet = true;
        overrideProp.HasSet = true;
        overrideProp.SetStatements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (null, "tempString"), new CodePropertySetValueReferenceExpression ()));
        overrideProp.GetStatements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression ("Hello World")));

        class1.Members.Add (overrideProp);

        // Private Property
        //     GENERATE (C#):
        //        private virtual int privProp1 {
        //            get {
        //                return (int1 + 1);
        //            }
        //            set {
        //                int1 = value;
        //            }
        //        }
        CodeMemberProperty privProp1 = new CodeMemberProperty ();
        privProp1.Name = "privProp1";
        privProp1.Type = new CodeTypeReference (typeof (int));
        privProp1.Attributes = MemberAttributes.Private;
        privProp1.HasGet = true;
        privProp1.HasSet = true;
        privProp1.GetStatements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (new CodeFieldReferenceExpression (null, "int1"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression (1))));
        privProp1.SetStatements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (null, "int1"), new CodePropertySetValueReferenceExpression ()));
        class1.Members.Add (privProp1);



        //     GENERATES (C#):
        //        protected virtual int protProp1 {
        //            get {
        //                return (int1 + 1);
        //            }
        //            set {
        //                int1 = value;
        //            }
        //        }
        CodeMemberProperty protProp1 = new CodeMemberProperty ();
        protProp1.Name = "protProp1";
        protProp1.Type = new CodeTypeReference (typeof (int));
        protProp1.Attributes = MemberAttributes.Family;
        protProp1.HasGet = true;
        protProp1.HasSet = true;
        protProp1.GetStatements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (new CodeFieldReferenceExpression (null, "int1"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression (1))));
        protProp1.SetStatements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (null, "int1"), new CodePropertySetValueReferenceExpression ()));
        class1.Members.Add (protProp1);

        // Internal Property
        //     GENERATE (C#):
        //        internal virtual int internalProp {
        //            get {
        //                return (int1 + 1);
        //            }
        //            set {
        //                int1 = value;
        //            }
        //        }
        CodeMemberProperty internalProp = new CodeMemberProperty ();
        internalProp.Name = "internalProp";
        internalProp.Type = new CodeTypeReference (typeof (int));
        internalProp.Attributes = MemberAttributes.Assembly;
        internalProp.HasGet = true;
        internalProp.HasSet = true;
        internalProp.GetStatements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (new CodeFieldReferenceExpression (null, "int1"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression (1))));
        internalProp.SetStatements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (null, "int1"), new CodePropertySetValueReferenceExpression ()));
        class1.Members.Add (internalProp);

        //     GENERATE (C#):
        //        public virtual int constStaticProp {
        //           get {
        //              return 99;
        //           }
        //        }

        AddScenario ("CheckconstStaticProp");
        CodeMemberProperty constStaticProp = new CodeMemberProperty ();
        constStaticProp.Name = "constStaticProp";
        constStaticProp.Type = new CodeTypeReference (typeof (int));
        constStaticProp.Attributes = MemberAttributes.Public | MemberAttributes.Const;
        constStaticProp.HasGet = true;
        constStaticProp.GetStatements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (99)));
        class1.Members.Add (constStaticProp);

        // 1) this reference
        //     GENERATE (C#):
        //        public virtual int thisRef(int value) {
        //            this.prop1 = value;
        //            return this.prop1;
        //        }
        AddScenario ("CheckthisRef");
        CodeMemberMethod thisRef = new CodeMemberMethod ();
        thisRef.Name = "thisRef";
        thisRef.ReturnType = new CodeTypeReference (typeof (int));
        thisRef.Attributes = MemberAttributes.Public;
        CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (typeof (int), "value");
        thisRef.Parameters.Add (param);

        thisRef.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeThisReferenceExpression (), "privProp1"), new CodeArgumentReferenceExpression ("value")));
        thisRef.Statements.Add (new CodeMethodReturnStatement (new CodePropertyReferenceExpression (new CodeThisReferenceExpression (), "privProp1")));
        class1.Members.Add (thisRef);


        // 1) protected
        //     GENERATE (C#):
        AddScenario ("CheckprotPropMethod");
        CodeMemberMethod protPropMethod = new CodeMemberMethod ();
        protPropMethod.Name = "protPropMethod";
        protPropMethod.ReturnType = new CodeTypeReference (typeof (int));
        protPropMethod.Attributes = MemberAttributes.Public;
        CodeParameterDeclarationExpression param2 = new CodeParameterDeclarationExpression (typeof (int), "value");
        protPropMethod.Parameters.Add (param2);

        protPropMethod.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeThisReferenceExpression (), "protProp1"), new CodeArgumentReferenceExpression ("value")));
        protPropMethod.Statements.Add (new CodeMethodReturnStatement (new CodePropertyReferenceExpression (new CodeThisReferenceExpression (), "protProp1")));
        class1.Members.Add (protPropMethod);

        // 1) internal
        //     GENERATE (C#):
        AddScenario ("CheckinternalPropMethod");
        CodeMemberMethod internalPropMethod = new CodeMemberMethod ();
        internalPropMethod.Name = "internalPropMethod";
        internalPropMethod.ReturnType = new CodeTypeReference (typeof (int));
        internalPropMethod.Attributes = MemberAttributes.Public;
        CodeParameterDeclarationExpression param3 = new CodeParameterDeclarationExpression (typeof (int), "value");
        internalPropMethod.Parameters.Add (param3);

        internalPropMethod.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeThisReferenceExpression (), "internalProp"), new CodeArgumentReferenceExpression ("value")));
        internalPropMethod.Statements.Add (new CodeMethodReturnStatement (new CodePropertyReferenceExpression (new CodeThisReferenceExpression (), "internalProp")));
        class1.Members.Add (internalPropMethod);

        // 2) set value
        //     GENERATE (C#):
        //        public virtual int setProp(int value) {
        //            prop1 = value;
        //            return int1;
        //        }
        AddScenario ("ChecksetProp");
        CodeMemberMethod setProp = new CodeMemberMethod ();
        setProp.Name = "setProp";
        setProp.ReturnType = new CodeTypeReference (typeof (int));
        setProp.Attributes = MemberAttributes.Public;
        CodeParameterDeclarationExpression intParam = new CodeParameterDeclarationExpression (typeof (int), "value");
        setProp.Parameters.Add (intParam);

        setProp.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeThisReferenceExpression (), "prop1"), new CodeArgumentReferenceExpression ("value")));
        setProp.Statements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (null, "int1")));
        class1.Members.Add (setProp);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateTest", "Find and instantiate Test.");
        if (!FindAndInstantiate ("NS.Test", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTest");


        int i1 = 392;
        if (VerifyMethod (genType, genObject, "thisRef", new object[] {i1}, i1 + 1)) {
            VerifyScenario ("CheckthisRef");
        }

        int i3 = -213;
        if (VerifyPropertySet (genType, genObject, "prop1", i3) &&
                VerifyPropertyGet (genType, genObject, "prop1", i3 + 1)) {
            VerifyScenario ("Checkprop1");
        }

        if (VerifyPropertyGet (genType, genObject, "constStaticProp", 99)) {
            VerifyScenario ("CheckconstStaticProp");
        }

        if (VerifyMethod (genType, genObject, "protPropMethod", new object[] {2317}, 2318)) {
            VerifyScenario ("CheckprotPropMethod");
        }

        if (VerifyMethod (genType, genObject, "internalPropMethod", new object[] {1}, 2)) {
            VerifyScenario ("CheckinternalPropMethod");
        }

        if (VerifyPropertyGet (genType, genObject, "Text", "Hello World")) {
            VerifyScenario ("CheckText");
        }

        if (VerifyMethod (genType, genObject, "setProp", new object[] {1}, 1)) {
            VerifyScenario ("ChecksetProp");
        }
    }
}

