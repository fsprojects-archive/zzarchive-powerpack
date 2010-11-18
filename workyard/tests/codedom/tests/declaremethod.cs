#define FSHARP
// F#: Added CodeTypeReferenceExpression as a target where calling static methods
//     Added casting where assigning inherited class to variable of base type
//
/*
  Currently there is the following porblem: 
 
  #light
  type ISome = 
    interface
      abstract InterfaceMethod : int -> int
    end
  and Test = 
    class
      interface ISome with
        member this.InterfaceMethod  (a) =
          a + 5
      end
    end

  >>> error: The type 'ISome' is not an interface type.
*/

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class DeclareMethod : CodeDomTestTree {


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
            return "DeclareMethod";
        }
    }

    public override string Description {
        get {
            return "Tests declaration of methods.";
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

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        CodeNamespace nspace = new CodeNamespace ("NSPC");
        nspace.Imports.Add (new CodeNamespaceImport ("System"));
        cu.Namespaces.Add (nspace);

        CodeTypeDeclaration cd = new CodeTypeDeclaration ("TEST");
        cd.IsClass = true;
        nspace.Types.Add (cd);
        if (Supports (provider, GeneratorSupport.ReferenceParameters)) {
            //************** static internal method with parameters with out and ref directions,    **************
            //**************                                              and void return type    ************** 
            // GENERATES (C#):
            //        /*FamANDAssem*/ internal static void Work(ref int i, out int j) {
            //            i = (i + 4);
            //            j = 5;
            //        }              
            CodeMemberMethod cmm1 = new CodeMemberMethod ();
            cmm1.Name = "Work";
            cmm1.ReturnType = new CodeTypeReference ("System.void");
            cmm1.Attributes = MemberAttributes.Static | MemberAttributes.FamilyAndAssembly;
            // add parameter with ref direction
            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (typeof (int), "i");
            param.Direction = FieldDirection.Ref;
            cmm1.Parameters.Add (param);
            // add parameter with out direction
            param = new CodeParameterDeclarationExpression (typeof (int), "j");
            param.Direction = FieldDirection.Out;
            cmm1.Parameters.Add (param);
            cmm1.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("i"),
                new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"),
                CodeBinaryOperatorType.Add, new CodePrimitiveExpression (4))));
            cmm1.Statements.Add (new CodeAssignStatement (new CodeArgumentReferenceExpression ("j"),
                new CodePrimitiveExpression (5)));
            cd.Members.Add (cmm1);
        }
        // ********* pass by value using a protected method ******
        // GENERATES (C#):
        //        protected static int ProtectedMethod(int a) {
        //            return a;
        //        }
        CodeMemberMethod cmm = new CodeMemberMethod ();
        cmm.Name = "ProtectedMethod";
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Attributes = MemberAttributes.Family | MemberAttributes.Static;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cd.Members.Add (cmm);

        // declare a method to test the protected method with new attribute
        // GENERATES (C#):
        //        public static int CallProtected(int a) {
        //            return (a + ProtectedMethod(a));
        //        }
        AddScenario ("CheckCallProtected");
        cmm = new CodeMemberMethod ();
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Name = "CallProtected";
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Attributes = MemberAttributes.Public;
        CodeMethodReferenceExpression meth = new CodeMethodReferenceExpression ();
        meth.MethodName = "ProtectedMethod";
        meth.TargetObject = new CodeTypeReferenceExpression("TEST2");        
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("a"),
            CodeBinaryOperatorType.Add, new CodeMethodInvokeExpression (meth, new CodeArgumentReferenceExpression ("a")))));
        cd.Members.Add (cmm);



        // GENERATES (C#):
        //        public static void Main() {
        //          }
        if (Supports (provider, GeneratorSupport.EntryPointMethod)) {
            CodeEntryPointMethod cep = new CodeEntryPointMethod ();
            cd.Members.Add (cep);
        }

        // add a second class 
        cd = new CodeTypeDeclaration ("TEST2");
        cd.BaseTypes.Add (new CodeTypeReference ("TEST"));
        cd.IsClass = true;
        nspace.Types.Add (cd);

        if (Supports (provider, GeneratorSupport.ReferenceParameters)) {
            // GENERATES (C#):
            //        public static int CallingWork(int a) {
            //            a = 10;
            //            int b;
            //            TEST.Work(ref a, out b);
            //            return (a + b);
            //        }
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
            CodeMethodInvokeExpression methodinvoked = new CodeMethodInvokeExpression (new CodeTypeReferenceExpression ("TEST"), "Work");
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

        // ***** declare a private method with value return type ******
        // GENERATES (C#):
        //        private static int PrivateMethod() {
        //            return 5;
        //        }
        cmm = new CodeMemberMethod ();
        cmm.Name = "PrivateMethod";
        cmm.Attributes = MemberAttributes.Private | MemberAttributes.Static;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (5)));
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cd.Members.Add (cmm);

        // declare a method to test the private method
        // GENERATES (C#):
        //        public static int CallPrivateMethod(int a) {
        //            return (a + PrivateMethod());
        //        }
        AddScenario ("CheckCallPrivateMethod");
        cmm = new CodeMemberMethod ();
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Name = "CallPrivateMethod";
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Attributes = MemberAttributes.Public;
        meth = new CodeMethodReferenceExpression ();
        meth.TargetObject = new CodeTypeReferenceExpression("TEST2");
        meth.MethodName = "PrivateMethod";

        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("a"),
            CodeBinaryOperatorType.Add, new CodeMethodInvokeExpression (meth))));
        cd.Members.Add (cmm);

        // ********* pass by value using a protected static method ******
        // this class needs to inherit from the first class so that we can call the protected method from here and call the 
        // public method that calls the protected method from that class
        // declare a method to test the protected method
        // GENERATES (C#):
        //        public static int CallProtectedAndPublic(int a) {
        //            return (CallProtected(a) + ProtectedMethod(a));
        //        }
        AddScenario ("CheckCallProtectedAndPublic");
        cmm = new CodeMemberMethod ();
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Name = "CallProtectedAndPublic";
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Attributes = MemberAttributes.Public;
        meth = new CodeMethodReferenceExpression ();
        meth.MethodName = "ProtectedMethod";
        meth.TargetObject = new CodeTypeReferenceExpression("TEST2");
        CodeMethodReferenceExpression meth2 = new CodeMethodReferenceExpression ();
        meth2.MethodName = "CallProtected";
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (new CodeMethodInvokeExpression (meth2,
            new CodeArgumentReferenceExpression ("a")),
            CodeBinaryOperatorType.Add, new CodeMethodInvokeExpression (meth,
            new CodeArgumentReferenceExpression ("a")))));
        cd.Members.Add (cmm);

        if (Supports (provider, GeneratorSupport.DeclareInterfaces)) {

            // ******** implement a single public interface ***********                      
            // declare an interface
            // GENERATES (C#):      
            //     public interface TEST3 {    
            //         int InterfaceMethod(int a);
            //     }
            cd = new CodeTypeDeclaration ("TEST3");
            cd.IsInterface = true;
            nspace.Types.Add (cd);
            cmm = new CodeMemberMethod ();
            cmm.Name = "InterfaceMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            cd.Members.Add (cmm);

            // implement the interface
            // GENERATES (C#):
            //    public class TEST3b : object, TEST3 {
            //         public virtual int InterfaceMethod(int a) {
            //             return a;
            //         }
            //     }
            cd = new CodeTypeDeclaration ("TEST3b");
            cd.BaseTypes.Add (new CodeTypeReference ("System.Object"));
            cd.BaseTypes.Add (new CodeTypeReference ("TEST3"));
            cd.IsClass = true;
            nspace.Types.Add (cd);
            cmm = new CodeMemberMethod ();
            cmm.Name = "InterfaceMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.ImplementationTypes.Add (new CodeTypeReference ("TEST3"));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            cmm.Attributes = MemberAttributes.Public;
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
            cd.Members.Add (cmm);

            // ********implement two interfaces with overloading method name*******
            // declare the second interface      
            // GENERATES (C#):
            //    public interface TEST4 {
            //         int InterfaceMethod(int a);
            //     }
            cd = new CodeTypeDeclaration ("TEST4");
            cd.IsInterface = true;
            nspace.Types.Add (cd);
            cmm = new CodeMemberMethod ();
            cmm.Name = "InterfaceMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            cd.Members.Add (cmm);

            // implement both of the interfaces
            // GENERATES (C#):
            //    public class TEST4b : object, TEST3, TEST4 {
            //         public int InterfaceMethod(int a) {
            //             return a;
            //         }
            //     }
            cd = new CodeTypeDeclaration ("TEST4b");
            cd.BaseTypes.Add (new CodeTypeReference ("System.Object"));
            cd.BaseTypes.Add (new CodeTypeReference ("TEST3"));
            cd.BaseTypes.Add (new CodeTypeReference ("TEST4"));
            cd.IsClass = true;
            nspace.Types.Add (cd);
            cmm = new CodeMemberMethod ();
            cmm.Name = "InterfaceMethod";
            cmm.ImplementationTypes.Add (new CodeTypeReference ("TEST3"));
            cmm.ImplementationTypes.Add (new CodeTypeReference ("TEST4"));
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
            cd.Members.Add (cmm);
        }

        // create a class which will have a method to call the method name that was overloaded
        // this class will also call a  method in the class that implements the private implements testcase
        cd = new CodeTypeDeclaration ("TEST5");
        cd.IsClass = true;
        nspace.Types.Add (cd);

        if (Supports (provider, GeneratorSupport.DeclareInterfaces)) {
            // GENERATES (C#):
            //        public static int TestMultipleInterfaces(int i) {
            //             TEST4b t = new TEST4b();
            //             TEST3 TEST3 = ((TEST3)(t));
            //             TEST4 TEST4 = ((TEST4)(t));
            //             return (TEST3.InterfaceMethod(i) - TEST4.InterfaceMethod(i));
            //         }
            AddScenario ("CheckTestMultipleInterfaces");
            cmm = new CodeMemberMethod ();
            cmm.Name = "TestMultipleInterfaces";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
            cmm.Attributes = MemberAttributes.Public;
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("TEST4b", "t", new CodeObjectCreateExpression ("TEST4b")));
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("TEST3", "TEST3", new CodeCastExpression ("TEST3",
                new CodeVariableReferenceExpression ("t"))));
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("TEST4", "TEST4", new CodeCastExpression ("TEST4",
                new CodeVariableReferenceExpression ("t"))));
            CodeMethodInvokeExpression methodinvoking = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("TEST3")
                , "InterfaceMethod");
            methodinvoking.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
            CodeMethodInvokeExpression methodinvoking2 = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("TEST4")
                , "InterfaceMethod");
            methodinvoking2.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
                methodinvoking, CodeBinaryOperatorType.Subtract, methodinvoking2)));
            cd.Members.Add (cmm);

            // GENERATES (C#):
            //        public static int PrivateImplements(int i) {
            //             TEST6 t = new TEST6();
            //             TEST3 TEST3 = ((TEST3)(t));
            //             TEST4 TEST4 = ((TEST4)(t));
            //             return (TEST3.InterfaceMethod(i) - TEST4.InterfaceMethod(i));
            //         }
            AddScenario ("CheckPrivateImplements");
            cmm = new CodeMemberMethod ();
            cmm.Name = "PrivateImplements";
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Attributes = MemberAttributes.Public;
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("TEST6", "t", new CodeObjectCreateExpression ("TEST6")));
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("TEST3", "TEST3", new CodeCastExpression ("TEST3",
                new CodeVariableReferenceExpression ("t"))));
            cmm.Statements.Add (new CodeVariableDeclarationStatement ("TEST4", "TEST4", new CodeCastExpression ("TEST4",
                new CodeVariableReferenceExpression ("t"))));
            methodinvoking = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("TEST3")
                , "InterfaceMethod");
            methodinvoking.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
            methodinvoking2 = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("TEST4")
                , "InterfaceMethod");
            methodinvoking2.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
                methodinvoking, CodeBinaryOperatorType.Subtract, methodinvoking2)));
            cd.Members.Add (cmm);

            //******************* private implements ***************************
            // implement both of the interfaces
            // GENERATES (C#):
            //    public class TEST6 : object, TEST3, TEST4 {
            //         int TEST3.InterfaceMethod(int a) {
            //             return a;
            //         }      
            //         int TEST4.InterfaceMethod(int a) {
            //             return (5 * a);
            //         }
            //     }
            CodeTypeDeclaration ctd = new CodeTypeDeclaration ("TEST6");
            ctd.BaseTypes.Add (new CodeTypeReference ("System.Object"));
            ctd.BaseTypes.Add (new CodeTypeReference ("TEST3"));
            ctd.BaseTypes.Add (new CodeTypeReference ("TEST4"));
            ctd.IsClass = true;
            nspace.Types.Add (ctd);
            // make a seperate implementation for each base 
            // first for TEST3 base
            cmm = new CodeMemberMethod ();
            cmm.Name = "InterfaceMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
            cmm.PrivateImplementationType = new CodeTypeReference ("TEST3");

            ctd.Members.Add (cmm);
            // now implement for TEST4 base
            cmm = new CodeMemberMethod ();
            cmm = new CodeMemberMethod ();
            cmm.Name = "InterfaceMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            cmm.PrivateImplementationType = new CodeTypeReference ("TEST4");
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (new
                CodePrimitiveExpression (5), CodeBinaryOperatorType.Multiply, new
                CodeArgumentReferenceExpression ("a"))));
            ctd.Members.Add (cmm);
        }

        // method to test the 'new' scenario by calling the 'new' method
        // GENERATES (C#):
        //        public int CallingNewScenario(int i) {
        //            ClassWVirtualMethod t = new ClassWNewMethod();
        //            return (((ClassWNewMethod)(t)).VirtualMethod(i) - t.VirtualMethod(i));
        //        }
      
        CodeMethodInvokeExpression methodinvoke;

#if !FSHARP 
        AddScenario ("CheckCallingNewScenario");
        cmm = new CodeMemberMethod ();
        cmm.Name = "CallingNewScenario";
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeVariableDeclarationStatement ("ClassWVirtualMethod", "t", new CodeCastExpression(new CodeTypeReference("ClassWVirtualMethod"), new CodeObjectCreateExpression ("ClassWNewMethod"))));
        CodeMethodInvokeExpression methodinvoke = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("t"), "VirtualMethod");
        methodinvoke.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
        CodeMethodInvokeExpression methodinvoke2 = new CodeMethodInvokeExpression (new CodeCastExpression ("ClassWNewMethod", new
            CodeVariableReferenceExpression ("t")), "VirtualMethod");
        methodinvoke2.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
            methodinvoke2, CodeBinaryOperatorType.Subtract, methodinvoke)));
        cd.Members.Add (cmm);
#endif

        // similar to the 'new' test, write a method to complete testing of the 'override' scenario
        // GENERATES (C#):
        //        public static int CallingOverrideScenario(int i) {
        //            ClassWVirtualMethod t = new ClassWOverrideMethod();
        //            return t.VirtualMethod(i);
        //        }
        AddScenario ("CheckCallingOverrideScenario");
        cmm = new CodeMemberMethod ();
        cmm.Name = "CallingOverrideScenario";
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add(new CodeVariableDeclarationStatement("ClassWVirtualMethod", "t", new CodeCastExpression(new CodeTypeReference("ClassWVirtualMethod"), new CodeObjectCreateExpression("ClassWOverrideMethod"))));
        methodinvoke = new CodeMethodInvokeExpression (new CodeVariableReferenceExpression ("t"), "VirtualMethod");
        methodinvoke.Parameters.Add (new CodeArgumentReferenceExpression ("i"));
        cmm.Statements.Add (new CodeMethodReturnStatement (methodinvoke));
        cd.Members.Add (cmm);



        //*************** overload member function ****************             
        // new class which will include both functions
        // GENERATES (C#):
        //    public class TEST7 {
        //         public static int OverloadedMethod(int a) {
        //             return a;
        //         }
        //         public static int OverloadedMethod(int a, int b) {
        //             return (b + a);
        //         }
        //         public static int CallingOverloadedMethods(int i) {
        //             return (OverloadedMethod(i, i) - OverloadedMethod(i));
        //         }
        //     }
        cd = new CodeTypeDeclaration ("TEST7");
        cd.IsClass = true;
        nspace.Types.Add (cd);
        cmm = new CodeMemberMethod ();
        cmm.Name = "OverloadedMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeArgumentReferenceExpression ("a")));
        cd.Members.Add (cmm);
        cmm = new CodeMemberMethod ();
        cmm.Name = "OverloadedMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "b"));
        cmm.Attributes = MemberAttributes.Public;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
            new CodeArgumentReferenceExpression ("b"), CodeBinaryOperatorType.Add,
            new CodeArgumentReferenceExpression ("a"))));
        cd.Members.Add (cmm);

        // declare a method that will call both OverloadedMethod functions
        AddScenario ("CheckCallingOverloadedMethods");
        cmm = new CodeMemberMethod ();
        cmm.Name = "CallingOverloadedMethods";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "i"));
        cmm.Attributes = MemberAttributes.Public;
        CodeMethodReferenceExpression methodref = new CodeMethodReferenceExpression ();
        methodref.MethodName = "OverloadedMethod";
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
            new CodeMethodInvokeExpression (methodref, new
            CodeArgumentReferenceExpression ("i"), new CodeArgumentReferenceExpression ("i"))
            , CodeBinaryOperatorType.Subtract, new CodeMethodInvokeExpression (methodref, new
            CodeArgumentReferenceExpression ("i")))));
        cd.Members.Add (cmm);


        // ***************** declare method using new ******************
        // first declare a class with a virtual method in it 
        // GENERATES (C#):
        //         public class ClassWVirtualMethod {
        //                 public virtual int VirtualMethod(int a) {
        //                     return a;
        //                 }
        //         }    
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

#if !FSHARP 
        // now declare a class that inherits from the previous class and has a 'new' method with the
        // name VirtualMethod
        // GENERATES (C#):
        //    public class ClassWNewMethod : ClassWVirtualMethod {
        //         public new virtual int VirtualMethod(int a) {
        //             return (2 * a);
        //         }
        //     }
        cd = new CodeTypeDeclaration ("ClassWNewMethod");
        cd.BaseTypes.Add (new CodeTypeReference ("ClassWVirtualMethod"));
        cd.IsClass = true;
        nspace.Types.Add (cd);
        cmm = new CodeMemberMethod ();
        cmm.Name = "VirtualMethod";
        cmm.ReturnType = new CodeTypeReference (typeof (int));
        cmm.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "a"));
        cmm.Attributes = MemberAttributes.Public | MemberAttributes.New;
        cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
            new CodePrimitiveExpression (2), CodeBinaryOperatorType.Multiply
            , new CodeArgumentReferenceExpression ("a"))));
        cd.Members.Add (cmm);
#endif

        // *************** declare a method using override ******************
        // now declare a class that inherits from the previous class and has a 'new' method with the
        // name VirtualMethod
        // GENERATES (C#):
        //            public class ClassWOverrideMethod : ClassWVirtualMethod {
        //                 public override int VirtualMethod(int a) {
        //                     return (2 * a);
        //                 }
        //             }
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
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateTEST2", "Find and instantiate TEST2.");
        if (!FindAndInstantiate ("NSPC.TEST2", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTEST2");

        if (Supports (provider, GeneratorSupport.ReferenceParameters) &&
                VerifyMethod (genType, genObject, "CallingWork", new object [] {1516}, 19)) {
            VerifyScenario ("CheckCallingWork");
        }

        if (VerifyMethod (genType, genObject, "CallPrivateMethod", new object [] {1516}, 1521)) {
            VerifyScenario ("CheckCallPrivateMethod");
        }

        if (VerifyMethod (genType, genObject, "CallProtectedAndPublic", new object [] {1516}, 4548)) {
            VerifyScenario ("CheckCallProtectedAndPublic");
        }
        
        AddScenario ("InstantiateTEST", "Find and instantiate TEST.");
        if (!FindAndInstantiate ("NSPC.TEST", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTEST");

        if (VerifyMethod (genType, genObject, "CallProtected", new object[] {2}, 4)) {
            VerifyScenario ("CheckCallProtected");
        }

        AddScenario ("InstantiateTEST5", "Find and instantiate TEST5.");
        if (!FindAndInstantiate ("NSPC.TEST5", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTEST5");

        if (VerifyMethod (genType, genObject, "CallingNewScenario", new object[] {2}, 2)) {
            VerifyScenario ("CheckCallingNewScenario");
        }

        if (VerifyMethod (genType, genObject, "CallingOverrideScenario", new object[] {2}, 4)) {
            VerifyScenario ("CheckCallingOverrideScenario");
        }

        if (Supports (provider, GeneratorSupport.DeclareInterfaces)) {
            if (VerifyMethod (genType, genObject, "TestMultipleInterfaces", new object[] {2}, 0)) {
                VerifyScenario ("CheckTestMultipleInterfaces");
            }
            if (VerifyMethod (genType, genObject, "PrivateImplements", new object[] {2}, -8)) {
                VerifyScenario ("CheckPrivateImplements");
            }
        }

        AddScenario ("InstantiateTEST7", "Find and instantiate TEST7.");
        if (!FindAndInstantiate ("NSPC.TEST7", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateTEST7");

        if (VerifyMethod (genType, genObject, "CallingOverloadedMethods", new object[] {2}, 2)) {
            VerifyScenario ("CheckCallingOverloadedMethods");
        }
    }
}

