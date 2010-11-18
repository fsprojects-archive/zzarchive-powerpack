using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Samples.CodeDomTestSuite;

public class CastingCodeDom : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Subset;
        }
    }

    public override string Name {
        get {
            return "CastingCodeDom";
        }
    }

    public override string Description {
        get {
            return "Tests casting";
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
        ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Forms"));
        cu.ReferencedAssemblies.Add ("System.Windows.Forms.dll");
        cu.Namespaces.Add (ns);

        // create a class
        CodeTypeDeclaration class1 = new CodeTypeDeclaration ();
        class1.Name = "Test";
        class1.IsClass = true;
        ns.Types.Add (class1);


        // create method to test casting enum -> int
        //     GENERATE (C#):
        //        public int EnumToInt(System.Windows.Forms.AnchorStyles enum1) {
        //            return ((int)(enum1));
        //        }
        AddScenario ("CheckEnumToInt1", "Check the return value of EnumToInt() with a single flag");
        AddScenario ("CheckEnumToInt2", "Check the return value of EnumToInt() with multiple flags");
        CodeMemberMethod enumToInt = new CodeMemberMethod ();
        enumToInt.Name = "EnumToInt";
        enumToInt.ReturnType = new CodeTypeReference (typeof (int));
        enumToInt.Attributes = MemberAttributes.Public;
        CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (typeof (System.Windows.Forms.AnchorStyles), "enum1");
        enumToInt.Parameters.Add (param);
        enumToInt.Statements.Add (new CodeMethodReturnStatement (new CodeCastExpression (typeof (int), new CodeArgumentReferenceExpression ("enum1"))));
        class1.Members.Add (enumToInt);

        // create method to test casting enum -> int
        //     GENERATE (C#):
        //       public virtual int CastReturnValue(string value) {
        //          float val = System.Single.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        //          return ((int) val);
        //       }
        AddScenario ("CheckCastReturnValue", "Check the return value of CastReturnValue()");
        CodeMemberMethod castReturnValue = new CodeMemberMethod ();
        castReturnValue.Name = "CastReturnValue";
        castReturnValue.ReturnType = new CodeTypeReference (typeof (int));
        castReturnValue.Attributes = MemberAttributes.Public;
        CodeParameterDeclarationExpression strParam = new CodeParameterDeclarationExpression (typeof (string), "value");
        castReturnValue.Parameters.Add (strParam);
        castReturnValue.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "val",
                    CDHelper.CreateMethodInvoke (new CodeTypeReferenceExpression (new CodeTypeReference ("System.Int32")), // F#: Type conversion "int -> float" is not a type-cast!
                        "Parse", new CodeArgumentReferenceExpression ("value"),
                        new CodePropertyReferenceExpression (new CodeTypeReferenceExpression ("System.Globalization.CultureInfo"),
                            "InvariantCulture"))));
        castReturnValue.Statements.Add (new CodeMethodReturnStatement (new CodeCastExpression (typeof (int), new CodeVariableReferenceExpression ("val"))));
        class1.Members.Add (castReturnValue);


        // create method to test casting interface -> class
        //     GENERATE (C#):
        //        public string CastInterface(System.ICloneable value) {
        //            return ((string)(value));
        //        }
        AddScenario ("CheckCastInterface", "Check the return value of CastInterface()");
        CodeMemberMethod castInterface = new CodeMemberMethod ();
        castInterface.Name = "CastInterface";
        castInterface.ReturnType = new CodeTypeReference (typeof (string));
        castInterface.Attributes = MemberAttributes.Public;
        CodeParameterDeclarationExpression interfaceParam = new CodeParameterDeclarationExpression (typeof (System.ICloneable), "value");
        castInterface.Parameters.Add (interfaceParam);
        castInterface.Statements.Add (new CodeMethodReturnStatement (new CodeCastExpression (typeof (string), new CodeArgumentReferenceExpression ("value"))));
        class1.Members.Add (castInterface);

        // create method to test casting value type -> reference type
        //     GENERATE (C#):
        //         public object ValueToReference(int value) {
        //             return ((object)(value));
        //         }
        AddScenario ("CheckValueToReference", "Check the return value of ValueToReference()");
        CodeMemberMethod valueToReference = new CodeMemberMethod ();
        valueToReference.Name = "ValueToReference";
        valueToReference.ReturnType = new CodeTypeReference (typeof (System.Object));
        valueToReference.Attributes = MemberAttributes.Public;
        CodeParameterDeclarationExpression valueParam = new CodeParameterDeclarationExpression (typeof (int), "value");
        valueToReference.Parameters.Add (valueParam);
        valueToReference.Statements.Add (new CodeMethodReturnStatement (new CodeCastExpression (typeof (System.Object), new CodeArgumentReferenceExpression ("value"))));
        class1.Members.Add (valueToReference);

    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateTest", "Find and instantiate CLASSNAME.");
        if (!FindAndInstantiate ("NS.Test", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTest");

        // enum -> int single value
        if (VerifyMethod (genType, genObject, "EnumToInt", new object[] {System.Windows.Forms.AnchorStyles.Top}, 1)) {
            VerifyScenario ("CheckEnumToInt1");
        }

        // enum -> int multiple flags set
        if (VerifyMethod (genType, genObject, "EnumToInt", new object[] {System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right}, 9)) {
            VerifyScenario ("CheckEnumToInt2");
        }

        // casting a return value of a method
        if (VerifyMethod (genType, genObject, "CastReturnValue", new object[] {"1"}, (int)1)) { // F#: type cast is not a type conversion 
            VerifyScenario ("CheckCastReturnValue");
        }

        // interface (IClonable) -> class (String)
        ICloneable str = "Hello World";
        if (VerifyMethod (genType, genObject, "CastInterface", new object[] {str}, (string) str)) {
            VerifyScenario ("CheckCastInterface");
        }

        // value type (int) -> reference type (object)
        int i = 10;
        if (VerifyMethod (genType, genObject, "ValueToReference", new object[] {i}, (object) i)) {
            VerifyScenario ("CheckValueToReference");
        }
    }
}

