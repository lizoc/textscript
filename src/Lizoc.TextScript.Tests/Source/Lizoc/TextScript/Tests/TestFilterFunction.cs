// -----------------------------------------------------------------------
// <copyright file="TestFilterFunction.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
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
    public class TestFilterFunction
    {
        private readonly ITestOutputHelper _output;

        public TestFilterFunction(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanFilterNumber()
        {
            var template = Template.Parse("{{ [1,20,30] | where $ < 10 }}");
            var result = template.Render();

            Assert.Equal("[1]", result);
        }

        [Fact]
        public void CanFilterString()
        {
            var template = Template.Parse("{{ ['cool', 'fool', 'holdoor'] | where $ == 'cool' }}");
            var result = template.Render();

            Assert.Equal("[cool]", result);
        }

        [Fact]
        public void CanPipeInOneNumber()
        {
            var template = Template.Parse("{{ 1 | where $ < 10 }}");
            var result = template.Render();

            Assert.Equal("1", result);
        }

        [Fact]
        public void CanPipeInOneString()
        {
            var template = Template.Parse("{{ 'cool' | where $ == 'cool' }}");
            var result = template.Render();

            Assert.Equal("cool", result);
        }

        [Fact]
        public void ReturnAsIsWithoutCondition()
        {
            var template = Template.Parse("{{ [1, 4, 5] | where }}");
            var result = template.Render();

            Assert.Equal("[1, 4, 5]", result);
        }

        [Fact]
        public void SpecialVariableCanBeRightHandSide()
        {
            var template = Template.Parse("{{ [1, 4, 5] | where 4 < $ }}");
            var result = template.Render();

            Assert.Equal("[5]", result);
        }

        [Fact]
        public void ConditionSupportFunctions1()
        {
            var template = Template.Parse("{{ [1, -4, 5] | where ($ | math.abs) > 3 }}");
            var result = template.Render();

            Assert.Equal("[-4, 5]", result);
        }

        [Fact]
        public void ConditionSupportFunctions2()
        {
            var template = Template.Parse("{{ ['fooone', 'footwo', 'cool'] | where ($ | string.starts_with 'foo') }}");
            var result = template.Render();

            Assert.Equal("[fooone, footwo]", result);
        }

        [Fact]
        public void ConditionSupportFunctions3()
        {
            var template = Template.Parse("{{ [1, 2, 3] | where (4.5432 | math.round $) > 4.542 }}");
            var result = template.Render();

            Assert.Equal("[3]", result);
        }

        [Fact]
        public void SupportMultipleConditions()
        {
            var template = Template.Parse("{{ [1, 3, 4, 5, 6, 8] | where (($ < 4) && ($ > 1)) || ($ > 6) }}");
            var result = template.Render();

            Assert.Equal("[3, 8]", result);
        }

        [Fact]
        public void CanFilterObject()
        {
            var template = Template.Parse("{{ [{ foo: 123 }, { foo: 456 }] | where $.foo == 123 }}");            
            var result = template.Render(new TemplateContext()
            {
                // #todo caveat: must relax or else error occurs
                EnableRelaxedMemberAccess = true
            });

            Assert.Equal("[{foo: 123}]", result);
        }
    }
}
