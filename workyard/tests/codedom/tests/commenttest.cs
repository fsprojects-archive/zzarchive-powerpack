using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class CommentTest : CodeDomTestTree {

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
            return true;
        }
    }


    public override string Name {
        get {
            return "CommentTest";
        }
    }

    public override bool ShouldSearch {
        get {
            return true;
        }
    }

    public override string Description {
        get {
            return "Tests comment statements.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        // Namespace to hold test scenarios
        // GENERATES (C#):
        //        namespace NSPC {
        //            using System;
        //            using System.Drawing;
        //            using System.Windows.Forms;
        //            using System.ComponentModel;
        //            }
        AddScenario ("FindNamespaceComment");
        CodeNamespace nspace = new CodeNamespace ("NSPC");
        nspace.Comments.Add (new CodeCommentStatement (new CodeComment ("Namespace to hold test scenarios")));
        nspace.Imports.Add (new CodeNamespaceImport ("System"));
        nspace.Imports.Add (new CodeNamespaceImport ("System.Drawing"));
        nspace.Imports.Add (new CodeNamespaceImport ("System.Windows.Forms"));
        nspace.Imports.Add (new CodeNamespaceImport ("System.ComponentModel"));
        cu.Namespaces.Add (nspace);

        cu.ReferencedAssemblies.Add ("System.Drawing.dll");
        cu.ReferencedAssemblies.Add ("System.Windows.Forms.dll");

        // GENERATES (C#):    
        //    // Class has a method to test static constructors
        //    public class ClassWithMethod {
        //        // This method is used to test a static constructor
        //        public static int TestStaticConstructor(int a) {
        //            // Testing a line comment
        //            TestClass t = new TestClass();
        //            t.i = a;
        //            return t.i;
        //        }
        //    }
        AddScenario ("FindClassComment");
        CodeTypeDeclaration class1 = new CodeTypeDeclaration ("ClassWithMethod");
        class1.IsClass = true;
        class1.Comments.Add (new CodeCommentStatement ("Class has a method to test static constructors"));
        nspace.Types.Add (class1);

        CodeMemberMethod cmm = new CodeMemberMethod ();
        cmm.Name = "TestStaticConstructor";
        AddScenario ("FindMethodComment");
        cmm.Comments.Add (new CodeCommentStatement (new CodeComment ("This method is used to test a static constructor")));
        cmm.Attributes = MemberAttributes.Public;
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        AddScenario ("FindLineComment");
        cmm.Statements.Add (new CodeCommentStatement ("Testing a line comment"));
        CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (typeof (int), "a");
        cmm.Parameters.Add (param);
        // utilize constructor
        cmm.Statements.Add (new CodeVariableDeclarationStatement ("TestClass", "t", new CodeObjectCreateExpression ("TestClass")));
        // set then get number
        cmm.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeVariableReferenceExpression ("t"), "i")
            , new CodeArgumentReferenceExpression ("a")));
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodePropertyReferenceExpression (new CodeVariableReferenceExpression ("t"), "i")));
        class1.Members.Add (cmm);


        // GENERATES (C#):
        //    public class TestClass {
        //        // This field is an integer counter
        //        private int number;
        //        static TestClass() {
        //        }
        //        // This property allows us to access the integer counter
        //        // We are able to both get and set the value of the counter
        //        public int i {
        //            get {
        //                return number;
        //            }
        //            set {
        //                number = value;
        //            }
        //        }
        //    }
        class1 = new CodeTypeDeclaration ();
        class1.Name = "TestClass";
        class1.IsClass = true;
        nspace.Types.Add (class1);
        AddScenario ("FindFieldComment");
        CodeMemberField mfield = new CodeMemberField (new CodeTypeReference (typeof (int)), "number");
        mfield.Comments.Add (new CodeCommentStatement ("This field is an integer counter"));
        class1.Members.Add (mfield);
        AddScenario ("FindPropertyComment");
        CodeMemberProperty prop = new CodeMemberProperty ();
        prop.Name = "i";
        prop.Comments.Add (new CodeCommentStatement ("This property allows us to access the integer counter"));
        prop.Comments.Add (new CodeCommentStatement ("We are able to both get and set the value of the counter"));
        prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        prop.Type = new CodeTypeReference (typeof (int));
        prop.GetStatements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (null, "number")));
        prop.SetStatements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (null, "number"),
            new CodePropertySetValueReferenceExpression ()));
        class1.Members.Add (prop);


        CodeTypeConstructor ctc = new CodeTypeConstructor ();
        class1.Members.Add (ctc);

        // ************* code a comment on an event *************
        if (Supports (provider, GeneratorSupport.DeclareEvents)) {

            // GENERATES (C#):
            //    public class Test : Form {
            //        private Button b = new Button();
            //        public Test() {
            //            this.Size = new Size(600, 600);
            //            b.Text = "Test";
            //            b.TabIndex = 0;
            //            b.Location = new Point(400, 525);
            //            this.MyEvent += new EventHandler(this.b_Click);
            //        }
            //        // This is a comment on an event
            //        public event System.EventHandler MyEvent;
            //        private void b_Click(object sender, System.EventArgs e) {
            //        }
            //    }
            class1 = new CodeTypeDeclaration ("Test");
            class1.IsClass = true;
            class1.BaseTypes.Add (new CodeTypeReference ("Form"));
            nspace.Types.Add (class1);

            mfield = new CodeMemberField (new CodeTypeReference ("Button"), "b");
            mfield.InitExpression = new CodeObjectCreateExpression (new CodeTypeReference ("Button"));
            class1.Members.Add (mfield);

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

            AddScenario ("FindEventComment");
            CodeMemberEvent evt = new CodeMemberEvent ();
            evt.Name = "MyEvent";
            evt.Type = new CodeTypeReference ("System.EventHandler");
            evt.Attributes = MemberAttributes.Public;
            evt.Comments.Add (new CodeCommentStatement ("This is a comment on an event"));
            class1.Members.Add (evt);

            cmm = new CodeMemberMethod ();
            cmm.Name = "b_Click";
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object), "sender"));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (EventArgs), "e"));
            class1.Members.Add (cmm);
        }

        if (Supports (provider, GeneratorSupport.DeclareDelegates)) {
            // GENERATES (C#):
            //
            // // This is a delegate comment
            // public delegate void Delegate();

            AddScenario ("FindDelegateComment");
            CodeTypeDelegate del = new CodeTypeDelegate ("Delegate");
            del.Comments.Add (new CodeCommentStatement ("This is a delegate comment"));
            nspace.Types.Add (del);
        }
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly ass) {
    }

    public override void Search (CodeDomProvider provider, String output) {
        int index;

        // check the comment on the line
        String str = "Testing a line comment";
        index = output.IndexOf (str);
        if (String.Compare (str, 0, output, index, str.Length, false, CultureInfo.InvariantCulture) == 0) {
           VerifyScenario ("FindLineComment");
        }
        // check comment on method
        str = "This method is used to test a static constructor";
        index = output.IndexOf (str);
        if (String.Compare (str, 0, output, index, str.Length, false, CultureInfo.InvariantCulture) == 0) {
           VerifyScenario ("FindMethodComment");
        }

        // check comment on class
        str = "Class has a method to test static constructors";
        index = output.IndexOf (str);
        if (String.Compare (str, 0, output, index, str.Length, false, CultureInfo.InvariantCulture) == 0) {
           VerifyScenario ("FindClassComment");
        }

        // check comment on namespace
        str = "Namespace to hold test scenarios";
        index = output.IndexOf (str);
        if (String.Compare (str, 0, output, index, str.Length, false, CultureInfo.InvariantCulture) == 0) {
           VerifyScenario ("FindNamespaceComment");
        }

        // check comment on field
        str = "This field is an integer counter";
        index = output.IndexOf (str);
        if (String.Compare (str, 0, output, index, str.Length, false, CultureInfo.InvariantCulture) == 0) {
           VerifyScenario ("FindFieldComment");
        }

        // check comment on property    
        str = "This property allows us to access the integer counter";
        index = output.IndexOf (str);
        if (String.Compare (str, 0, output, index, str.Length, false, CultureInfo.InvariantCulture) == 0) {
            str = "We are able to both get and set the value of the counter";
            index = output.IndexOf (str);
            if (String.Compare (str, 0, output, index, str.Length, false, CultureInfo.InvariantCulture) == 0)
                VerifyScenario ("FindPropertyComment");
        }

        // check comment on events
        if (Supports (provider, GeneratorSupport.DeclareEvents)) {
            str = "This is a comment on an event";
            index = output.IndexOf (str);
            if (String.Compare (str, 0, output, index, str.Length, false, CultureInfo.InvariantCulture) == 0)
                VerifyScenario ("FindEventComment");
        }

        // check comment on delegates
        if (Supports (provider, GeneratorSupport.DeclareDelegates)) {
            str = "This is a delegate comment";
            index = output.IndexOf (str);
            if (String.Compare (str, 0, output, index, str.Length, false, CultureInfo.InvariantCulture) == 0)
                VerifyScenario ("FindDelegateComment");
        }
    }
}

