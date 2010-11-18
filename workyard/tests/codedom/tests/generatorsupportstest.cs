// F#: don't test structs - this is covered by other tests

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.VisualBasic;

public class GeneratorSupportsTest : CodeDomTestTree {

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
            return "GeneratorSupportsTest";
        }
    }

    public override string Description {
        get {
            return "Tests GeneratorSupport enumeration.";
        }
    }

    public override CompilerParameters GetCompilerParameters (CodeDomProvider provider) {
        CompilerParameters parms = base.GetCompilerParameters (provider);

        // some languages don't compile correctly if no executable is
        // generated and an entry point is defined
        if (Supports (provider, GeneratorSupport.EntryPointMethod)) {
            parms.GenerateExecutable = true;
        }
        return parms;
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu)
    {

        CodeNamespace nspace = new CodeNamespace ("NSPC");
        nspace.Imports.Add (new CodeNamespaceImport ("System"));
        nspace.Imports.Add (new CodeNamespaceImport ("System.Drawing"));
        nspace.Imports.Add (new CodeNamespaceImport ("System.Windows.Forms"));
        nspace.Imports.Add (new CodeNamespaceImport ("System.ComponentModel"));
        cu.Namespaces.Add (nspace);

        cu.ReferencedAssemblies.Add ("System.Drawing.dll");
        cu.ReferencedAssemblies.Add ("System.Windows.Forms.dll");
        cu.ReferencedAssemblies.Add ("System.Xml.dll");

        CodeTypeDeclaration cd = new CodeTypeDeclaration ("TEST");
        cd.IsClass = true;
        nspace.Types.Add (cd);

        CodeMemberMethod cmm;

        // Arrays of Arrays
#if !WHIDBEY
        // Everett VB code provider doesn't support array of array initialization
        if (!(provider is Microsoft.VisualBasic.VBCodeProvider)) {
#endif
            if (Supports (provider, GeneratorSupport.ArraysOfArrays)) {
                AddScenario ("CheckArrayOfArrays");
                cmm = new CodeMemberMethod ();
                cmm.Name = "ArraysOfArrays";
                cmm.ReturnType = new CodeTypeReference (typeof (int));
                cmm.Attributes = MemberAttributes.Final | MemberAttributes.Public;
                cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference (typeof (int[][])),
                    "arrayOfArrays", new CodeArrayCreateExpression (typeof (int[][]),
                    new CodeArrayCreateExpression (typeof (int[]), new CodePrimitiveExpression (3), new CodePrimitiveExpression (4)),
                    new CodeArrayCreateExpression (typeof (int[]), new CodeExpression[] {new CodePrimitiveExpression (1)}))));
                cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArrayIndexerExpression (
                    new CodeArrayIndexerExpression (new CodeVariableReferenceExpression ("arrayOfArrays"), new CodePrimitiveExpression (0)),
                    new CodePrimitiveExpression (1))));
                cd.Members.Add (cmm);
            }
#if !WHIDBEY
        }
#endif

        // assembly attributes
        if (Supports (provider, GeneratorSupport.AssemblyAttributes)) {
            AddScenario ("CheckAssemblyAttributes");
            CodeAttributeDeclarationCollection attrs = cu.AssemblyCustomAttributes;
            attrs.Add (new CodeAttributeDeclaration ("System.Reflection.AssemblyTitle", new
                CodeAttributeArgument (new CodePrimitiveExpression ("MyAssembly"))));
            attrs.Add (new CodeAttributeDeclaration ("System.Reflection.AssemblyVersion", new
                CodeAttributeArgument (new CodePrimitiveExpression ("1.0.6.2"))));
        }

        CodeTypeDeclaration class1 = new CodeTypeDeclaration ();
        if (Supports (provider, GeneratorSupport.ChainedConstructorArguments)) {
            AddScenario ("CheckChainedConstructorArgs");
            class1.Name = "Test2";
            class1.IsClass = true;
            nspace.Types.Add (class1);

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

            CodeConstructor cctor = new CodeConstructor ();
            cctor.Attributes = MemberAttributes.Public;
            cctor.ChainedConstructorArgs.Add (new CodePrimitiveExpression ("testingString"));
            cctor.ChainedConstructorArgs.Add (new CodePrimitiveExpression (null));
            cctor.ChainedConstructorArgs.Add (new CodePrimitiveExpression (null));
            class1.Members.Add (cctor);

            CodeConstructor cc = new CodeConstructor ();
            cc.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded;
            cc.Parameters.Add (new CodeParameterDeclarationExpression (typeof (string), "p1"));
            cc.Parameters.Add (new CodeParameterDeclarationExpression (typeof (string), "p2"));
            cc.Parameters.Add (new CodeParameterDeclarationExpression (typeof (string), "p3"));
            cc.Statements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (new CodeThisReferenceExpression ()
                , "stringField"), new CodeArgumentReferenceExpression ("p1")));
            class1.Members.Add (cc);

            // verify chained constructors work
            cmm = new CodeMemberMethod ();
            cmm.Name = "ChainedConstructorUse";
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            cmm.ReturnType = new CodeTypeReference (typeof (String));
            // utilize constructor
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("Test2", "t", new CodeObjectCreateExpression ("Test2")));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodePropertyReferenceExpression (
                new CodeVariableReferenceExpression ("t"), "accessStringField")));
            cd.Members.Add (cmm);
        }

        // complex expressions
        if (Supports (provider, GeneratorSupport.ComplexExpressions)) {
            AddScenario ("CheckComplexExpressions");
            cmm = new CodeMemberMethod ();
            cmm.Name = "ComplexExpressions";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Final | MemberAttributes.Public;
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
            cmm.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("i"),
                new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"), CodeBinaryOperatorType.Multiply,
                new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"), CodeBinaryOperatorType.Add,
                new CodePrimitiveExpression (3)))));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("i")));
            cd.Members.Add (cmm);
        }

        if (Supports (provider, GeneratorSupport.DeclareEnums)) {
            AddScenario ("CheckDeclareEnums");
            CodeTypeDeclaration ce = new CodeTypeDeclaration ("DecimalEnum");
            ce.IsEnum = true;
            nspace.Types.Add (ce);

            // things to enumerate
            for (int k = 0; k < 5; k++)
            {
                CodeMemberField Field = new CodeMemberField ("System.Int32", "Num" + (k).ToString ());
                Field.InitExpression = new CodePrimitiveExpression (k);
                ce.Members.Add (Field);
            }
            cmm = new CodeMemberMethod ();
            cmm.Name = "OutputDecimalEnumVal";
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (typeof (int), "i");
            cmm.Parameters.Add (param);
            CodeBinaryOperatorExpression eq       = new CodeBinaryOperatorExpression (
                new CodeArgumentReferenceExpression ("i"), CodeBinaryOperatorType.ValueEquality,
                new CodePrimitiveExpression (3));
            CodeMethodReturnStatement    truestmt = new CodeMethodReturnStatement (
                new CodeCastExpression (typeof (int),
                new CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("DecimalEnum"), "Num3")));
            CodeConditionStatement       condstmt = new CodeConditionStatement (eq, truestmt);
            cmm.Statements.Add (condstmt);

            eq = new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"),
                CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression (4));
            truestmt = new CodeMethodReturnStatement (new CodeCastExpression (typeof (int), new
                CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("DecimalEnum"), "Num4")));
            condstmt = new CodeConditionStatement (eq, truestmt);
            cmm.Statements.Add (condstmt);
            eq = new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"),
                CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression (2));
            truestmt = new CodeMethodReturnStatement (new CodeCastExpression (typeof (int), new
                CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("DecimalEnum"), "Num2")));
            condstmt = new CodeConditionStatement (eq, truestmt);
            cmm.Statements.Add (condstmt);

            eq = new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"),
                CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression (1));
            truestmt = new CodeMethodReturnStatement (new CodeCastExpression (typeof (int), new
                CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("DecimalEnum"), "Num1")));
            condstmt = new CodeConditionStatement (eq, truestmt);
            cmm.Statements.Add (condstmt);

            eq = new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"),
                CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression (0));
            truestmt = new CodeMethodReturnStatement (new CodeCastExpression (typeof (int), new
                CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("DecimalEnum"), "Num0")));
            condstmt = new CodeConditionStatement (eq, truestmt);
            cmm.Statements.Add (condstmt);

            cmm.ReturnType = new CodeTypeReference ("System.Int32");

            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
                new CodeArgumentReferenceExpression ("i"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression (10))));
            cd.Members.Add (cmm);
        }

        if (Supports (provider, GeneratorSupport.DeclareInterfaces)) {
            AddScenario ("CheckDeclareInterfaces");
            cmm = new CodeMemberMethod ();
            cmm.Name = "TestSingleInterface";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("TestSingleInterfaceImp", "t", new CodeObjectCreateExpression ("TestSingleInterfaceImp")));
            CodeMethodInvokeExpression methodinvoke = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("t")
                , "InterfaceMethod");
            methodinvoke.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
            cmm.Statements.Add (new CodeMethodReturnStatement (methodinvoke));
            cd.Members.Add (cmm);

            class1 = new CodeTypeDeclaration ("InterfaceA");
            class1.IsInterface = true;
            nspace.Types.Add (class1);
            cmm = new CodeMemberMethod ();
            cmm.Attributes = MemberAttributes.Public;
            cmm.Name = "InterfaceMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            class1.Members.Add (cmm);

            if (Supports (provider, GeneratorSupport.MultipleInterfaceMembers)) {
                AddScenario ("CheckMultipleInterfaceMembers");
                CodeTypeDeclaration classDecl = new CodeTypeDeclaration ("InterfaceB");
                classDecl.IsInterface = true;
                nspace.Types.Add (classDecl);
                cmm = new CodeMemberMethod ();
                cmm.Name = "InterfaceMethod";
                cmm.Attributes = MemberAttributes.Public;
                cmm.ReturnType = new CodeTypeReference (typeof (int));
                cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
                classDecl.Members.Add (cmm);

                CodeTypeDeclaration class2 = new CodeTypeDeclaration ("TestMultipleInterfaceImp");
                class2.BaseTypes.Add (new CodeTypeReference ("System.Object"));
                class2.BaseTypes.Add (new CodeTypeReference ("InterfaceB"));
                class2.BaseTypes.Add (new CodeTypeReference ("InterfaceA"));
                class2.IsClass = true;
                nspace.Types.Add (class2);
                cmm = new CodeMemberMethod ();
                cmm.ImplementationTypes.Add (new CodeTypeReference ("InterfaceA"));
                cmm.ImplementationTypes.Add (new CodeTypeReference ("InterfaceB"));
                cmm.Name = "InterfaceMethod";
                cmm.ReturnType = new CodeTypeReference (typeof (int));
                cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
                cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
                class2.Members.Add (cmm);

                cmm = new CodeMemberMethod ();
                cmm.Name = "TestMultipleInterfaces";
                cmm.ReturnType = new CodeTypeReference (typeof (int));
                cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
                cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                cmm.Statements.Add (new CodeVariableDeclarationStatement ("TestMultipleInterfaceImp", "t", new CodeObjectCreateExpression ("TestMultipleInterfaceImp")));
                cmm.Statements.Add (new CodeVariableDeclarationStatement ("InterfaceA", "interfaceAobject", new CodeCastExpression ("InterfaceA",
                    new CodeVariableReferenceExpression ("t"))));
                cmm.Statements.Add (new CodeVariableDeclarationStatement ("InterfaceB", "interfaceBobject", new CodeCastExpression ("InterfaceB",
                    new CodeVariableReferenceExpression ("t"))));
                methodinvoke = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("interfaceAobject")
                    , "InterfaceMethod");
                methodinvoke.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
                CodeMethodInvokeExpression methodinvoke2 = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("interfaceBobject")
                    , "InterfaceMethod");
                methodinvoke2.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
                cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
                    methodinvoke,
                    CodeBinaryOperatorType.Subtract, methodinvoke2)));
                cd.Members.Add (cmm);
            }

            class1 = new CodeTypeDeclaration ("TestSingleInterfaceImp");
            class1.BaseTypes.Add (new CodeTypeReference ("System.Object"));
            class1.BaseTypes.Add (new CodeTypeReference ("InterfaceA"));
            class1.IsClass = true;
            nspace.Types.Add (class1);
            cmm = new CodeMemberMethod ();
            cmm.ImplementationTypes.Add (new CodeTypeReference ("InterfaceA"));
            cmm.Name = "InterfaceMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            cmm.Attributes = MemberAttributes.Public;
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
            class1.Members.Add (cmm);
        }

        /*if (Supports (provider, GeneratorSupport.DeclareValueTypes)) {
            AddScenario ("CheckDeclareValueTypes");

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
            structB.TypeAttributes = TypeAttributes.NestedPublic;
            structB.Attributes = MemberAttributes.Public;
            structB.IsStruct = true;

            CodeMemberField firstInt = new CodeMemberField (typeof (int), "int1");
            firstInt.Attributes = MemberAttributes.Public;
            structB.Members.Add (firstInt);

            CodeMemberField innerStruct = new CodeMemberField ("structB", "innerStruct");
            innerStruct.Attributes = MemberAttributes.Public;

            structA.Members.Add (structB);
            structA.Members.Add (innerStruct);
            nspace.Types.Add (structA);

            CodeMemberMethod nestedStructMethod = new CodeMemberMethod ();
            nestedStructMethod.Name = "NestedStructMethod";
            nestedStructMethod.ReturnType = new CodeTypeReference (typeof (int));
            nestedStructMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            CodeVariableDeclarationStatement varStructA = new CodeVariableDeclarationStatement ("structA", "varStructA");
            nestedStructMethod.Statements.Add (varStructA);
            nestedStructMethod.Statements.Add
                (
                new CodeAssignStatement
                (
									new CodeFieldReferenceExpression (new CodeFieldReferenceExpression (new CodeVariableReferenceExpression ("varStructA"), "innerStruct"), "int1"),
									new CodePrimitiveExpression (3)
                )
                );
            nestedStructMethod.Statements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (new CodeFieldReferenceExpression (new CodeVariableReferenceExpression ("varStructA"), "innerStruct"), "int1")));
            cd.Members.Add (nestedStructMethod);
        }*/
        if (Supports (provider, GeneratorSupport.EntryPointMethod)) {
            AddScenario ("CheckEntryPointMethod");
            CodeEntryPointMethod cep = new CodeEntryPointMethod ();
            cd.Members.Add (cep);
        }
        // goto statements
        if (Supports (provider, GeneratorSupport.GotoStatements)) {
            AddScenario ("CheckGotoStatements");
            cmm = new CodeMemberMethod ();
            cmm.Name = "GoToMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (typeof (int), "i");
            cmm.Parameters.Add (param);
            CodeConditionStatement condstmt = new CodeConditionStatement (new CodeBinaryOperatorExpression (
                new CodeArgumentReferenceExpression ("i"), CodeBinaryOperatorType.LessThan, new CodePrimitiveExpression (1)),
                new CodeGotoStatement ("comehere"));
            cmm.Statements.Add (condstmt);
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (6)));
            cmm.Statements.Add (new CodeLabeledStatement ("comehere",
                new CodeMethodReturnStatement (new CodePrimitiveExpression (7))));
            cd.Members.Add (cmm);
        }
        if (Supports (provider, GeneratorSupport.NestedTypes)) {
            AddScenario ("CheckNestedTypes");
            cmm = new CodeMemberMethod ();
            cmm.Name = "CallingPublicNestedScenario";
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference
                ("PublicNestedClassA+PublicNestedClassB2+PublicNestedClassC"), "t",
                new CodeObjectCreateExpression (new CodeTypeReference
                ("PublicNestedClassA+PublicNestedClassB2+PublicNestedClassC"))));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("t"),
                "publicNestedClassesMethod",
                new CodeArgumentReferenceExpression ("i"))));
            cd.Members.Add (cmm);

            class1 = new CodeTypeDeclaration ("PublicNestedClassA");
            class1.IsClass = true;
            nspace.Types.Add (class1);
            CodeTypeDeclaration nestedClass = new CodeTypeDeclaration ("PublicNestedClassB1");
            nestedClass.IsClass = true;
            nestedClass.TypeAttributes = TypeAttributes.NestedPublic;
            class1.Members.Add (nestedClass);
            nestedClass = new CodeTypeDeclaration ("PublicNestedClassB2");
            nestedClass.TypeAttributes = TypeAttributes.NestedPublic;
            nestedClass.IsClass = true;
            class1.Members.Add (nestedClass);
            CodeTypeDeclaration innerNestedClass = new CodeTypeDeclaration ("PublicNestedClassC");
            innerNestedClass.TypeAttributes = TypeAttributes.NestedPublic;
            innerNestedClass.IsClass = true;
            nestedClass.Members.Add (innerNestedClass);
            cmm = new CodeMemberMethod ();
            cmm.Name = "publicNestedClassesMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
            innerNestedClass.Members.Add (cmm);
        }
        // Parameter Attributes
        if (Supports (provider, GeneratorSupport.ParameterAttributes)) {
            AddScenario ("CheckParameterAttributes");
            CodeMemberMethod method1 = new CodeMemberMethod ();
            method1.Name = "MyMethod";
            method1.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            CodeParameterDeclarationExpression param1 = new CodeParameterDeclarationExpression (typeof (string), "blah");
            param1.CustomAttributes.Add (
                new CodeAttributeDeclaration (
                "System.Xml.Serialization.XmlElementAttribute",
                new CodeAttributeArgument (
                "Form",
                new CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("System.Xml.Schema.XmlSchemaForm"), "Unqualified")),
                new CodeAttributeArgument (
                "IsNullable",
                new CodePrimitiveExpression (false))));
            method1.Parameters.Add (param1);
            cd.Members.Add (method1);
        }
        // public static members
        if (Supports (provider, GeneratorSupport.PublicStaticMembers)) {
            AddScenario ("CheckPublicStaticMembers");
            cmm = new CodeMemberMethod ();
            cmm.Name = "PublicStaticMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (16)));
            cd.Members.Add (cmm);
        }
        // reference parameters
        if (Supports (provider, GeneratorSupport.ReferenceParameters)) {
            AddScenario ("CheckReferenceParameters");
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
            cd.Members.Add (cmm);

            cmm = new CodeMemberMethod ();
            cmm.Name = "CallingWork";
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            CodeParameterDeclarationExpression parames = new CodeParameterDeclarationExpression (typeof (int), "a");
            cmm.Parameters.Add (parames);
            cmm.ReturnType = new CodeTypeReference ("System.Int32");
            cmm.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("a"),
                new CodePrimitiveExpression (10)));
            cmm.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "b"));
            // invoke the method called "work"
            CodeMethodInvokeExpression methodinvoked = new CodeMethodInvokeExpression (new CodeMethodReferenceExpression
                (new CodeTypeReferenceExpression ("TEST"), "Work"));
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
            cd.Members.Add (cmm);
        }
        if (Supports (provider, GeneratorSupport.ReturnTypeAttributes)) {
            AddScenario ("CheckReturnTypeAttributes");
            CodeMemberMethod function1 = new CodeMemberMethod ();
            function1.Name = "MyFunction";
            function1.ReturnType = new CodeTypeReference (typeof (string));
            function1.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            function1.ReturnTypeCustomAttributes.Add (new
                CodeAttributeDeclaration ("System.Xml.Serialization.XmlIgnoreAttribute"));
            function1.ReturnTypeCustomAttributes.Add (new CodeAttributeDeclaration ("System.Xml.Serialization.XmlRootAttribute", new
                CodeAttributeArgument ("Namespace", new CodePrimitiveExpression ("Namespace Value")), new
                CodeAttributeArgument ("ElementName", new CodePrimitiveExpression ("Root, hehehe"))));
            function1.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression ("Return")));
            cd.Members.Add (function1);
        }
        if (Supports (provider, GeneratorSupport.StaticConstructors)) {
            AddScenario ("CheckStaticConstructors");
            cmm = new CodeMemberMethod ();
            cmm.Name = "TestStaticConstructor";
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (typeof (int), "a");
            cmm.Parameters.Add (param);
            // utilize constructor
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("Test4", "t", new CodeObjectCreateExpression ("Test4")));
            // set then get number
            cmm.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (new CodeVariableReferenceExpression ("t"), "i")
                , new CodeArgumentReferenceExpression ("a")));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodePropertyReferenceExpression (
                new CodeVariableReferenceExpression ("t"), "i")));
            cd.Members.Add (cmm);

            class1 = new CodeTypeDeclaration ();
            class1.Name = "Test4";
            class1.IsClass = true;
            nspace.Types.Add (class1);

            class1.Members.Add (new CodeMemberField (new CodeTypeReference (typeof (int)), "number"));
            CodeMemberProperty prop = new CodeMemberProperty ();
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
        if (Supports (provider, GeneratorSupport.TryCatchStatements)) {
            AddScenario ("CheckTryCatchStatements");
            cmm = new CodeMemberMethod ();
            cmm.Name = "TryCatchMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (typeof (int), "a");
            cmm.Parameters.Add (param);

            CodeTryCatchFinallyStatement tcfstmt = new CodeTryCatchFinallyStatement ();
            tcfstmt.FinallyStatements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("a"), new
                CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("a"), CodeBinaryOperatorType.Add,
                new CodePrimitiveExpression (5))));
            cmm.Statements.Add (tcfstmt);
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
            cd.Members.Add (cmm);
        }
        if (Supports (provider, GeneratorSupport.DeclareEvents)) {
            AddScenario ("CheckDeclareEvents");
            CodeNamespace ns = new CodeNamespace ();
            ns.Name = "MyNamespace";
            ns.Imports.Add (new CodeNamespaceImport ("System"));
            ns.Imports.Add (new CodeNamespaceImport ("System.Drawing"));
            ns.Imports.Add (new CodeNamespaceImport ("System.Windows.Forms"));
            ns.Imports.Add (new CodeNamespaceImport ("System.ComponentModel"));
            cu.Namespaces.Add (ns);
            class1 = new CodeTypeDeclaration ("Test");
            class1.IsClass = true;
            class1.BaseTypes.Add (new CodeTypeReference ("Form"));
            ns.Types.Add (class1);

            CodeMemberField mfield = new CodeMemberField (new CodeTypeReference ("Button"), "b");
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

            CodeMemberEvent evt = new CodeMemberEvent ();
            evt.Name = "MyEvent";
            evt.Type = new CodeTypeReference ("System.EventHandler");
            evt.Attributes = MemberAttributes.Public;
            class1.Members.Add (evt);

            cmm = new CodeMemberMethod ();
            cmm.Name = "b_Click";
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object), "sender"));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (EventArgs), "e"));
            class1.Members.Add (cmm);
        }
        if (Supports (provider, GeneratorSupport.MultidimensionalArrays)) {
            // no codedom language represents declaration of multidimensional arrays
        }
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        // if we get to this point, Entry point method and Event declaration
        // have been successfully compiled so
        if (Supports (provider, GeneratorSupport.EntryPointMethod))
            VerifyScenario ("CheckEntryPointMethod");
        if (Supports (provider, GeneratorSupport.DeclareEvents))
            VerifyScenario ("CheckDeclareEvents");

        // Verifying Assembly Attributes
        if (Supports (provider, GeneratorSupport.AssemblyAttributes)) {
            object[] attributes = asm.GetCustomAttributes (true);
            bool verified = VerifyAttribute (attributes, typeof (AssemblyTitleAttribute), "Title", "MyAssembly");
            verified &= VerifyAttribute (attributes, typeof (AssemblyVersionAttribute), "Version", "1.0.6.2") ||
                asm.GetName ().Version.Equals (new Version (1, 0, 6, 2));
            if (verified)
                VerifyScenario ("CheckAssemblyAttributes");
        }

        AddScenario ("InstantiateTEST", "Find and instantiate TEST.");
        if (!FindAndInstantiate ("NSPC.TEST", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTEST");

        // verify arrays of arrays
#if !WHIDBEY
        if (!(provider is VBCodeProvider)) {
#endif
            if (Supports (provider, GeneratorSupport.ArraysOfArrays) &&
                    VerifyMethod (genType, genObject, "ArraysOfArrays", new object[] {}, 4)) {
                VerifyScenario ("CheckArrayOfArrays");
            }
#if !WHIDBEY
        }
#endif

        // verify chained constructors
        if (Supports (provider, GeneratorSupport.ChainedConstructorArguments) &&
                VerifyMethod (genType, genObject, "ChainedConstructorUse", new object[] {}, "testingString")) {
            VerifyScenario ("CheckChainedConstructorArgs");
        }
        // verify complex expressions
        if (Supports (provider, GeneratorSupport.ComplexExpressions) &&
                VerifyMethod (genType, genObject, "ComplexExpressions", new object[] {8}, 88)) {
            VerifyScenario ("CheckComplexExpressions");
        }
        // verify enumerations
        if (Supports (provider, GeneratorSupport.DeclareEnums) &&
                VerifyMethod (genType, genObject, "OutputDecimalEnumVal", new object[] {4}, 4)) {
            VerifyScenario ("CheckDeclareEnums");
        }
        // verify interfaces
        if (Supports (provider, GeneratorSupport.DeclareInterfaces) &&
                VerifyMethod (genType, genObject, "TestSingleInterface", new object[] {4}, 4)) {
            VerifyScenario ("CheckDeclareInterfaces");
        }
        // verify nested structs
        /*if (Supports (provider, GeneratorSupport.DeclareValueTypes) &&
                VerifyMethod (genType, genObject, "NestedStructMethod", new object[] {}, 3)) {
            VerifyScenario ("CheckDeclareValueTypes");
        }*/
        // verify goto
        if (Supports (provider, GeneratorSupport.GotoStatements) &&
                VerifyMethod (genType, genObject, "GoToMethod", new object[] {0}, 7) &&
                VerifyMethod (genType, genObject, "GoToMethod", new object[] {2}, 6)) {
            VerifyScenario ("CheckGotoStatements");
        }
        // multiple interfaces
        if (Supports (provider, GeneratorSupport.DeclareInterfaces) &&
                Supports (provider, GeneratorSupport.MultipleInterfaceMembers) &&
                VerifyMethod (genType, genObject, "TestMultipleInterfaces", new object[] {6}, 0)) {
            VerifyScenario ("CheckMultipleInterfaceMembers");
        }
        // nested types
        if (Supports (provider, GeneratorSupport.NestedTypes) &&
                VerifyMethod (genType, genObject, "CallingPublicNestedScenario", new object[] {7}, 7)) {
            VerifyScenario ("CheckNestedTypes");
        }
        // parameter attributes
        MethodInfo methodInfo;
        if (Supports (provider, GeneratorSupport.ParameterAttributes)) {
            methodInfo = genType.GetMethod ("MyMethod");
            ParameterInfo[] paramInfo = methodInfo.GetParameters ();

            object[] paramAttributes = paramInfo[0].GetCustomAttributes (typeof (System.Xml.Serialization.XmlElementAttribute), true);
            if (paramAttributes.GetLength (0) == 1) {
                VerifyScenario ("CheckParameterAttributes");
            }
        }
        // public static members
        if (Supports (provider, GeneratorSupport.PublicStaticMembers) &&
                VerifyMethod (genType, genObject, "PublicStaticMethod", new object[] {}, 16)) {
            VerifyScenario ("CheckPublicStaticMembers");
        }
        // reference parameters
        if (Supports (provider, GeneratorSupport.ReferenceParameters) &&
                VerifyMethod (genType, genObject, "CallingWork", new object[] {5}, 19)) {
            VerifyScenario ("CheckReferenceParameters");
        }
        // return type attributes
        if (Supports (provider, GeneratorSupport.ReturnTypeAttributes)) {
            methodInfo = genType.GetMethod ("MyFunction");
            ICustomAttributeProvider returnTypeAttributes = methodInfo.ReturnTypeCustomAttributes;
            if (returnTypeAttributes.IsDefined (typeof (System.Xml.Serialization.XmlRootAttribute), true) &&
                returnTypeAttributes.IsDefined (typeof (System.Xml.Serialization.XmlIgnoreAttribute), true)) {
                VerifyScenario ("CheckReturnTypeAttributes");
            }
        }
        // static constructors
        if (Supports (provider, GeneratorSupport.StaticConstructors) &&
                VerifyMethod (genType, genObject, "TestStaticConstructor", new object[] {7}, 7)) {
            VerifyScenario ("CheckStaticConstructors");
        }
        // try catch statements
        if (Supports (provider, GeneratorSupport.TryCatchStatements) &&
                VerifyMethod (genType, genObject, "TryCatchMethod", new object[] {1}, 6) &&
                VerifyMethod (genType, genObject, "TryCatchMethod", new object[] {10}, 15)) {
            VerifyScenario ("CheckTryCatchStatements");
        }
    }
}

