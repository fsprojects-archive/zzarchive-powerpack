using System;
using System.Text;
using System.CodeDom;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace Microsoft.Samples.CodeDomTestSuite {

    [Serializable]
    public class SubsetConformanceException : Exception {
        public SubsetConformanceException () {}
        public SubsetConformanceException (string e) : base (e) {}
        public SubsetConformanceException (string message, Exception innerException): base (message, innerException) {}
        protected SubsetConformanceException (SerializationInfo info, StreamingContext context) : base (info, context) {}
    }

    public class SubsetConformance {

        Stack errorStack;
        Stack locationStack;

        public SubsetConformance () {
            errorStack = new Stack ();
            locationStack = new Stack ();
        }

        public void ClearStacks () {
            errorStack.Clear ();
            locationStack.Clear ();
        }

        private void PushLocation (object exp) {
            locationStack.Push (exp.ToString ());
        }

        private void PopLocation () {
            locationStack.Pop ();
        }

        public override string ToString () {
            StringBuilder sb = new StringBuilder ();
            foreach (string error in errorStack)
                sb.Append (error + Environment.NewLine);

            return sb.ToString ();
        }

        private string GetLocationStack () {
            StringBuilder sb = new StringBuilder ();

            bool first = true;

            // show location
            foreach (string loc in locationStack)
                if (first) {
                    sb.AppendFormat ("{0}{1}", loc, Environment.NewLine);
                    first = false;
                } else
                    sb.AppendFormat ("\t-> {0}{1}", loc, Environment.NewLine);

            return sb.ToString ();
        }

        private void PushError (string error) {

            // push the error on the stack
            errorStack.Push (GetLocationStack () + "\t*** " + error);
        }

        // PushError with format
        private void PushError (string error, params object [] parms) {
            PushError (String.Format (CultureInfo.InvariantCulture, error, parms));
        }

        private bool IsSimpleTarget (CodeObject exp) {
            bool result = false;

            // A Simple Target is any argument, Local Variables, Field references,
            // Property References, ThisReference, BaseReference, CodeCastExpression,
            // CodePrimitives, CodeObjectCreate, CodeArrayCreate, TypeOf,
            // ArrayReferences, TypeReferences

            if (exp == null) {
                result = true;
            } else if (exp is CodeTypeReference) {
                result = ValidateCodeTypeReference ((CodeTypeReference) exp);
            } else if (exp is CodeExpression) {

                if (exp is CodeVariableReferenceExpression) {
                    result = ValidateCodeVariableReferenceExpression ((CodeVariableReferenceExpression) exp);
                }

                else if (exp is CodeThisReferenceExpression) {
                    result = ValidateCodeThisReferenceExpression ((CodeThisReferenceExpression) exp);
                }

                else if (exp is CodeBaseReferenceExpression) {
                    result = ValidateCodeBaseReferenceExpression ((CodeBaseReferenceExpression) exp);
                }

                else if (exp is CodeTypeOfExpression) {
                    result = ValidateCodeTypeOfExpression ((CodeTypeOfExpression) exp);
                }

                else if (exp is CodeTypeReferenceExpression) {
                    result = ValidateCodeTypeReferenceExpression ((CodeTypeReferenceExpression) exp);
                }

                else if (exp is CodeFieldReferenceExpression) {
                    result = ValidateCodeFieldReferenceExpression ((CodeFieldReferenceExpression) exp);
                }

                else if (exp is CodePropertyReferenceExpression) {
                    result = ValidateCodePropertyReferenceExpression ((CodePropertyReferenceExpression) exp);
                }
                
                else if (exp is CodeCastExpression) {
                    result = ValidateCodeCastExpression ((CodeCastExpression) exp);
                }

                else if (exp is CodePrimitiveExpression) {
                    result = ValidateCodePrimitiveExpression ((CodePrimitiveExpression) exp);
                }

                else if (exp is CodeObjectCreateExpression) {
                    result = ValidateCodeObjectCreateExpression ((CodeObjectCreateExpression) exp);
                }

                else if (exp is CodeArrayCreateExpression) {
                    result = ValidateCodeArrayCreateExpression ((CodeArrayCreateExpression) exp);
                }

                else if (exp is CodeArrayIndexerExpression) {
                    result = ValidateCodeArrayIndexerExpression ((CodeArrayIndexerExpression) exp);
                }

                else if (exp is CodeArgumentReferenceExpression) {
                    result = ValidateCodeArgumentReferenceExpression ((CodeArgumentReferenceExpression) exp);
                }

                else if (exp is CodeDelegateCreateExpression) {
                    result = ValidateCodeDelegateCreateExpression ((CodeDelegateCreateExpression) exp);
                }
            }

            if (!result)
                if (exp != null)
                    PushError ("This is not a Simple Target {0}.", exp.ToString ());
                else
                    PushError ("This is not a Simple Target.");

            return result;
        }

        private bool ValidateCodeParameterDeclarationExpression (CodeParameterDeclarationExpression exp) {
            bool result = false;
            PushLocation (exp);

            // all conditions must be met for this to be true at the end
            // of the method
            bool allValid = true;

            // A simple parameter has default FieldDirection (In), and is not an array
            // or is a single-dimensional array. Must not have CustomAttributes.

            if (exp.Direction != FieldDirection.In) {
                allValid = false;
                PushError ("Can only be an 'in' direction parameter.");
            }

            if (exp.Type.ArrayRank > 1) {
                allValid = false;
                PushError ("Can only declare single-dimensional array parameters.");
            }

            if (exp.CustomAttributes.Count > 0) {
                allValid = false;
                PushError ("Cannot declare custom attributes on parameters.");
            }

            if (allValid)
                result = true;

            PopLocation ();
            return result;
        }


        /*private bool IsSingleDimensionalArrayType (CodeTypeReference type) {
            bool result = false;
            PushLocation (exp);

            if (type.ArrayRank == 1)
                result = true;
            else
                PushError ("Type is not a single dimensional array.")

            PopLocation ();
            return result;
        }*/

        private bool IsOfBasicType (object obj) {
            bool result = false;

            if (obj == null ||
                    obj is Boolean ||
                    obj is Byte ||
                    obj is Int16 ||
                    obj is Int32 ||
                    obj is Int64 ||
                    obj is Single ||
                    obj is Double ||
                    obj is Char ||
                    obj is String)
                result =  true;

            return result;
        }

        public bool ValidateCodeCompileUnit (CodeCompileUnit exp) {
            bool result = true;
            PushLocation (exp);

            foreach (CodeAttributeDeclaration attr in exp.AssemblyCustomAttributes)
                if (!ValidateCodeAttributeDeclaration (attr))
                    result = false;

            foreach (CodeNamespace ns in exp.Namespaces)
                if (!ValidateCodeNamespace (ns))
                    result = false;

            PopLocation ();
            return result;
        }

        public bool ValidateCodePrimitiveExpression (CodePrimitiveExpression exp) {
            bool result = false;
            PushLocation (exp);

            if (IsOfBasicType (exp.Value))
                result = true;
            else
                PushError ("Value must be a basic type.  Currently it is '{0}'.",
                        exp != null ? exp.Value.GetType ().ToString () : "null");

            PopLocation();
            return result;
        }

        public bool ValidateCodeArrayCreateExpression (CodeArrayCreateExpression exp) {
            bool result = false;
            PushLocation (exp);

            // are all conditions valid?
            bool allValid = true;

            // check the type of the array create
            if (exp.CreateType.ArrayRank != 1) {
                allValid = false;
                PushError ("ArrayRank must be 1.");
            }

            // check initializers... all must be simple targets and
            // not arraycreate statements

            foreach (CodeExpression e in exp.Initializers) {
                if (e is CodeArrayCreateExpression) {
                    allValid = false;
                    PushError ("Cannot nest CodeArrayCreateExpressions.");
                } else if (!IsSimpleTarget (e))
                    allValid = false;
            }

            // check size expression
            if (exp.SizeExpression != null &&
                    !IsSimpleTarget (exp.SizeExpression))
                allValid = false;

            if (allValid)
                result = true;

            PopLocation ();
            return result;
        }

        private bool IsValidAttribute (MemberAttributes attr) {
            bool result = true;

            // TODO: Validate combinations.
            if ((attr & ~(MemberAttributes.Public |
                     MemberAttributes.Private |
                     MemberAttributes.Family |
                     MemberAttributes.Final |
                     MemberAttributes.Overloaded |
                     MemberAttributes.Override |
                     MemberAttributes.Static |
                     MemberAttributes.New)) != 0) {
                result = false;
                PushError ("Attribute must be some combination of assembly, public, private, family, final, new, overload, and override.");
            }

            //TODO: FIX SO IT WORKS
            /*if ((attr & MemberAttributes.Static) != 0 &&
                    ((attr & MemberAttributes.Public) != 0 ||
                    (attr & MemberAttributes.Assembly) != 0 ||
                    (attr & MemberAttributes.Family) != 0)) {
                result = false;
                PushError ("Non-private static attributes are not in the subset.");
            }*/

            return result;
        }

        private bool IsValidTypeAttribute (TypeAttributes attr) {
            bool result = true;

            // TODO: Validate combinations.
            if ((attr & ~(TypeAttributes.Class |
                     TypeAttributes.Interface |
                     TypeAttributes.Public |
                     TypeAttributes.Sealed |
                     TypeAttributes.Abstract)) != 0) {
                result = false;
                PushError ("TypeAttribute must be some combination of class, interface, public, sealed, abstract, or assembly.");
            }

            return result;
        }

        public bool ValidateCodeMemberMethod (CodeMemberMethod exp){
            bool result = true;
            PushLocation (exp);

            if (!IsValidAttribute (exp.Attributes))
                result = false;

            foreach (CodeAttributeDeclaration attr in exp.CustomAttributes)
                if (!ValidateCodeAttributeDeclaration (attr))
                    result = false;

            foreach (CodeParameterDeclarationExpression param in exp.Parameters) {
                if (!ValidateCodeParameterDeclarationExpression (param))
                    result = false;
            }

            if (exp.ReturnTypeCustomAttributes != null && exp.ReturnTypeCustomAttributes.Count > 0) {
                PushError ("Cannot use return type custom attributes.");
                result = false;
            }


            if (exp.ImplementationTypes.Count > 1) {
                PushError ("ImplementationTypes may only be a collection of one type.");
                result = false;
            }

            if (exp.PrivateImplementationType != null &&
                    (exp.ImplementationTypes != null || exp.ImplementationTypes.Count > 0)) {
                PushError ("Only one of ImplementationTypes and PrivateImplementationType may be declared at once.");
                result = false;
            }

            foreach (CodeStatement stmt in exp.Statements) {
                if (!ValidateCodeStatement (stmt))
                    result = false;
            }

            PopLocation();
            return result;
        }

        public bool ValidateCodeExpression (CodeExpression exp){
            bool result = false;

            if (exp is CodeArrayCreateExpression) {
                result = ValidateCodeArrayCreateExpression ((CodeArrayCreateExpression) exp);
            }
            else if (exp is CodeArrayIndexerExpression) {
                result = ValidateCodeArrayIndexerExpression ((CodeArrayIndexerExpression) exp);
            }
            else if (exp is CodeArgumentReferenceExpression) {
                result = ValidateCodeArgumentReferenceExpression ((CodeArgumentReferenceExpression) exp);
            }
            else if (exp is CodeBaseReferenceExpression) {
                result = ValidateCodeBaseReferenceExpression ((CodeBaseReferenceExpression) exp);
            }
            else if (exp is CodeBinaryOperatorExpression) {
                result = ValidateCodeBinaryOperatorExpression ((CodeBinaryOperatorExpression) exp);
            }
            else if (exp is CodeCastExpression) {
                result = ValidateCodeCastExpression ((CodeCastExpression) exp);
            }
            else if (exp is CodeDelegateCreateExpression) {
                result = ValidateCodeDelegateCreateExpression ((CodeDelegateCreateExpression) exp);
            }
            else if (exp is CodeDelegateInvokeExpression) {
                result = ValidateCodeDelegateInvokeExpression ((CodeDelegateInvokeExpression) exp);
            }
            else if (exp is CodeDirectionExpression) {
                result = ValidateCodeDirectionExpression ((CodeDirectionExpression) exp);
            }
            else if (exp is CodeEventReferenceExpression) {
                result = ValidateCodeEventReferenceExpression ((CodeEventReferenceExpression) exp);
            }
            else if (exp is CodeFieldReferenceExpression) {
                result = ValidateCodeFieldReferenceExpression ((CodeFieldReferenceExpression) exp);
            }
            else if (exp is CodeIndexerExpression) {
                result = ValidateCodeIndexerExpression ((CodeIndexerExpression) exp);
            }
            else if (exp is CodeMethodInvokeExpression) {
                result = ValidateCodeMethodInvokeExpression ((CodeMethodInvokeExpression) exp);
            }
            else if (exp is CodeMethodReferenceExpression) {
                result = ValidateCodeMethodReferenceExpression ((CodeMethodReferenceExpression) exp);
            }
            else if (exp is CodeObjectCreateExpression) {
                result = ValidateCodeObjectCreateExpression ((CodeObjectCreateExpression) exp);
            }
            else if (exp is CodeParameterDeclarationExpression) {
                result = ValidateCodeParameterDeclarationExpression ((CodeParameterDeclarationExpression) exp);
            }
            else if (exp is CodePrimitiveExpression) {
                result = ValidateCodePrimitiveExpression ((CodePrimitiveExpression) exp);
            }
            else if (exp is CodePropertyReferenceExpression) {
                result = ValidateCodePropertyReferenceExpression ((CodePropertyReferenceExpression) exp);
            }
            else if (exp is CodePropertySetValueReferenceExpression) {
                result = ValidateCodePropertySetValueReferenceExpression ((CodePropertySetValueReferenceExpression) exp);
            }
            else if (exp is CodeSnippetExpression) {
                result = ValidateCodeSnippetExpression ((CodeSnippetExpression) exp);
            }
            else if (exp is CodeThisReferenceExpression) {
                result = ValidateCodeThisReferenceExpression ((CodeThisReferenceExpression) exp);
            }
            else if (exp is CodeTypeOfExpression) {
                result = ValidateCodeTypeOfExpression ((CodeTypeOfExpression) exp);
            }
            else if (exp is CodeTypeReferenceExpression) {
                result = ValidateCodeTypeReferenceExpression ((CodeTypeReferenceExpression) exp);
            }
            else if (exp is CodeVariableReferenceExpression) {
                result = ValidateCodeVariableReferenceExpression ((CodeVariableReferenceExpression) exp);
            }
            else {
                PushError ("Cannot validate an unknown CodeExpression.");
                result = false;
            }

            return result;
        }

        public bool ValidateCodeTypeMember (CodeTypeMember exp) {
            bool result = false;
            PushLocation (exp);

            if (exp is CodeMemberEvent) {
                result = ValidateCodeMemberEvent ((CodeMemberEvent) exp);
            }

            else if (exp is CodeMemberField) {
                result = ValidateCodeMemberField ((CodeMemberField) exp);
            }

            else if (exp is CodeMemberMethod) {
                result = ValidateCodeMemberMethod ((CodeMemberMethod) exp);
            }

            else if (exp is CodeMemberProperty) {
                result = ValidateCodeMemberProperty ((CodeMemberProperty) exp);
            }

            else if (exp is CodeSnippetTypeMember) {
                result = ValidateCodeSnippetTypeMember ((CodeSnippetTypeMember) exp);
            }

            else if (exp is CodeTypeDeclaration) {
                result = ValidateCodeTypeDeclaration ((CodeTypeDeclaration) exp);
            }
            else {
                PushError ("Cannot validate an unknown CodeExpression.");
                result = false;
            }

            PopLocation();
            return result;
        }

        public bool ValidateCodeStatement (CodeStatement exp){
            bool result = false;
            PushLocation (exp);

            // branch off into the corresponding statement based on the type
            if (exp is CodeAssignStatement) {
                result = ValidateCodeAssignStatement ((CodeAssignStatement) exp);
            }
            else if (exp is CodeAttachEventStatement) {
                result = ValidateCodeAttachEventStatement ((CodeAttachEventStatement) exp);
            }
            else if (exp is CodeCommentStatement) {
                result = ValidateCodeCommentStatement ((CodeCommentStatement) exp);
            }
            else if (exp is CodeConditionStatement) {
                result = ValidateCodeConditionStatement ((CodeConditionStatement) exp);
            }
            else if (exp is CodeExpressionStatement) {
                result = ValidateCodeExpressionStatement ((CodeExpressionStatement) exp);
            }
            else if (exp is CodeGotoStatement) {
                result = ValidateCodeGotoStatement ((CodeGotoStatement) exp);
            }
            else if (exp is CodeIterationStatement) {
                result = ValidateCodeIterationStatement ((CodeIterationStatement) exp);
            }
            else if (exp is CodeLabeledStatement) {
                result = ValidateCodeLabeledStatement ((CodeLabeledStatement) exp);
            }
            else if (exp is CodeMethodReturnStatement) {
                result = ValidateCodeMethodReturnStatement ((CodeMethodReturnStatement) exp);
            }
            else if (exp is CodeRemoveEventStatement) {
                result = ValidateCodeRemoveEventStatement ((CodeRemoveEventStatement) exp);
            }
            else if (exp is CodeSnippetStatement) {
                result = ValidateCodeSnippetStatement ((CodeSnippetStatement) exp);
            }
            else if (exp is CodeThrowExceptionStatement) {
                result = ValidateCodeThrowExceptionStatement ((CodeThrowExceptionStatement) exp);
            }
            else if (exp is CodeTryCatchFinallyStatement) {
                result = ValidateCodeTryCatchFinallyStatement ((CodeTryCatchFinallyStatement) exp);
            }
            else if (exp is CodeVariableDeclarationStatement) {
                result = ValidateCodeVariableDeclarationStatement ((CodeVariableDeclarationStatement) exp);
            } else {
                PushError ("Cannot validate an unknown statement.");
                result = false;
            }

            PopLocation();
            return result;
        }

        public bool ValidateCodeObjectCreateExpression (CodeObjectCreateExpression exp){
            bool result = true;
            PushLocation (exp);

            foreach (CodeExpression e in exp.Parameters)
                if (!IsSimpleTarget (e))
                    result = false;

            PopLocation();
            return result;
        }

        public bool ValidateCodeCastExpression (CodeCastExpression exp){
            bool result = false;
            PushLocation (exp);

            if (IsSimpleTarget (exp.Expression))
                result = true;

            PopLocation();
            return result;
        }

        public bool ValidateCodeFieldReferenceExpression (CodeFieldReferenceExpression exp) {
            bool result = false;
            PushLocation (exp);

            if (IsSimpleTarget (exp.TargetObject))
                result = true;

            PopLocation();
            return result;
        }

        public bool ValidateCodePropertyReferenceExpression (CodePropertyReferenceExpression exp){
            bool result = false;
            PushLocation (exp);

            if (IsSimpleTarget (exp.TargetObject))
                result = true;

            PopLocation();
            return result;
        }


        public bool ValidateCodeAssignStatement (CodeAssignStatement exp) {
            bool result = false;
            PushLocation (exp);

            if (IsSimpleTarget (exp.Left) &&
                    ValidateCodeExpression (exp.Right))
                result = true;

            PopLocation();
            return result;
        }

        public bool ValidateCodeAttributeArgument (CodeAttributeArgument exp){
            bool result = true;
            PushLocation(exp);

            if (exp.Name != null && exp.Name.Length > 0) {
                PushError ("Cannot use named attribute arguments.");
                result = false;
            }

            if (!IsSimpleTarget (exp.Value))
                result = false;

            PopLocation();
            return result;
        }

        public bool ValidateCodeAttributeDeclaration (CodeAttributeDeclaration exp) {
            bool result = true;
            PushLocation(exp);

            foreach (CodeAttributeArgument arg in exp.Arguments)
                if (!ValidateCodeAttributeArgument (arg))
                    result = false;

            PopLocation();
            return result;
        }

        public bool ValidateCodeAttachEventStatement (CodeAttachEventStatement exp) {
            bool result = false;
            PushLocation (exp);

            if (ValidateCodeEventReferenceExpression (exp.Event) &&
                    IsSimpleTarget (exp.Listener))
                result = true;

            PopLocation();
            return result;
        }

        public bool ValidateCodeCommentStatement (CodeCommentStatement exp) {
            PushLocation (exp);

            bool result = ValidateCodeComment (exp.Comment);

            PopLocation();
            return result;
        }

        public bool ValidateCodeComment (CodeComment exp){
            return true;
        }

        public bool ValidateCodeConditionStatement (CodeConditionStatement exp) {
            bool result = true;
            PushLocation (exp);

            // are binary ops allowed?  I sure hope so.
            if (!(exp.Condition is CodeBinaryOperatorExpression) &&
                    !IsSimpleTarget (exp.Condition))
                result = false;

            foreach (CodeStatement stmt in exp.FalseStatements)
                if (!ValidateCodeStatement (stmt))
                    result = false;

            foreach (CodeStatement stmt in exp.TrueStatements)
                if (!ValidateCodeStatement (stmt))
                    result = false;

            PopLocation();
            return result;
        }

        public bool ValidateCodeConstructor (CodeConstructor exp){
            bool result = true;
            PushLocation(exp);

            foreach (CodeExpression e in exp.BaseConstructorArgs)
                if (!IsSimpleTarget (e))
                    result = false;

            if (exp.ChainedConstructorArgs != null ||
                    exp.ChainedConstructorArgs.Count > 0) {
                PushError ("Chained constructors are not in the subset.");
                result = false;
            }

            if (!(((exp.Attributes & ~MemberAttributes.Assembly) == 0 &&
                    (exp.Attributes & MemberAttributes.Assembly) != 0) &&
                        (exp.Attributes & ~MemberAttributes.Family) == 0 &&
                            (exp.Attributes & MemberAttributes.Family) != 0) &&
                        ((exp.Attributes & ~MemberAttributes.Public) == 0 &&
                            (exp.Attributes & MemberAttributes.Public) != 0) &&
                        ((exp.Attributes & ~MemberAttributes.Private) == 0 &&
                            (exp.Attributes & MemberAttributes.Private) != 0)) {
                PushError ("Attributes must be one of Assembly, Family, Private or Public.");
                result = false;
            }

            foreach (CodeParameterDeclarationExpression param in exp.Parameters)
                if (!ValidateCodeParameterDeclarationExpression (param))
                    result = false;

            foreach (CodeStatement stmt in exp.Statements)
                if (!ValidateCodeStatement (stmt))
                    result = false;

            PopLocation();
            return result;
        }

        public bool ValidateCodeDelegateCreateExpression (CodeDelegateCreateExpression exp) {
            bool result = false;
            PushLocation (exp);

            if (IsSimpleTarget (exp.TargetObject))
                result = true;

            PopLocation ();
            return result;
        }

        public bool ValidateCodeExpressionStatement (CodeExpressionStatement exp) {
            bool result = false;
            PushLocation (exp);

            if (ValidateCodeExpression (exp.Expression))
                result = true;

            PopLocation();
            return result;
        }

        public bool ValidateCodeGotoStatement (CodeGotoStatement exp) {
            PushError ("CodeGotoStatement is not allowed.");
            return false;
        }

        public bool ValidateCodeIterationStatement (CodeIterationStatement exp) {
            bool result = true;
            PushLocation (exp);

            if (!ValidateCodeStatement (exp.IncrementStatement))
                result = false;

            if (!ValidateCodeStatement (exp.InitStatement))
                result = false;

            if (exp.IncrementStatement is CodeVariableDeclarationStatement ||
                exp.InitStatement is CodeVariableDeclarationStatement) {
                PushError ("Increment and/or init statements may not be variable declarations.");
                result = false;
            }

            foreach (CodeStatement stmt in exp.Statements)
                if (!ValidateCodeStatement (stmt))
                    result = false;

            // may need to add something about simple targets
            if (!ValidateCodeExpression (exp.TestExpression))
                result = false;

            PopLocation();
            return result;
        }

        public bool ValidateCodeLabeledStatement (CodeLabeledStatement exp) {
            PushError ("CodeLabeledStatement is not allowed.");
            return false;
        }

        public bool ValidateCodeMethodReturnStatement (CodeMethodReturnStatement exp) {
            bool result = false;
            PushLocation (exp);

            if (ValidateCodeExpression (exp.Expression))
                result = true;

            PopLocation();
            return result;
        }

        public bool ValidateCodeNamespace (CodeNamespace exp) {
            bool result = true;
            PushLocation (exp);

            foreach (CodeCommentStatement comment in exp.Comments)
                if (!ValidateCodeCommentStatement (comment))
                    result = false;

            foreach (CodeNamespaceImport import in exp.Imports)
                if (!ValidateCodeNamespaceImport (import))
                    result = false;

            foreach (CodeTypeDeclaration decl in exp.Types)
                if (!ValidateCodeTypeDeclaration (decl))
                    result = false;

            PopLocation();
            return result;
        }

        public bool ValidateCodeNamespaceImport (CodeNamespaceImport exp) {
            return true;
        }

        public bool ValidateCodeRemoveEventStatement (CodeRemoveEventStatement exp) {
            bool result = false;
            PushLocation (exp);

            if (IsSimpleTarget (exp.Listener) &&
                    ValidateCodeEventReferenceExpression (exp.Event))
                result = true;

            PopLocation();
            return result;
        }

        public bool ValidateCodeSnippetStatement (CodeSnippetStatement exp) {
            return true;
        }

        public bool ValidateCodeThrowExceptionStatement (CodeThrowExceptionStatement exp) {
            PushError ("CodeThrowExceptionStatement is not in the subset.");
            return false;
        }

        public bool ValidateCodeTryCatchFinallyStatement (CodeTryCatchFinallyStatement exp) {
            PushError ("CodeTryCatchFinallyStatement is not in the subset.");
            return false;
        }

        public bool ValidateCodeVariableDeclarationStatement (CodeVariableDeclarationStatement exp) {
            bool result = true;
            PushLocation (exp);

            if (exp.InitExpression != null && !ValidateCodeExpression (exp.InitExpression))
                result = false;

            PopLocation();
            return result;
        }


        public bool ValidateCodeArgumentReferenceExpression (CodeArgumentReferenceExpression exp) {
            return true;
        }

        public bool ValidateCodeArrayIndexerExpression (CodeArrayIndexerExpression exp) {
            bool result = true;
            PushLocation (exp);

            if (exp.Indices != null && exp.Indices.Count > 0) {
                if (exp.Indices.Count > 1) {
                    PushError ("Can only have one index.");
                    result = false;
                } else {
                    if (!IsSimpleTarget (exp.Indices[0]))
                        result = false;
                }
            }

            if (!IsSimpleTarget (exp.TargetObject))
                result = false;

            PopLocation();
            return result;
        }

        public bool ValidateCodeBaseReferenceExpression (CodeBaseReferenceExpression exp) {
            // could include code that looks up the location stack to see if there's a type member
            // that has a MemberAttribute of Override
            return true;
        }

        public bool ValidateCodeBinaryOperatorExpression (CodeBinaryOperatorExpression exp) {
            bool result = true;
            PushLocation (exp);

            if (!IsSimpleTarget (exp.Left)) {
                PushError ("Left must be a simple target.");
                result = false;
            }

            if (!IsSimpleTarget (exp.Right)) {
                PushError ("Right must be a simple target.");
                result = false;
            }

            if (((exp.Left is CodePrimitiveExpression &&
                    ((CodePrimitiveExpression) exp.Left).Value is String) ||
                (exp.Right is CodePrimitiveExpression &&
                    ((CodePrimitiveExpression) exp.Right).Value is String)) &&
                exp.Operator != CodeBinaryOperatorType.Add) {

                PushError ("Can only use add operator when either operand is a string.");
                result = false;
            }


            // what about !=????
            if (exp.Operator == CodeBinaryOperatorType.IdentityEquality ||
                exp.Operator == CodeBinaryOperatorType.Assign) {
                PushError ("May not use operator {0}.", Enum.Format (typeof (CodeBinaryOperatorType),
                            exp.Operator, "F"));
                result = false;
            }

            PopLocation();
            return result;
        }

        public bool ValidateCodeDelegateInvokeExpression (CodeDelegateInvokeExpression exp) {
            bool result = true;
            PushLocation (exp);

            foreach (CodeExpression e in exp.Parameters)
                if (!IsSimpleTarget (e))
                    result = false;

            if (exp.TargetObject != null && !IsSimpleTarget (exp.TargetObject))
                result = false;

            PopLocation();
            return result;
        }

        public bool ValidateCodeDirectionExpression (CodeDirectionExpression exp) {
            PushError ("CodeDirectionExpression is not allowed.");
            return false;
        }

        public bool ValidateCodeEventReferenceExpression (CodeEventReferenceExpression exp) {
            bool result = false;
            PushLocation (exp);

            if (IsSimpleTarget (exp.TargetObject))
                result = true;

            PopLocation();
            return result;
        }

        public bool ValidateCodeIndexerExpression (CodeIndexerExpression exp) {
            bool result = true;
            PushLocation (exp);

            if (!IsSimpleTarget (exp.TargetObject))
                result = false;

            if (exp.Indices == null || exp.Indices.Count != 1) {
                PushError ("There must be at least one index.");
                result = false;
            }

            if (exp.Indices.Count == 1 && !IsSimpleTarget (exp.Indices[0])) {
                PushError ("Index must be a simple target.");
                result = false;
            }

            PopLocation();
            return result;
        }

        public bool ValidateCodeMethodInvokeExpression (CodeMethodInvokeExpression exp) {
            bool result = true;
            PushLocation (exp);

            if (!ValidateCodeMethodReferenceExpression (exp.Method))
                result = false;

            foreach (CodeExpression e in exp.Parameters)
                if (!IsSimpleTarget (e))
                    result = false;

            PopLocation();
            return result;
        }

        public bool ValidateCodeMethodReferenceExpression (CodeMethodReferenceExpression exp) {
            bool result = false;
            PushLocation (exp);

            if (IsSimpleTarget (exp.TargetObject))
                result = true;

            PopLocation();
            return result;
        }

        public bool ValidateCodePropertySetValueReferenceExpression (CodePropertySetValueReferenceExpression exp) {
            return true;
        }

        public bool ValidateCodeSnippetExpression (CodeSnippetExpression exp) {
            return true;
        }

        public bool ValidateCodeThisReferenceExpression (CodeThisReferenceExpression exp) {
            return true;
        }

        public bool ValidateCodeTypeOfExpression (CodeTypeOfExpression exp) {
            return true;
        }

        public bool ValidateCodeTypeReferenceExpression (CodeTypeReferenceExpression exp) {
            bool result = false;
            PushLocation (exp);

            if (ValidateCodeTypeReference (exp.Type))
                result = true;

            PopLocation();
            return result;
        }

        public bool ValidateCodeVariableReferenceExpression (CodeVariableReferenceExpression exp) {
            return true;
        }


        public bool ValidateCodeMemberEvent (CodeMemberEvent exp) {
            bool result = false;
            PushLocation(exp);

            if (IsValidAttribute (exp.Attributes) &&
                    ValidateCodeTypeReference (exp.Type))
                result = true;

            PopLocation();
            return result;
        }

        public bool ValidateCodeMemberField (CodeMemberField exp) {
            bool result = false;
            PushLocation(exp);

            if (IsValidAttribute (exp.Attributes) &&
                    IsSimpleTarget (exp.InitExpression))
                result = true;

            PopLocation();
            return result;
        }

        public bool ValidateCodeMemberProperty (CodeMemberProperty exp) {
            bool result = true;
            PushLocation(exp);

            if (!IsValidAttribute (exp.Attributes))
                result = false;

            if (exp.Parameters != null && exp.Parameters.Count > 0) {
                PushError ("Cannot use parameters with properties.");
                result = false;
            }

            foreach (CodeAttributeDeclaration attr in exp.CustomAttributes)
                if (!ValidateCodeAttributeDeclaration (attr))
                    result = false;

            if (exp.ImplementationTypes.Count > 1) {
                PushError ("ImplementationTypes may only be a collection of one type.");
                result = false;
            }

            if (exp.PrivateImplementationType != null &&
                    (exp.ImplementationTypes != null || exp.ImplementationTypes.Count > 0)) {
                PushError ("Only one of ImplementationTypes and PrivateImplementationType may be declared at once.");
                result = false;
            }

            foreach (CodeStatement stmt in exp.SetStatements) {
                if (!ValidateCodeStatement (stmt))
                    result = false;
            }

            foreach (CodeStatement stmt in exp.GetStatements) {
                if (!ValidateCodeStatement (stmt))
                    result = false;
            }

            PopLocation();
            return result;
        }

        public bool ValidateCodeSnippetTypeMember (CodeSnippetTypeMember exp) {
            return true;
        }

        public bool ValidateCodeTypeDeclaration (CodeTypeDeclaration exp) {
            bool result = true;
            PushLocation(exp);

            /*if (((exp.Attributes & MemberAttributes.Public) != 0 &&
                    (exp.Attributes & MemberAttributes.Private) != 0) ||
                    (((exp.Attributes & MemberAttributes.Public) == 0) &&
                        (exp.Attributes & MemberAttributes.Private) == 0)) {
                PushError ("Attributes must be either public or private.  Attributes currently are {0}.",
                        Enum.Format (typeof (MemberAttributes), exp.Attributes, "F"));
                result = false;
            }*/

            if ((exp.IsClass && (exp.IsEnum || exp.IsStruct || exp.IsInterface)) ||
                    (exp.IsEnum && (exp.IsClass || exp.IsStruct || exp.IsInterface)) ||
                    (exp.IsStruct && (exp.IsEnum || exp.IsClass || exp.IsInterface)) ||
                    (exp.IsInterface && (exp.IsEnum || exp.IsStruct || exp.IsClass))) {
                PushError ("Only one of IsClass, IsEnum, IsStruct, and IsInterface may be defined at any given time.");
                result = false;
            }

            if (exp.IsEnum || exp.IsStruct) {
                PushError ("Enumerations and structs are not in the subset.");
                result = false;
            }

            foreach (CodeTypeMember mem in exp.Members) {
                if (mem is CodeTypeDeclaration) {
                    PushError ("Cannot nest type declarations.");
                    result = false;
                } else if (!ValidateCodeTypeMember (mem))
                    result = false;
            }

            if (!IsValidTypeAttribute (exp.TypeAttributes))
                result = false;

            PopLocation();
            return result;
        }

        public bool ValidateCodeTypeReference (CodeTypeReference exp){
            bool result = true;
            PushLocation(exp);

            if (exp.ArrayRank != 0 && exp.ArrayRank != 1) {
                PushError ("Arrays may only be single dimensional.");
                result = false;
            }

            PopLocation();
            return result;
        }


    }
}
