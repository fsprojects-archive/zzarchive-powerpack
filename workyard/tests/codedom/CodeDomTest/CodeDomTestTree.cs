using System;
using System.IO;
using System.CodeDom;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace Microsoft.Samples.CodeDomTestSuite {
    /// <summary>
    /// Marks incorrect usage of scenario management (AddScenario()/VerifyScenario()).
    /// </summary>
    [Serializable]
    public class ScenarioException : Exception {
        public ScenarioException() {}
        public ScenarioException(string s) : base (s) {}
        public ScenarioException(string s, Exception innerException) : base (s, innerException) {}
        protected ScenarioException (SerializationInfo info, StreamingContext context) : base(info, context) {}
    }

    /// <summary>
    /// Base class for all CodeDom test cases.  You must inherit from this class
    /// if you are writing a test case for the CodeDom test suite.
    /// </summary>
    public abstract class CodeDomTestTree : CodeDomTest {
        // path to save source and assembly output from the compiler
        // if specified
        string saveSourceFileName = String.Empty;
        string saveAssemblyFileName = String.Empty;

        // used for scenario management (verification) of this test
        // case
        ScenarioCollection scenarios = new ScenarioCollection();

        /// <summary>
        /// A test case that has this set to true and does not compile will be marked
        /// as failed.
        /// </summary>
        /// <value>Whether or not the CodeDom tree should be compiled.</value>
        public virtual bool ShouldCompile {
            get {
                return true;
            }
        }

        /// <summary>
        /// The VerifyAssembly() method of this test case will be called depending on
        /// this value.
        /// </summary>
        /// <value>Whether or not assembly verification should take place.</value>
        public virtual bool ShouldVerify {
            get {
                return false;
            }
        }

        /// <summary>
        /// The Search() method will be called depending on this value.
        /// </summary>
        /// <value>Whether or not the source code search should take place.</value>
        public virtual bool ShouldSearch {
            get {
                return false;
            }
        }

        /// <summary>
        /// Compiler parameters that the test suite should use to compile the generated
        /// source code.
        /// </summary>
        /// <param name="provider">Used to determine if certain compiler switches
        /// should be on or off (i.e. based on GeneratorSupport).</param>
        /// <returns>The compiler parameters with which the generated code should be
        /// compiled.</returns>
        public virtual CompilerParameters GetCompilerParameters (CodeDomProvider provider) {
            CompilerParameters parms = new CompilerParameters (new string[] {
                "System.dll", "System.Windows.Forms.dll", "System.Data.dll",
                "System.Drawing.dll", "System.Xml.dll"});
            parms.GenerateExecutable = false;
            parms.TreatWarningsAsErrors = true;
            parms.IncludeDebugInformation = true;
            parms.TempFiles.KeepFiles = false;

            return parms;
        }

        /// <summary>
        /// Generator options that the test suite should use to generate the
        /// source code.
        /// </summary>
        /// <param name="provider">Used to determine if certain compiler switches
        /// should be on or off (i.e. based on GeneratorSupport).</param>
        /// <returns>The generator options with which the code should be
        /// generated.</returns>
        public virtual CodeGeneratorOptions GetGeneratorOptions (CodeDomProvider provider) {
            CodeGeneratorOptions genOptions = new CodeGeneratorOptions ();
            return genOptions;
        }

        /// <summary>
        /// Save any generated source to this file.
        /// </summary>
        /// <value>The string value that points to the file to write
        /// to.  Will default to writing to the local directory if no
        /// path is specified in the string.</value>
        public string SourceFileName {
            set { saveSourceFileName = value; }
            get { return saveSourceFileName; }
        }

        /// <summary>
        /// Write to the given assembly file name.
        /// </summary>
        /// <value>The string that points to the assembly file name
        /// you would like the compiled code to be saved to.</value>
        public string AssemblyFileName {
            set { saveAssemblyFileName = value; }
            get { return saveAssemblyFileName; }
        }

        /// <summary>
        /// Clears all added scenarios.
        /// </summary>
        public void ClearScenarios () {
            scenarios.Clear ();
        }

        /// <summary>
        /// This overrides the CodeDomTest Run method that does verification
        /// on the tree provided in the BuildTree method you provide.
        /// </summary>
        /// <param name="provider">Provider to test.</param>
        /// <returns>True if the tree builds, compiles, searches and passes
        /// assembly verification.  False if any of these fails.</returns>
        public override bool Run (CodeDomProvider provider) {
            bool fail = false;

            // build the tree
            LogMessageIndent ();
            LogMessage ("- Generating tree.");
            CodeCompileUnit cu = new CodeCompileUnit ();
            LogMessageIndent ();
            BuildTree (provider, cu);
            LogMessageUnindent ();

            // validate tree using 'experimental' subset tester
            // but only if the test believes its in the subset
            if ((TestType & TestTypes.Subset) != 0) {
                SubsetConformance subsConf = new SubsetConformance ();
                LogMessage ("- Checking tree subset conformance.");
                if (!subsConf.ValidateCodeCompileUnit (cu))
                    LogMessage ("Failed subset tester: {0}",
                            subsConf.ToString ());
            }

            // realize source
            StringWriter sw = new StringWriter (CultureInfo.InvariantCulture);
#if WHIDBEY
            provider.GenerateCodeFromCompileUnit (cu, sw, GetGeneratorOptions (provider));
#else
            ICodeGenerator generator = provider.CreateGenerator ();
            generator.GenerateCodeFromCompileUnit (cu, sw, GetGeneratorOptions (provider));
#endif

            // only continue if the source could be realized into a string.
            if (!fail) {
                string source = sw.ToString ();

                if (saveSourceFileName.Length > 0) {
                    LogMessage ("- Saving source into '" + saveSourceFileName + "'");

                    // save this source to a file
                    DumpStringToFile (source, saveSourceFileName);
                }

                // log the source code
                //LogMessage (source);

                // search the source if the test case asks us to
                if (ShouldSearch) {
                    LogMessageIndent ();
                    Search (provider, source);
                    LogMessageUnindent ();
                }
                
                // continue only if the test case wants to compile or verify
                if (ShouldCompile || ShouldVerify) {

                    // ask the test case which compiler parameters it would like to use
                    CompilerParameters parms = GetCompilerParameters (provider);

#if FSHARP
                    // If the generated code has entrypoint, then F# requires us to generate EXE
                    bool hasEntryPoint = false;
                    foreach(CodeNamespace ns in cu.Namespaces)
                        foreach (CodeTypeDeclaration ty in ns.Types)
                            foreach(CodeTypeMember mem in ty.Members)
                                if (mem is CodeEntryPointMethod) { hasEntryPoint = true; }

                    // If the output file name is specified then it should be EXE
                    if (hasEntryPoint && parms.GenerateExecutable == false)
                    {
                        parms.GenerateExecutable = true;
                        if (saveAssemblyFileName.ToLower().EndsWith(".dll"))
                            saveAssemblyFileName = saveAssemblyFileName.Substring(0, saveAssemblyFileName.Length - 4) + ".exe";
                    }
#endif
                    
                    // add the appropriate compiler parameters if the user asked us
                    // to save assemblies to file
                    if (saveAssemblyFileName.Length > 0) {
                        parms.OutputAssembly = saveAssemblyFileName;
                        LogMessage ("- Compiling to '" + saveAssemblyFileName + "'.");
                    }

                    // always generate in memory for verification purposes
                    parms.GenerateInMemory = true;

                    // compile!
#if WHIDBEY
                    CompilerResults results = provider.CompileAssemblyFromDom (parms, cu);
#else
                    ICodeCompiler compiler = provider.CreateCompiler ();
                    CompilerResults results = compiler.CompileAssemblyFromDom (parms, cu);
#endif

                    if (results.NativeCompilerReturnValue != 0) {
                        // compilation failed
                        fail = true;
                        LogMessage ("- Compilation failed.");
                        
                        // log the compilation failed output
                        foreach (string msg in results.Output)
                            LogMessage (msg);

                    } else if (ShouldVerify) {
                        // compilation suceeded and we are asked to verify the
                        // compiled assembly
                        LogMessage ("- Verifying assembly.");

                        // verify the compiled assembly if it's there
                        if (results.CompiledAssembly != null) {
                            LogMessageIndent ();
                            VerifyAssembly (provider, results.CompiledAssembly);
                            LogMessageUnindent ();
                        }
                    }
                }
            }

            if (fail || !AreAllValidated ()) {
                // one of the steps above failed or a scenario was not
                // verified within the test case
                fail = true;
                LogMessage ("! Test '" + Name + "' failed.");

                // output failing scenarios
                if (!AreAllValidated()) {
                    LogMessage ("! Failing scenarios:");
                    foreach (Scenario s in GetNotValidated())
                    {
                        LogMessage ("-  " + s.ToString());
                    }
                }
            } else {
                // test passed
                LogMessage ("* Test '" + Name + "' passed.");
            }
            LogMessageUnindent ();

            // return true on success, false on failure
            return !fail;
        }

        /// <summary>
        /// Takes the given provider and generates a tree in the given CodeCompileUnit.
        /// This is where the bulk of your test case should reside.  You may use AddScenario()
        /// as you build your tree to enable scenario verification.
        /// </summary>
        /// <param name="provider">The provider that will be used to generate and
        /// compile the tree.</param>
        /// <param name="cu">The compile unit on which to build your tree.</param>
        public abstract void BuildTree (CodeDomProvider provider, CodeCompileUnit cu);

        /// <summary>
        /// Runs verification of the compiled source code emitted by the provider.  The
        /// source code is generated from the tree you built in BuildTree().  You should
        /// use VerifyScenario() to verify any scenarios defined in BuildTree().  You may
        /// also add new scenarios as long as they are also verified by the end of
        /// VerifyAssembly()'s execution.  Refer to the accompanying documentation for
        /// more information on how to write a test case.
        /// </summary>
        /// <param name="provider">The provider that was used to compile the given
        /// assembly.</param>
        /// <param name="asm">The assembly compiled by the given provider.</param>
        public abstract void VerifyAssembly (CodeDomProvider provider, Assembly builtAssembly);

        /// <summary>
        /// Searches the given code output as you define.  The given string will
        /// be the source code emitted by the given provider that was generated
        /// from the tree defined in BuildTree().  You should use VerifyScenario()
        /// as in VerifyAssembly() to match any successful scenarios.
        /// </summary>
        /// <param name="provider">The provider used to generate the given
        /// output code.</param>
        /// <param name="output">The code generated from the given provider using
        /// the tree defined in BuildTree().</param>
        public virtual void Search (CodeDomProvider provider, String output) {}

        /// <summary>
        /// Adds a scenario to this test case's scenario collection.  The
        /// key must be unique among this collection otherwise a ScenarioException
        /// will be thrown.
        /// </summary>
        /// <param name="key">A unique key.</param>
        /// <param name="description">Short description of the scenario.</param>
        protected void AddScenario(string key, string description) {
            if (!scenarios.Contains(key))
                scenarios.Add(key, description);
            else
                throw new ScenarioException(String.Format(CultureInfo.InvariantCulture,
                            "Key {0} already exists.", key));
        }

        /// <summary>
        /// Adds a scenario to this test case's scenario collection.  The
        /// key must be unique among this collection otherwise a ScenarioException
        /// will be thrown.
        /// </summary>
        /// <param name="key">A unique key.</param>
        protected void AddScenario(string key) {
            if (!scenarios.Contains(key))
                scenarios.Add(key);
            else
                throw new ScenarioException(String.Format(CultureInfo.InvariantCulture,
                            "Key {0} already exists.", key));
        }

        /// <summary>
        /// Marks the scenario with the given key as validated.  If no scenario
        /// is found with the given key an exception is thrown.  Also, if the
        /// given scenario key was already validated an exception is thrown.
        /// </summary>
        /// <param name="key">Scenario key to validate.</param>
        protected void VerifyScenario(string key) {
            bool found = false;
            bool alreadyValidated = false;

            foreach (Scenario scenario in scenarios) {
                if (scenario.KeyMatch(key)) {

                    if (!scenario.Validated) {
                        scenario.Validated = true;
                        found = true;
                    }
                    else
                        alreadyValidated = true;

                    break;
                }
            }

            if (alreadyValidated)
                throw new ScenarioException(String.Format(CultureInfo.InvariantCulture,
                            "Scenario {0} was already validated.", key));

            if (!found)
                throw new ScenarioException(String.Format(CultureInfo.InvariantCulture,
                            "Scenario {0} not found.", key));
        }

        /// <summary>
        /// If all scenarios in the scenario collection are validated this method
        /// returns true.
        /// </summary>
        /// <returns>True if all scenarios have been validated, false otherwise.</returns>
        bool AreAllValidated() {
            bool allValidated = true;

            foreach (Scenario scenario in scenarios) {
                if (!scenario.Validated)
                    allValidated = false;
            }

            return allValidated;
        }

        /// <returns>The scenarios that have not been validated in this test case.</returns>
        ScenarioCollection GetNotValidated() {
            ScenarioCollection unvalidated = new ScenarioCollection();

            foreach (Scenario scenario in scenarios) {
                if (!scenario.Validated)
                    unvalidated.Add(scenario);
            }

            return unvalidated;
        }

        // Scenario classes (scenario class, exception, collection)
        #region Scenario management classes

        /// <summary>
        /// Scenario type.  Describes a scenario by key, description and validity
        /// flags.  The key of the scenario is the most important, and can be
        /// any arbitrary string that is not already in the test case's scenario
        /// collection.  The test case developer should choose this key carefully
        /// so they can easily search to it when they audit their code.
        /// </summary>
        class Scenario {
            string key;
            string description;
            bool   validated;

            /// <summary>
            /// Creates a new scenario with the given key.  Description is
            /// set to the empty string, and is marked as not valid.
            /// </summary>
            /// <param name="key">Key to give the scenario.</param>
            public Scenario(string key) {
                if (key == null)
                    throw new ArgumentNullException("key");

                this.key = key;
                this.description = String.Empty;
                this.validated = false;
            }

            /// <summary>
            /// Creates a new scenario with the given key and description.
            /// It is marked as not valid, by default.
            /// </summary>
            /// <param name="key">Key to give the scenario.</param>
            /// <param name="description">Description that describes the scenario.</param>
            public Scenario(string key, string description) {
                if (key == null)
                    throw new ArgumentNullException("key");
                if (description == null)
                    throw new ArgumentNullException("description");

                this.key = key;
                this.description = description;
                this.validated = false;
            }

            /// <value>Unique (among others in its collection) key of this scenario.</value>
            public string Key {
                get { return key; }
            }

            /// <value>Quick description of this scenario to enable the user to get more
            /// information about what this scenario is about.</value>
            public string Description
            {
                get { return description; }
            }

            /// <value>True if this scenario has been validated.  False otherwise.</value>
            public bool Validated {
                get { return validated; }
                set { validated = value; }
            }

            /// <summary>
            /// Utility function that compares the given string with the internal key.  It
            /// uses the invariant culture to do a <b>case-insensitive</b> comparison.
            /// </summary>
            /// <param name="outKey">String to match to the key.</param>
            /// <returns>If the key matches the given string, true is returned.  False otherwise</returns>
            public bool KeyMatch(string outKey) {
                return String.Compare(outKey, key, true, CultureInfo.InvariantCulture) == 0;
            }

            /// <returns>Key along with the description of this scenario in a human
            /// readable format.  No valdiation information is returned.</returns>
            public override string ToString() {
                return String.Format(CultureInfo.InvariantCulture, "{0}{1}", key,
                        (description == null || description.Length <= 0) ?
                            String.Empty :
                            (": " + description));
            }
        }

        /// <summary>
        /// Strongly-typed collection for Scenario objects.
        /// </summary>
        class ScenarioCollection : CollectionBase {
            public ScenarioCollection() {
            }

            public bool Contains(string key) {
                bool cont = false;
                foreach (Scenario s in InnerList)
                    if (s.KeyMatch (key)) {
                        cont = true;
                        break;
                    }
                return cont;
            }

            public ScenarioCollection (ScenarioCollection sc) {
                foreach (Scenario s in sc)
                    InnerList.Add (s);
            }

            public void Add (string key, string description) {
                InnerList.Add (new Scenario(key, description));
            }

            public void Add (string key) {
                InnerList.Add (new Scenario(key));
            }

            public void Add (Scenario s) {
                InnerList.Add (s);
            }
        }
        #endregion
    }
}
