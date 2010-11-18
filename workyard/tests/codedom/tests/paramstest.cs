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

public class ParamsTest : CodeDomTestTree {

    public override TestTypes TestType {
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
            return "ParamsTest";
        }
    }

    public override string Description {
        get {
            return "Tests variable method parameters.";
        }
    }

    public override void BuildTree(CodeDomProvider provider, CodeCompileUnit cu) {
        //cu.UserData["AllowLateBound"] = true;

        // GENERATES (C#):
        //  namespace Namespace1 {
        //      using System;
        //      
        //      
        //      public class Class1 {
        //          
        //          public virtual string Foo1(string format, [System.Runtime.InteropServices.OptionalAttribute()] params object[] array) {
        //              string str;
        //              str = format.Replace("{0}", array[0].ToString());
        //              str = str.Replace("{1}", array[1].ToString());
        //              str = str.Replace("{2}", array[2].ToString());
        //              return str;
        //          }
        //      }
        //  }

        CodeNamespace ns = new CodeNamespace("Namespace1");
        ns.Imports.Add(new CodeNamespaceImport("System"));
        cu.Namespaces.Add(ns);

        // Full Verification Objects
        CodeTypeDeclaration class1 = new CodeTypeDeclaration();
        class1.Name = "Class1";

        ns.Types.Add(class1);

        AddScenario ("CheckFoo1");
        CodeMemberMethod fooMethod1 = new CodeMemberMethod();
        fooMethod1.Name = "Foo1";
        fooMethod1.Attributes = MemberAttributes.Public ; 
        fooMethod1.ReturnType = new CodeTypeReference(typeof(string));

        CodeParameterDeclarationExpression parameter1 = new CodeParameterDeclarationExpression();
        parameter1.Name = "format";
        parameter1.Type = new CodeTypeReference(typeof(string));
        fooMethod1.Parameters.Add(parameter1);

        CodeParameterDeclarationExpression parameter2 = new CodeParameterDeclarationExpression();
        parameter2.Name = "array";
        parameter2.Type = new CodeTypeReference(typeof(object[]));

        if (Supports (provider, GeneratorSupport.ParameterAttributes)) {
            parameter2.CustomAttributes.Add( new CodeAttributeDeclaration("System.ParamArrayAttribute"));
            parameter2.CustomAttributes.Add( new CodeAttributeDeclaration("System.Runtime.InteropServices.OptionalAttribute"));
        }
        fooMethod1.Parameters.Add(parameter2);
        class1.Members.Add(fooMethod1);
        
        fooMethod1.Statements.Add( new CodeVariableDeclarationStatement(typeof(string), "str")); 
            
        fooMethod1.Statements.Add(CreateStatement(new CodeArgumentReferenceExpression ("format"), 0));
        fooMethod1.Statements.Add(CreateStatement(new CodeVariableReferenceExpression ("str"), 1));
        fooMethod1.Statements.Add(CreateStatement(new CodeVariableReferenceExpression ("str"), 2));
       
        fooMethod1.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("str")));
    }

    public CodeAssignStatement CreateStatement(CodeExpression objName, int iNum){
        CodeAssignStatement statement =
            new CodeAssignStatement (new CodeVariableReferenceExpression("str"),
                new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                objName, "Replace"), 
                new CodeExpression[]{
                    new CodePrimitiveExpression("{" + iNum + "}"),
                    new CodeMethodInvokeExpression(
                            new CodeArrayIndexerExpression(new CodeArgumentReferenceExpression("array"),
                                new CodePrimitiveExpression(iNum)),
                            "ToString")
                }));
        return statement;
    }

    public override void VerifyAssembly(CodeDomProvider provider, Assembly asm) {
        object genObject;
        Type   genType;

        AddScenario ("InstantiateClass1");
        if (!FindAndInstantiate ("Namespace1.Class1", asm, out genObject, out genType))
            return;
        VerifyScenario ("InstantiateClass1");

        if(VerifyMethod(genType, genObject, "Foo1", new object[]{"{0} + {1} = {2}", new object[]{1, 2, 3}} , "1 + 2 = 3")) {
            VerifyScenario ("CheckFoo1");
        }
    }
}   

