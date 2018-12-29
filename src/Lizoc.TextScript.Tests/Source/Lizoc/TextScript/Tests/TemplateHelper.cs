// -----------------------------------------------------------------------
// <copyright file="TemplateHelper.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
// </copyright>
// -----------------------------------------------------------------------

// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Tests
{
    public class TemplateCompareResult
    {
        public bool IsEqual { get; set; }

        public List<string> Tokens { get; set; }

        public List<string> VerboseMessage { get; set; }

        public string DebugCode { get; set; }

        public string Result { get; set; }

        public string Expected { get; set; }
    }

    internal class TemplateHelper
    {
        public static TemplateCompareResult TestTemplate(string expected, string input, bool isLiquid = false, bool isRoundtripTest = false, bool supportExactRoundtrip = true, object model = null, bool specialLiquid = false)
        {
            var parserOptions = new ParserOptions()
            {
                ConvertLiquidFunctions = isLiquid
            };
            var lexerOptions = new LexerOptions()
            {
                Mode = isLiquid ? ScriptMode.Liquid : ScriptMode.Default
            };

            if (isRoundtripTest)
            {
                lexerOptions.KeepTrivia = true;
            }

            if (specialLiquid)
            {
                parserOptions.ExpressionDepthLimit = 500;
            }

            List<string> tokensDesc = new List<string>();
            var lexer = new Lexer(input, options: lexerOptions);
            foreach (var token in lexer)
            {
                tokensDesc.Add($"{token.Type}: {token.GetText(input)}");
            }

            string roundtripText = null;
            List<string> verboseMessage = new List<string>();

            // We loop first on input text, then on rountrip
            while (true)
            {
                bool isRoundtrip = roundtripText != null;
                bool hasErrors = false;
                if (isRoundtrip)
                {
                    verboseMessage.Add("Rountrip");
                    verboseMessage.Add("======================================");
                    verboseMessage.Add(roundtripText);
                    lexerOptions.Mode = ScriptMode.Default;

                    if (lexerOptions.Mode == ScriptMode.Default && !isLiquid && supportExactRoundtrip)
                    {
                        verboseMessage.Add("Checking Exact Roundtrip - Input");
                        verboseMessage.Add("======================================");
                        EqualityCompareResult equalResult = TextAssert.Equal(input, roundtripText);
                        if (equalResult.VerboseMessage != null && equalResult.VerboseMessage.Count > 0)
                        {
                            verboseMessage.AddRange(equalResult.VerboseMessage);
                        }
                        if (equalResult.IsEqual == false)
                        {
                            return new TemplateCompareResult()
                            {
                                Tokens = tokensDesc,
                                IsEqual = false,
                                VerboseMessage = verboseMessage,
                                DebugCode = "INPUT_ROUNDTRIP_TEXT_NOT_EQUAL"
                            };
                        }
                    }
                    input = roundtripText;
                }
                else
                {
                    verboseMessage.Add("Input");
                    verboseMessage.Add("======================================");
                    verboseMessage.Add(input);
                }

                var template = Template.Parse(input, "text", parserOptions, lexerOptions);

                var result = string.Empty;
                if (template.HasErrors)
                {
                    hasErrors = true;
                    for (int i = 0; i < template.Messages.Count; i++)
                    {
                        var message = template.Messages[i];
                        if (i > 0)
                        {
                            result += "\n";
                        }
                        result += message;
                    }
                    if (specialLiquid)
                    {
                        throw new InvalidOperationException("Parser errors: " + result);
                    }
                }
                else
                {
                    if (isRoundtripTest)
                    {
                        result = template.ToText();
                    }
                    else
                    {
                        if (template.Page == null)
                        {
                            return new TemplateCompareResult()
                            {
                                Tokens = tokensDesc,
                                IsEqual = false,
                                VerboseMessage = verboseMessage,
                                DebugCode = "PAGE_IS_NULL",
                                Result = result,
                                Expected = expected
                            };
                        }

                        if (!isRoundtrip)
                        {
                            // Dumps the rountrip version
                            var lexerOptionsForTrivia = lexerOptions;
                            lexerOptionsForTrivia.KeepTrivia = true;
                            var templateWithTrivia = Template.Parse(input, "input", parserOptions, lexerOptionsForTrivia);
                            roundtripText = templateWithTrivia.ToText();
                        }

                        try
                        {
                            // Setup a default model context for the tests
                            if (model == null)
                            {
                                var scriptObj = new ScriptObject
                                {
                                    ["page"] = new ScriptObject { ["title"] = "This is a title" },
                                    ["user"] = new ScriptObject { ["name"] = "John" },
                                    ["product"] = new ScriptObject { ["title"] = "Orange", ["type"] = "fruit" },
                                    ["products"] = new ScriptArray()
                                    {
                                        new ScriptObject {["title"] = "Orange", ["type"] = "fruit"},
                                        new ScriptObject {["title"] = "Banana", ["type"] = "fruit"},
                                        new ScriptObject {["title"] = "Apple", ["type"] = "fruit"},
                                        new ScriptObject {["title"] = "Computer", ["type"] = "electronics"},
                                        new ScriptObject {["title"] = "Mobile Phone", ["type"] = "electronics"},
                                        new ScriptObject {["title"] = "Table", ["type"] = "furniture"},
                                        new ScriptObject {["title"] = "Sofa", ["type"] = "furniture"},
                                    }
                                };
                                scriptObj.Import(typeof(SpecialFunctionProvider));
                                model = scriptObj;
                            }

                            var context = isLiquid
                                ? new LiquidTemplateContext()
                                {
                                    TemplateLoader = new LiquidCustomTemplateLoader()
                                }
                                : new TemplateContext()
                                {
                                    TemplateLoader = new CustomTemplateLoader()
                                };

                            // We use a custom output to make sure that all output is using the "\n"
                            context.PushOutput(new TextWriterOutput(new StringWriter() { NewLine = "\n" }));

                            var contextObj = new ScriptObject();
                            contextObj.Import(model);
                            context.PushGlobal(contextObj);

                            result = template.Render(context);
                        }
                        catch (Exception exception)
                        {
                            if (specialLiquid)
                            {
                                throw;
                            }
                            else
                            {
                                result = GetReason(exception);
                            }
                        }
                    }
                }

                var testContext = isRoundtrip ? "Roundtrip - " : String.Empty;

                verboseMessage.Add($"{testContext}Result");
                verboseMessage.Add("======================================");
                verboseMessage.Add(result);
                verboseMessage.Add($"{testContext}Expected");
                verboseMessage.Add("======================================");
                verboseMessage.Add(expected);

                EqualityCompareResult testEqualResult = TextAssert.Equal(expected, result);
                if (testEqualResult.VerboseMessage != null && testEqualResult.VerboseMessage.Count > 0)
                    verboseMessage.AddRange(testEqualResult.VerboseMessage);

                if (testEqualResult.IsEqual == false)
                {
                    return new TemplateCompareResult
                    {
                        IsEqual = false,
                        VerboseMessage = verboseMessage,
                        DebugCode = "UNKNOWN",
                        Tokens = tokensDesc,
                        Result = result,
                        Expected = expected
                    };
                }

                if (isRoundtripTest || isRoundtrip || hasErrors)
                {
                    break;
                }
            }

            return new TemplateCompareResult
            {
                IsEqual = true,
                VerboseMessage = verboseMessage,
                DebugCode = "",
                Tokens = tokensDesc
            };
        }

        private static string GetReason(Exception ex)
        {
            var text = new StringBuilder();
            while (ex != null)
            {
                text.Append(ex);
                if (ex.InnerException != null)
                {
                    text.Append(". Reason: ");
                }
                ex = ex.InnerException;
            }
            return text.ToString();
        }
    }
}
