using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Lizoc.XmlDoc;
using Lizoc.TextScript;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Functions;
using Lizoc.TextScript.Parsing;

namespace TextScriptDocGen
{
    internal class MarkdownVisitor : Visitor
    {
        private readonly Dictionary<string, string> _builtinClassNames;
        private readonly Dictionary<string, ClassWriter> _classWriters;
        private readonly StringWriter _writerToc;

        private StringWriter _writer;
        private StringWriter _writerParameters;
        private StringWriter _writerReturns;
        private StringWriter _writerSummary;
        private StringWriter _writerRemarks;

        public class ClassWriter
        {
            public readonly StringWriter Head;
            public readonly StringWriter Body;

            public ClassWriter()
            {
                Head = new StringWriter();
                Body = new StringWriter();
            }
        }

        public MarkdownVisitor(Dictionary<string, string> builtinClassNames)
        {
            _classWriters = new Dictionary<string, ClassWriter>();
            _writerToc = new StringWriter();
            _writerParameters = new StringWriter();
            _writerReturns = new StringWriter();
            _writerSummary = new StringWriter();
            _writerRemarks = new StringWriter();
            _builtinClassNames = builtinClassNames;
        }

        public StringWriter Toc => _writerToc;

        public Dictionary<string, ClassWriter> ClassWriters => _classWriters;

        private bool IsBuiltinType(Type type, out string shortName)
        {
            shortName = null;
            return (type.Namespace == "Lizoc.TextScript.Functions") && (_builtinClassNames.TryGetValue(type.Name, out shortName));
        }

        public override void VisitMember(Member member)
        {
            var type = member.Info as Type;
            var methodInfo = member.Info as MethodInfo;
            string shortName;

            //                if (type != null && )
            if (type != null && IsBuiltinType(type, out shortName))
            {
                    var classWriter = new ClassWriter();
                    _classWriters[shortName] = classWriter;

                    _writer = classWriter.Head;

                    _writer.WriteLine("[:top:](#builtins)");
                    _writer.WriteLine();
                    _writer.WriteLine("************************************************************************");
                    _writer.WriteLine();
                    _writer.WriteLine(string.Format("{0} functions", shortName));
                    _writer.WriteLine("--------------");
                    _writer.WriteLine();

                    base.VisitMember(member);

                    _writer = classWriter.Head;

                    _writer.WriteLine(_writerSummary);
                    _writer.WriteLine();

                    // Write the toc
                    _writerToc.WriteLine(string.Format("- [`{0}` functions](#{0}-functions)", shortName));
            }
            else if (methodInfo != null && IsBuiltinType(methodInfo.DeclaringType, out shortName))
            {
                var methodShortName = StandardMemberRenamer.Default(methodInfo);

                var classWriter = _classWriters[shortName];

                // Write the toc
                classWriter.Head.WriteLine(string.Format("- [`{0}.{1}`](#{0}{1})", shortName, methodShortName));
                    
                _writer = classWriter.Body;
                _writer.WriteLine();
                _writer.WriteLine("[:top:](#builtins)");
                _writer.WriteLine();
                _writer.WriteLine("************************************************************************");
                _writer.WriteLine();
                _writer.WriteLine(string.Format("### `{0}.{1}`", shortName, methodShortName));
                _writer.WriteLine();
                _writer.WriteLine("#### SYNTAX");
                _writer.WriteLine("```");
                _writer.Write(string.Format("{0}.{1}", shortName, methodShortName));

                foreach (var parameter in methodInfo.GetParameters())
                {
                    if (parameter.ParameterType == typeof(TemplateContext) || parameter.ParameterType == typeof(SourceSpan))
                        continue;

                    _writer.Write(" ");

                    _writer.Write("<" + parameter.Name);
                    if (parameter.IsOptional)
                    {
                        var defaultValue = parameter.DefaultValue;
                        if (defaultValue is string)
                            defaultValue = "\"" + defaultValue + "\"";

                        if (defaultValue != null)
                            defaultValue = ": " + defaultValue;

                        _writer.Write(defaultValue + ">?");
                    }
                    else
                    {
                        _writer.Write(">");
                    }
                }
                _writer.WriteLine();
                _writer.WriteLine("```");
                _writer.WriteLine();

                base.VisitMember(member);

                _writer = classWriter.Body;

                // Write parameters after the signature
                _writer.WriteLine("#### DESCRIPTION");
                _writer.WriteLine(_writerSummary);
                _writer.WriteLine();
                _writer.WriteLine("#### PARAMETERS");
                _writer.WriteLine(_writerParameters);
                _writer.WriteLine();
                _writer.WriteLine("#### RETURNS");
                _writer.WriteLine(_writerReturns);
                _writer.WriteLine();
                _writer.WriteLine("#### EXAMPLES");
                _writer.WriteLine(_writerRemarks);
                _writer.WriteLine();
            }

            _writerSummary = new StringWriter();
            _writerParameters = new StringWriter();
            _writerReturns = new StringWriter();
            _writerRemarks = new StringWriter();
        }

        public override void VisitSummary(Summary summary)
        {
            _writer = _writerSummary;
            base.VisitSummary(summary);
        }

        public override void VisitRemarks(Remarks remarks)
        {
            _writer = _writerRemarks;
            base.VisitRemarks(remarks);
        }

        public override void VisitExample(Example example)
        {
            //base.VisitExample(example);
        }

        public override void VisitC(C code)
        {
            //// Wrap inline code in ` according to Markdown syntax.
            //Console.Write(" `");
            //Console.Write(code.Content);
            //Console.Write("` ");

            base.VisitC(code);
        }

        public override void VisitParam(Param param)
        {
            if (param.Name == "context" || param.Name == "span")
                return;

            _writer = _writerParameters;
            _writer.Write(string.Format("- `{0}`: ", param.Name));
            base.VisitParam(param);
            _writer.WriteLine();
        }

        public override void VisitReturns(Returns returns)
        {
            _writer = _writerReturns;
            base.VisitReturns(returns);
        }

        public override void VisitCode(Code code)
        {
            //base.VisitCode(code);
        }

        public override void VisitText(Text text)
        {
            var content = text.Content;

            string inputPrefix = "> **input**" + Environment.NewLine + "```template-text";
            string outputPrefix = "> **output**" + Environment.NewLine + "```html";

            content = content.Replace("```template-text", inputPrefix);
            content = content.Replace("```html", outputPrefix);

            _writer.Write(content);
        }

        public override void VisitPara(Para para)
        {
            //base.VisitPara(para);
        }

        public override void VisitSee(See see)
        {
            //var cref = NormalizeLink(see.Cref);
            //Console.Write(" [{0}]({1}) ", cref.Substring(2), cref);
        }

        public override void VisitSeeAlso(SeeAlso seeAlso)
        {
            //if (seeAlso.Cref != null)
            //{
            //    var cref = NormalizeLink(seeAlso.Cref);
            //    Console.WriteLine("[{0}]({1})", cref.Substring(2), cref);
            //}
        }
    }
}
