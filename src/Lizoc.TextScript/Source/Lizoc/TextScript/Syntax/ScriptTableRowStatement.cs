// -----------------------------------------------------------------------
// <copyright file="ScriptTableRowStatement.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Syntax
{
    /// <summary>
    /// Statement handling the `tablerow`
    /// </summary>
    public class ScriptTableRowStatement : ScriptForStatement
    {
        private int _columnsCount;

        public ScriptTableRowStatement()
        {
            _columnsCount = 1;
        }

        protected override void ProcessArgument(TemplateContext context, ScriptNamedArgument argument)
        {
            _columnsCount = 1;
            if (argument.Name == "cols")
            {
                _columnsCount = context.ToInt(argument.Value.Span, context.Evaluate(argument.Value));
                if (_columnsCount <= 0)
                    _columnsCount = 1;

                return;
            }
            base.ProcessArgument(context, argument);
        }

        protected override void BeforeLoop(TemplateContext context)
        {
            context.Write("<tr class=\"row1\">");
        }

        protected override void AfterLoop(TemplateContext context)
        {
            context.Write("</tr>").WriteLine();
        }

        protected override bool Loop(TemplateContext context, int index, int localIndex, bool isLast)
        {
            IScriptOutput output = context.Output;

            int columnIndex = localIndex % _columnsCount;

            context.SetValue(ScriptVariable.TableRowCol, columnIndex + 1);

            if (columnIndex == 0 && localIndex > 0)
            {
                output.Write("</tr>").WriteLine();
                output.Write("<tr class=\"row").Write((localIndex / _columnsCount) + 1).Write("\">");
            }
            output.Write("<td class=\"col").Write(columnIndex + 1).Write("\">");

            bool result = base.Loop(context, index, localIndex, isLast);

            output.Write("</td>");

            return result;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("tablerow").ExpectSpace();
            context.Write(Variable).ExpectSpace();
            context.Write("in").ExpectSpace();
            context.Write(Iterator);
            context.Write(NamedArguments);
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }
    }
}