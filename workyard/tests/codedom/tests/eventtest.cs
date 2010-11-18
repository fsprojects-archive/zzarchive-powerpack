using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class EventTest : CodeDomTestTree {

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
            return false;
        }
    }


    public override string Name {
        get {
            return "EventTest";
        }
    }

    public override string Description {
        get {
            return "Tests events.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        // GENERATES (C#):
        //  [assembly: System.CLSCompliantAttribute(false)]
        //  
        //  namespace MyNamespace {
        //      using System;
        //      using System.Drawing;
        //      using System.Windows.Forms;
        //      using System.ComponentModel;
        //      
        //      
        //      public class Test : Form {
        //          
        //          private Button b = new Button();
        //          
        //          public Test() {
        //              this.Size = new Size(600, 600);
        //              b.Text = "Test";
        //              b.TabIndex = 0;
        //              b.Location = new Point(400, 525);
        //              this.MyEvent += new EventHandler(this.b_Click);
        //              this.MyEvent -= new EventHandler(this.b_Click);
        //          }
        //          
        //          [System.CLSCompliantAttribute(false)]
        //          public event System.EventHandler MyEvent;
        //          
        //          private void b_Click(object sender, System.EventArgs e) {
        //          }
        //      }
        //  }
        //  namespace SecondNamespace {
        //      using System;
        //      using System.Drawing;
        //      using System.Windows.Forms;
        //      using System.ComponentModel;
        //      
        //      
        //      public class Test : Form {
        //          
        //          private Button b = new Button();
        //          
        //          public Test() {
        //              this.Size = new Size(600, 600);
        //              b.Text = "Test";
        //              b.TabIndex = 0;
        //              b.Location = new Point(400, 525);
        //              this.MyEvent += new EventHandler(this.b_Click);
        //          }
        //          
        //          private event System.EventHandler MyEvent;
        //          
        //          private void b_Click(object sender, System.EventArgs e) {
        //          }
        //      }
        //  }
        //  namespace ThirdNamespace {
        //      using System;
        //      using System.Drawing;
        //      using System.Windows.Forms;
        //      using System.ComponentModel;
        //      
        //      
        //      public class Test : Form {
        //          
        //          private Button b = new Button();
        //          
        //          public Test() {
        //              this.Size = new Size(600, 600);
        //              b.Text = "Test";
        //              b.TabIndex = 0;
        //              b.Location = new Point(400, 525);
        //              this.MyEvent += new EventHandler(this.b_Click);
        //          }
        //          
        //          [System.CLSCompliantAttribute(false)]
        //          protected event System.EventHandler MyEvent;
        //          
        //          private void b_Click(object sender, System.EventArgs e) {
        //          }
        //      }
        //  }
        //  namespace FourthNamespace {
        //      using System;
        //      using System.Drawing;
        //      using System.Windows.Forms;
        //      using System.ComponentModel;
        //      
        //      
        //      public class Test : Form {
        //          
        //          private Button b = new Button();
        //          
        //          public Test() {
        //              this.Size = new Size(600, 600);
        //              b.Text = "Test";
        //              b.TabIndex = 0;
        //              b.Location = new Point(400, 525);
        //              this.MyEvent += new EventHandler(this.b_Click);
        //          }
        //          
        //          /*FamANDAssem*/ internal event System.EventHandler MyEvent;
        //          
        //          private void b_Click(object sender, System.EventArgs e) {
        //          }
        //      }
        //  }

        if (Supports (provider, GeneratorSupport.DeclareEvents)) {
            // *********************************
            // public
            CodeNamespace ns = new CodeNamespace ();
            ns.Name = "MyNamespace";
            ns.Imports.Add (new CodeNamespaceImport ("System"));
            ns.Imports.Add (new CodeNamespaceImport ("System.Drawing"));
            ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Forms"));
            ns.Imports.Add (new CodeNamespaceImport ("System.ComponentModel"));
            cu.Namespaces.Add (ns);

            cu.ReferencedAssemblies.Add ("System.Drawing.dll");
            cu.ReferencedAssemblies.Add ("System.Windows.Forms.dll");

            // Assembly Attributes
            if (Supports (provider, GeneratorSupport.AssemblyAttributes)) {
                CodeAttributeDeclarationCollection attrs = cu.AssemblyCustomAttributes;
                attrs.Add (new CodeAttributeDeclaration ("System.CLSCompliantAttribute", new
                    CodeAttributeArgument (new CodePrimitiveExpression (false))));
            }

            CodeTypeDeclaration class1 = new CodeTypeDeclaration ("Test");
            class1.IsClass = true;
            class1.BaseTypes.Add (new CodeTypeReference ("Form"));
            ns.Types.Add (class1);

            CodeMemberField mfield = new CodeMemberField (new CodeTypeReference ("Button"), "b");
            mfield.InitExpression = new CodeObjectCreateExpression (new CodeTypeReference ("Button"));
            class1.Members.Add (mfield);

            CodeConstructor ctor = new CodeConstructor ();
            ctor.Attributes = MemberAttributes.Public;
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeThisReferenceExpression (),
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
            ctor.Statements.Add (new CodeRemoveEventStatement (new CodeEventReferenceExpression (new
                CodeThisReferenceExpression (), "MyEvent"), new CodeDelegateCreateExpression (new CodeTypeReference ("EventHandler")
                , new CodeThisReferenceExpression (), "b_Click")));
            class1.Members.Add (ctor);

            CodeMemberEvent evt = new CodeMemberEvent ();
            evt.Name = "MyEvent";
            evt.Type = new CodeTypeReference ("System.EventHandler");
            evt.Attributes = MemberAttributes.Public;
            evt.CustomAttributes.Add (new CodeAttributeDeclaration ("System.CLSCompliantAttribute", new CodeAttributeArgument (new CodePrimitiveExpression (false))));
            class1.Members.Add (evt);

            CodeMemberMethod cmm = new CodeMemberMethod ();
            cmm.Name = "b_Click";
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object), "sender"));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (EventArgs), "e"));
            class1.Members.Add (cmm);

            // *********************************
            // private
            ns = new CodeNamespace ();
            ns.Name = "SecondNamespace";
            ns.Imports.Add (new CodeNamespaceImport ("System"));
            ns.Imports.Add (new CodeNamespaceImport ("System.Drawing"));
            ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Forms"));
            ns.Imports.Add (new CodeNamespaceImport ("System.ComponentModel"));
            cu.Namespaces.Add (ns);

            class1 = new CodeTypeDeclaration ("Test");
            class1.IsClass = true;
            class1.BaseTypes.Add (new CodeTypeReference ("Form"));
            ns.Types.Add (class1);

            mfield = new CodeMemberField (new CodeTypeReference ("Button"), "b");
            mfield.InitExpression = new CodeObjectCreateExpression (new CodeTypeReference ("Button"));
            class1.Members.Add (mfield);

            ctor = new CodeConstructor ();
            ctor.Attributes = MemberAttributes.Public;
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeThisReferenceExpression (),
                "Size"), new CodeObjectCreateExpression (new CodeTypeReference ("Size"),
                new CodePrimitiveExpression (600), new CodePrimitiveExpression (600))));
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeFieldReferenceExpression (null, "b"),
                "Text"), new CodePrimitiveExpression ("Test")));
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeFieldReferenceExpression (null, "b"),
                "TabIndex"), new CodePrimitiveExpression (0)));
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeFieldReferenceExpression (null, "b"),
                "Location"), new CodeObjectCreateExpression (new CodeTypeReference ("Point"),
                new CodePrimitiveExpression (400), new CodePrimitiveExpression (525))));

            CodeAttachEventStatement addevt = new CodeAttachEventStatement (new CodeThisReferenceExpression (),
                    "MyEvent", new CodeDelegateCreateExpression (new CodeTypeReference ("EventHandler")
                , new CodeThisReferenceExpression (), "b_Click"));

            ctor.Statements.Add (addevt);

            // remove event statement 
            CodeRemoveEventStatement rem = new CodeRemoveEventStatement (new CodeThisReferenceExpression (), "MyEvent",
                        new CodeDelegateCreateExpression (new CodeTypeReference ("EventHandler"),
                            new CodeThisReferenceExpression (), "b_Click"));
            ctor.Statements.Add (rem);


            class1.Members.Add (ctor);

            evt = new CodeMemberEvent ();
            evt.Name = "MyEvent";
            evt.Type = new CodeTypeReference ("System.EventHandler");
            evt.Attributes = MemberAttributes.Private;
            class1.Members.Add (evt);

            cmm = new CodeMemberMethod ();
            cmm.Name = "b_Click";
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object), "sender"));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (EventArgs), "e"));
            class1.Members.Add (cmm);

            // *********************************
            // protected
            ns = new CodeNamespace ();
            ns.Name = "ThirdNamespace";
            ns.Imports.Add (new CodeNamespaceImport ("System"));
            ns.Imports.Add (new CodeNamespaceImport ("System.Drawing"));
            ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Forms"));
            ns.Imports.Add (new CodeNamespaceImport ("System.ComponentModel"));
            cu.Namespaces.Add (ns);

            class1 = new CodeTypeDeclaration ("Test");
            class1.IsClass = true;
            class1.BaseTypes.Add (new CodeTypeReference ("Form"));
            ns.Types.Add (class1);

            mfield = new CodeMemberField (new CodeTypeReference ("Button"), "b");
            mfield.InitExpression = new CodeObjectCreateExpression (new CodeTypeReference ("Button"));
            class1.Members.Add (mfield);

            ctor = new CodeConstructor ();
            ctor.Attributes = MemberAttributes.Public;
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeThisReferenceExpression (),
                "Size"), new CodeObjectCreateExpression (new CodeTypeReference ("Size"),
                new CodePrimitiveExpression (600), new CodePrimitiveExpression (600))));
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeFieldReferenceExpression (null, "b"),
                "Text"), new CodePrimitiveExpression ("Test")));
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeFieldReferenceExpression (null, "b"),
                "TabIndex"), new CodePrimitiveExpression (0)));
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeFieldReferenceExpression (null, "b"),
                "Location"), new CodeObjectCreateExpression (new CodeTypeReference ("Point"),
                new CodePrimitiveExpression (400), new CodePrimitiveExpression (525))));

            addevt = new CodeAttachEventStatement ();
            addevt.Event = new CodeEventReferenceExpression (new CodeThisReferenceExpression (), "MyEvent");
            addevt.Listener = new CodeDelegateCreateExpression (new CodeTypeReference ("EventHandler"),
                    new CodeThisReferenceExpression (), "b_Click");
            ctor.Statements.Add (addevt);

            // remove event
            rem = new CodeRemoveEventStatement ();
            rem.Event = new CodeEventReferenceExpression (new CodeThisReferenceExpression (), "MyEvent");
            rem.Listener = new CodeDelegateCreateExpression (new CodeTypeReference ("EventHandler"),
                    new CodeThisReferenceExpression (), "b_Click");
            ctor.Statements.Add (rem);

            class1.Members.Add (ctor);

            evt = new CodeMemberEvent ();
            evt.Name = "MyEvent";
            evt.Type = new CodeTypeReference ("System.EventHandler");
            evt.Attributes = MemberAttributes.Family;
            evt.CustomAttributes.Add (new CodeAttributeDeclaration ("System.CLSCompliantAttribute", new CodeAttributeArgument (new CodePrimitiveExpression (false))));
            class1.Members.Add (evt);

            cmm = new CodeMemberMethod ();
            cmm.Name = "b_Click";
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object), "sender"));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (EventArgs), "e"));
            class1.Members.Add (cmm);

            // *********************************
            // internal
            ns = new CodeNamespace ();
            ns.Name = "FourthNamespace";
            ns.Imports.Add (new CodeNamespaceImport ("System"));
            ns.Imports.Add (new CodeNamespaceImport ("System.Drawing"));
            ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Forms"));
            ns.Imports.Add (new CodeNamespaceImport ("System.ComponentModel"));
            cu.Namespaces.Add (ns);

            class1 = new CodeTypeDeclaration ("Test");
            class1.IsClass = true;
            class1.BaseTypes.Add (new CodeTypeReference ("Form"));
            ns.Types.Add (class1);

            mfield = new CodeMemberField (new CodeTypeReference ("Button"), "b");
            mfield.InitExpression = new CodeObjectCreateExpression (new CodeTypeReference ("Button"));
            class1.Members.Add (mfield);

            ctor = new CodeConstructor ();
            ctor.Attributes = MemberAttributes.Public;
            ctor.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeThisReferenceExpression (),
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

            evt = new CodeMemberEvent ();
            evt.Name = "MyEvent";
            evt.Type = new CodeTypeReference ("System.EventHandler");
            evt.Attributes = MemberAttributes.FamilyAndAssembly;
            class1.Members.Add (evt);

            cmm = new CodeMemberMethod ();
            cmm.Name = "b_Click";
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object), "sender"));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (EventArgs), "e"));
            class1.Members.Add (cmm);
        }
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        // compile only
    }
}

