// F#: Our delegates work with <= 5 arguments. Changed test to use only 5 arguments (originally it used 20!)

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.VisualBasic;

public class DelegateTest : CodeDomTestTree {

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
            return true;
        }
    }


    public override string Name
    {
        get
        {
            return "DelegateTest";
        }
    }

    public override string Description
    {
        get
        {
            return "Tests delegates.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu)
    {

        // GENERATES (C#):
        //
        //  namespace NSPC {
        //    public class DelegateClass {
        //        
        //        public virtual int Sum(
        //                    int val1, 
        //                    int val2, 
        //                    int val3, 
        //                    int val4, 
        //                    int val5, 
        //                    int val6, 
        //                    int val7, 
        //                    int val8, 
        //                    int val9, 
        //                    int val10, 
        //                    int val11, 
        //                    int val12, 
        //                    int val13, 
        //                    int val14, 
        //                    int val15, 
        //                    int val16, 
        //                    int val17, 
        //                    int val18, 
        //                    int val19, 
        //                    int val20) {
        //            int mySum = 0;
        //            mySum = (mySum + val1);
        //            mySum = (mySum + val2);
        //            mySum = (mySum + val3);
        //            mySum = (mySum + val4);
        //            mySum = (mySum + val5);
        //            mySum = (mySum + val6);
        //            mySum = (mySum + val7);
        //            mySum = (mySum + val8);
        //            mySum = (mySum + val9);
        //            mySum = (mySum + val10);
        //            mySum = (mySum + val11);
        //            mySum = (mySum + val12);
        //            mySum = (mySum + val13);
        //            mySum = (mySum + val14);
        //            mySum = (mySum + val15);
        //            mySum = (mySum + val16);
        //            mySum = (mySum + val17);
        //            mySum = (mySum + val18);
        //            mySum = (mySum + val19);
        //            mySum = (mySum + val20);
        //            return mySum;
        //        }
        //        
        //        public virtual int Do() {
        //            MyDelegate myDel = new DelegateClass.MyDelegate(this.Sum);
        //            return myDel(1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181, 6765);
        //        }
        //    }
        //        
        //    public delegate int MyDelegate(
        //                    int val1, 
        //                    int val2, 
        //                    int val3, 
        //                    int val4, 
        //                    int val5, 
        //                    int val6, 
        //                    int val7, 
        //                    int val8, 
        //                    int val9, 
        //                    int val10, 
        //                    int val11, 
        //                    int val12, 
        //                    int val13, 
        //                    int val14, 
        //                    int val15, 
        //                    int val16, 
        //                    int val17, 
        //                    int val18, 
        //                    int val19, 
        //                    int val20);
        //    }

        CodeNamespace nspace = new CodeNamespace ("NSPC");
        cu.Namespaces.Add (nspace);

        // only produce code if the generator can declare delegates
        if (Supports (provider, GeneratorSupport.DeclareDelegates)) {
            CodeTypeDeclaration class1 = new CodeTypeDeclaration ("DelegateClass");
            class1.IsClass = true;
            nspace.Types.Add (class1);

            CodeTypeDelegate td = new CodeTypeDelegate ("MyDelegate");
            td.ReturnType = new CodeTypeReference (typeof (Int32));
            for (int i = 1; i <= 5; i++)
                td.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (Int32)), "val" + i));
            nspace.Types.Add (td);

            CodeMemberMethod cmm = new CodeMemberMethod ();
            cmm.Name = "Sum";
            cmm.ReturnType = new CodeTypeReference (typeof (Int32));
            for (int i = 1; i <= 5; i++)
                cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (Int32)), "val" + i));
            cmm.Attributes = MemberAttributes.Public;

            cmm.Statements.Add (new CodeVariableDeclarationStatement (typeof (int), "mySum", new CodePrimitiveExpression (0)));

            for (int i = 1; i <= 5; i++)
                cmm.Statements.Add (CDHelper.CreateIncrementByStatement ("mySum", new CodeArgumentReferenceExpression ("val" + i)));

            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeVariableReferenceExpression ("mySum")));

            class1.Members.Add (cmm);

#if !WHIDBEY
            if (!(provider is VBCodeProvider)) {
#endif
                AddScenario ("CheckDo", "Check Do()'s return value.");
                cmm = new CodeMemberMethod ();
                cmm.Name = "Do";
                cmm.ReturnType = new CodeTypeReference (typeof (Int32));
                cmm.Attributes = MemberAttributes.Public;

                cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference ("MyDelegate"), "myDel",
                    new CodeDelegateCreateExpression (new CodeTypeReference ("NSPC.MyDelegate"),
                    new CodeThisReferenceExpression (), "Sum")));

                CodeDelegateInvokeExpression delegateInvoke = new CodeDelegateInvokeExpression ();
                delegateInvoke.TargetObject = new CodeVariableReferenceExpression ("myDel");
                for (int i = 1; i <= 5; i++)
                    delegateInvoke.Parameters.Add (new CodePrimitiveExpression (fib (i)));
                cmm.Statements.Add (new CodeMethodReturnStatement (delegateInvoke));

                class1.Members.Add (cmm);
#if !WHIDBEY
            }
#endif
        }
    }

    // return the n-th Fibonacci number where n >= 1
    int fib (int n) {
        if (n <= 2)
            return 1;
        else
            return fib (n - 1) + fib (n - 2);
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
#if !WHIDBEY
        if (!(provider is VBCodeProvider)) {
#endif
            if (Supports (provider, GeneratorSupport.DeclareDelegates)) {
                int    fibSum = 0;
                object genObject;
                Type   genType;

                // calculate the expected sum
                for (int i = 1; i <= 5; i++)
                    fibSum += fib (i);

                AddScenario ("instantiateDelegateClass", "Find and instantiate DelegateClass.");
                if (!FindAndInstantiate ("NSPC.DelegateClass", asm, out genObject, out genType))
                    return;
                VerifyScenario ("instantiateDelegateClass");

                if (VerifyMethod (genType, genObject, "Do", new object[] {}, fibSum))
                    VerifyScenario ("CheckDo");
            }
#if !WHIDBEY
        }
#endif
    }
}

