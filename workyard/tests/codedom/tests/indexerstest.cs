using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.JScript;

public class IndexersTest : CodeDomTestTree {
    public override string Comment
    {
        get { return "Overriding of abstract indexers not supported by F#"; }
    }		

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
            return "IndexersTest";
        }
    }

    public override string Description {
        get {
            return "Tests indexers.";
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

#if WHIDBEY
        if (Supports (provider, GeneratorSupport.DeclareIndexerProperties)) {
#else
        // JScript doesn't support indexers
        if (!(provider is JScriptCodeProvider)) {
#endif

            // GENERATES (C#):
            // namespace NSPC {
            //      public class TEST {
            //          
            //          public int[] PublicField = new int[] {
            //                  0,
            //                  0,
            //                  0,
            //                  0,
            //                  0,
            //                  0,
            //                  0};
            //          
            //          public int this[int i] {
            //              get {
            //                  return this.PublicField[i];
            //              }
            //              set {
            //                  this.PublicField[i] = value;
            //              }
            //          }
            //          
            //          public int this[int a, int b] {
            //              get {
            //                  return this.PublicField[(a + b)];
            //              }
            //              set {
            //                  this.PublicField[(a + b)] = value;
            //              }
            //          }
            //      }
            //      
            //      public class UseTEST {
            //          
            //          public int TestMethod(int i) {
            //              TEST temp = new TEST();
            //              temp[1] = i;
            //              temp[2, 4] = 83;
            //              return (temp[1] + temp[2, 4]);
            //          }
            //      }
            //  }

            CodeNamespace nspace = new CodeNamespace ("NSPC");
            cu.Namespaces.Add (nspace);

            CodeTypeDeclaration cd = new CodeTypeDeclaration ("TEST");
            cd.IsClass = true;
            nspace.Types.Add (cd);

            CodeMemberField field = new CodeMemberField ();
            field.Name = "PublicField";
            field.InitExpression = new CodeArrayCreateExpression (new CodeTypeReference (typeof (int[])), new CodeExpression[]{
                    new CodePrimitiveExpression (0), new CodePrimitiveExpression (0), new CodePrimitiveExpression (0), new CodePrimitiveExpression (0),
                    new CodePrimitiveExpression (0), new CodePrimitiveExpression (0), new CodePrimitiveExpression (0)});
            field.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            field.Type = new CodeTypeReference (typeof (int[]));
            cd.Members.Add (field);

            // nonarray indexers
            CodeMemberProperty indexerProperty = new CodeMemberProperty ();
            indexerProperty.Name = "Item";
            indexerProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            indexerProperty.Type = new CodeTypeReference (typeof (int));
            indexerProperty.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));

            // uses array indexer
            indexerProperty.SetStatements.Add (new CodeAssignStatement (new CodeArrayIndexerExpression (
                new CodeFieldReferenceExpression (new CodeThisReferenceExpression ()
                , "PublicField"), new CodeExpression[] {new CodeArgumentReferenceExpression ("i")}),
                new CodePropertySetValueReferenceExpression ()));
            indexerProperty.GetStatements.Add (new CodeMethodReturnStatement (new CodeArrayIndexerExpression (
                new CodeFieldReferenceExpression (new CodeThisReferenceExpression (), "PublicField"),
                new CodeArgumentReferenceExpression ("i"))));
            cd.Members.Add (indexerProperty);

            // nonarray indexers
            indexerProperty = new CodeMemberProperty ();
            indexerProperty.Name = "Item";
            indexerProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            indexerProperty.Type = new CodeTypeReference (typeof (int));
            indexerProperty.SetStatements.Add (new CodeAssignStatement (new CodeArrayIndexerExpression (
                new CodeFieldReferenceExpression (new CodeThisReferenceExpression ()
                , "PublicField"), new CodeExpression[] {new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("a"), CodeBinaryOperatorType.Add,
                    new CodeArgumentReferenceExpression ("b"))}),
                new CodePropertySetValueReferenceExpression ()));
            indexerProperty.GetStatements.Add (new CodeMethodReturnStatement (new CodeArrayIndexerExpression (
                new CodeFieldReferenceExpression (new CodeThisReferenceExpression (), "PublicField"),
                new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("a"), CodeBinaryOperatorType.Add, new CodeArgumentReferenceExpression ("b")))));
            indexerProperty.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "a"));
            indexerProperty.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "b"));
            cd.Members.Add (indexerProperty);

            // uses array indexer
            cd = new CodeTypeDeclaration ("UseTEST");
            cd.IsClass = true;
            nspace.Types.Add (cd);

            AddScenario ("CheckTestMethod");
            CodeMemberMethod cmm = new CodeMemberMethod ();
            cmm.Name = "TestMethod";
            cmm.ReturnType = new CodeTypeReference (typeof (int));
            cmm.Parameters.Add (new CodeParameterDeclarationExpression (new CodeTypeReference (typeof (int)), "i"));
            cmm.Attributes = MemberAttributes.Final | MemberAttributes.Public;
            cmm.Statements.Add (new CodeVariableDeclarationStatement (new CodeTypeReference ("TEST"),
                "temp", new CodeObjectCreateExpression ("TEST")));
            cmm.Statements.Add (new CodeAssignStatement (new CodeIndexerExpression (
                new CodeVariableReferenceExpression ("temp"), new CodeExpression[]{new CodePrimitiveExpression (1)}),
                new CodeArgumentReferenceExpression ("i")));
            cmm.Statements.Add (new CodeAssignStatement (new CodeIndexerExpression (
                new CodeVariableReferenceExpression ("temp"), new CodeExpression[]{new CodePrimitiveExpression (2),
                    new CodePrimitiveExpression (4)}),
                new CodePrimitiveExpression (83)));
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
                new CodeIndexerExpression (new CodeVariableReferenceExpression ("temp"), new CodeExpression[]{new CodePrimitiveExpression (1)}),
                CodeBinaryOperatorType.Add,
                new CodeIndexerExpression (new CodeVariableReferenceExpression ("temp"), new CodeExpression[]{new CodePrimitiveExpression (2), new CodePrimitiveExpression (4)}))));

            cd.Members.Add (cmm);
        }
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
#if WHIDBEY
        if (Supports (provider, GeneratorSupport.DeclareIndexerProperties)) {
#else
        // JScript doesn't support indexers
        if (!(provider is JScriptCodeProvider)) {
#endif
            object genObject;
            Type   genType;

            AddScenario ("InstantiateUseTEST", "Find and instantiate UseTEST.");
            if (!FindAndInstantiate ("NSPC.UseTEST", asm, out genObject, out genType))
                return;
            VerifyScenario ("InstantiateUseTEST");

            if (VerifyMethod (genType, genObject, "TestMethod", new object[] {5}, 88))
                VerifyScenario ("CheckTestMethod");
        }
    }
}

