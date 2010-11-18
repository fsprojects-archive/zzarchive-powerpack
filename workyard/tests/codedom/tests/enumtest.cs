using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.Samples.CodeDomTestSuite;

public class EnumTest : CodeDomTestTree {

    public override TestTypes TestType {
        get {
            return TestTypes.Everett;
        }
    }

    public override string Name {
        get {
            return "EnumTest";
        }
    }

    public override string Description {
        get {
            return "Test enumerations";
        }
    }

    public override bool ShouldVerify {
        get {
            return true;
        }
    }

    public override bool ShouldCompile {
        get {
            return true;
        }
    }

    public override void BuildTree (CodeDomProvider provider, CodeCompileUnit cu) {

        CodeNamespace ns = new CodeNamespace ("Namespace1");

        cu.Namespaces.Add (ns);

        CodeTypeDeclaration cd = new CodeTypeDeclaration ("TEST");
        cd.IsClass = true;
        ns.Types.Add (cd);

        if (Supports (provider, GeneratorSupport.DeclareEnums)) {
            // GENERATE (C#):
            //    public enum DecimalEnum {
            //        Num0 = 0,
            //        Num1 = 1,
            //        Num2 = 2,
            //        Num3 = 3,
            //        Num4 = 4,
            //    }

            CodeTypeDeclaration ce = new CodeTypeDeclaration ("DecimalEnum");
            ce.IsEnum = true;
            ns.Types.Add (ce);

            // things to enumerate
            for (int k = 0; k < 5; k++) {
                CodeMemberField Field = new CodeMemberField ("System.Int32", "Num" + (k).ToString ());
                //Field.InitExpression = new CodePrimitiveExpression (k);
                ce.Members.Add (Field);
            }

            // GENERATE (C#):
            //    public enum BinaryEnum {
            //        Bin1 = 1,
            //        Bin2 = 2,
            //        Bin3 = 4,
            //        Bin4 = 8,
            //        Bin5 = 16,
            //    }

            ce = new CodeTypeDeclaration ("BinaryEnum");
            ce.IsEnum = true;
            ns.Types.Add (ce);

            // things to enumerate
            int i = 0x01;
            for (int k = 1; k < 6; k++) {
                CodeMemberField Field = new CodeMemberField (typeof (int), "Bin" + (k).ToString ());
                Field.InitExpression = new CodePrimitiveExpression (i);
                i = i * 2;
                ce.Members.Add (Field);
            }

#if WHIDBEY
            // GENERATE (C#):
            //    public enum MyEnum: System.UInt64 {
            //        small = 0,
            //        medium = Int64.MaxValue/10,
            //        large = Int64.MaxValue,
            //    }
            ce = new CodeTypeDeclaration ("MyEnum");
            ce.BaseTypes.Add (new CodeTypeReference (typeof (UInt64)));
            ce.IsEnum = true;
            ns.Types.Add (ce);

            // Add fields     
            ce.Members.Add (CreateFieldMember ("Small", 0));
            ce.Members.Add (CreateFieldMember ("Medium", Int64.MaxValue / 10));
            ce.Members.Add (CreateFieldMember ("Large", Int64.MaxValue));
#endif

            // GENERATE (C#):
            //        public int OutputDecimalEnumVal(int i) {
            //                if ((i == 3)) {
            //                        return ((int)(DecimalEnum.Num3));
            //                }
            //                if ((i == 4)) {
            //                        return ((int)(DecimalEnum.Num4));
            //                }
            //                if ((i == 2)) {
            //                        return ((int)(DecimalEnum.Num2));
            //                }
            //                if ((i == 1)) {
            //                        return ((int)(DecimalEnum.Num1));
            //                }
            //                if ((i == 0)) {
            //                        return ((int)(DecimalEnum.Num0));
            //                }
            //                    return (i + 10);
            //            }
    
            // generate 5 scenarios for OutputDecimalEnumVal
            for (int k = 0; k < 5; k++)
                AddScenario ("CheckOutputDecimalEnumVal" + k);

            CodeMemberMethod cmm = new CodeMemberMethod ();
            cmm.Name = "OutputDecimalEnumVal";
            cmm.Attributes = MemberAttributes.Public;
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


            // GENERATE (C#):
            //        public int OutputBinaryEnumVal(int i) {
            //            if ((i == 3)) {
            //                return ((int)(BinaryEnum.Bin3));
            //            }
            //            if ((i == 4)) {
            //                return ((int)(BinaryEnum.Bin4));
            //            }
            //            if ((i == 2)) {
            //                return ((int)(BinaryEnum.Bin2));
            //            }
            //            if ((i == 1)) {
            //                return ((int)(BinaryEnum.Bin1));
            //            }
            //            if ((i == 5)) {
            //                return ((int)(BinaryEnum.Bin5));
            //            }
            //            return (i + 10);
            //        }

            // generate 6 scenarios for OutputBinaryEnumVal
            for (int k = 1; k < 6; k++)
                AddScenario ("CheckOutputBinaryEnumVal" + k);
           AddScenario ("CheckOutputBinaryEnumValRet17", "Check for a return value of 17");

            cmm = new CodeMemberMethod ();
            cmm.Name = "OutputBinaryEnumVal";
            cmm.Attributes = MemberAttributes.Public;
            param = new CodeParameterDeclarationExpression (typeof (int), "i");
            cmm.Parameters.Add (param);
            eq = new CodeBinaryOperatorExpression (
                new CodeArgumentReferenceExpression ("i"), CodeBinaryOperatorType.ValueEquality,
                new CodePrimitiveExpression (3));
            truestmt = new CodeMethodReturnStatement (
                new CodeCastExpression (typeof (int),
                new CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("BinaryEnum"), "Bin3")));
            condstmt = new CodeConditionStatement (eq, truestmt);
            cmm.Statements.Add (condstmt);

            eq = new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"),
                CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression (4));
            truestmt = new CodeMethodReturnStatement (new CodeCastExpression (typeof (int), new
                CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("BinaryEnum"), "Bin4")));
            condstmt = new CodeConditionStatement (eq, truestmt);
            cmm.Statements.Add (condstmt);
            eq = new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"),
                CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression (2));
            truestmt = new CodeMethodReturnStatement (new CodeCastExpression (typeof (int), new
                CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("BinaryEnum"), "Bin2")));
            condstmt = new CodeConditionStatement (eq, truestmt);
            cmm.Statements.Add (condstmt);

            eq = new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"),
                CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression (1));
            truestmt = new CodeMethodReturnStatement (new CodeCastExpression (typeof (int), new
                CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("BinaryEnum"), "Bin1")));
            condstmt = new CodeConditionStatement (eq, truestmt);
            cmm.Statements.Add (condstmt);

            eq = new CodeBinaryOperatorExpression (new CodeArgumentReferenceExpression ("i"),
                CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression (5));
            truestmt = new CodeMethodReturnStatement (new CodeCastExpression (typeof (int), new
                CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("BinaryEnum"), "Bin5")));
            condstmt = new CodeConditionStatement (eq, truestmt);
            cmm.Statements.Add (condstmt);

            cmm.ReturnType = new CodeTypeReference ("System.Int32");

            cmm.Statements.Add (new CodeMethodReturnStatement (new CodeBinaryOperatorExpression (
                new CodeArgumentReferenceExpression ("i"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression (10))));
            cd.Members.Add (cmm);


#if WHIDBEY
            // GENERATE (C#):
            //        public long VerifyMyEnumExists(int num) {
            //            if ((num == Int32.MaxValue)) {
            //                return ((int)(MyEnum.Large));
            //            }
            //            return 0;
            //        }

            AddScenario ("CheckVerifyMyEnumExists");
            cmm = new CodeMemberMethod ();
            cmm.Name = "VerifyMyEnumExists";
            cmm.Attributes = MemberAttributes.Public;
            param = new CodeParameterDeclarationExpression (typeof (int), "num");
            cmm.Parameters.Add (param);
            cmm.ReturnType = new CodeTypeReference ("System.Int64");
            eq = new CodeBinaryOperatorExpression (
                new CodeArgumentReferenceExpression ("num"), CodeBinaryOperatorType.ValueEquality,
                new CodePrimitiveExpression (Int32.MaxValue));
            truestmt = new CodeMethodReturnStatement (new CodeCastExpression (typeof (long),
                        new CodeFieldReferenceExpression (new CodeTypeReferenceExpression ("MyEnum"),
                            "Large")));
            condstmt = new CodeConditionStatement (eq, truestmt);
            cmm.Statements.Add (condstmt);
            cmm.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (0)));
            cd.Members.Add (cmm);
#endif
        }
    }

    private CodeMemberField CreateFieldMember (String strFieldName, long strFieldValue){
        CodeMemberField field = new CodeMemberField (typeof (UInt64), strFieldName);
        field.InitExpression = new CodePrimitiveExpression (strFieldValue);
        return field;
    }

    public override void VerifyAssembly (CodeDomProvider provider, Assembly asm)  {
        if (Supports (provider, GeneratorSupport.DeclareEnums)) {
            object genObject;
            Type   genType;

            AddScenario ("InstantiateTEST", "Find and instantiate TEST.");
            if (!FindAndInstantiate ("Namespace1.TEST", asm, out genObject, out genType))
                return;
            VerifyScenario ("InstantiateTEST");

            // verify method return value, verify that ref and out worked accordingly

            // verifying first enumeration 
            for (int a = 0; a < 5; a++) {
                if (VerifyMethod (genType, genObject, "OutputDecimalEnumVal", new object[]{a}, a)) {
                    VerifyScenario ("CheckOutputDecimalEnumVal" + a);
                }
            }

            // verifying second enumeration
            int val = 1;
            for (int i = 1; i < 6; i++) {
                if (VerifyMethod (genType, genObject, "OutputBinaryEnumVal", new object[]{i}, val)) {
                    VerifyScenario ("CheckOutputBinaryEnumVal" + i);
                }
                val = val * 2;
            }

            if (VerifyMethod (genType, genObject, "OutputBinaryEnumVal", new object[]{7}, 17)) {
                VerifyScenario ("CheckOutputBinaryEnumValRet17");
            }

#if WHIDBEY
            if (VerifyMethod (genType, genObject, "VerifyMyEnumExists", new object[]{Int32.MaxValue}, Int64.MaxValue)) {
                VerifyScenario ("CheckVerifyMyEnumExists");
            }
#endif
        }
    }
}

