using System;
using System.CodeDom;
using System.Reflection;
using System.Globalization;

namespace Microsoft.Samples.CodeDomTestSuite {

    [CLSCompliant(true)]
#if WHIDBEY
    public static class CDHelper {
#else
    public class CDHelper {
#endif

        /////////////////////////////////////////////////////////////
        // CodeTypeDeclaration helpers

        // classes
        public static CodeTypeDeclaration CreateClass (string name) {
            CodeTypeDeclaration ct = new CodeTypeDeclaration (name);
            ct.IsClass = true;

            return ct;
        }

        public static CodeTypeDeclaration CreateClass (string name, MemberAttributes attribute) {
            CodeTypeDeclaration ct = new CodeTypeDeclaration (name);
            ct.Attributes = attribute;
            ct.IsClass = true;

            return ct;
        }

        public static CodeTypeDeclaration CreateClass (string name, MemberAttributes attribute,
                TypeAttributes typeAttr) {
            CodeTypeDeclaration ct = new CodeTypeDeclaration (name);
            ct.Attributes = attribute;
            ct.TypeAttributes = typeAttr;
            ct.IsClass = true;

            return ct;
        }

        public static CodeTypeDeclaration CreateClass (string name, MemberAttributes attribute,
                TypeAttributes typeAttr, CodeTypeReference[] baseTypes) {
            CodeTypeDeclaration ct = new CodeTypeDeclaration (name);
            ct.Attributes = attribute;
            ct.TypeAttributes = typeAttr;
            ct.BaseTypes.AddRange (baseTypes);
            ct.IsClass = true;

            return ct;
        }

        public static CodeTypeDeclaration CreateClass (string name, MemberAttributes attribute,
                CodeTypeReference[] baseTypes) {
            CodeTypeDeclaration ct = new CodeTypeDeclaration (name);
            ct.Attributes = attribute;
            ct.BaseTypes.AddRange (baseTypes);
            ct.IsClass = true;

            return ct;
        }

        // interfaces
        public static CodeTypeDeclaration CreateInterface (string name) {
            CodeTypeDeclaration ct = new CodeTypeDeclaration (name);
            ct.IsInterface = true;

            return ct;
        }

        public static CodeTypeDeclaration CreateInterface (string name, MemberAttributes attribute) {
            CodeTypeDeclaration ct = new CodeTypeDeclaration (name);
            ct.Attributes = attribute;
            ct.IsInterface = true;

            return ct;
        }

        public static CodeTypeDeclaration CreateInterface (string name, MemberAttributes attribute,
            TypeAttributes typeAttr)
        {
            CodeTypeDeclaration ct = new CodeTypeDeclaration (name);
            ct.Attributes = attribute;
            ct.TypeAttributes = typeAttr;
            ct.IsInterface = true;

            return ct;
        }

        public static CodeTypeDeclaration CreateInterface (string name, MemberAttributes attribute,
            TypeAttributes typeAttr, CodeTypeReference[] baseTypes)
        {
            CodeTypeDeclaration ct = new CodeTypeDeclaration (name);
            ct.Attributes = attribute;
            ct.TypeAttributes = typeAttr;
            ct.BaseTypes.AddRange (baseTypes);
            ct.IsInterface = true;

            return ct;
        }

        // end CodeTypeDeclaration helpers
        /////////////////////////////////////////////////////////////

        // CodeConstructor helpers
        public static CodeConstructor CreateConstructor () {
            return new CodeConstructor ();
        }

        public static CodeConstructor CreateConstructor (MemberAttributes attribute) {
            CodeConstructor cc = CreateConstructor ();
            cc.Attributes = attribute;

            return cc;
        }

        // CodeMemberMethod helpers
        public static CodeMemberMethod CreateMethod (string name) {
            CodeMemberMethod cm = new CodeMemberMethod ();
            cm.Name = name;

            return cm;
        }

        public static CodeMemberMethod CreateMethod (string name, CodeTypeReference returnType)
        {
            CodeMemberMethod cm = CreateMethod (name);
            cm.ReturnType = returnType;

            return cm;
        }

        public static CodeMemberMethod CreateMethod (string name, CodeTypeReference returnType,
            MemberAttributes attribute)
        {
            CodeMemberMethod cm = CreateMethod (name, returnType);
            cm.Attributes = attribute;

            return cm;
        }

        public static CodeMemberMethod CreateMethod (string name, CodeTypeReference returnType,
            MemberAttributes attribute, CodeParameterDeclarationExpressionCollection parameters)
        {
            CodeMemberMethod cm = CreateMethod (name, returnType, attribute);
            if (parameters != null)
                foreach (CodeParameterDeclarationExpression p in parameters)
                    cm.Parameters.Add (p);

            return cm;
        }

        // CodeNamespace helpers
        public static CodeNamespace CreateNamespace (string name) {
            return new CodeNamespace (name);
        }

        // CodeParameterDeclarationExpressionCollection helpers
        // -- this one is possibly dangerous?
        public static CodeParameterDeclarationExpressionCollection CreateParameterList (params object[] parameters) {

            // must have an even number of arguments for
            // a list of pairs to be valid
            if (parameters.Length % 2 != 0)
                throw new ArgumentException ("Must supply an even number of arguments.");

            CodeParameterDeclarationExpressionCollection exps =
                new CodeParameterDeclarationExpressionCollection ();

            for (Int32 i = 0; i < parameters.Length; i += 2) {
                if (!typeof (string).IsInstanceOfType (parameters[i + 1]))
                    throw new ArgumentException (String.Format (CultureInfo.InvariantCulture,
                        "Argument {0} should be of type string.", i + 1));

                if (typeof (Type).IsInstanceOfType (parameters[i])) {
                    exps.Add (new CodeParameterDeclarationExpression (
                        (Type) parameters[i], (string) parameters[i + 1]));
                } else if (typeof (string).IsInstanceOfType (parameters[i])) {
                    exps.Add (new CodeParameterDeclarationExpression (
                        (string) parameters[i], (string) parameters[i + 1]));
                } else if (typeof (CodeTypeReference).IsInstanceOfType (parameters[i])) {
                    exps.Add (new CodeParameterDeclarationExpression (
                        (CodeTypeReference) parameters[i], (string) parameters[i + 1]));
                } else {
                    throw new ArgumentException (String.Format (CultureInfo.InvariantCulture,
                        "Argument {0} should be of Type, string, or CodeTypeReference.", i));
                }
            }

            return exps;
        }

        // CodeMemberProperty helpers
        // -- type defined by string
        public static CodeMemberProperty CreateProperty (string type, string name) {
            CodeMemberProperty cp = new CodeMemberProperty ();
            cp.Name = name;
            cp.Type = new CodeTypeReference (type);

            return cp;
        }

        public static CodeMemberProperty CreateProperty (string type, string name,
            MemberAttributes attribute)
        {
            CodeMemberProperty cp = CreateProperty (type, name);
            cp.Attributes = attribute;

            return cp;
        }

        // -- type defined by Type
        public static CodeMemberProperty CreateProperty (Type type, string name) {
            CodeMemberProperty cp = new CodeMemberProperty ();
            cp.Name = name;
            cp.Type = new CodeTypeReference (type);

            return cp;
        }

        public static CodeMemberProperty CreateProperty (Type type, string name,
            MemberAttributes attribute)
        {
            CodeMemberProperty cp = CreateProperty (type, name);
            cp.Attributes = attribute;

            return cp;
        }


        // -- type defined by CodeTypeReference
        public static CodeMemberProperty CreateProperty (CodeTypeReference type, string name,
            MemberAttributes attribute)
        {
            CodeMemberProperty cp = CreateProperty (type, name);
            cp.Attributes = attribute;

            return cp;
        }

        public static CodeMemberProperty CreateProperty (CodeTypeReference type, string name) {
            CodeMemberProperty cp = new CodeMemberProperty ();
            cp.Name = name;
            cp.Type = type;

            return cp;
        }


        // ----------------------------------------------------
        // CodeStatement helpers

        // Creates:
        // for (varName = startVal; varName < limit; varName = varName + 1) {
        //
        // }
        public static CodeIterationStatement CreateCountUpLoop (string varName, int startVal, CodeExpression limit) {
            CodeIterationStatement iter = new CodeIterationStatement ();

            iter.InitStatement = new CodeAssignStatement (new CodeVariableReferenceExpression (varName),
                new CodePrimitiveExpression (startVal));
            iter.TestExpression = new CodeBinaryOperatorExpression (new CodeVariableReferenceExpression (varName),
                CodeBinaryOperatorType.LessThan, limit);
            iter.IncrementStatement = new CodeAssignStatement (new CodeVariableReferenceExpression (varName),
                new CodeBinaryOperatorExpression (new CodeVariableReferenceExpression (varName), CodeBinaryOperatorType.Add,
                new CodePrimitiveExpression (1)));

            return iter;
        }

        // creates:
        // var1 (op) var2
        //
        // It automatically boxes any strings as variable references,
        // ints and doubles become primitives everything else generates a
        // runtime exception
        public static CodeBinaryOperatorExpression CreateBinaryOperatorExpression (object var1,
                CodeBinaryOperatorType op, object var2) {

            if (var1 == null)
                throw new ArgumentNullException ("var1");
            if (var2 == null)
                throw new ArgumentNullException ("var2");

            CodeExpression var1Exp = null;
            CodeExpression var2Exp = null;

            if (var1 is string)
                var1Exp = new CodeVariableReferenceExpression ((string) var1);
            else if (var1 is int)
                var1Exp = new CodePrimitiveExpression ((int) var1);
            else if (var1 is double)
                var1Exp = new CodePrimitiveExpression ((double) var2);
            else
                var1Exp = (CodeExpression) var1;

            if (var2 is string)
                var2Exp = new CodeVariableReferenceExpression ((string) var2);
            else if (var2 is int)
                var2Exp = new CodePrimitiveExpression ((int) var2);
            else if (var2 is double)
                var2Exp = new CodePrimitiveExpression ((double) var2);
            else
                var2Exp = (CodeExpression) var2;

            return new CodeBinaryOperatorExpression (var1Exp, op, var2Exp);
        }

        // creates:
        // varName = var1 (OP) var2
        //
        // See above for rules about var1, var2
        public static CodeAssignStatement CreateBinaryOperatorStatement (string varName, object var1, CodeBinaryOperatorType op,
                object var2) {
            return new CodeAssignStatement (new CodeVariableReferenceExpression (varName),
                CreateBinaryOperatorExpression (var1, op, var2));
        }


        // creates:
        // varName = (type) var1 (OP) var2
        public static CodeAssignStatement CreateBinaryOperatorStatementWithCast (string varName, CodeTypeReference type,
               object var1, CodeBinaryOperatorType op, object var2) {
            return new CodeAssignStatement (new CodeVariableReferenceExpression (varName),
                new CodeCastExpression (type, CreateBinaryOperatorExpression (var1, op, var2)));
        }

        // creates:
        // varName = (type) var1 (OP) var2
        public static CodeAssignStatement CreateBinaryOperatorStatementWithCast (string varName, Type type,
               object var1, CodeBinaryOperatorType op, object var2) {
            return new CodeAssignStatement (new CodeVariableReferenceExpression (varName),
                new CodeCastExpression (type, CreateBinaryOperatorExpression (var1, op, var2)));
        }

        // creates:
        // varName = varName + expression;
        public static CodeAssignStatement CreateIncrementByStatement (string varName, CodeExpression expression) {
            return CreateBinaryOperatorStatement (varName, new CodeVariableReferenceExpression (varName),
                    CodeBinaryOperatorType.Add, expression);

        }

        public static CodeAssignStatement CreateIncrementByStatement (string varName, int expression) {
            return CreateBinaryOperatorStatement (varName, new CodeVariableReferenceExpression (varName),
                    CodeBinaryOperatorType.Add, new CodePrimitiveExpression (expression));
        }

        public static CodeAssignStatement CreateIncrementByStatement (CodeExpression varName, int expression) {
            return new CodeAssignStatement (varName,
                    new CodeBinaryOperatorExpression (varName, CodeBinaryOperatorType.Add,
                        new CodePrimitiveExpression (expression)));
        }

        public static CodeAssignStatement CreateIncrementByStatement (string varName, string expression) {
            return CreateBinaryOperatorStatement (varName, new CodeVariableReferenceExpression (varName),
                    CodeBinaryOperatorType.Add, new CodeVariableReferenceExpression (expression));
        }

/*        public static CodeAssignStatement CreateIncrementByStatement (CodeExpression varName, string expression) {
            return new CodeBinaryOperatorExpression (varName,
                    CodeBinaryOperatorType.Add, new CodeVariableReferenceExpression (expression));
        }*/

        // creates:
        // array[index]
        //
        // Automatically boxes strings into CodeVariableReferenceExpression
        public static CodeArrayIndexerExpression CreateArrayRef (string array, string index) {
            return new CodeArrayIndexerExpression (new CodeVariableReferenceExpression (array),
                    new CodeVariableReferenceExpression (index));
        }

        // Same but automatically boxes int into CodePrimitiveExpression
        public static CodeArrayIndexerExpression CreateArrayRef (string array, int index) {
            return new CodeArrayIndexerExpression (new CodeVariableReferenceExpression (array),
                    new CodePrimitiveExpression (index));
        }
        
        // creates:
        // var.fieldName
        public static CodeFieldReferenceExpression CreateFieldRef (string var, string fieldName) {
            return new CodeFieldReferenceExpression (new CodeVariableReferenceExpression (var),
                    fieldName);
        }

        // creates:
        // target.methodName ()
        //
        // Boxes all string params as CodeVariableReferenceExpressions,
        //   CodeExpressions get passed directly in
        //   all others get boxed as CodePrimitiveExpressions
        public static CodeMethodInvokeExpression CreateMethodInvoke (CodeExpression target, string methodName,
                params object [] args) {
            CodeMethodInvokeExpression exp = new CodeMethodInvokeExpression (
                    new CodeMethodReferenceExpression (target, methodName));
            foreach (object a in args) {
                if (a is string)
                    exp.Parameters.Add (new CodeVariableReferenceExpression ((string) a));
                else if (a is CodeExpression)
                    exp.Parameters.Add ((CodeExpression) a);
                else
                    exp.Parameters.Add (new CodePrimitiveExpression (a));
            }

            return exp;
        }

        public static CodeStatement ConsoleWriteLineStatement (CodeExpression exp) {
            return new CodeExpressionStatement (
                new CodeMethodInvokeExpression (
                new CodeMethodReferenceExpression (
                new CodeTypeReferenceExpression (new CodeTypeReference (typeof (System.Console))),
                "WriteLine"),
                new CodeExpression[] {
                    exp,
                }));
        }

        public static CodeStatement ConsoleWriteLineStatement (string txt) {
            return ConsoleWriteLineStatement (new CodePrimitiveExpression (txt));
        }
    }
}
