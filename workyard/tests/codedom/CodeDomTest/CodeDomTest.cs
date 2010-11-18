using System;
using System.IO;
using System.Text;
using System.CodeDom;
using System.Security;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Runtime.InteropServices;

[assembly:ComVisible(false)]
[assembly:CLSCompliantAttribute (true)]
[assembly:FileIOPermission(SecurityAction.RequestMinimum)]
namespace Microsoft.Samples.CodeDomTestSuite {

    /// <summary>
    /// Base class for all CodeDom test cases.  You must inherit from this class
    /// if you are writing a test case for the CodeDom test suite.
    /// </summary>
    public abstract class CodeDomTest {

			  /// <summary>
			  /// Any additional comment needed...
			  /// </summary>
		  	public virtual string Comment {
						get { return ""; }
		  	}

        /// <summary>
        /// The classification of this test.  This will determine which
        /// bucket of test cases this test case falls into.
        /// </summary>
        /// <value>The classification of this test case.</value>
        public abstract TestTypes TestType {
            get;
        }

        /// <summary>
        /// The name of the test case.  This will be displayed to the user.
        /// The user will also use this name to identify this test case.
        /// </summary>
        /// <value>The name of this test case.</value>
        public abstract string Name {
            get;
        }

        /// <summary>
        /// This will be shown along with the test case name in the list
        /// of test cases.
        /// </summary>
        /// <value>Short description of the test case.</value>
        public abstract string Description {
            get;
        }

        /// <summary>
        /// Overriden to provide test functionality for the given
        /// provider.  Return true to signal a test pass, false for
        /// test failure.  Use LogMessage to output test status
        /// along the way.
        /// </summary>
        /// <param name="provider">Provider to test.</param>
        /// <returns>True if the test passed.  False otherwise.</returns>
        public abstract bool Run (CodeDomProvider provider);

        protected bool Supports (CodeDomProvider provider, GeneratorSupport support) {
#if WHIDBEY
            return provider.Supports (support);
#else
            return (provider.CreateGenerator ()).Supports (support);
#endif
        }

        /// <summary>
        /// Used to drop the given string to the given filename.  All IO
        /// and security exceptions are caught and displayed using LogMessage.
        /// </summary>
        protected void DumpStringToFile (string dumpText, string fileName) {
            try {
                using (StreamWriter srcWriter = new StreamWriter (
                            new FileStream (fileName, FileMode.Create),
                            Encoding.UTF8)) {
                    srcWriter.Write (dumpText);
                }
            } catch (IOException e) {
                // catch exceptions here because we want to continue testing if possible
                LogMessage ("Problem writing to '{0}'.", fileName);
                LogMessage ("Exception stack:");
                LogMessage (e.ToString ());
            } catch (SecurityException e) {
                // catch exceptions here because we want to continue testing if possible
                LogMessage ("Problem writing to '{0}'.", fileName);
                LogMessage ("Exception stack:");
                LogMessage (e.ToString ());
            }
        }

        #region Message logging
        string indentString = String.Empty;
        readonly string indentIncrement = "  ";
        TextWriter log = null;
        bool echoToConsole = false;

        /// <summary>
        /// Gets/sets the TextWriter stream that log messages should
        /// be written to.
        /// </summary>
        /// <value>The TextWriter to write messages to.</value>
        public TextWriter LogStream {
            get { return log; }
            set { log = value; }
        }

        /// <summary>
        /// In addition to writing to the TextWriter specified by the
        /// LogStream property, a true EchoToConsole property will also
        /// echo any output to the console.
        /// </summary>
        /// <value>Whether to echo to the console or not.</value>
        public bool EchoToConsole {
            get { return echoToConsole; }
            set { echoToConsole = value; }
        }

        /// <summary>
        /// Unindents the current indent string.  Used in combination
        /// with LogMessageIndent to make output follow a somewhat
        /// heirachial format.
        /// </summary>
        public void LogMessageUnindent () {
            if (indentString.Length >= 1)
                indentString = indentString.Substring (indentIncrement.Length);
        }

        /// <summary>
        /// Indents the current indent string.  Used in combination
        /// with LogMessageIndent to make output follow a somewhat
        /// heirachial format.
        /// </summary>
        public void LogMessageIndent () {
            indentString += indentIncrement;
        }

        /// <summary>
        /// Provides communication with the test suite.  Use this to log
        /// any arbitrary message during any part of your test.
        /// </summary>
        /// <param name="txt">The message to log.</param>
        public void LogMessage (string txt) {
            // always echo to console if the log stream is null
            if (log == null || echoToConsole) {
                Console.Write (indentString);
                Console.WriteLine (txt);
            }
            if (log != null) {
                log.Write (indentString);
                log.WriteLine (txt);
            }
        }

        /// <summary>
        /// Provides communication with the test suite.  Use this to log
        /// any arbitrary message during any part of your test.  This overload
        /// allows formatting.
        /// </summary>
        /// <param name="text">Message to log.  You may use formats as in String.Format.</param>
        /// <param name="p">Parameters to your message.</param>
        public void LogMessage (string message, params object[] p) {
            LogMessage (String.Format (CultureInfo.InvariantCulture, message, p));
        }
        #endregion

        #region Common verification routines
        /// <summary>
        /// This method finds the given methodName in the type and object
        /// and invokes it with the given parameters.  If the return value of
        /// this call matches the given expected return value, VerifyMethod()
        /// returns true.  False otherwise.  It makes liberal use of LogMessage().
        /// </summary>
        /// <param name="container">Type of the given instance.</param>
        /// <param name="instance">An instantiated object of the given type.</param>
        /// <param name="methodName">The method to find and invoke.</param>
        /// <param name="parameters">The parameters to pass to the method.</param>
        /// <param name="expectedReturn">The expected return value of the method.</param>
        /// <returns>True if verification succeeded.  False otherwise.</returns>
        protected bool VerifyMethod (Type container, object instance, string methodName, object[] parameters, object expectedReturn) {

            if (container == null)
                throw new ArgumentNullException ("container");
            if (instance == null)
                throw new ArgumentNullException ("instance");
            if (methodName == null)
                throw new ArgumentNullException ("methodName");

            LogMessage ("Calling {0}, expecting '{1}' for a return value.", methodName, expectedReturn);

            // output parameters
            if (parameters != null) {
                LogMessage ("with parameters");
                int i = 0;
                LogMessageIndent ();
                foreach (object parameter in parameters) {
                    LogMessage ("{0}: type={1}, value='{2}'", i++, parameter.GetType (), parameter);
                }
                LogMessageUnindent ();
            }

            Type [] typeArray = parameters == null ? new Type[0] :
                System.Type.GetTypeArray (parameters);
            MethodInfo methodInfo = container.GetMethod (methodName, typeArray);

            if (methodInfo == null) {
                LogMessage ("Unable to get " + methodName);
                return false;
            }

            object returnValue = methodInfo.Invoke (instance, parameters == null ?
                    new Object [0] : parameters);

            if (expectedReturn == null && returnValue == null) {
                LogMessage ("Return value was '{0}'", returnValue);
                LogMessage ("");
                return true;
            }

            if (returnValue == null) {
                LogMessage ("Unable to get return value.");
                return false;
            }
            if (returnValue.GetType () != expectedReturn.GetType ()) {
                LogMessage ("Return value is wrong type. Returned type=" +
                    returnValue.GetType ().ToString () + ", expected " +
                    expectedReturn.GetType ().ToString () + ".");
                return false;
            }
            if (!returnValue.Equals (expectedReturn)) {
                LogMessage ("Return value is incorrect. Returned '" +
                    returnValue.ToString () + "', expected '" + expectedReturn.ToString () + "'.");
                return false;
            }

            LogMessage ("Return value was '{0}'", returnValue);
            LogMessage ("");

            return true;
        }

        /// <summary>
        /// This method finds the given propName in the type and object
        /// and sets its value with the given object (setVal).  If the set
        /// succeeds, VerifyPropertySet() returns true.  It makes liberal
        /// use of LogMessage().
        /// </summary>
        /// <param name="container">Type of the given instance.</param>
        /// <param name="instance">An instantiated object of the given type.</param>
        /// <param name="propName">Property name to set.</param>
        /// <param name="setVal">Value to set the property to.</param>
        /// <returns>True if the operation succeeded.  False otherwise.</returns>
        protected bool VerifyPropertySet (Type container, object instance, string propName, object setVal) {

            if (container == null)
                throw new ArgumentNullException ("container");
            if (instance == null)
                throw new ArgumentNullException ("instance");
            if (propName == null)
                throw new ArgumentNullException ("propName");

            PropertyInfo propInfo;

            LogMessage ("Setting property '{0}' to '{1}'.", propName, setVal);
            if (setVal != null && setVal.GetType () != null) {
                propInfo = container.GetProperty(propName, setVal.GetType());
            } else {
                propInfo = container.GetProperty(propName);
            }

            if (propInfo == null) {
                LogMessage ("Unable to get property '{0}' from the assembly.", propName);
                return false;
            }

            propInfo.SetValue(instance, setVal, null);

            LogMessage ("Set.");
            LogMessage ("");
            return true;
        }

        /// <summary>
        /// This method finds the given propName in the type and object
        /// and gets its value in order to compare it with the given object
        /// (expectedReturn).  If they match, VerifyPropertyGet() returns true.
        /// It makes liberal use of LogMessage().
        /// </summary>
        /// <param name="testType">Type of the given instance.</param>
        /// <param name="instance">An instantiated object of the given type.</param>
        /// <param name="propName">Property name to get.</param>
        /// <param name="setVal">Value you expect the property to return.</param>
        /// <returns>True if the operation succeeded.  False otherwise.</returns>
        protected bool VerifyPropertyGet (Type container, object instance, string propName, object expectedReturn) {

            if (container == null)
                throw new ArgumentNullException ("container");
            if (instance == null)
                throw new ArgumentNullException ("instance");
            if (propName == null)
                throw new ArgumentNullException ("propName");

            PropertyInfo propInfo = null;
            LogMessage ("Getting property '{0}' expecting '{1}'.", propName, expectedReturn);
            if (expectedReturn != null && expectedReturn.GetType () != null) {
                propInfo = container.GetProperty (propName, expectedReturn.GetType ());
            } else {
                propInfo = container.GetProperty (propName);
            }

            if (propInfo == null) {
                LogMessage ("Unable to get property '{0}'.", propName);
                return false;
            }

            object retVal = propInfo.GetValue (instance, new object [0]);
            if (retVal == null) {
                LogMessage ("Return value is null.");
                return false;
            }
            if (retVal.GetType () != expectedReturn.GetType ()) {
                LogMessage ("Return value is wrong type. Returned type=" +
                        retVal.GetType ().ToString () + ", expected " + expectedReturn.GetType ().ToString () + ".");
                return false;
            }
            if (!retVal.Equals (expectedReturn)) {
                LogMessage ("Return value is incorrect. Returned '" + retVal.ToString () + "', expected '" + expectedReturn.ToString () + "'.");
                return false;
            }
            LogMessage ("Return value was '{0}'.", retVal);
            LogMessage ("");
            return true;
        }

        /// <summary>
        /// Verifies that the given exception (expectedException) is thrown
        /// when the given methodName is called on the instance which is
        /// of type container with the parameters.  True is returned if this
        /// is the case.  False otherwise.
        /// </summary>
        /// <param name="container">The type in which to find the method.</param>
        /// <param name="instance">An instantiated object of the given type.</param>
        /// <param name="methodName">The name of the method to find.</param>
        /// <param name="parameters">The parameters to pass to the method.</param>
        /// <param name="expectedException">The expected exception to be thrown.</param>
        /// <returns>True if the exception was thrown.  False otherwise.</returns>
        protected bool VerifyException (Type container, object instance, string methodName, object[] parameters, object expectedException) {

            if (container == null)
                throw new ArgumentNullException ("container");
            if (instance == null)
                throw new ArgumentNullException ("instance");
            if (methodName == null)
                throw new ArgumentNullException ("methodName");
            
            LogMessage ("Calling {0} and expecting {1} to be thrown.", methodName, expectedException.GetType ());
            MethodInfo methodInfo = container.GetMethod (methodName);
            if (methodInfo == null) {
                LogMessage ("Unable to get " + methodName);
                return false;
            }

            try {
                object returnValue = methodInfo.Invoke (instance, parameters);
            }
            catch (Exception e) {
                if (e.GetType () != expectedException.GetType ()) {
                    LogMessage ("Return value type is incorrect.  Returned " + e.GetType ().ToString ());
                    return false;
                }
            }
            LogMessage ("Exception was thrown as expected.");
            LogMessage ("");
            return true;
        }

        /// <summary>
        /// Used for attribute verification.  This method finds the given type
        /// and attempts to retrieve the value of the property specified in
        /// propertyName.  It compares this value with the given propertyValue
        /// and returns true if they match.  False is otherwise returned.  The
        /// type is searched for in the given attributes array.
        /// </summary>
        /// <param name="attributes">Array of objects in which to find the type.</param>
        /// <param name="type">The type to find.</param>
        /// <param name="propertyName">The property to validate against.</param>
        /// <param name="propertyValue">The expected value of this property.</param>
        /// <returns></returns>
        protected bool VerifyAttribute (object[] attributes, Type type, string propertyName, object propertyValue) {
            if (attributes == null)
                throw new ArgumentNullException ("attributes");
            if (type == null)
                throw new ArgumentNullException ("type");
            if (propertyName == null)
                throw new ArgumentNullException ("propertyName");

            LogMessage ("Attempting to find attribute " + type.Name);
            foreach (object attr in attributes) {
                if (attr.GetType () == type) {
                    object retVal = type.InvokeMember (propertyName, BindingFlags.GetProperty, null, attr, null, CultureInfo.InvariantCulture);
                    if (retVal == null) {
                        LogMessage ("Unable to get return value.");
                        return false;
                    }
                    if (retVal.GetType () != propertyValue.GetType ()) {
                        LogMessage ("Return value is wrong type. Returned type=" +
                                retVal.GetType ().ToString () + ", expected " + propertyValue.GetType ().ToString () + ".");
                        return false;
                    }
                    if (!retVal.Equals (propertyValue)) {
                        LogMessage ("Return value is incorrect. Returned '" +
                                retVal.ToString () + "', expected '" + propertyValue.ToString () + "'.");
                        return false;
                    }
                    LogMessage ("Found attribute: " + type.Name);
                    LogMessage ("");
                    return true;
                }
            }
            LogMessage ("Unable to find attribute: " + type.Name);
            LogMessage ("");
            return false;
        }

        /// <summary>
        /// Finds and instantiates the given type name in the assembly.  Both the
        /// type and instantiated object are out parameters.  If the type can't be
        /// found or instantiated, false is returned.
        /// </summary>
        /// <param name="typeName">Type to find in the given assembly.</param>
        /// <param name="typeAssembly">Assembly to search through for the given type name.</param>
        /// <param name="obj">Instantiated object of the given type.</param>
        /// <param name="genType">The type that was found in the assembly.</param>
        /// <returns>True if the type was found and instantiated, false otherwise.</returns>
        protected bool FindAndInstantiate (string typeName, Assembly typeAssembly, out object obj, out Type genType) {

            if (typeAssembly == null)
                throw new ArgumentNullException ("typeAssembly");

            obj = null;
            if (FindType (typeName, typeAssembly, out genType)) {
                LogMessage ("Instantiating type '{0}'.", typeName);
                new ReflectionPermission (ReflectionPermissionFlag.NoFlags).Demand ();
                obj = Activator.CreateInstance (genType);
                if (obj == null) {
                    LogMessage ("Unable to instantiate {0}", typeName);
                    return false;
                }
                LogMessage ("Instantiated.");
                LogMessage ("");
                return true;
            }

            return false;
        }
        /// <summary>
        /// Finds the given type name in the assembly.  The type is an out parameter.
        /// If the type can't be found, false is returned.
        /// </summary>
        /// <param name="typeName">Type to find in the given assembly.</param>
        /// <param name="typeAssembly">Assembly to search through for the given type name.</param>
        /// <param name="genType">The type that was found in the assembly.</param>
        /// <returns>True if the type was found, false otherwise.</returns>
        protected bool FindType (string typeName, Assembly typeAssembly, out Type genType) {
            if (typeAssembly == null)
                throw new ArgumentNullException ("typeAssembly");
            genType = null;
            LogMessage ("Finding type '{0}' in the given assembly.", typeName);
            genType = typeAssembly.GetType (typeName);
            if (genType == null) {
                LogMessage ("Unable to get type {0}.", typeName);
                return false;
            }
            LogMessage ("Found.");
            LogMessage ("");
            return true;
        }
        #endregion
    }
}

