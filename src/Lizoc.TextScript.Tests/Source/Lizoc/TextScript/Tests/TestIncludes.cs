// -----------------------------------------------------------------------
// <copyright file="TestIncludes.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Xunit;
using Xunit.Abstractions;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Syntax;
using SRS = Lizoc.TextScript.RS;

namespace Lizoc.TextScript.Tests
{
    public class TestIncludes
    {
        private readonly ITestOutputHelper _output;

        public TestIncludes(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestJekyllInclude()
        {
            var input = "{% include /this/is/a/test.htm %}";
            var template = Template.ParseLiquid(input, lexerOptions: new LexerOptions() { EnableIncludeImplicitString = true, Mode = ScriptMode.Liquid });
            var context = new TemplateContext { TemplateLoader = new LiquidCustomTemplateLoader() };
            var result = template.Render(context);
            EqualityCompareResult compareResult = TextAssert.Equal("/this/is/a/test.htm", result);
            if (compareResult.VerboseMessage != null && compareResult.VerboseMessage.Count > 0)
            {
                foreach (string vmsg in compareResult.VerboseMessage)
                {
                    _output.WriteLine(vmsg);
                }
            }
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestTemplateLoaderNoArgs()
        {
            var template = Template.Parse("Test with a include {{ include }}");
            var context = new TemplateContext();
            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            var expectedString = string.Format(SRS.BadFunctionInvokeArgEmpty, "include");
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `{expectedString}`");
        }

        [Fact]
        public void TestTemplateLoaderNotSetup()
        {
            var template = Template.Parse("Test with a include {{ include 'yoyo' }}");
            var context = new TemplateContext();
            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            var expectedString = string.Format(SRS.NoTemplateLoader, "include");
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `{expectedString}`");
        }

        [Fact]
        public void TestTemplateLoaderNotNull()
        {
            var template = Template.Parse("Test with a include {{ include null }}");
            var context = new TemplateContext();
            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            var expectedString = SRS.IncludeNameRequired;
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `${expectedString}`");
        }

        [Fact]
        public void TestSimple()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("Test with a include yoyo", "Test with a include {{ include 'yoyo' }}");
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestArguments()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("1 + 2", "{{ include 'arguments' 1 2 }}");
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestProduct()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("product: Orange", "{{ include 'product' }}");
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestNested()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("This is a header body This is a body_detail This is a footer", "{{ include 'nested_templates' }}");
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestRecursiveNested()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("56789", "{{ include 'recursive_nested_templates' 5 }}");
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestLiquidNull()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("", "{% include a %}", true);
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestLiquidWith()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("with_product: Orange", "{% include 'with_product' with product %}", true);
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestLiquidFor()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("for_product: Orange for_product: Banana for_product: Apple for_product: Computer for_product: Mobile Phone for_product: Table for_product: Sofa ", "{% include 'for_product' for products %}", true);
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestLiquidArguments()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("1 + yoyo", "{% include 'arguments' var1: 1, var2: 'yoyo' %}", true);
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestLiquidWithAndArguments()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("tada : 1 + yoyo", "{% include 'with_arguments' with 'tada' var1: 1, var2: 'yoyo' %}", true);
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestTemplateLoaderIncludeWithParsingErrors()
        {
            var template = Template.Parse("Test with a include {{ include 'invalid' }}");
            var context = new TemplateContext() { TemplateLoader = new CustomTemplateLoader() };
            var exception = Assert.Throws<ScriptParserRuntimeException>(() => template.Render(context));
            var expectedString = string.Format(SRS.IncludeParseError, "invalid", "invalid");
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `${expectedString}`");
        }

        [Fact]
        public void TestTemplateLoaderIncludeWithLexerErrors()
        {
            var template = Template.Parse("Test with a include {{ include 'invalid2' }}");
            var context = new TemplateContext() { TemplateLoader = new CustomTemplateLoader() };
            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            var expectedString = string.Format(SRS.IncludeContentEmpty, "invalid2", "invalid2");
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `${expectedString}`");
        }

        [Fact]
        public void TestTemplateLoaderIncludeWithNullGetPath()
        {
            var template = Template.Parse("{{ include 'null' }}");
            var context = new TemplateContext() { TemplateLoader = new CustomTemplateLoader() };
            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            var expectedString = string.Format(SRS.IncludePathNullError, "null");
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `${expectedString}`");
        }

        private void WriteTemplateResult(TemplateCompareResult compareResult)
        {
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
        }
    }
}