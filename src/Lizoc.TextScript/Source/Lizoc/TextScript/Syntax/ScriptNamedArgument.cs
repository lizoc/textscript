// -----------------------------------------------------------------------
// <copyright file="ScriptNamedArgument.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Lizoc.TextScript.Syntax
{
    public class ScriptNamedArgument : ScriptExpression
    {
        public ScriptNamedArgument()
        {
        }

        public ScriptNamedArgument(string name)
        {
            Name = name;
        }

        public ScriptNamedArgument(string name, ScriptExpression value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public ScriptExpression Value { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            if (Value != null)
                return context.Evaluate(Value);

            return true;
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (Name == null)
                return;

            context.Write(Name);

            if (Value != null)
            {
                context.Write(":");
                context.Write(Value);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, Value);
        }
    }
}