// F#: Added cast where initializing variable with subclass
//     Removed test for 'sealed' classes
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class ClassTest : CodeDomTestTree {

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
            return "ClassTest";
        }
    }

    public override string Description {
        get {
            return "Tests classes.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        CodeNamespace nspace = new CodeNamespace ("NSPC");
        nspace.Imports.Add (new CodeNamespaceImport ("System"));
        cu.Namespaces.Add (nspace);

        cu.ReferencedAssemblies.Add ("System.Windows.Forms.dll");

        CodeTypeDeclaration cd = new CodeTypeDeclaration ("TestingClass");
        cd.IsClass = true;
        nspace.Types.Add (cd);

        CodeMemberMethod cmm;
        CodeMethodInvokeExpression methodinvoke;

        // GENERATES (C#):
        //        public int CallingPublicNestedScenario(int i) {
        //                PublicNestedClassA.PublicNestedClassB2.PublicNestedClassC t = 
        //                        new PublicNestedClassA.PublicNestedClassB2.PublicNestedClassC();
        //                return t.publicNestedClassesMethod(i);
        //        }
        if (Supports (provider, GeneratorSupport.NestedTypes)) {
            AddScenario ("CheckCallingPublicNestedScenario");
            cmm = new CodeMemberMethod ();
            cmm.Name = "CallingPublicNestedScenario";
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference
                ("PublicNestedClassA+PublicNestedClassB2+PublicNestedClassC"), "t",
                new CodeObjectCreateExpression (new CodeTypeReference
                ("PublicNestedClassA+PublicNestedClassB2+PublicNestedClassC"))));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("t"),
                "publicNestedClassesMethod",
                new CodeArgumentReferenceExpression ("i"))));
            cd.Members.Add (cmm);

            AddScenario ("CheckCallingPrivateNestedScenario");
            cmm = new CodeMemberMethod ();
            cmm.Name = "CallingPrivateNestedScenario";
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference
                ("PrivateNestedClassA"), "t",
                new CodeObjectCreateExpression (new CodeTypeReference ("PrivateNestedClassA"))));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("t"),
                "TestPrivateNestedClass",
                new CodeArgumentReferenceExpression ("i"))));
            cd.Members.Add (cmm);
        }

        // GENERATES (C#):
        //        public int CallingAbstractScenario(int i) {
        //            InheritAbstractClass t = new InheritAbstractClass();
        //            return t.AbstractMethod(i);
        //        }
        AddScenario ("CheckCallingAbstractScenario");
        cmm = new CodeMemberMethod ();
        cmm.Name = "CallingAbstractScenario";
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        cmm.Statements.Add (new CodeVariableDeclarationStatement ("InheritAbstractClass", "t", new CodeObjectCreateExpression ("InheritAbstractClass")));
        methodinvoke = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("t"), "AbstractMethod");
        methodinvoke.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
        cmm.Statements.Add (new CodeMethodReturnStatement (methodinvoke));
        cd.Members.Add (cmm);


        if (Supports (provider, GeneratorSupport.DeclareInterfaces)) {
            // testing multiple interface implementation class
            // GENERATES (C#):
            //        public int TestMultipleInterfaces(int i) {
            //             TestMultipleInterfaceImp t = new TestMultipleInterfaceImp();
            //             InterfaceA interfaceAobject = ((InterfaceA)(t));
            //             InterfaceB interfaceBobject = ((InterfaceB)(t));
            //             return (interfaceAobject.InterfaceMethod(i) - interfaceBobject.InterfaceMethod(i));
            //         }
            if (Supports (provider, GeneratorSupport.MultipleInterfaceMembers)) {
                cmm = new CodeMemberMethod();
                cmm.Name = "TestMultipleInterfaces";
                cmm.ReturnType = new CodeTypeReference(typeof(int));
                cmm.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "i"));
                cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
                cmm.Statements.Add(new CodeVariableDeclarationStatement("TestMultipleInterfaceImp","t",new CodeObjectCreateExpression("TestMultipleInterfaceImp")));
                cmm.Statements.Add(new CodeVariableDeclarationStatement("InterfaceA", "interfaceAobject", new CodeCastExpression("InterfaceA" , 
                                         new CodeVariableReferenceExpression("t"))));
                cmm.Statements.Add(new CodeVariableDeclarationStatement("InterfaceB", "interfaceBobject", new CodeCastExpression("InterfaceB" , 
                                         new CodeVariableReferenceExpression("t"))));
                methodinvoke = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("interfaceAobject")
                                                  , "InterfaceMethod");
                methodinvoke.Parameters.Add(new CodeArgumentReferenceExpression("i"));
                CodeMethodInvokeExpression methodinvoke2 = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("interfaceBobject")
                                                  , "InterfaceMethod");
                methodinvoke2.Parameters.Add(new CodeArgumentReferenceExpression("i"));
                cmm.Statements.Add(new CodeMethodReturnStatement(new CodeBinaryOperatorExpression(
                                         methodinvoke , 
                                         CodeBinaryOperatorType.Subtract, methodinvoke2)));
                cd.Members.Add(cmm);
            }

            // testing single interface implementation
            // GENERATES (C#):
            //        public int TestSingleInterface(int i) {
            //             TestMultipleInterfaceImp t = new TestMultipleInterfaceImp();
            //             return t.InterfaceMethod(i);
            //         }
            AddScenario ("CheckTestSingleInterface");
            cmm = new CodeMemberMethod ();
            cmm.Name = "TestSingleInterface";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
            cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("TestSingleInterfaceImp", "t", new CodeObjectCreateExpression ("TestSingleInterfaceImp")));
            methodinvoke = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("t")
                , "InterfaceMethod");
            methodinvoke.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
            cmm.Statements.Add (new CodeMethodReturnStatement (methodinvoke));
            cd.Members.Add (cmm);
        }


        // similar to the 'new' test, write a method to complete testing of the 'override' scenario
        // GENERATES (C#):
        //        public int CallingOverrideScenario(int i) {
        //            ClassWVirtualMethod t = new ClassWOverrideMethod();
        //            return t.VirtualMethod(i);
        //        }
        AddScenario ("CheckCallingOverrideScenario");
        cmm = new CodeMemberMethod ();
        cmm.Name = "CallingOverrideScenario";
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
        cmm.Statements.Add (new CodeVariableDeclarationStatement ("ClassWVirtualMethod", "t", new CodeCastExpression(new CodeTypeReference("ClassWVirtualMethod"), new CodeObjectCreateExpression ("ClassWOverrideMethod"))));
        methodinvoke = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("t"), "VirtualMethod");
        methodinvoke.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
        cmm.Statements.Add (new CodeMethodReturnStatement (methodinvoke));
        cd.Members.Add (cmm);

        // declare a base class
        // GENERATES (C#):
        //    public class ClassWVirtualMethod {
        //         public virtual int VirtualMethod(int a) {
        //             return a;
        //         }
        //     }
        cd = new CodeTypeDeclaration ("ClassWVirtualMethod");
        cd.IsClass = true;
        nspace.Types.Add (cd);
        cmm = new CodeMemberMethod ();
        cmm.Name = "VirtualMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
        cd.Members.Add (cmm);

        // inherit the base class
        // GENERATES (C#):
        //    public class ClassWMethod : ClassWVirtualMethod {
        //    }
        cd = new CodeTypeDeclaration ("ClassWMethod");
        cd.BaseTypes.Add (new CodeTypeReference ("ClassWVirtualMethod"));
        cd.IsClass = true;
        nspace.Types.Add (cd);

        // inheritance the base class with override method
        // GENERATES (C#):
        //    public class ClassWOverrideMethod : ClassWVirtualMethod {
        //         public override int VirtualMethod(int a) {
        //             return (2 * a);
        //         }
        //     }
        cd = new CodeTypeDeclaration ("ClassWOverrideMethod");
        cd.BaseTypes.Add (new CodeTypeReference ("ClassWVirtualMethod"));
        cd.IsClass = true;
        nspace.Types.Add (cd);
        cmm = new CodeMemberMethod ();
        cmm.Name = "VirtualMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Override;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
            new CodePrimitiveExpression (2), CodeBinaryOperatorType.Multiply
            , new CodeArgumentReferenceExpression ("a"))));
        cd.Members.Add (cmm);


        if (Supports (provider, GeneratorSupport.DeclareInterfaces)) {

            // ******** implement a single public interface ***********                      
            // declare an interface
            // GENERATES (C#):
            //    public interface InterfaceA {
            //         int InterfaceMethod(int a);
            //     }
            cd = new CodeTypeDeclaration ("InterfaceA");
            cd.IsInterface = true;
            nspace.Types.Add (cd);
            cmm = new CodeMemberMethod ();
            cmm.Attributes = MemberAttributes.Public;
            cmm.Name = "InterfaceMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            cd.Members.Add (cmm);

            // implement the interface
            // GENERATES (C#):
            //    public class TestSingleInterfaceImp : InterfaceA {
            //         public virtual int InterfaceMethod(int a) {
            //             return a;
            //         }
            //     }
            cd = new CodeTypeDeclaration ("TestSingleInterfaceImp");
            cd.BaseTypes.Add (new CodeTypeReference ("System.Object"));
            cd.BaseTypes.Add (new CodeTypeReference ("InterfaceA"));
            cd.IsClass = true;
            nspace.Types.Add (cd);
            cmm = new CodeMemberMethod ();
            cmm.ImplementationTypes.Add (new CodeTypeReference ("InterfaceA"));
            cmm.Name = "InterfaceMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            cmm.Attributes = MemberAttributes.Public;
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
            cd.Members.Add (cmm);

            // ********implement two interfaces with overloading method name*******
            // declare the second interface
            // GENERATES (C#):
            //    public interface InterfaceB {
            //         int InterfaceMethod(int a);
            //     }
            cd = new CodeTypeDeclaration ("InterfaceB");
            cd.IsInterface = true;
            nspace.Types.Add (cd);
            cmm = new CodeMemberMethod ();
            cmm.Name = "InterfaceMethod";
            cmm.Attributes = MemberAttributes.Public;
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            cd.Members.Add (cmm);

            // implement both of the interfaces
            // GENERATES (C#):
            //    public class TestMultipleInterfaceImp : InterfaceB, InterfaceA {
            //         public int InterfaceMethod(int a) {
            //             return a;
            //         }
            //     }
            if (Supports (provider, GeneratorSupport.MultipleInterfaceMembers)) {
                AddScenario ("CheckTestMultipleInterfaces");
                cd = new CodeTypeDeclaration ("TestMultipleInterfaceImp");
                cd.BaseTypes.Add (new CodeTypeReference ("System.Object"));
                cd.BaseTypes.Add (new CodeTypeReference ("InterfaceB"));
                cd.BaseTypes.Add (new CodeTypeReference ("InterfaceA"));
                cd.IsClass = true;
                nspace.Types.Add (cd);
                cmm = new CodeMemberMethod ();
                cmm.ImplementationTypes.Add (new CodeTypeReference ("InterfaceA"));
                cmm.ImplementationTypes.Add (new CodeTypeReference ("InterfaceB"));
                cmm.Name = "InterfaceMethod";
                cmm.ReturnType = new CodeTypeReference (typeof (int));
                cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
                cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
                cd.Members.Add (cmm);
            }
        }

        if (Supports (provider, GeneratorSupport.NestedTypes)) {
            // *********** public nested classes *************************
            // GENERATES (C#):
            //    public class PublicNestedClassA {
            //         public class PublicNestedClassB1 {
            //         }
            //         public class PublicNestedClassB2 {
            //             public class PublicNestedClassC {
            //                 public int publicNestedClassesMethod(int a) {
            //                     return a;
            //                 }
            //             }
            //         }
            //    }
            cd = new CodeTypeDeclaration ("PublicNestedClassA");
            cd.IsClass = true;
            nspace.Types.Add (cd);
            CodeTypeDeclaration nestedClass = new CodeTypeDeclaration ("PublicNestedClassB1");
            nestedClass.IsClass = true;
            nestedClass.TypeAttributes = TypeAttributes.NestedPublic;
            cd.Members.Add (nestedClass);
            nestedClass = new CodeTypeDeclaration ("PublicNestedClassB2");
            nestedClass.TypeAttributes = TypeAttributes.NestedPublic;
            nestedClass.IsClass = true;
            cd.Members.Add (nestedClass);
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

            // *********** private nested classes *************************
            // GENERATES (C#):
            //    public class PrivateNestedClassA {
            //         public int TestPrivateNestedClass(int i) {
            //             return PrivateNestedClassB.PrivateNestedClassesMethod(i);
            //         }
            //         private class PrivateNestedClassB {
            //             public int PrivateNestedClassesMethod(int a) {
            //                 return a;
            //             }
            //         }
            //     }
            cd = new CodeTypeDeclaration ("PrivateNestedClassA");
            cd.IsClass = true;
            nspace.Types.Add (cd);
            AddScenario ("CantFindPrivateNestedClass");
            nestedClass = new CodeTypeDeclaration ("PrivateNestedClassB");
            nestedClass.IsClass = true;
            nestedClass.TypeAttributes = TypeAttributes.NestedPrivate;
            cd.Members.Add (nestedClass);
            cmm = new CodeMemberMethod ();
            cmm.Name = "PrivateNestedClassesMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
            nestedClass.Members.Add (cmm);

            // test private, nested classes
            cmm = new CodeMemberMethod ();
            cmm.Name = "TestPrivateNestedClass";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
            cmm.Attributes = (cmm.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference ("PrivateNestedClassB"),
                        "objB"));
            cmm.Statements.Add (new CodeAssignStatement (new CodeVariableReferenceExpression ("objB"),
                        new CodeObjectCreateExpression (new CodeTypeReference ("PrivateNestedClassB"))));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeMethodInvokeExpression (new
                CodeVariableReferenceExpression ("objB"), "PrivateNestedClassesMethod", new
                CodeArgumentReferenceExpression ("i"))));
            cd.Members.Add (cmm);
        }

        //     ************ abstract class ***************
        // GENERATES (C#):
        //    public abstract class AbstractClass {
        //         public abstract int AbstractMethod(int i);
        //     }
        //    public class InheritAbstractClass : AbstractClass {
        //
        //         public override int AbstractMethod(int i) {
        //             return i;
        //         }
        //     }
        AddScenario ("TestAbstractAttributes");
        cd = new CodeTypeDeclaration ("AbstractClass");
        cd.TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public;
        cd.IsClass = true;
        nspace.Types.Add (cd);

        CodeTypeDeclaration inheritAbstractClass = new CodeTypeDeclaration ("InheritAbstractClass");
        inheritAbstractClass.IsClass = true;
        inheritAbstractClass.BaseTypes.Add (new CodeTypeReference ("AbstractClass"));
        nspace.Types.Add (inheritAbstractClass);

        cmm = new CodeMemberMethod ();
        cmm.Name = "AbstractMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Abstract;
        cd.Members.Add (cmm);

        cmm = new CodeMemberMethod ();
        cmm.Name = "AbstractMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Override;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("i")));
        inheritAbstractClass.Members.Add (cmm);

        //  ************  sealed class *****************
        // GENERATES (C#):
        //            sealed class SealedClass {
        //                 public int SealedClassMethod(int i) {
        //                     return i;
        //                 }
        //             }
        AddScenario ("TestSealedAttributes");
        cd = new CodeTypeDeclaration ("SealedClass");
        cd.IsClass = true;
        cd.TypeAttributes = TypeAttributes.Sealed;
        nspace.Types.Add (cd);

        cmm = new CodeMemberMethod ();
        cmm.Name = "SealedClassMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("i")));
        cd.Members.Add (cmm);

        // *************  class with custom attributes  ********************
        // GENERATES (C#):
        //     [System.Obsolete("Don\'t use this Class")]
        //     [System.Windows.Forms.AxHost.ClsidAttribute("Class.ID")]
        //     public class ClassWCustomAttributes {
        //     }
        cd = new CodeTypeDeclaration ("ClassWCustomAttributes");
        cd.IsClass = true;
        AddScenario ("FindObsoleteAttribute");
        cd.CustomAttributes.Add (new CodeAttributeDeclaration ("System.Obsolete", new CodeAttributeArgument (new CodePrimitiveExpression ("Don't use this Class"))));
        AddScenario ("FindOtherAttribute");
        cd.CustomAttributes.Add (new CodeAttributeDeclaration (typeof (System.Windows.Forms.AxHost.ClsidAttribute).FullName,
            new CodeAttributeArgument (new CodePrimitiveExpression ("Class.ID"))));
        nspace.Types.Add (cd);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateTestingClass", "Find and instantiate TestingClass.");
        if (!FindAndInstantiate ("NSPC.TestingClass", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTestingClass");

        if (Supports (provider, GeneratorSupport.NestedTypes)) {
            if (VerifyMethod (genType, genObject, "CallingPrivateNestedScenario", new object[] {7}, 7))
                VerifyScenario ("CheckCallingPrivateNestedScenario");
            if (VerifyMethod (genType, genObject, "CallingPublicNestedScenario", new object[] {7}, 7))
                VerifyScenario ("CheckCallingPublicNestedScenario");
        }
        if (Supports (provider, GeneratorSupport.DeclareInterfaces)) {
            if (VerifyMethod (genType, genObject, "TestSingleInterface", new object[] {7}, 7))
                VerifyScenario ("CheckTestSingleInterface");

            if (Supports (provider, GeneratorSupport.MultipleInterfaceMembers) &&
                   VerifyMethod (genType, genObject, "TestMultipleInterfaces", new object[] {7}, 0)) {
                VerifyScenario ("CheckTestMultipleInterfaces");
            }
        }
        if (VerifyMethod (genType, genObject, "CallingOverrideScenario", new object[] {7}, 14)) {
            VerifyScenario ("CheckCallingOverrideScenario");
        }
        if (VerifyMethod (genType, genObject, "CallingAbstractScenario", new object[] {7}, 7)) {
            VerifyScenario ("CheckCallingAbstractScenario");
        }

        // check that can't access private nested class from outside the class 
        if (Supports (provider, GeneratorSupport.NestedTypes) &&
                !FindType ("NSPC.PrivateNestedClassA.PrivateNestedClassB", asm, out genType)) {
            VerifyScenario ("CantFindPrivateNestedClass");
        }

        // verify abstract attribute on classes  
        if (FindType ("NSPC.AbstractClass", asm, out genType) &&
                (genType.Attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract) {
            VerifyScenario ("TestAbstractAttributes");
        }


        // verify sealed attribute on classes  
        if (FindType ("NSPC.SealedClass", asm, out genType) /*&&
                (genType.Attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed*/) {
            VerifyScenario ("TestSealedAttributes");
        }

        // verify custom attributes on classes  
        if (FindType ("NSPC.ClassWCustomAttributes", asm, out genType)) {
            object[] customAttributes = genType.GetCustomAttributes (typeof (System.ObsoleteAttribute), true);
            if (customAttributes.GetLength (0) == 1) {
                VerifyScenario ("FindObsoleteAttribute");
            }

            customAttributes = genType.GetCustomAttributes (typeof (System.Windows.Forms.AxHost.ClsidAttribute), true);
            if (customAttributes.GetLength (0) == 1) {
                VerifyScenario ("FindOtherAttribute");
            }
        }
    }
}

