// F#: The CodeDOM provider ignores type constructors (static constructors), 
//     but this is handled because test suite checks for "Supports" flags
//
//     Next issue is that F# generates properties instead of fields (modified lookup in the test)

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Microsoft.JScript;

public class SubsetAttributesTest : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Subset;
        }
    }

    public override string Name {
        get {
            return "SubsetAttributesTest";
        }
    }

    public override string Description {
        get {
            return "Tests metadata attributes while staying in the subset.";
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

        // GENERATES (C#):
        // [assembly: System.Reflection.AssemblyTitle("MyAssembly")]
        // [assembly: System.Reflection.AssemblyVersion("1.0.6.2")]
        // [assembly: System.CLSCompliantAttribute(false)]
        // 
        // namespace MyNamespace {
        //     using System;
        //     using System.Drawing;
        //     using System.Windows.Forms;
        //     using System.ComponentModel;
        //
        CodeNamespace ns = new CodeNamespace ();
        ns.Name = "MyNamespace";
        ns.Imports.Add (new CodeNamespaceImport ("System"));
        ns.Imports.Add (new CodeNamespaceImport ("System.Drawing"));
        ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Forms"));
        ns.Imports.Add (new CodeNamespaceImport ("System.ComponentModel"));
        cu.Namespaces.Add (ns);

        cu.ReferencedAssemblies.Add ("System.Xml.dll");
        cu.ReferencedAssemblies.Add ("System.Drawing.dll");
        cu.ReferencedAssemblies.Add ("System.Windows.Forms.dll");

        // Assembly Attributes
        if (Supports (provider, GeneratorSupport.AssemblyAttributes)) {
            AddScenario ("CheckAssemblyAttributes", "Check that assembly attributes get generated properly.");
            CodeAttributeDeclarationCollection attrs = cu.AssemblyCustomAttributes;
            attrs.Add (new CodeAttributeDeclaration ("System.Reflection.AssemblyTitle", new
                CodeAttributeArgument (new CodePrimitiveExpression ("MyAssembly"))));
            attrs.Add (new CodeAttributeDeclaration ("System.Reflection.AssemblyVersion", new
                CodeAttributeArgument (new CodePrimitiveExpression ("1.0.6.2"))));
            attrs.Add (new CodeAttributeDeclaration ("System.CLSCompliantAttribute", new
                CodeAttributeArgument (new CodePrimitiveExpression (false))));
        }

        // GENERATES (C#):
        //     [System.Serializable()]
        //     [System.Obsolete("Don\'t use this Class")]
        //     [System.Windows.Forms.AxHost.ClsidAttribute("Class.ID")]
        //     public class MyClass {
        //

#if !WHIDBEY
        if (!(provider is CSharpCodeProvider) && !(provider is VBCodeProvider) && !(provider is JScriptCodeProvider))
            AddScenario ("CheckClassAttributes", "Check that class attributes get generated properly.");
#else
        AddScenario ("CheckClassAttributes", "Check that class attributes get generated properly.");
#endif
        CodeTypeDeclaration class1 = new CodeTypeDeclaration ();
        class1.Name = "MyClass";
        class1.CustomAttributes.Add (new CodeAttributeDeclaration ("System.Serializable"));
        class1.CustomAttributes.Add (new CodeAttributeDeclaration ("System.Obsolete", new
            CodeAttributeArgument (new CodePrimitiveExpression ("Don't use this Class"))));
        class1.CustomAttributes.Add (new
            CodeAttributeDeclaration (typeof (System.Windows.Forms.AxHost.ClsidAttribute).FullName,
            new CodeAttributeArgument (new CodePrimitiveExpression ("Class.ID"))));
        ns.Types.Add (class1);

        // GENERATES (C#):
        //         [System.Obsolete("Don\'t use this Method")]
        //         [System.ComponentModel.Editor("This", "That")]
        //         public void MyMethod(string blah, int[] arrayit) {
        //         }
        AddScenario ("CheckMyMethodAttributes", "Check that attributes are generated properly on MyMethod().");
        CodeMemberMethod method1 = new CodeMemberMethod ();
        method1.Attributes = (method1.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        method1.Name = "MyMethod";
        method1.CustomAttributes.Add (new CodeAttributeDeclaration ("System.Obsolete", new
            CodeAttributeArgument (new CodePrimitiveExpression ("Don't use this Method"))));
        method1.CustomAttributes.Add (new CodeAttributeDeclaration ("System.ComponentModel.Editor", new
            CodeAttributeArgument (new CodePrimitiveExpression ("This")), new CodeAttributeArgument (new
            CodePrimitiveExpression ("That"))));
        CodeParameterDeclarationExpression param1 = new CodeParameterDeclarationExpression (typeof (string), "blah");

        method1.Parameters.Add (param1);
        CodeParameterDeclarationExpression param2 = new CodeParameterDeclarationExpression (typeof (int[]), "arrayit");

        method1.Parameters.Add (param2);
        class1.Members.Add (method1);

        // GENERATES (C#):
        //         [System.Xml.Serialization.XmlElementAttribute()]
        //         private string myField = "hi!";
        //
        AddScenario ("CheckMyFieldAttributes", "Check that attributes are generated properly on MyField.");
        CodeMemberField field1 = new CodeMemberField ();
        field1.Name = "myField";
        field1.Attributes = MemberAttributes.Public;
        field1.Type = new CodeTypeReference (typeof (string));
        field1.CustomAttributes.Add (new CodeAttributeDeclaration ("System.Xml.Serialization.XmlElementAttribute"));
        field1.InitExpression = new CodePrimitiveExpression ("hi!");
        class1.Members.Add (field1);


        // GENERATES (C#):
        //         [System.Obsolete("Don\'t use this Property")]
        //         public string MyProperty {
        //             get {
        //                 return this.myField;
        //             }
        //         }
        AddScenario ("CheckMyPropertyAttributes", "Check that attributes are generated properly on MyProperty.");
        CodeMemberProperty prop1 = new CodeMemberProperty ();
        prop1.Attributes = MemberAttributes.Public;
        prop1.Name = "MyProperty";
        prop1.Type = new CodeTypeReference (typeof (string));
        prop1.CustomAttributes.Add (new CodeAttributeDeclaration ("System.Obsolete", new
            CodeAttributeArgument (new CodePrimitiveExpression ("Don't use this Property"))));
        prop1.GetStatements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (new CodeThisReferenceExpression (), "myField")));
        class1.Members.Add (prop1);

        // GENERATES (C#):
        //         [System.Obsolete("Don\'t use this Constructor")]
        //         public MyClass() {
        //         }
        //     }

        if (!(provider is JScriptCodeProvider))
            AddScenario ("CheckConstructorAttributes", "Check that attributes are generated properly on the constructor.");
        CodeConstructor const1 = new CodeConstructor ();
        const1.Attributes = MemberAttributes.Public;
        const1.CustomAttributes.Add (new CodeAttributeDeclaration ("System.Obsolete", new
            CodeAttributeArgument (new CodePrimitiveExpression ("Don't use this Constructor"))));
        class1.Members.Add (const1);

        if (Supports (provider, GeneratorSupport.DeclareEvents)) {
            // GENERATES (C#):
            //     public class Test : Form {
            //         
            //         private Button b = new Button();
            //
            // 
            AddScenario ("CheckEventAttributes", "test attributes on an event");
            class1 = new CodeTypeDeclaration ("Test");
            class1.IsClass = true;
            class1.BaseTypes.Add (new CodeTypeReference ("Form"));
            ns.Types.Add (class1);
            CodeMemberField mfield = new CodeMemberField (new CodeTypeReference ("Button"), "b");
            mfield.InitExpression = new CodeObjectCreateExpression (new CodeTypeReference ("Button"));
            class1.Members.Add (mfield);

            // GENERATES (C#):
            //         public Test() {
            //             this.Size = new Size(600, 600);
            //             b.Text = "Test";
            //             b.TabIndex = 0;
            //             b.Location = new Point(400, 525);
            //             this.MyEvent += new EventHandler(this.b_Click);
            //         }
            //
            CodeConstructor ctor = new CodeConstructor ();
            ctor.Attributes = MemberAttributes.Public;
            ctor.Statements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (new CodeThisReferenceExpression (),
                "Size"), new CodeObjectCreateExpression (new CodeTypeReference ("Size"),
                new CodePrimitiveExpression (600), new CodePrimitiveExpression (600))));
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeFieldReferenceExpression (null, "b"),
                "Text"), new CodePrimitiveExpression ("Test")));
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeFieldReferenceExpression (null, "b"),
                "TabIndex"), new CodePrimitiveExpression (0)));
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeFieldReferenceExpression (null, "b"),
                "Location"), new CodeObjectCreateExpression (new CodeTypeReference ("Point"),
                new CodePrimitiveExpression (400), new CodePrimitiveExpression (525))));
            ctor.Statements.Add (new CodeAttachEventStatement (new CodeEventReferenceExpression (new
                CodeThisReferenceExpression (), "MyEvent"), new CodeDelegateCreateExpression (new CodeTypeReference ("EventHandler")
                , new CodeThisReferenceExpression (), "b_Click")));
            class1.Members.Add (ctor);

            // GENERATES (C#):
            //         [System.CLSCompliantAttribute(false)]
            //         public event System.EventHandler MyEvent;
            CodeMemberEvent evt = new CodeMemberEvent ();
            evt.Name = "MyEvent";
            evt.Type = new CodeTypeReference ("System.EventHandler");
            evt.Attributes = MemberAttributes.Public;
            evt.CustomAttributes.Add (new CodeAttributeDeclaration ("System.CLSCompliantAttribute", new CodeAttributeArgument (new CodePrimitiveExpression (false))));
            class1.Members.Add (evt);

            // GENERATES (C#):
            //         private void b_Click(object sender, System.EventArgs e) {
            //         }
            //     }
            // }
            //
            CodeMemberMethod cmm = new CodeMemberMethod ();
            cmm.Name = "b_Click";
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object), "sender"));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (EventArgs), "e"));
            class1.Members.Add (cmm);
        }

    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        Type genType;

        // Verifying Assembly Attributes
        if (Supports (provider, GeneratorSupport.AssemblyAttributes)) {
            object[] attributes = asm.GetCustomAttributes (true);
            bool verifiedAssemblyAttributes = VerifyAttribute (attributes, typeof (AssemblyTitleAttribute), "Title", "MyAssembly");
            verifiedAssemblyAttributes &=
                VerifyAttribute (attributes, typeof (AssemblyVersionAttribute), "Version", "1.0.6.2") ||
                asm.GetName ().Version.Equals (new Version (1, 0, 6, 2));
            if (verifiedAssemblyAttributes) {
                VerifyScenario ("CheckAssemblyAttributes");
            }
        }

        object[] customAttributes;
        object[] customAttributes2;
        object[] customAttributesAll;

        if (FindType ("MyNamespace.MyClass", asm, out genType)) {
            customAttributes    = genType.GetCustomAttributes (typeof (System.ObsoleteAttribute), true);
            customAttributes2   = genType.GetCustomAttributes (typeof (System.SerializableAttribute), true);
            customAttributesAll = genType.GetCustomAttributes (true);

#if !WHIDBEY
            if (!(provider is CSharpCodeProvider) && !(provider is VBCodeProvider) && !(provider is JScriptCodeProvider)) {
#endif
                if (customAttributes.GetLength (0) == 1 &&
                        customAttributes2.GetLength (0) >= 1 &&
                        customAttributesAll.GetLength (0) >= 3) {
                    VerifyScenario ("CheckClassAttributes");
                }
#if !WHIDBEY
            }
#endif

            // verify method attributes
            MethodInfo methodInfo = genType.GetMethod ("MyMethod");
            if (methodInfo != null &&
                    methodInfo.GetCustomAttributes (typeof (System.ObsoleteAttribute), true).GetLength (0) > 0 &&
                    methodInfo.GetCustomAttributes (typeof (System.ComponentModel.EditorAttribute), true).GetLength (0) > 0) {
                VerifyScenario ("CheckMyMethodAttributes");
            }

            // verify property attributes
            PropertyInfo propertyInfo = genType.GetProperty ("MyProperty");
            if (propertyInfo != null &&
                    propertyInfo.GetCustomAttributes (typeof (System.ObsoleteAttribute), true).GetLength (0) > 0) {
                VerifyScenario ("CheckMyPropertyAttributes");
            }

            // verify constructor attributes
            ConstructorInfo[] constructorInfo = genType.GetConstructors ();
            if (constructorInfo != null && constructorInfo.Length > 0 && !(provider is JScriptCodeProvider) &&
                    constructorInfo[0].GetCustomAttributes (typeof (System.ObsoleteAttribute), true).GetLength (0) > 0) {
                VerifyScenario ("CheckConstructorAttributes");
            }
            // verify field attributes
            FieldInfo fieldInfo = genType.GetField("myField");
            if (fieldInfo != null &&
                    fieldInfo.GetCustomAttributes (typeof (System.Xml.Serialization.XmlElementAttribute), true).GetLength (0) > 0) {
                VerifyScenario ("CheckMyFieldAttributes");
            }
        }

        // verify event attributes
        if (Supports (provider, GeneratorSupport.DeclareEvents) && FindType ("MyNamespace.Test", asm, out genType)) {
            EventInfo eventInfo = genType.GetEvent ("MyEvent");
            if (eventInfo != null &&
                    eventInfo.GetCustomAttributes (typeof (System.CLSCompliantAttribute), true).GetLength (0) > 0) {
                VerifyScenario ("CheckEventAttributes");
            }
        }
    }
}

