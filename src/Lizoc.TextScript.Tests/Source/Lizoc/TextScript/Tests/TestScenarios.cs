// -----------------------------------------------------------------------
// <copyright file="TestScenarios.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Lizoc.TextScript.Tests
{
    public class TestScenarios
    {
        private const string TestAssemblyPrefix = "Lizoc.TextScript.Tests.Resource";

        /// <summary>
        /// Lists of the tests that don't support exact byte-to-byte roundtrip (due to reformatting...etc.)
        /// </summary>
        private static readonly HashSet<string> NotSupportingExactRoundtrip = new HashSet<string>()
        {
            "003-whitespaces.txt",
            "010-literals.txt",
            "205-case-when-statement2.txt",
            "230-capture-statement2.txt",
            "470-html.txt"
        };

        private string _scenarioOutputDir = null;

        private readonly ITestOutputHelper _output;

        public TestScenarios(ITestOutputHelper output)
        {
            _output = output;
            WriteFailureToFile = true;
            StopOnFailure = false;
        }

        public string ScenarioResultOutputDirectory
        {
            get
            {
                if (_scenarioOutputDir == null)
                {
                    string assemblyLocation = GetExecutingAssembly().CodeBase;
                    if (assemblyLocation.StartsWith("file:///"))
                        assemblyLocation = assemblyLocation.Substring("file:///".Length);

                    _scenarioOutputDir = Path.GetDirectoryName(assemblyLocation);
                }

                return _scenarioOutputDir;
            }
        }

        public bool WriteFailureToFile { get; set; } = false;

        public bool StopOnFailure { get; set; } = true;

        [Fact]
        public void CanPassScenarios()
        {
            string[] scenarios = new string[]
            {
                "000-basic",
                "010-literals",
                "100-expressions",
                "200-statements",
                "300-functions",
                "400-builtins",
                "500-liquid"
            };

            List<string> failedScenarios = new List<string>();

            foreach (string scenario in scenarios)
            {
                string[] scenarioFiles = GetScenarioFiles(scenario);

                foreach (string file in scenarioFiles)
                {
                    if (StopOnFailure)
                    {
                        Assert.True(TestFile(file));
                    }
                    else
                    {
                        if (TestFile(file) == false)
                            failedScenarios.Add(file);
                    }
                }
            }

            if (!StopOnFailure && failedScenarios.Count > 0)
            {
                foreach (string filename in failedScenarios)
                {
                    _output.WriteLine($"[!] Scenario failure -- {filename}");
                }
                Assert.Empty(failedScenarios);
            }
        }

        private string[] GetScenarioFiles(string scenarioName)
        {
            // Lizoc.TextScript.Tests.Resource._010_literals.010 - literals.txt
            // Lizoc.TextScript.Tests.Resource._010_literals.010 - literals.out.txt
            string[] allResource = GetResourceFiles(GetExecutingAssembly(), TestAssemblyPrefix + "._" + scenarioName.Replace('-', '_') + ".");
            List<string> filtered = new List<string>();
            foreach (string name in allResource)
            {
                if (!name.EndsWith(".out.txt"))
                    filtered.Add(name);
            }

            return filtered.ToArray();
        }

        private static Assembly GetExecutingAssembly()
        {
#if NETSTANDARD
            return typeof(TestScenarios).GetTypeInfo().Assembly;
#else
            return Assembly.GetExecutingAssembly();
#endif
        }

        private static string GetEmbedFileContent(Assembly assembly, string fileName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(fileName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static string[] GetResourceFiles(Assembly assembly, string prefix)
        {
            string[] resources = assembly.GetManifestResourceNames();

            if (string.IsNullOrEmpty(prefix))
                return resources;

            List<string> output = new List<string>();
            foreach (string res in resources)
            {
                if (res.StartsWith(prefix))
                    output.Add(res);
            }

            return output.ToArray();
        }

        private bool TestFile(string inputName)
        {
            string expectedOutputFile = null;
            string expectedInputFile = null;
            _output.WriteLine(inputName);
            if (!inputName.EndsWith(".txt"))
            {
                throw new ArgumentException("Template name should end with .txt");
            }
            else
            {
                expectedInputFile = inputName;
                expectedOutputFile = inputName.Substring(0, inputName.Length - 4) + ".out.txt";
            }

            bool isSupportingExactRoundtrip = true;
            foreach (string item in NotSupportingExactRoundtrip)
            {
                if (inputName.EndsWith(inputName))
                {
                    isSupportingExactRoundtrip = false;
                    break;
                }
            }

            bool isLiquid = inputName.Contains("-liquid-");

            string inputText = GetEmbedFileContent(GetExecutingAssembly(), expectedInputFile);
            string outputText = GetEmbedFileContent(GetExecutingAssembly(), expectedOutputFile);

            TemplateCompareResult compareResult = TemplateHelper.TestTemplate(outputText, inputText, isLiquid, false, isSupportingExactRoundtrip);
            if (compareResult.DebugCode != "")
                _output.WriteLine("Debug code: " + compareResult.DebugCode);

            if (compareResult.Tokens != null)
            {
                foreach (string line in compareResult.Tokens)
                {
                    _output.WriteLine(line);
                }
            }

            if (compareResult.VerboseMessage != null)
            {
                foreach (string line in compareResult.VerboseMessage)
                {
                    _output.WriteLine(line);
                }
            }

            if (!compareResult.IsEqual)
            {
                _output.WriteLine("[FAILED SCENARIO] " + inputName);

                if (WriteFailureToFile && compareResult.Result != null)
                {
                    string outputFileName = expectedOutputFile.StartsWith(TestAssemblyPrefix)
                        ? expectedOutputFile.StartsWith(TestAssemblyPrefix + "._")
                            ? expectedOutputFile.Substring(TestAssemblyPrefix.Length + 2)
                            : expectedOutputFile.Substring(TestAssemblyPrefix.Length)
                        : expectedOutputFile;
                    
                    string outputPath = Path.Combine(ScenarioResultOutputDirectory, outputFileName);
                    _output.WriteLine($"  --> Write result to {outputPath}");

                    File.WriteAllText(outputPath, compareResult.Result);
                }
            }

            return compareResult.IsEqual;
        }
    }
}
