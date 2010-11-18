using System;
using System.IO;
using System.Text;
using System.CodeDom;
using System.Security;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Configuration;
using System.Globalization;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Microsoft.JScript;

[assembly:ComVisible (false)]
[assembly:CLSCompliantAttribute (true)]
[assembly:FileIOPermission (SecurityAction.RequestMinimum)]
namespace Microsoft.Samples.CodeDomTestSuite {

#if WHIDBEY
    public static class Program {
#else
    public class Program {
#endif

        // returns true if an intformational option
        // was printed
        static bool PrintInformationalOptions (TestTypes setType) {

            // only print help if it is specified
            if (GetOption ("help").Set) {
                PrintUsage (true);
                return true;
            }

            // list tests
            if (GetOption ("list").Set) {
                PrintTestList (setType, false);
                return true;
            } else if (GetOption ("listdesc").Set) {
                PrintTestList (setType, true);
                return true;
            }
            
            // list valid sets
            if (GetOption ("setlist").Set) {
                Console.WriteLine ("Sets of tests:");
                foreach (TestTypeOption opt in testTypeOptions) {
                    Console.WriteLine ("-  " + opt.Name);
                }
                return true;
            }

#if !WHIDBEY
            if (GetOption ("languagename").Set) {
                // lang is not supported in non-Whidbey environements
                ErrorWriteLine ("The /languagename (short: /lang) option was not compiled into this assembly, ");
                ErrorWriteLine ("and is not supported.  Did you compile with /d:WHIDBEY?");
                return true;
            }
#endif

            // check for required options
            if ((!GetOption ("codedomprovider").Set || !GetOption ("languagename").Set) &&
                    !GetOption ("testcaselib").Set) {
                PrintUsage (false);
                ErrorWriteLine ();
                ErrorWriteLine ("Missing required options.");
                return true;
            }

            return false;
        }

        static void ProcessOptionFile () {
            string currentFile = String.Empty;

            // attempt reading the specified configuration file(s)
            foreach (string file in GetOption ("optionfile").Values) {
                currentFile = file;
                using (StreamReader rdr = new StreamReader (currentFile)) {
                    string    line  = null;
                    ArrayList lines = new ArrayList ();

                    while ((line = rdr.ReadLine ()) != null)
                        lines.Add (line);

                    if (!ParseArgs ((string[]) lines.ToArray (typeof (string)), true)) {
                        throw new InvalidOperationException (String.Format (CultureInfo.CurrentCulture,
                                    "Invalid options specified in '{0}'.", currentFile));
                    }
                }
            }
        }

        // process the -set option
        static TestTypes ProcessSetOption () {
            TestTypes setType = TestTypes.None;
            if (GetOption ("set").Set) {

                // get the TestTypeOption as described by the command-line argument
                // -set
                string option = GetOption("set").GetSingleValue ();
                TestTypeOption opt = null;

                foreach (TestTypeOption ttopt in testTypeOptions) {
                    if (ttopt.IsSameName (option))
                        opt = ttopt;
                }

                if (opt == null)
                    throw new InvalidOperationException (String.Format (CultureInfo.CurrentCulture,
                                "Invalid set '{0}' specified.", GetOption ("set").GetSingleValue ()));
                
                // determine version compatibility
                if (!opt.IsVersionCompatible (Environment.Version)) {
                    ErrorWriteLine ("Given set of tests may not function properly because your current");
                    ErrorWriteLine ("runtime version {0} is less than the tests' required runtime {1}.",
                            Environment.Version, opt.RuntimeVersion);
                }

                setType = opt.Types;
            }

            return setType;
        }

        [STAThread]
        public static void Main (string[] args) {

            // parse args
            if (ParseArgs (args, false)) {

                #region Option handling
                TestTypes setType = TestTypes.None;

                try {
                    setType = ProcessSetOption ();
                } catch (InvalidOperationException e) {
                    ErrorWriteLine (e.Message);
                    Environment.ExitCode = 1;
                    return;
                }

                if (GetOption ("optionfile").Set) {
                    try {
                        ProcessOptionFile ();
                    } catch (Exception e) {
                        ErrorWriteLine ("Error reading option files: {0}", e.Message);
                        Environment.ExitCode = 1;
                        return;
                    }
                }

                if (PrintInformationalOptions (setType)) {
                    Environment.ExitCode = 1;
                    return;
                }

                #endregion

                #region Load test CodeDomProvider assemblies
                Assembly  cdpAssembly    = null;
                ArrayList testAssemblies = null;

                try {
                    testAssemblies = LoadTestAssemblies ();
                } catch (Exception e) {
                    ErrorWriteLine ("Couldn't load test case assembly!");
                    Console.WriteLine ("Exception stack: {0}", e.ToString ());
                    Environment.ExitCode = 1;
                    return;
                }

                try {
                    cdpAssembly = LoadProviderAssembly ();
                } catch (Exception e) {
                    ErrorWriteLine ("Couldn't load provider assembly '{0}': {1}",
                        GetOption ("codedomproviderlib").GetSingleValue (), e.ToString ());
                    Environment.ExitCode = 1;
                    return;
                }

                if ((cdpAssembly == null && !GetOption("languagename").Set) || testAssemblies == null) {
                    ErrorWriteLine ("Couldn't load provider assembly and/or test assemblies.  Quitting.");
                    Environment.ExitCode = 1;
                    return;
                }
                #endregion

                #region Create save directory if specified
                // defaults to .\testoutput\
                string dir = "testoutput";
                if (GetOption ("savedirectory").Set) {
                    dir = GetOption ("savedirectory").GetSingleValue ();
                }
                dir = String.Format (CultureInfo.InvariantCulture, 
                        @"{0}\{1}", Environment.CurrentDirectory, dir);

                try {
                    if (!Directory.Exists (dir))
                        Directory.CreateDirectory (dir);
                } catch (Exception) {
                    ErrorWriteLine ("Couldn't create directory '{0}'.", dir);
                    Environment.ExitCode = 1;
                    return;
                }
                #endregion

                #region Find and create an instance of the given code provider
                CodeDomProvider cdp = null;

#if WHIDBEY
                // first try to use the given string as an argument to CodeDomProvider.CreateProvider
                // but only if we're being built in Whidbey
                if (GetOption ("languagename").Set) {
                    string lname = GetOption ("languagename").GetSingleValue ();
                    try {
                        if (lname != null)
                            cdp = CodeDomProvider.CreateProvider (lname);
                    } catch (Exception e) {
                        cdp = null;
                        ErrorWriteLine ("Tried to use CodeDomProvider.CreateProvider() to get '{0}', but failed.",
                                lname);
                        Console.WriteLine ("Exception stack: {0}", e.ToString ());
                    }
                }
#endif

                if (cdp == null && GetOption ("codedomprovider").Set) {
                    string typeName = GetOption ("codedomprovider").GetSingleValue ();
                    // try with the given assembly
                    Type cdpType = cdpAssembly.GetType (typeName, false, true);

                    if (cdpType != null) {
                        if (!cdpType.IsSubclassOf (typeof (System.CodeDom.Compiler.CodeDomProvider))) {
                            ErrorWriteLine ("{0} must be a subclass of CodeDomProvider.", typeName);
                            Environment.ExitCode = 1;
                            return;
                        }

                        new ReflectionPermission (ReflectionPermissionFlag.NoFlags).Demand ();
                        cdp = (CodeDomProvider) cdpAssembly.CreateInstance (typeName, false);
                    } else {
                        Console.WriteLine ("Couldn't find '{0}' in assembly \"{1}\".",
                            typeName, cdpAssembly);
                    }
                }

                if (cdp == null) {
                    ErrorWriteLine ("Couldn't create provider '{0}'.  Make sure ", GetOption ("codedomprovider").GetSingleValue ());
                    ErrorWriteLine ("you supply the full type name including namespace.");
                    Environment.ExitCode = 1;
                    return;
                }
                #endregion

                // load tests
                ArrayList runTests = null;

                try {
                    runTests = LoadTests (testAssemblies, setType);
                } catch (InvalidOperationException e) {
                    ErrorWriteLine ();
                    ErrorWriteLine (e.Message);
                    Environment.ExitCode = 1;
                    return;
                }

                if (runTests == null) {
                    ErrorWriteLine ();
                    ErrorWriteLine ("Error trying to load tests.");
                    Environment.ExitCode = 1;
                    return;
                }

                #region Run tests
                // run tests
                StringCollection failedTests = new StringCollection ();
                foreach (CodeDomTest test in runTests) {
                    try {
                        if (RunTest(cdp, test, dir))
                        {
                            if (test.Comment != "")
                                Console.WriteLine("{0} is now passing - well done!", test.Name);
                        }
                        else
                            failedTests.Add(string.Format("{0} ({1})", test.Name, test.Comment == "" ? "TEST FAILED" : test.Comment));

                    } catch (Exception e) {
                        ErrorWriteLine ("Caught '{0}' while running test '{3}'{1}{2}",
                                e.GetType(), Environment.NewLine, e.ToString (), test.Name);
												failedTests.Add(string.Format("{0}\t\t({1})", test.Name, test.Comment == "" ? "TEST FAILED" : test.Comment));
                    }
                }
                #endregion

                // print the summary information if there were >1 tests run
                if (runTests.Count > 1) {
                    Console.WriteLine ();
                    Console.WriteLine ("Ran {0} total tests.", runTests.Count);
                    Console.WriteLine ("    {0} failed", failedTests.Count);
                    Console.WriteLine ("    {0} passed", runTests.Count - failedTests.Count);
                }

                Console.WriteLine ();
                if (failedTests.Count > 0) {
                    if (failedTests.Count > 1) {
                        Console.WriteLine ("We have the following failures in CodeDom tests. If a comment is shown then these are expected (CodeDom support for F# is not fully complete):");

                        foreach (string failedTest in failedTests) {
                            Console.WriteLine (" - {0}", failedTest);
                        }

                        Console.WriteLine ();
                        Console.WriteLine ("You can find the log, source, and/or assembly files in the directory:");
                        Console.WriteLine ("  " + dir);
                        Console.WriteLine ("Each log, source or assembly file is called <testname>.{log|src|dll}, ");
                        Console.WriteLine ("respectively.");
                    } else {
                        string failedTest = failedTests[0];
                        Console.WriteLine ("{0} failed.", failedTest);
                        Console.WriteLine (" Log file: {0}", dir + "\\" + failedTest + ".log");
                        if (File.Exists (dir + "\\" + failedTest + ".src.fs"))
                            Console.WriteLine (" Source file: {0}", dir + "\\" + failedTest + ".src.fs");
                    }

                    Console.WriteLine ();

                    Environment.ExitCode = 1;
                    Console.WriteLine ("FAILED");
                } else {
                    Environment.ExitCode = 100;
                    Console.WriteLine ("PASSED");
                }
            } else {
                PrintUsage (false);
            }
        }

        // Load tests from the test case assemblies
        static ArrayList LoadTests (ArrayList testAssemblies, TestTypes setType) {
            // determine specific tests to run from the commandline
            // if none are given, list.Count=0 signals to run them all
            ArrayList runTests = new ArrayList ();
            StringCollection specTests = GetOption ("runtestcase").Values;
            StringCollection dontRun = GetOption ("dontruntestcase").Values;

            StringCollection duplicateCheck = new StringCollection ();

            // while looping through the assemblies, this will reflect
            // whether or not a certain test was found
            BitArray specTestFound = new BitArray (specTests.Count);

            // check to see if there are any tests that were both
            // specified to run and specified NOT to run
            foreach (string specTest in specTests) {
                if (StringCollectionContainsIgnoreCase (dontRun, specTest)) {
                    throw new InvalidOperationException (String.Format (CultureInfo.CurrentCulture,
                                "Test '{0}' was both specified to run and *not* to run. " +
                            "This is ambiguous. Giving up.", specTest));
                }
            }

            foreach (Assembly asm in testAssemblies) {
                bool foundAtLeastOneTest = false;

                foreach (Type type in asm.GetTypes ()) {
                    if (!type.Equals (typeof (CodeDomTest)) &&
                            !type.Equals (typeof (CodeDomTestTree)) &&
                            type.IsSubclassOf (typeof (CodeDomTest))) {
                        CodeDomTest test = null;
                        try {
                            ConstructorInfo cons = type.GetConstructor (new Type[0]);
                            test = (CodeDomTest) cons.Invoke (new Object[0]);
                        } catch (Exception e) {
                            throw new InvalidOperationException (String.Format (CultureInfo.CurrentCulture,
                                        "Couldn't create instance of '{0}': {1}", type, e.Message));
                        }

                        // check for duplicate names
                        if (test != null && StringCollectionContainsIgnoreCase (duplicateCheck, test.Name)) {
                            throw new InvalidOperationException (String.Format (CultureInfo.CurrentCulture,
                                        " *** Found a duplicate test name '{0}' among test case assemblies. Check your test cases.", test.Name));
                        } else if (test != null) {
                            duplicateCheck.Add (test.Name);
                        }

                        // if we got here, we found at least one type that inherits
                        // from CodeDomTest and we were able to instantiate it
                        if (test != null)
                            foundAtLeastOneTest = true;

                        // if certain tests were specified attempt to find them
                        // NOTE: All test cases that match the given name will be
                        //       picked up for running.  This includes test cases
                        //       that share the same name
                        if (test != null && (GetOption ("set").Set ?
                                (test.TestType & setType) > 0 : true) &&
                                !StringCollectionContainsIgnoreCase (dontRun, test.Name)) {
                            if (specTests.Count > 0) {

                                int index = -1;
                                for (int i = 0; i < specTests.Count; i++)
                                    if (String.Compare ((string) specTests[i], test.Name, true, CultureInfo.InvariantCulture) == 0) {
                                        index = i;
                                        break;
                                    }

                                if (index >= 0) {
                                    specTestFound[index] = true;
                                    runTests.Add (test);
                                }
                            } else {
                                // run them all
                                runTests.Add (test);
                            }
                        }
                    }
                }

                if (!foundAtLeastOneTest) {
                    ErrorWriteLine ("Couldn't find at least one type that inherits from CodeDomTest in");
                    ErrorWriteLine (asm.CodeBase);
                    ErrorWriteLine ("Make sure your test case assembly references the same CodeDomTest.dll");
                    ErrorWriteLine ("that the current program ({0}) does.", (Environment.GetCommandLineArgs ())[0]);
                    ErrorWriteLine ("Continuing...");
                }
            }

            if (specTests.Count > 0) {
                // check if any specified tests weren't found
                for (int i = 0; i < specTestFound.Count; i++) {
                    if (!specTestFound[i]) {
                        PrintTestList (setType, false);
                        if (GetOption ("set").Set) {
                            throw new InvalidOperationException (String.Format (CultureInfo.CurrentCulture,
                                        "Can't find test '{0}' in the set '{1}'." +
                                        " Above is the list of available test cases.",
                                        specTests[i], GetOption ("set").GetSingleValue ()));
                        } else {
                            throw new InvalidOperationException (String.Format (CultureInfo.CurrentCulture,
                                        "Can't find test '{0}' in any test case assemblies. " +
                                        " Above is the list of available test cases.", specTests[i]));
                        }
                    }
                }
            }

            return runTests;
        }

        static void PrintTestList (TestTypes setType, bool printDescriptions) {
            // check for required option
            if (!GetOption ("testcaselib").Set) {
                ErrorWriteLine ("You must specify a test case library to list available tests.");
                ErrorWriteLine ();
                return;
            }

            if (GetOption ("set").Set)
                Console.WriteLine ("Available tests in {0}:", setType);
            else
                Console.WriteLine ("Available tests:");

            if (!printDescriptions)
                Console.WriteLine (" (Use -listdesc to get a full description for each test.)");

            Console.WriteLine ();

            foreach (Assembly asm in LoadTestAssemblies ()) {
                bool foundAtLeastOne = false;

                foreach (Type type in asm.GetTypes ())
                    if (type.IsSubclassOf (typeof (CodeDomTest))) {

                        // instantiate the test to get the name, description
                        // and type
                        CodeDomTest test = null;
                        try {
                            ConstructorInfo cons = type.GetConstructor (new Type[0]);
                            test = (CodeDomTest) cons.Invoke (new Object[0]);
                        } catch (Exception e) {
                            ErrorWriteLine ("Couldn't create instance of '{0}'.", type);
                            ErrorWriteLine ("Exception stack: {0}", e.ToString ());
                            return;
                        }

                        foundAtLeastOne = true;

                        if (test != null && (GetOption ("set").Set ? (test.TestType & setType) > 0 : true))
                            if (printDescriptions)
                                Console.WriteLine (" {0}{1}   * Set: {3}{1}    {2}{1}", test.Name,
                                    Environment.NewLine, test.Description,
                                    Enum.Format (typeof (TestTypes), test.TestType, "F"));
                            else
                                Console.WriteLine (" - {0}", test.Name);
                    }

                if (!foundAtLeastOne) {
                    ErrorWriteLine ("Couldn't find at least one type that inherits from CodeDomTest in");
                    ErrorWriteLine (asm.CodeBase);
                    ErrorWriteLine ("Make sure your test case assembly references the same CodeDomTest.dll");
                    ErrorWriteLine ("that the current program ({0}) does.", (Environment.GetCommandLineArgs ())[0]);
                }
            }
        }

        static ArrayList LoadTestAssemblies () {
            ArrayList testAssemblies = new ArrayList ();
            StringCollection values = GetOption ("testcaselib").Values;
            foreach (string val in values)
                testAssemblies.Add (Assembly.LoadFrom (val));

            return testAssemblies;
        }

        static Assembly LoadProviderAssembly () {
            Assembly cdpAssembly = null;

            // load from our assemblies if the user provides
            // one of the "standard" code dom providers.  this allows
            // (provider is BlahCodeProvider) to work in the test cases
            if (GetOption ("codedomprovider").Set) {
                switch (GetOption ("codedomprovider").GetSingleValue ().ToLower (CultureInfo.InvariantCulture)) {
                    case "microsoft.jscript.jscriptcodeprovider":
                        cdpAssembly = Assembly.GetAssembly (typeof (JScriptCodeProvider));
                        break;

                    case "microsoft.visualbasic.vbcodeprovider":
                        cdpAssembly = Assembly.GetAssembly (typeof (VBCodeProvider));
                        break;

                    case "microsoft.csharp.csharpcodeprovider":
                        cdpAssembly = Assembly.GetAssembly (typeof (CSharpCodeProvider));
                        break;
                }
            }

            if (cdpAssembly == null && GetOption ("codedomproviderlib").Set) {
                string asmName = GetOption ("codedomproviderlib").GetSingleValue ();

                if (File.Exists (asmName)) {
                    // valid file, load from and get the strong name so we
                    // can then do a 'strong load'
                    cdpAssembly = Assembly.LoadFrom (asmName);
                } else {
                    // attempt to load from a strong name
                    cdpAssembly = Assembly.Load (asmName);
                }
            }

            return cdpAssembly;
        }

        /// <summary>
        /// Runs a given CodeDom test case against the given
        /// provider.  If the test case succeeds, true is returned.
        /// False otherwise.
        /// </summary>
        /// <param name="provider">The provider to test against.</param>
        /// <param name="test">The test case.</param>
        /// <param name="outDir">The output directory for assemblies, logs and source files.</param>
        /// <returns></returns>
        static bool RunTest (CodeDomProvider provider, CodeDomTest test, string outDir) {

            bool   success    = false;
            string filePrefix = String.Format(CultureInfo.InvariantCulture, @"{0}\{1}", outDir, test.Name);

            if (test.TestType == TestTypes.Whidbey && Environment.Version.Major < 2) {
                ErrorWriteLine ("Test '{0}' will not run properly on a pre-Whidbey runtime version.",
                        test.Name);
                ErrorWriteLine ("Succeeding this test without running it.");
                return true;
            }

            // create the log file
            StreamWriter log = null;
            log = new StreamWriter (filePrefix + ".log");

            // give the test case somewhere to write
            test.LogStream = log;

            // if -verbose, echo everything to the console
            test.EchoToConsole = GetOption ("verbose").Set;

            try {
                test.LogMessage ("");
                test.LogMessage ("* Running test case '{0}'", test.Name);
                test.LogMessage ("- {0}", test.Description);

                // if the test is a specialized version of the test tree
                // set the options so that the source and assembly files
                // can be saved properly by the Run method
                if (test.GetType ().IsSubclassOf (typeof (CodeDomTestTree))) {
                    CodeDomTestTree ttree = (CodeDomTestTree) test;
                    ttree.SourceFileName = filePrefix + ".src.fs";
                    ttree.AssemblyFileName = GetOption ("saveassemblies").Set ?
                        filePrefix + ".dll" : String.Empty;

                    try {
                        // delete any files of with these names
                        if (ttree.SourceFileName.Length > 0 &&
                                File.Exists (ttree.SourceFileName))
                            File.Delete (ttree.SourceFileName);
                        if (ttree.AssemblyFileName.Length > 0 &&
                                File.Exists (ttree.AssemblyFileName))
                            File.Delete (ttree.AssemblyFileName);

                    // ignore file in use and argument exceptions
                    } catch (IOException) {
                    } catch (ArgumentException) {
                    }
                }

                success = test.Run (provider);

            } catch (ScenarioException e) {
                // A scenario exception happened.  This means the test case has
                // some internal inconsistencies.
                ErrorWriteLine ("! Scenario exception: " + e.Message);
                ErrorWriteLine ("! This is a problem with the test case, not the provider.");
                
                // repeat for the log file
                log.WriteLine ("---------------------------------------------------------");
                log.WriteLine ("! Scenario exception: " + e.Message);
                log.WriteLine ("! This is a problem with the test case, not the provider.");

                success = false;
            } catch (Exception e) {
                ErrorWriteLine ("! Exception occurred while running test '{0}'.", test.Name);
                ErrorWriteLine ("! Stack: {0}", e.ToString ());

                // repeat for the log file
                log.WriteLine ("---------------------------------------------------------");
                log.WriteLine ("! Exception occurred while running test '{0}'.", test.Name);
                log.WriteLine ("! Stack: {0}", e.ToString ());

                success = false;
            } finally {
                // remove the log listener
                log.Close ();
            }

            if (success) {
                // on test case success, we need to clean up the log and
                // source files we generated
                try {
                    if (!GetOption ("savesources").Set)
                        File.Delete (filePrefix + ".src.fs");
                    if (!GetOption ("savelogs").Set)
                        File.Delete (filePrefix + ".log");
                } catch (Exception e) {
                    ErrorWriteLine ("Error trying to delete a file.");
                    ErrorWriteLine ("Exception stack: " + e.ToString ());
                }
            } else {
                if (GetOption ("dump").Set) {
                    // the test failed, so we should write the log and source if
                    // dump is specified
                    try {
                        // if verbose was given, the log file will have already been
                        // shown on the console
                        if (!GetOption ("verbose").Set) {
                            Console.WriteLine ("-------------- LOG (test case: {0}) --------------", test.Name);
                            using (StreamReader rLog = new StreamReader (filePrefix + ".log")) {
                                Console.WriteLine (rLog.ReadToEnd ());
                            }
                            Console.WriteLine ("-------------- END LOG (test case: {0}) --------------", test.Name);
                        }

                        // src file may not exist
                        if (File.Exists (filePrefix + ".src.fs")) {
                            Console.WriteLine ("-------------- SOURCE (test case: {0}) --------------", test.Name);
                            using (StreamReader rSrc = new StreamReader (filePrefix + ".src.fs")) {
                                Console.WriteLine (rSrc.ReadToEnd ());
                            }
                            Console.WriteLine ("-------------- END SOURCE (test case: {0}) --------------", test.Name);
                        }

                    } catch (Exception e) {
                        ErrorWriteLine ("Error reading files!");
                        ErrorWriteLine ("Exception stack:");
                        ErrorWriteLine (e.ToString ());
                    }
                }
            }

            // return true on success, false on failure
            return success;
        }

        /// <summary>
        /// Retrieves the given option name from the list of options.  If no
        /// option with the given name is found, and exception is thrown.
        /// </summary>
        /// <param name="name">The option name to retrieve.</param>
        /// <returns>The Option object corresponding to the given name.</returns>
        static Option GetOption (string name) {
            foreach (Option opt in options) {
                if (String.Compare (opt.Name, name, false,
                    CultureInfo.InvariantCulture) == 0)
                    return opt;
            }

            throw new ArgumentException (String.Format(CultureInfo.InvariantCulture, "Option name '{0}' doesn't exist.", name));
        }

        /// <summary>
        /// Prints the usage of this program to the console.
        /// </summary>
        static void PrintUsage (bool showDescriptions) {

            Console.WriteLine (Header);
            Console.WriteLine ();

            Console.WriteLine ("Usage:");
            Console.WriteLine ();

            string name = (Environment.GetCommandLineArgs ())[0];
            string pad  = new String (' ', name.Length + 1);

#if WHIDBEY
            Console.WriteLine ("{0} /langname:<language name as specified in machine.config>", name);
            Console.WriteLine ("{0}/testcaselib:<assem> [[/testcaselib:<assem2>] ... ]", pad);
            Console.WriteLine ();
            Console.WriteLine ("  -- alternatively --");
            Console.WriteLine ();
#endif
            Console.WriteLine ("{0} /codedomprovider:<full name>", name);
            Console.WriteLine ("{0}/testcaselib:<assem> [[/testcaselib:<assem2>] ... ]", pad);
            Console.WriteLine ("{0}[/codedomproviderlib:<library file or strong name>]", pad);

            Console.WriteLine ();
#if WHIDBEY
            Console.Write ("** /codedomprovider and /testcaselib");
            Console.WriteLine (" -- OR -- /langname and /testcaselib");
            Console.WriteLine ("   are required.");
#else
            Console.Write ("** /codedomprovider and /testcaselib are required.");
#endif
            Console.WriteLine ();
            Console.WriteLine ("Options are case insensitive. You may also specify options with a '-'.");
            Console.WriteLine ("Use /help to show full descriptions.");
            Console.WriteLine ();


            // Loop through every option and output its usage.
            foreach (Option opt in options) {
                Console.WriteLine (opt.GetUsage (showDescriptions));
            }
        }

        /// <summary>
        /// Parses the given string array as arguments, and saves the results of the parse
        /// to the Options array stored in this class.
        /// </summary>
        /// <param name="args">The arguments to parse.</param>
        /// <param name="ignoreDash">Whether or not to ignore dashes in the beginning of each option.</param>
        /// <returns></returns>
        static bool ParseArgs (string[] args, bool ignoreDash) {
            bool success = true;

            if (args.Length > 0) {
                foreach (string arg in args) {
                    // no matches for this argument
                    bool nomatches = true;

                    foreach (Option opt in options) {
                        Option.OptionParseResult result = opt.Parse (arg, ignoreDash);

                        if (result == Option.OptionParseResult.Malformed) {
                            success = false;
                            ErrorWriteLine ("Option '{0}' not specified correctly.", arg);
                            break;
                        } else if (result == Option.OptionParseResult.Match)
                            nomatches = false;
                    }

                    // none of the options matched the given argument
                    // error out
                    if (success && nomatches) {
                        success = false;
                        ErrorWriteLine ("Option '{0}' is not valid.", arg);
                        ErrorWriteLine ();
                        break;
                    }
                }
            } else {
                success = false;
            }

            return success;
        }

        static bool StringCollectionContainsIgnoreCase (StringCollection collect, string find) {
            foreach (string s in collect) {
                if (String.Compare (s, find, true, CultureInfo.InvariantCulture) == 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Private option class that describes an option.
        /// </summary>
        private class Option {
            string    name        = null;
            string[]  shortnames  = null;
            string    description = null;
            bool      hasArg      = false;
            bool      wasSet      = false;
            StringCollection values      = new StringCollection ();

            public enum OptionParseResult {
                Malformed,
                NoMatch,
                Match
            }

            public Option (string name, string[] shortnames, string description, bool hasArg) {
                if (name == null)
                    throw new ArgumentNullException ("name");

                if (shortnames == null)
                    throw new ArgumentNullException ("shortnames");

                if (description == null)
                    throw new ArgumentNullException ("description");

                this.name = name;
                this.shortnames = shortnames;
                this.description = description;
                this.hasArg = hasArg;

                this.wasSet = false;
            }

            public bool Set {
                get { return wasSet; }
            }

            public string Name {
                get { return name; }
            }

            public StringCollection Values {
                get { return values; }
            }

            public OptionParseResult Parse (string arg, bool ignoreDash) {
                OptionParseResult res = OptionParseResult.NoMatch;

                StringBuilder sb = new StringBuilder ();

                // generate a regex from the given options
                sb.Append ("^");
                if (!ignoreDash)
                    sb.Append ("[-/]");

                sb.AppendFormat ("({0}", Regex.Escape (name));

                foreach (string sn in shortnames)
                    sb.AppendFormat ("|{0}", Regex.Escape (sn));
                sb.Append (")");
                if (hasArg)
                    sb.Append (":\"?([^\"]*)\"?$");
                else
                    sb.Append ("$");

                Regex rx = new Regex (sb.ToString (),
                    RegexOptions.IgnoreCase);

                // attempt a match
                Match m = rx.Match (arg);

                if (m.Success) {
                    res = OptionParseResult.Match;
                    wasSet = true;

                    // parse any args if there are
                    // any to parse
                    if (hasArg) {
                        if (m.Groups.Count > 1) {
                            if (!values.Contains (m.Groups[2].Value))
                                values.Add (m.Groups[2].Value);
                        } else {
                            res = OptionParseResult.Malformed;
                            wasSet = false;
                        }
                    }
                }

                return res;
            }

            public string GetSingleValue () {
                if (values.Count >= 1)
                    return (string) values[0];
                return null;
            }

            public String GetUsage (bool showDescriptions) {
                StringBuilder sb = new StringBuilder ();

                sb.AppendFormat (" /{0}{1}", name, hasArg ? ":<value>" : String.Empty);
                foreach (string sn in shortnames) {
                    sb.AppendFormat (" or /{0}{1}", sn, hasArg ? ":<value>" : String.Empty);
                }

                if (showDescriptions) {
                    sb.Append (Environment.NewLine);

                    // wrap long lines
                    int currentLength = 0;
                    foreach (string word in description.Split (' ')) {
                        if (currentLength >= 55) {
                            sb.Append (Environment.NewLine);
                            currentLength = 0;
                        }

                        if (currentLength == 0) {
                            sb.AppendFormat ("    {0} ", word);
                            currentLength += word.Length + 4;
                        } else {
                            sb.AppendFormat ("{0} ", word);
                            currentLength += word.Length;
                        }
                    }
                    sb.Append (Environment.NewLine);
                }

                return sb.ToString ();
            }

            public override String ToString () {
                StringBuilder sb = new StringBuilder ();
                sb.AppendFormat ("name={0}", name);
                sb.AppendFormat (",description={0}", description);
                sb.AppendFormat (",set={0}", wasSet);
                foreach (string val in values)
                    sb.AppendFormat (",val={0}", val);

                return sb.ToString ();
            }
        }

        /// <summary>
        /// Describes how test type options are handled.  This is specified
        /// as -set on the command-line.
        /// </summary>
        private class TestTypeOption {
            string    optionName;
            TestTypes types;
            Version   runtimeVersion;

            public TestTypeOption (string optionName, TestTypes types, Version runtimeVersion) {
                if (optionName == null)
                    throw new ArgumentNullException ("optionName");
                if (runtimeVersion == null)
                    throw new ArgumentNullException ("runtimeVersion");

                this.optionName = optionName;
                this.types = types;
                this.runtimeVersion = runtimeVersion;
            }

            public string Name {
                get { return optionName; }
            }

            public TestTypes Types {
                get { return types; }
            }

            public Version RuntimeVersion {
                get { return runtimeVersion; }
            }

            public bool IsSameName (string name) {
                return String.Compare (name, optionName, true,
                        CultureInfo.InvariantCulture) == 0;
            }

            public bool IsVersionCompatible (Version ver) {
                return runtimeVersion <= ver;
            }
        }

        /// <summary>
        /// Obligatory header information.
        /// </summary>
        /// <returns>A short description of this program.</returns>
        static string Header {
            get {
                return "Microsoft (R) CodeDom test suite" + Environment.NewLine +
                    ".NET Version " + Environment.Version + Environment.NewLine +
                    "Copyright (C) Microsoft Corp. 2004.  All rights reserved.";
            }
        }

        /// <summary>
        /// Listed below are the names of each test classification, the types
        /// of tests they cover and the CLR version they will run on.
        /// </summary>
        static TestTypeOption [] testTypeOptions = new TestTypeOption [] {
                new TestTypeOption ("Subset", TestTypes.Subset, new Version (1, 0, 0)),
                new TestTypeOption ("Everett", TestTypes.Subset | TestTypes.Everett, new Version (1, 0, 0)),
                new TestTypeOption ("Whidbey", TestTypes.Subset | TestTypes.Everett | TestTypes.Whidbey, new Version (2, 0, 0))
        };

        /// <summary>
        /// The options and their descriptions.  The boolean value specifies whether the
        /// option takes an argument or not.
        /// </summary>
        static Option[] options = new Option[] {
                new Option ("help", new string[] {"?", "h"},
                "Shows this usage description.",
                false
                ),
                new Option ("codedomprovider", new string[] {"p"},
                "** Either this or languagename required ** CodeDomProvider you wish to use to run the test cases. " +
                "This should be specified using the full name. You should also specify the codedomproviderlib option " +
                "if this provider is not in an assembly in the GAC. " +
                "If both languagename and codedomprovider options are specified, languagename is taken in favor of " +
                "the two.",
                true
                ),
                new Option ("languagename", new string[] {"lang"},
                "** Either this or languagename required ** The language to run the test " +
                "cases against. This is the name given to CodeDomProvider.CreateProvider(). " +
                "This only works in CLR versions >= 2.0. If both languagename and codedomprovider " +
                "options are specified, languagename is taken in favor of the two.",
                true
                ),
                new Option ("testcaselib", new string[] {"tl"},
                "**required** Test case assembly that stores the test cases you wish to " +
                "test against. You may specify more than one.",
                true
                ),
                new Option ("codedomproviderlib", new string[] {"pl"},
                "The assembly where the CodeDomProvider lives. This should " +
                "be specified either by file name or the strong name. If this " +
                "option is not specified, the current assembly is used.",
                true
                ),
                new Option ("list", new string[] {"l"},
                "Lists available tests. Use -listdesc to get descriptions. Note " +
                "you must pass in valid test assemblies in order for this to print anything.",
                false
                ),
                new Option ("listdesc", new string[] {"ld"},
                "Lists available tests and their descriptions. Note " +
                "you must pass in valid test assemblies for this to print anything.",
                false
                ),
                new Option ("set", new string[] {"s"},
                "The set of tests to run. Only those matching the given " +
                "option will be run. See -setlist for a list of valid set names.",
                true
                ),
                new Option ("setlist", new string[] {"sls"},
                "The list of test sets you may run.",
                false
                ),
                new Option ("optionfile", new string[] {"of"},
                "Path to a option file(s) that lists these options " +
                "one per line. Options specified in these file will 'add to' " +
                "those also specified on the command-line. This is similar to " +
                "the CSharp compiler's response file except options should not have " +
                "'-' or '/' prepended before them.",
                true
                ),
                new Option ("runtestcase", new string[] {"t"},
                "Select specific test case(s) to run. You may specify more " +
                "than one.",
                true
                ),
                new Option ("dontruntestcase", new string[] {"n"},
                "Select specific test case(s) *NOT* to run. You may specify more " +
                "than one.",
                true
                ),
                new Option ("savedirectory", new string[] {"dir", "savedir"},
                "The directory to save all logs, assemblies and source files.",
                true
                ),
                new Option ("savesources", new string[] {"ss"},
                "Save the sources even if all tests pass.",
                false
                ),
                new Option ("saveassemblies", new string[] {"sa"},
                "Save the generated assemblies even if all tests pass.  Note that only " +
                "test cases that derive from CodeDomTestTree have the ability to save " +
                "assemblies.",
                false
                ),
                new Option ("savelogs", new string[] {"sl"},
                "Save the generated logs even if all tests pass.",
                false
                ),
                new Option ("verbose", new string[] {"v"},
                "Be verbose. All test case output is written to the console. " +
                "If this is not given, no output is given unless the " +
                "test case fails.",
                false
                ),
                new Option ("dump", new string[] {"d"},
                "If during the run a test case fails, dump the log and source " +
                "file to the console for that test case.",
                false
                )
            };

        static void ErrorWriteLine () {
            ErrorWriteLine (String.Empty);
        }

        static void ErrorWriteLine (string msg) {
            ErrorWriteLine (msg, String.Empty);
        }

        static void ErrorWriteLine (string msg, params object [] oparms) {
#if WHIDBEY
           Console.ForegroundColor = ConsoleColor.Red;
#endif
           //if (!GetOption ("verbose").Set)
           Console.Error.WriteLine (String.Format (CultureInfo.InvariantCulture, msg, oparms));
#if WHIDBEY
           Console.ResetColor ();
#endif
        }
    }
}

