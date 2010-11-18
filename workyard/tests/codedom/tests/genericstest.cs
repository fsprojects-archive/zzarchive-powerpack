// F#: changed multiplication from decimal*int to int*int
//     unfortunately this fails because the following code doesn't compile 
//     (type arguments 'k and 'v are restricted to values from use in the 'Test' class)
//     otherwise it should be correct
/*
    #light
		open System.Collections.Generic

		type MyDictionary<'K, 'V> = 
			class
				inherit Dictionary<'K, 'V> as base
				new() = {}
			end
		and Test = 
			class
				member this.MyMethod() =
					let d = new MyDictionary<string, List<string>>()
					0
			end
*/
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.Samples.CodeDomTestSuite;

public class GenericsTest : CodeDomTestTree {

    public override string Comment
    {
        get
        {
            return "F# doesn't permit Dictionary<_,_>() to be called without an equality constraint";
        }
    }
    public override TestTypes TestType
    {
        get {
            return TestTypes.Whidbey;
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
            return "GenericsTest";
        }
    }

    public override string Description {
        get {
            return "Tests generating generics from CodeDom.";
        }
    }

    public override void BuildTree(CodeDomProvider provider, CodeCompileUnit cu) {
#if WHIDBEY
        // GENERATES (C#):
        //  namespace TestNamespace {
        //      using System;
        //      using System.Collections.Generic;
        //      
        //      
        //      public class MyDictionary<K, V> : Dictionary<K, V>
        //          where K : System.IComparable, IComparable<K>, new ()
        //          where V : IList<string> {
        //          
        //          public virtual int Calculate<S, T>(int value1, int value2)
        //              where S : new()
        //           {
        //              return (value1 * value2);
        //          }
        //      }
        //      
        //      public class Test {
        //          
        //          public virtual int MyMethod() {
        //              int dReturn;
        //              MyDictionary<int, List<string>> dict = new MyDictionary<int, List<string>>();
        //              dReturn = dict.Calculate<int, int>(2.5, 11);
        //              return dReturn;
        //          }
        //      }
        //  }
        
        if (!provider.Supports(GeneratorSupport.GenericTypeReference | GeneratorSupport.GenericTypeDeclaration)) {
            return;
        }

        CodeNamespace ns = new CodeNamespace("TestNamespace");
        ns.Imports.Add(new CodeNamespaceImport("System"));
        ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
        cu.Namespaces.Add (ns);

        // Declare a generic class
        CodeTypeDeclaration class1 = new CodeTypeDeclaration();
        class1.Name = "MyDictionary";
        class1.BaseTypes.Add( new CodeTypeReference("Dictionary", 
                                  new CodeTypeReference[] { new CodeTypeReference("K"), new CodeTypeReference("V"),}));

        CodeTypeParameter kType = new CodeTypeParameter("K");
        kType.HasConstructorConstraint= true;

        kType.Constraints.Add( new CodeTypeReference(typeof(IComparable)));
        kType.CustomAttributes.Add(new CodeAttributeDeclaration(
            "System.ComponentModel.DescriptionAttribute", new CodeAttributeArgument(new CodePrimitiveExpression("KeyType"))));

        CodeTypeReference iComparableT = new CodeTypeReference("IComparable");
        iComparableT.TypeArguments.Add(new CodeTypeReference(kType));            

        kType.Constraints.Add(iComparableT);
        
        CodeTypeParameter vType = new CodeTypeParameter("V");
        vType.Constraints.Add(new CodeTypeReference("IList[System.String]"));

        class1.TypeParameters.Add(kType);
        class1.TypeParameters.Add(vType);
                    
        ns.Types.Add(class1);

        // declare a generic method
        CodeMemberMethod method = new CodeMemberMethod();
        CodeTypeParameter sType = new CodeTypeParameter("S");
        sType.HasConstructorConstraint = true;

        CodeTypeParameter tType = new CodeTypeParameter("T");
        sType.HasConstructorConstraint = true;

        method.Name = "Calculate";
        method.TypeParameters.Add(sType);
        method.TypeParameters.Add(tType);
        method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "value1"));
        method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "value2"));
        method.ReturnType = new CodeTypeReference(typeof(int));

        method.Statements.Add(new CodeMethodReturnStatement(new CodeBinaryOperatorExpression(new 
                                 CodeVariableReferenceExpression("value1"), CodeBinaryOperatorType.Multiply, new 
                                 CodeVariableReferenceExpression("value2"))));


        method.Attributes = MemberAttributes.Public;
        class1.Members.Add(method);


        CodeTypeDeclaration class2 = new CodeTypeDeclaration();
        class2.Name = "Test";

        AddScenario ("CheckMyMethod");
        CodeMemberMethod method2 =  new CodeMemberMethod();
        method2.Name = "MyMethod";
        method2.Attributes = MemberAttributes.Public;
        method2.ReturnType = new CodeTypeReference(typeof(int));
        method2.Statements.Add( new CodeVariableDeclarationStatement(typeof(int), "dReturn"));

        CodeTypeReference myClass = new CodeTypeReference( "MyDictionary", 
                                    new CodeTypeReference[] { new CodeTypeReference(typeof(int)), new CodeTypeReference("List", 
                                    new CodeTypeReference[] {new CodeTypeReference("System.String") })} ); 

        method2.Statements.Add(  new CodeVariableDeclarationStatement( myClass, "dict", new CodeObjectCreateExpression(myClass) ));

        method2.Statements.Add(new CodeAssignStatement( new CodeVariableReferenceExpression("dReturn"), 
                               new CodeMethodInvokeExpression(
                               new CodeMethodReferenceExpression( new CodeVariableReferenceExpression("dict"), "Calculate",
                               new CodeTypeReference[] {
                               new CodeTypeReference("System.Int32"),
                               new CodeTypeReference("System.Int32"),}), 
                               new CodeExpression[]{new CodePrimitiveExpression(25), new CodePrimitiveExpression(11)})));

        method2.Statements.Add (new CodeMethodReturnStatement(new CodeVariableReferenceExpression("dReturn")));
        class2.Members.Add(method2);
        ns.Types.Add(class2);
#endif
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
#if WHIDBEY
        if (provider.Supports(GeneratorSupport.GenericTypeReference | GeneratorSupport.GenericTypeDeclaration)) {
            object genObject;
            Type   genType;

            AddScenario ("InstantiateTest");
            if (!FindAndInstantiate ("TestNamespace.Test", asm, out genObject, out genType))
                return;
            VerifyScenario ("InstantiateTest");

            // verify scenario with 'new' attribute
            if (VerifyMethod(genType, genObject, "MyMethod", null, 275)) {
                VerifyScenario ("CheckMyMethod");
            }
        }
#endif
    }
}   

