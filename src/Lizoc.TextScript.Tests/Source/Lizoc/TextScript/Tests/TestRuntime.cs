// -----------------------------------------------------------------------
// <copyright file="TestRuntime.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;
using System.Collections.Generic;
using SRS = Lizoc.TextScript.RS;

namespace Lizoc.TextScript.Tests
{
    public partial class TestRuntime
    {
        private readonly ITestOutputHelper _output;

        public TestRuntime(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestNullDateTime()
        {
            var template = Template.Parse("{{ null | date.to_string '%g' }}");
            var context = new TemplateContext();
            var result = template.Render(context);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void TestDecimal()
        {
            var template = Template.Parse("{{ if value > 0 }}yes{{end}}");
            decimal x = 5;
            var result = template.Render(new { value = x });
            Assert.Equal("yes", result);
        }

        [Fact]
        public void TestCulture()
        {
            var number = 11232.123;
            var customCulture = new CultureInfo(CultureInfo.CurrentCulture.Name)
            {
                NumberFormat =
                {
                    NumberDecimalSeparator = ",",
                    NumberGroupSeparator = "."
                }
            };

            var numberAsStr = number.ToString(customCulture);

            var template = Template.Parse("{{ 11232.123 }}");
            var context = new TemplateContext();
            context.PushCulture(customCulture);
            var result = template.Render(context);
            context.PopCulture();

            Assert.Equal(numberAsStr, result);
        }


        [Fact]
        public void TestEvaluateScriptOnly()
        {
            {
                var lexerOptions = new LexerOptions() { Mode = ScriptMode.ScriptOnly };
                var template = Template.Parse("y = x + 1; y;", lexerOptions: lexerOptions);
                var result = template.Evaluate(new { x = 10 });
                Assert.Equal(11, result);
            }
            {
                var result = Template.Evaluate("y = x + 1; y;", new { x = 10 });
                Assert.Equal(11, result);
            }
        }

        [Fact]
        public void TestEvaluateDefault()
        {
            {
                var template = Template.Parse("{{y = x + 1; y;}} yoyo");
                var result = template.Evaluate(new { x = 10 });
                Assert.Equal(" yoyo", result);
            }
            {
                var template = Template.Parse("{{y = x + 1; y;}} yoyo {{y}}");
                var result = template.Evaluate(new { x = 10 });
                Assert.Equal(11, result);
            }
        }

        [Fact]
        public void TestReadOnly()
        {
            var template = Template.Parse("Test {{ a.b.c = 1 }}");

            var a = new ScriptObject()
            {
                {"b", new ScriptObject() {IsReadOnly = true}}
            };

            var context = new TemplateContext();
            context.PushGlobal(new ScriptObject()
            {
                {"a", a}
            });
            var exception = Assert.Throws<ScriptRuntimeException>(() => context.Evaluate(template.Page));
            var result = exception.ToString();
            Assert.True(result.Contains("a.b.c"), $"The exception string `{result}` does not contain a.b.c");
        }

        [Fact]
        public void TestDynamicVariable()
        {
            var context = new TemplateContext
            {
                TryGetVariable = (TemplateContext templateContext, SourceSpan span, ScriptVariable variable, out object value) =>
                {
                    value = null;
                    if (variable.Name == "myvar")
                    {
                        value = "yes";
                        return true;
                    }
                    return false;
                }
            };

            {
                var template = Template.Parse("Test with a dynamic {{ myvar }}");
                context.Evaluate(template.Page);
                var result = context.Output.ToString();

                EqualityCompareResult compareResult = TextAssert.Equal("Test with a dynamic yes", result);
                if (compareResult.VerboseMessage != null && compareResult.VerboseMessage.Count > 0)
                {
                    foreach (string vmsg in compareResult.VerboseMessage)
                    {
                        _output.WriteLine(vmsg);
                    }
                }
                Assert.True(compareResult.IsEqual);
            }

            {
                // Test StrictVariables
                var template = Template.Parse("Test with a dynamic {{ myvar2 }}");
                context.StrictVariables = true;
                var exception = Assert.Throws<ScriptRuntimeException>(() => context.Evaluate(template.Page));
                var result = exception.ToString();
                var check = string.Format(SRS.VariableNotFound, "myvar2");
                Assert.True(result.Contains(check), $"The exception string `{result}` does not contain the expected value");
            }
        }

        [Fact]
        public void TestDynamicMember()
        {
            var template = Template.Parse("Test with a dynamic {{ a.myvar }}");

            var globalObject = new ScriptObject();
            globalObject.SetValue("a", new ScriptObject(), true);

            var context = new TemplateContext
            {
                TryGetMember = (TemplateContext localContext, SourceSpan span, object target, string member, out object value) =>
                {
                    value = null;
                    if (member == "myvar")
                    {
                        value = "yes";
                        return true;
                    }
                    return false;
                }
            };

            context.PushGlobal(globalObject);
            context.Evaluate(template.Page);
            var result = context.Output.ToString();

            EqualityCompareResult compareResult = TextAssert.Equal("Test with a dynamic yes", result);
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
        public void TestScriptObjectImport()
        {
            {
                var obj = new ScriptObject();
                obj.Import(typeof(MyStaticObject));

                Assert.True(obj.ContainsKey("static_field_a"));
                Assert.Equal("ValueStaticFieldA", obj["static_field_a"]);
                Assert.True(obj.ContainsKey("static_field_b"));
                Assert.Equal("ValueStaticFieldB", obj["static_field_b"]);
                Assert.True(obj.ContainsKey("static_property_a"));
                Assert.Equal("ValueStaticPropertyA", obj["static_property_a"]);
                Assert.True(obj.ContainsKey("static_property_b"));
                Assert.Equal("ValueStaticPropertyB", obj["static_property_b"]);
                Assert.True(obj.ContainsKey("static_yoyo"));
                Assert.False(obj.ContainsKey("invalid"));
            }

            // Test MemberFilterDelegate
            {
                var obj = new ScriptObject();
                obj.Import(typeof(MyStaticObject), filter: member => member.Name.Contains("Property"));

                Assert.False(obj.ContainsKey("static_field_a"));
                Assert.False(obj.ContainsKey("static_field_b"));
                Assert.True(obj.ContainsKey("static_property_a"));
                Assert.Equal("ValueStaticPropertyA", obj["static_property_a"]);
                Assert.True(obj.ContainsKey("static_property_b"));
                Assert.Equal("ValueStaticPropertyB", obj["static_property_b"]);
                Assert.False(obj.ContainsKey("static_yoyo"));
                Assert.False(obj.ContainsKey("invalid"));
            }

            // Test MemberRenamerDelegate
            {
                var obj = new ScriptObject();
                obj.Import(typeof(MyStaticObject), renamer: member => member.Name);

                Assert.True(obj.ContainsKey(nameof(MyStaticObject.StaticFieldA)));
                Assert.True(obj.ContainsKey(nameof(MyStaticObject.StaticFieldB)));
                Assert.True(obj.ContainsKey(nameof(MyStaticObject.StaticPropertyA)));
                Assert.Equal("ValueStaticPropertyA", obj[nameof(MyStaticObject.StaticPropertyA)]);
                Assert.True(obj.ContainsKey(nameof(MyStaticObject.StaticPropertyB)));
                Assert.Equal("ValueStaticPropertyB", obj[nameof(MyStaticObject.StaticPropertyB)]);
                Assert.True(obj.ContainsKey(nameof(MyStaticObject.StaticYoyo)));
                Assert.False(obj.ContainsKey(nameof(MyStaticObject.Invalid)));
            }

            {
                var obj = new ScriptObject();
                obj.Import(new MyObject2(), renamer: member => member.Name);

                Assert.Equal(9, obj.Count);
                Assert.True(obj.ContainsKey(nameof(MyStaticObject.StaticFieldA)));
                Assert.True(obj.ContainsKey(nameof(MyObject.PropertyA)));
                Assert.True(obj.ContainsKey(nameof(MyObject2.PropertyC)));
            }
        }


        [Fact]
        public void TestScriptObjectAccessor()
        {
            {
                var context = new TemplateContext();
                var obj = new MyObject();
                var accessor = context.GetMemberAccessor(obj);

                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "field_a"));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "field_b"));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_a"));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_b"));

                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_field_a"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_field_b"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_property_a"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_property_b"));
            }

            // Test Filter
            {
                var context = new TemplateContext { MemberFilter = member => member is PropertyInfo };
                var obj = new MyObject();
                var accessor = context.GetMemberAccessor(obj);

                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "field_a"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "field_b"));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_a"));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_b"));

                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_field_a"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_field_b"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_property_a"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_property_b"));
            }


            // Test Renamer
            {
                var context = new TemplateContext { MemberRenamer = member => member.Name };
                var obj = new MyObject();
                var accessor = context.GetMemberAccessor(obj);

                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyObject.FieldA)));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyObject.FieldB)));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyObject.PropertyA)));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyObject.PropertyB)));

                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyStaticObject.StaticFieldA)));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyStaticObject.StaticFieldB)));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyStaticObject.StaticPropertyA)));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyStaticObject.StaticPropertyB)));
            }
        }

        [Fact]
        public void TestNullableArgument()
        {
            var template = Template.Parse("{{ tester 'input1' 1 }}");
            var context = new TemplateContext();
            var testerObj = new ScriptObjectWithNullable();
            context.PushGlobal(testerObj);
            var result = template.Render(context);

            EqualityCompareResult compareResult = TextAssert.Equal("input1 Value: 1", result);
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
        public void TestPropertyInheritance()
        {
            var scriptObject = new ScriptObject
            {
                {"a", new MyObject {PropertyA = "ClassA"}},
                {"b", new MyObject2 {PropertyA = "ClassB", PropertyC = "ClassB-PropC"}}
            };

            var context = new TemplateContext();
            context.PushGlobal(scriptObject);

            var result = Template.Parse("{{a.property_a}}-{{b.property_a}}-{{b.property_c}}").Render(context);

            EqualityCompareResult compareResult = TextAssert.Equal("ClassA-ClassB-ClassB-PropC", result);
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
        public void TestRelaxedMemberAccess()
        {
            var scriptObject = new ScriptObject
            {
                {"a", new MyObject {PropertyA = "A"}}
            };

            // Test unrelaxed member access.
            {
                var context = new TemplateContext();
                context.PushGlobal(scriptObject);
                var result = Template.Parse("{{a.property_a").Render(context);
                Assert.Equal("A", result);
                Assert.Throws<ScriptRuntimeException>(() =>
                    Template.Parse("{{a.property_a.null_ref}}").Render(context));
                Assert.Throws<ScriptRuntimeException>(() =>
                    Template.Parse("{{null_ref.null_ref}}").Render(context));
            }

            // Test relaxed member access.
            {
                var context = new TemplateContext { EnableRelaxedMemberAccess = true };
                context.PushGlobal(scriptObject);
                var result = Template.Parse("{{a.property_a").Render(context);
                Assert.Equal("A", result);
                result = Template.Parse("{{a.property_a.null_ref}}").Render(context);
                Assert.Equal(string.Empty, result);
                result = Template.Parse("{{null_ref.null_ref}}").Render(context);
                Assert.Equal(string.Empty, result);
            }
        }

        [Fact]
        public void TestRelaxedListIndexerAccess()
        {
            var scriptObject = new ScriptObject
            {
                {"list", new List<string> {"value" } }
            };

            // Test unrelaxed indexer access.
            {
                var context = new TemplateContext();
                context.PushGlobal(scriptObject);
                var result = Template.Parse("{{list[0]").Render(context);
                Assert.Equal("value", result);

                Assert.Throws<ScriptRuntimeException>(() =>
                  Template.Parse("{{list[0].null_ref.null_ref}}").Render(context));
                Assert.Throws<ScriptRuntimeException>(() =>
                  Template.Parse("{{list[-1].null_ref}}").Render(context));
                Assert.Throws<ScriptRuntimeException>(() =>
                  Template.Parse("{{null_ref[-1].null_ref}}").Render(context));
            }

            // Test relaxed member access.
            {
                var context = new TemplateContext { EnableRelaxedMemberAccess = true };
                context.PushGlobal(scriptObject);
                var result = Template.Parse("{{list[0]").Render(context);
                Assert.Equal("value", result);
                result = Template.Parse("{{list[0].null_ref.null_ref}}").Render(context);
                Assert.Equal(string.Empty, result);
                result = Template.Parse("{{list[-1].null_ref}}").Render(context);
                Assert.Equal(string.Empty, result);
                result = Template.Parse("{{null_ref[-1].null_ref}}").Render(context);
                Assert.Equal(string.Empty, result);
            }
        }

        [Fact]
        public void TestRelaxedDictionaryIndexerAccess()
        {
            var scriptObject = new ScriptObject
            {
                {"dictionary", new Dictionary<string, string> { { "key", "value" } } }
            };

            // Test unrelaxed indexer access.
            {
                var context = new TemplateContext();
                context.PushGlobal(scriptObject);
                var result = Template.Parse("{{dictionary['key']").Render(context);
                Assert.Equal("value", result);
                Assert.Throws<ScriptRuntimeException>(() =>
                  Template.Parse("{{dictionary['key'].null_ref.null_ref}}").Render(context));
                Assert.Throws<ScriptRuntimeException>(() =>
                  Template.Parse("{{dictionary['null_ref'].null_ref}}").Render(context));
                Assert.Throws<ScriptRuntimeException>(() =>
                  Template.Parse("{{null_ref['null_ref'].null_ref}}").Render(context));
            }

            // Test relaxed member access.
            {
                var context = new TemplateContext { EnableRelaxedMemberAccess = true };
                context.PushGlobal(scriptObject);
                var result = Template.Parse("{{dictionary['key']").Render(context);
                Assert.Equal("value", result);
                result = Template.Parse("{{dictionary['key'].null_ref.null_ref}}").Render(context);
                Assert.Equal(string.Empty, result);
                result = Template.Parse("{{dictionary['null_ref'].null_ref}}").Render(context);
                Assert.Equal(string.Empty, result);
                result = Template.Parse("{{null_ref['null_ref'].null_ref}}").Render(context);
                Assert.Equal(string.Empty, result);
            }
        }

        private class MyObject : MyStaticObject
        {
            public string FieldA = null;

            public string FieldB = null;

            public string PropertyA { get; set; }

            public string PropertyB { get; set; }

        }

        private class MyObject2 : MyObject
        {
            public string PropertyC { get; set; }
        }

        private class MyStaticObject
        {
            static MyStaticObject()
            {
                StaticPropertyA = "ValueStaticPropertyA";
                StaticPropertyB = "ValueStaticPropertyB";
            }

            public static string StaticFieldA = "ValueStaticFieldA";

            public static string StaticFieldB = "ValueStaticFieldB";

            public static string StaticPropertyA { get; set; }

            public static string StaticPropertyB { get; set; }

            public string Invalid()
            {
                return null;
            }

            public static string StaticYoyo(string text)
            {
                return "yoyo " + text;
            }
        }

        public class ScriptObjectWithNullable : ScriptObject
        {
            public static string Tester(string text, int? value = null)
            {
                return value.HasValue ? text + " Value: " + value.Value : text;
            }
        }
    }
}