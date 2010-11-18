using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class ImplementingStructsTest : CodeDomTestTree {

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
            return "ImplementingStructsTest";
        }
    }

    public override string Description {
        get {
            return "Tests structs that implement other things.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        CodeNamespace nspace = new CodeNamespace ("NSPC");
        nspace.Imports.Add (new CodeNamespaceImport ("System"));
        cu.Namespaces.Add (nspace);

        CodeTypeDeclaration cd = new CodeTypeDeclaration ("TestingStructs");
        cd.IsClass = true;
        nspace.Types.Add (cd);
        if (Supports (provider, GeneratorSupport.DeclareValueTypes)) {
            // GENERATES (C#):
            //        public int CallingStructMethod(int i) {
            //            StructImplementation o = new StructImplementation ();
            //            return o.StructMethod(i);
            //        }
            AddScenario ("CheckCallingStructMethod");
            CodeMemberMethod cmm = new CodeMemberMethod ();
            cmm.Name = "CallingStructMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
            cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference ("StructImplementation"), "o", new
                CodeObjectCreateExpression (new CodeTypeReference ("StructImplementation"))));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (new CodeMethodReferenceExpression (
                new CodeVariableReferenceExpression ("o"),
                "StructMethod"), new CodeArgumentReferenceExpression ("i"))));
            cd.Members.Add (cmm);

            // GENERATES (C#):
            //        public int UsingValueStruct(int i) {
            //            ValueStruct StructObject = new ValueStruct();
            //            StructObject.x = i;
            //            return StructObject.x;
            //        }
            AddScenario ("CheckUsingValueStruct");
            cmm = new CodeMemberMethod ();
            cmm.Name = "UsingValueStruct";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("ValueStruct", "StructObject", new
                CodeObjectCreateExpression ("ValueStruct")));
            cmm.Statements.Add (new CodeAssignStatement (new CodeFieldReferenceExpression (new CodeVariableReferenceExpression ("StructObject"), "x"),
                new CodeArgumentReferenceExpression ("i")));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeFieldReferenceExpression (new
                CodeVariableReferenceExpression ("StructObject"), "x")));
            cd.Members.Add (cmm);

            // GENERATES (C#):
            //        public int UsingStructProperty(int i) {
            //            StructImplementation StructObject = new StructImplementation();
            //            StructObject.UseIField = i;
            //            return StructObject.UseIField;
            //        }
            AddScenario ("CheckUsingStructProperty");
            cmm = new CodeMemberMethod ();
            cmm.Name = "UsingStructProperty";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("StructImplementation", "StructObject", new
                CodeObjectCreateExpression ("StructImplementation")));
            cmm.Statements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression (
                new CodeVariableReferenceExpression ("StructObject"), "UseIField"),
                new CodeArgumentReferenceExpression ("i")));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodePropertyReferenceExpression (new
                CodeVariableReferenceExpression ("StructObject"), "UseIField")));
            cd.Members.Add (cmm);

            // GENERATES (C#):
            //        public int UsingInterfaceStruct(int i) {
            //            ImplementInterfaceStruct IStructObject = new ImplementInterfaceStruct();
            //            return IStructObject.InterfaceMethod(i);
            //        }
            if (Supports (provider, GeneratorSupport.DeclareInterfaces)) {
                AddScenario ("CheckUsingInterfaceStruct");
                // method to test struct implementing interfaces
                cmm = new CodeMemberMethod ();
                cmm.Name = "UsingInterfaceStruct";
                cmm.ReturnType = new CodeTypeReference (typeof (int));
                cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
                cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
                cmm.Statements.Add (new CodeVariableDeclarationStatement ("ImplementInterfaceStruct", "IStructObject", new
                    CodeObjectCreateExpression ("ImplementInterfaceStruct")));
                cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (
                    new CodeVariableReferenceExpression ("IStructObject"), "InterfaceMethod",
                    new CodeArgumentReferenceExpression ("i"))));
                cd.Members.Add (cmm);
            }

            // GENERATES (C#):
            //    public struct StructImplementation { 
            //        int i;
            //        public int UseIField {
            //            get {
            //                return i;
            //            }
            //            set {
            //                i = value;
            //            }
            //        }
            //        public int StructMethod(int i) {
            //            return (5 + i);
            //        }
            //    }
            cd = new CodeTypeDeclaration ("StructImplementation");
            cd.IsStruct = true;
            nspace.Types.Add (cd);

            // declare an integer field
            CodeMemberField field = new CodeMemberField (new CodeTypeReference (typeof (int)), "i");
            field.Attributes = MemberAttributes.Public;
            cd.Members.Add (field);

            CodeMemberProperty prop = new CodeMemberProperty ();
            prop.Name = "UseIField";
            prop.Type = new CodeTypeReference (typeof (int));
            prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            CodeFieldReferenceExpression fref = new CodeFieldReferenceExpression ();
            fref.FieldName = "i";
            prop.GetStatements.Add (new CodeMethodReturnStatement (fref));
            prop.SetStatements.Add (new CodeAssignStatement (fref,
                new CodePropertySetValueReferenceExpression ()));

            cd.Members.Add (prop);

            cmm = new CodeMemberMethod ();
            cmm.Name = "StructMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
                new CodePrimitiveExpression (5), CodeBinaryOperatorType.Add, new CodeArgumentReferenceExpression ("i"))));
            cd.Members.Add (cmm);

            // GENERATES (C#):
            //    public struct ValueStruct {   
            //        public int x;
            //    }
            cd = new CodeTypeDeclaration ("ValueStruct");
            cd.IsStruct = true;
            nspace.Types.Add (cd);

            // declare an integer field
            field = new CodeMemberField (new CodeTypeReference (typeof (int)), "x");
            field.Attributes = MemberAttributes.Public;
            cd.Members.Add (field);

            if (Supports (provider, GeneratorSupport.DeclareInterfaces)) {
                // interface to be implemented    
                // GENERATES (C#):
                //    public interface InterfaceStruct {   
                //        int InterfaceMethod(int i);
                //    }
                cd = new CodeTypeDeclaration ("InterfaceStruct");
                cd.IsInterface = true;
                nspace.Types.Add (cd);

                // method in the interface
                cmm = new CodeMemberMethod ();
                cmm.Name = "InterfaceMethod";
                cmm.ReturnType = new CodeTypeReference (typeof (int));
                cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
                cd.Members.Add (cmm);

                // struct to implement an interface
                // GENERATES (C#):
                //    public struct ImplementInterfaceStruct : InterfaceStruct {
                //        public int InterfaceMethod(int i) {
                //            return (8 + i);
                //        }
                //    }
                cd = new CodeTypeDeclaration ("ImplementInterfaceStruct");
                cd.BaseTypes.Add (new CodeTypeReference ("InterfaceStruct"));
                cd.IsStruct = true;
                nspace.Types.Add (cd);

                field = new CodeMemberField (new CodeTypeReference (typeof (int)), "i");
                field.Attributes = MemberAttributes.Public;
                cd.Members.Add (field);
                // implement interface method
                cmm = new CodeMemberMethod ();
                cmm.Name = "InterfaceMethod";
                cmm.ImplementationTypes.Add (new CodeTypeReference ("InterfaceStruct"));
                cmm.ReturnType = new CodeTypeReference (typeof (int));
                cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (new CodePrimitiveExpression (8),
                    CodeBinaryOperatorType.Add,
                    new CodeArgumentReferenceExpression ("i"))));
                cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
                cd.Members.Add (cmm);
            }
        }
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {

        if (Supports (provider, GeneratorSupport.DeclareValueTypes)) {
            object genObject;
            Type   genType;

            AddScenario ("InstantiateTestingStructs", "Find and instantiate TestingStructs.");
            if (!FindAndInstantiate ("NSPC.TestingStructs", asm, out genObject, out genType))
                return;
            VerifyScenario ("InstantiateTestingStructs");

            // verify method return value for method which calls struct method
            if (VerifyMethod (genType, genObject, "CallingStructMethod", new object[] {6}, 11)) {
                VerifyScenario ("CheckCallingStructMethod");
            }
            // verify method return value for method which uses struct property
            if (VerifyMethod (genType, genObject, "UsingStructProperty", new object[] {6}, 6)) {
                VerifyScenario ("CheckUsingStructProperty");
            }
            // verify method return value for using struct as value
            if (VerifyMethod (genType, genObject, "UsingValueStruct", new object[] {8}, 8)) {
                VerifyScenario ("CheckUsingValueStruct");
            }
            // verify method return value for method which uses struct that implements and interface
            if (Supports (provider, GeneratorSupport.DeclareInterfaces) &&
                    VerifyMethod (genType, genObject, "UsingInterfaceStruct", new object[] {6}, 14))
                VerifyScenario ("CheckUsingInterfaceStruct");
        }
    }
}

