using System;
using System.IO;
using System.CodeDom;
using System.Reflection;
using System.Collections;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using Microsoft.Samples.CodeDomTestSuite;

using Microsoft.VisualBasic;
using Microsoft.JScript;

public class CheckSumTest : CodeDomTestTree {

		public override string Comment
		{
			get { return "no #pragma checksum in F# afaik"; }
		}		


    public override TestTypes TestType {
        get {
            return TestTypes.Whidbey;
        }
    }

    public override bool ShouldCompile {
        get {
            return false;
        }
    }

    public override bool ShouldVerify {
        get {
            return false;
        }
    }

    public override bool ShouldSearch {
        get {
            return true;
        }
    }

    public override string Name {
        get {
            return "CheckSumTest";
        }
    }

    public override string Description {
        get {
            return "Checksum testing.";
        }
    }

    private Guid HashMD5 = new Guid(0x406ea660, 0x64cf, 0x4c82, 0xb6, 0xf0, 0x42, 0xd4, 0x81, 0x72, 0xa7, 0x99);
    private Guid HashSHA1 = new Guid(0xff1816ec, 0xaa5e, 0x4d10, 0x87, 0xf7, 0x6f, 0x49, 0x63, 0x83, 0x34, 0x60);        

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {
#if WHIDBEY
        // VB code provider doesn't currently generate checksum statements.  This
        // will be completed before the next release of the CLR.
        // JScript does not currently support checksum pragmas.
        if (!(provider is VBCodeProvider) && !(provider is JScriptCodeProvider)) {
            // GENERATES (C#):
            // #pragma checksum "c:\foo\bar\OuterLinePragma.txt" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "DEAD"
            // #pragma checksum "bogus.txt" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "F00BAA"
            // #pragma checksum "" "{00000000-0000-0000-0000-000000000000}" ""
            //
            //  // Namespace Comment
            //  namespace Namespace1 {
            //      
            //      
            //      // Outer Type Comment
            //      
            //      #line 300 "c:\foo\bar\OuterLinePragma.txt"
            //      public class Class1 {
            //          
            //          public void Method1() {
            //          }
            //          
            //          // Method 2 Comment
            //          public void Method2() {
            //          }
            //      }
            //      
            //      #line default
            //      #line hidden
            //  }

            AddScenario ("CheckPragmasInSrcCode", "Checks to see if the pragmas are in the generated source code.");
            CodeChecksumPragma pragma1 = new CodeChecksumPragma();
            pragma1.FileName = "c:\\foo\\bar\\OuterLinePragma.txt";
            pragma1.ChecksumAlgorithmId = HashMD5;
            pragma1.ChecksumData = new byte[] {0xDE, 0xAD};            
            cu.StartDirectives.Add(pragma1);
            CodeChecksumPragma pragma2 = new CodeChecksumPragma("bogus.txt", HashSHA1, new byte[]{0xF0, 0x0B, 0xAA});
            cu.StartDirectives.Add(pragma2);
            CodeChecksumPragma pragma3 = new CodeChecksumPragma();
            cu.StartDirectives.Add(pragma3);

            CodeNamespace ns = new CodeNamespace("Namespace1");
            ns.Comments.Add(new CodeCommentStatement("Namespace Comment"));

            cu.Namespaces.Add(ns);

            CodeTypeDeclaration cd = new CodeTypeDeclaration ("Class1");
            ns.Types.Add(cd);

            cd.Comments.Add(new CodeCommentStatement("Outer Type Comment"));
            cd.LinePragma = new CodeLinePragma("c:\\foo\\bar\\OuterLinePragma.txt", 300);

            CodeMemberMethod method1 = new CodeMemberMethod();
            method1.Name = "Method1";
            method1.Attributes = (method1.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;            


            CodeMemberMethod method2 = new CodeMemberMethod();
            method2.Name = "Method2";
            method2.Attributes = (method2.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            method2.Comments.Add(new CodeCommentStatement("Method 2 Comment"));                                            

            cd.Members.Add(method1);
            cd.Members.Add(method2);
        }
#endif
    }

    public override void Search (CodeDomProvider provider, string strGeneratedCode)  {
#if WHIDBEY
        // see note above
        if (!(provider is VBCodeProvider) && !(provider is JScriptCodeProvider)) {
            string[] strPragmas = new string[] { @"""{406ea660-64cf-4c82-b6f0-42d48172a799}"" ""DEAD""", 
                                                @"""{ff1816ec-aa5e-4d10-87f7-6f4963833460}"" ""F00BAA""",
                                                @"""{00000000-0000-0000-0000-000000000000}"" """""};
            int startIndex = 0; 
            bool valid = true;

            for (int i = 0; i < strPragmas.Length; i++) {
                startIndex = strGeneratedCode.IndexOf (strPragmas[i], startIndex);
                if (startIndex ==  -1) {
                    valid = false;
                    break;
                } else
                    startIndex += strPragmas[i].Length ;
            }

            if (valid)
                VerifyScenario ("CheckPragmasInSrcCode");
        }
#endif
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm) {
        // no verification, just searching
    }
}

