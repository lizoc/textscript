// -----------------------------------------------------------------------
// <copyright file="TestParser.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Globalization;
using Xunit;
using Xunit.Abstractions;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Tests
{
    public class TestParser
    {
        private readonly ITestOutputHelper _output;

        public TestParser(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestCanRoundtrip()
        {
            var text = "This is a text {{ code # With some comment }} and a text";
            Assert.True(TestRoundtrip(text));
        }

        [Fact]
        public void TestRoundtrip1()
        {
            var text = "This is a text {{ code | pipe a b c | a + b }} and a text";
            Assert.True(TestRoundtrip(text));
        }

        [Fact]
        public void RoundtripFunction()
        {
            var text = @"{{ func inc
    ret $0 + 1
end }}";
            Assert.True(TestRoundtrip(text));
        }

        [Fact]
        public void RoundtripFunction2()
        {
            var text = @"{{
func   inc
    ret $0 + 1
end
xxx 1
}}";
            Assert.True(TestRoundtrip(text));
        }


        [Fact]
        public void RoundtripIf()
        {
            var text = @"{{
if true
    ""yes""
end
}}
raw
";
            Assert.True(TestRoundtrip(text));
        }

        [Fact]
        public void RoundtripIfElse()
        {
            var text = @"{{
if true
    ""yes""
else
    ""no""
end
}}
raw
";
            Assert.True(TestRoundtrip(text));
        }

        [Fact]
        public void RoundtripIfElseIf()
        {
            var text = @"{{
if true
    ""yes""
elseif yo
    ""no""
end
y
}}
raw
";
            Assert.True(TestRoundtrip(text));
        }

        [Fact]
        public void RoundtripCapture()
        {
            var text = @" {{ capture variable -}}
    This is a capture
{{- end -}}
{{ variable }}";
            Assert.True(TestRoundtrip(text));
        }


        [Fact]
        public void RoundtripRaw()
        {
            var text = @"This is a raw     {{~ x ~}}     end";
            Assert.True(TestRoundtrip(text));
        }

        [Fact]
        public void TestDateNow()
        {
            // default is dd MM yyyy
            var dateNow = DateTime.Now.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            var template = ParseTemplate(_output, @"{{ date.now }}");
            var result = template.Render();
            Assert.Equal(dateNow, result);

            template = ParseTemplate(_output, @"{{ date.format = '%Y'; date.now }}");
            result = template.Render();
            Assert.Equal(DateTime.Now.ToString("yyyy", CultureInfo.InvariantCulture), result);

            template = ParseTemplate(_output, @"{{ date.format = '%Y'; date.now | date.add_years 1 }}");
            result = template.Render();
            Assert.Equal(DateTime.Now.AddYears(1).ToString("yyyy", CultureInfo.InvariantCulture), result);
        }

        [Fact]
        public void TestHelloWorld()
        {
            var template = ParseTemplate(_output, @"This is a {{ text }} World from textscript!");
            var result = template.Render(new { text = "Hello" });
            Assert.Equal("This is a Hello World from textscript!", result);
        }

        [Fact]
        public void TestFrontMatter()
        {
            var options = new LexerOptions() { Mode = ScriptMode.FrontMatterAndContent };
            var input = @"+++
variable = 1
name = 'yes'
+++
This is after the frontmatter: {{ name }}
{{
variable + 1
}}";
            input = input.Replace("\r\n", "\n");
            var template = ParseTemplate(_output, input, options);

            // Make sure that we have a front matter
            Assert.NotNull(template.Page.FrontMatter);

            var context = new TemplateContext();

            // Evaluate front-matter
            var frontResult = context.Evaluate(template.Page.FrontMatter);
            Assert.Null(frontResult);

            // Evaluate page-content
            context.Evaluate(template.Page);
            var pageResult = context.Output.ToString();

            EqualityCompareResult compareResult = TextAssert.Equal("This is after the frontmatter: yes\n2", pageResult);
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
        public void TestFrontMatterOnly()
        {
            var options = new ParserOptions();

            var input = @"+++
variable = 1
name = 'yes'
+++
This is after the frontmatter: {{ name }}
{{
variable + 1
}}";
            input = input.Replace("\r\n", "\n");

            var lexer = new Lexer(input, null, new LexerOptions() { Mode = ScriptMode.FrontMatterOnly });
            var parser = new Parser(lexer, options);

            var page = parser.Run();
            foreach (var message in parser.Messages)
            {
                _output.WriteLine(message.Message);
            }
            Assert.False(parser.HasErrors);

            // Check that the parser finished parsing on the first code exit }}
            // and hasn't tried to run the lexer on the remaining text
            Assert.Equal(new TextPosition(30, 3, 0), parser.CurrentSpan.Start);
            Assert.Equal(new TextPosition(33, 3, 3), parser.CurrentSpan.End);

            var startPositionAfterFrontMatter = parser.CurrentSpan.End.NextLine();

            // Make sure that we have a front matter
            Assert.NotNull(page.FrontMatter);
            Assert.Null(page.Body);

            var context = new TemplateContext();

            // Evaluate front-matter
            var frontResult = context.Evaluate(page.FrontMatter);
            Assert.Null(frontResult);

            lexer = new Lexer(input, null, new LexerOptions() { StartPosition = startPositionAfterFrontMatter });
            parser = new Parser(lexer);
            page = parser.Run();
            foreach (var message in parser.Messages)
            {
                _output.WriteLine(message.Message);
            }
            Assert.False(parser.HasErrors);
            context.Evaluate(page);
            var pageResult = context.Output.ToString();

            EqualityCompareResult compareResult = TextAssert.Equal("This is after the frontmatter: yes\n2", pageResult);
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
        public void TestScriptOnly()
        {
            var options = new LexerOptions() { Mode = ScriptMode.ScriptOnly };
            var template = ParseTemplate(_output, @"
variable = 1
name = 'yes'
", options);

            var context = new TemplateContext();

            template.Render(context);

            var outputStr = context.Output.ToString();
            Assert.Equal(string.Empty, outputStr);

            var global = context.CurrentGlobal;
            object value;
            Assert.True(global.TryGetValue("name", out value));
            Assert.Equal("yes", value);

            Assert.True(global.TryGetValue("variable", out value));
            Assert.Equal(1, value);
        }

        [Fact]
        public void TestFunctionCallInExpression()
        {
            var lexer = new Lexer(@"{{
with math
    round pi
end
}}");
            var parser = new Parser(lexer);

            var scriptPage = parser.Run();

            foreach (var message in parser.Messages)
            {
                _output.WriteLine(message.Message);
            }
            Assert.False(parser.HasErrors);
            Assert.NotNull(scriptPage);

            var rootObject = new ScriptObject();
            rootObject.SetValue("math", ScriptObject.From(typeof(MathObject)), true);

            var context = new TemplateContext();
            context.PushGlobal(rootObject);
            context.Evaluate(scriptPage);
            context.PopGlobal();

            // Result
            var result = context.Output.ToString();

            _output.WriteLine(result);
        }

        private static Template ParseTemplate(ITestOutputHelper outputHelper, string text, LexerOptions? lexerOptions = null, ParserOptions? parserOptions = null)
        {
            var template = Template.Parse(text, "text", parserOptions, lexerOptions);
            foreach (var message in template.Messages)
            {
                outputHelper.WriteLine(message.Message);
            }
            Assert.False(template.HasErrors);
            return template;
        }

        private bool TestRoundtrip(string inputText, bool isLiquid = false)
        {
            inputText = inputText.Replace("\r\n", "\n");
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate(inputText, inputText, isLiquid, true);
            WriteTemplateResult(compareResult);
            return compareResult.IsEqual;
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
