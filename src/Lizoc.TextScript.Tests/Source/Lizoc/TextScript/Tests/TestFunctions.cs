using Xunit;
using Xunit.Abstractions;
using Lizoc.TextScript.Functions;
using SRS = Lizoc.TextScript.RS;

namespace Lizoc.TextScript.Tests
{
    public class TestFunctions
    {
        private readonly ITestOutputHelper _output;

        public TestFunctions(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestOffset()
        {
            Assert.Null(ArrayFunctions.Offset(null, 0));
        }

        [Fact]
        public void TestLimit()
        {
            Assert.Null(ArrayFunctions.Limit(null, 0));
        }

        [Fact]
        public void TestSortError()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("text(1,19) : error : " + string.Format(SRS.BadFunctionInvokeArgCountMin, "array.sort", 0, 1), 
                "{{ [1,2] || array.sort }}");
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestSliceError()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("text(1,11) : error : " + string.Format(SRS.BadFunctionInvokeArgCountMin, "string.slice", 0, 2), 
                "{{ string.slice }}");
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
        }

        [Fact]
        public void TestSliceAtError()
        {
            TemplateCompareResult compareResult = TemplateHelper.TestTemplate("text(1,11) : error : " + string.Format(SRS.BadFunctionInvokeArgCountMin, "string.slice1", 0, 2),
                "{{ string.slice1 }}");
            WriteTemplateResult(compareResult);
            Assert.True(compareResult.IsEqual);
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