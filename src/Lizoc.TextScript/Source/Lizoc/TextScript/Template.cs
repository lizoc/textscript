// -----------------------------------------------------------------------
// <copyright file="Template.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript
{
    /// <summary>
    /// Basic entry point class to parse templates and render them. For more advanced scenario, you should use <see cref="TemplateContext"/> directly.
    /// </summary>
    public class Template
    {
        private readonly ParserOptions _parserOptions;
        private readonly LexerOptions _lexerOptions;

        private Template(ParserOptions? parserOptions, LexerOptions? lexerOptions, string sourceFilePath)
        {
            _parserOptions = parserOptions ?? new ParserOptions();
            _lexerOptions = lexerOptions ?? new LexerOptions();
            Messages = new List<LogMessage>();
            this.SourceFilePath = sourceFilePath;
        }

        /// <summary>
        /// Gets the source file path.
        /// </summary>
        public string SourceFilePath { get; }

        /// <summary>
        /// Gets the resulting compiled <see cref="ScriptPage"/>. May be null if this template <see cref="HasErrors"/> 
        /// </summary>
        public ScriptPage Page { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this template has errors. Error messages are in <see cref="Messages"/>.
        /// </summary>
        public bool HasErrors { get; private set; }

        /// <summary>
        /// Gets the lexer and parsing messages.
        /// </summary>
        public List<LogMessage> Messages { get; private set; }

        /// <summary>
        /// Parses the specified scripting text into a <see cref="Template"/> .
        /// </summary>
        /// <param name="text">The scripting text.</param>
        /// <param name="sourceFilePath">The source file path. Optional, used for better error reporting if the source file has a location on the disk</param>
        /// <param name="parserOptions">The templating parsing parserOptions.</param>
        /// <param name="lexerOptions">The options passed to the lexer</param>
        /// <returns>A template</returns>
        public static Template Parse(string text, string sourceFilePath = null, ParserOptions? parserOptions = null, LexerOptions? lexerOptions = null)
        {
            Template template = new Template(parserOptions, lexerOptions, sourceFilePath);
            template.ParseInternal(text, sourceFilePath);
            return template;
        }

        /// <summary>
        /// Parses the specified Liquid script text into a <see cref="Template"/> .
        /// </summary>
        /// <param name="text">The liquid scripting text.</param>
        /// <param name="sourceFilePath">The source file path. Optional, used for better error reporting if the source file has a location on the disk</param>
        /// <param name="parserOptions">The templating parsing parserOptions.</param>
        /// <param name="lexerOptions">The options passed to the lexer</param>
        /// <returns>A template</returns>
        public static Template ParseLiquid(string text, string sourceFilePath = null, ParserOptions? parserOptions = null, LexerOptions? lexerOptions = null)
        {
            LexerOptions localLexerOptions = lexerOptions ?? new LexerOptions();
            localLexerOptions.Mode = ScriptMode.Liquid;
            return Parse(text, sourceFilePath, parserOptions, localLexerOptions);
        }

        /// <summary>
        /// Parse and evaluates a code only expression (without enclosing `{{` and `}}`) within the specified context.
        /// </summary>
        /// <param name="expression">A code only expression (without enclosing `{{` and `}}`)</param>
        /// <param name="context">The template context</param>
        /// <returns>The result of the evaluation of the expression</returns>
        public static object Evaluate(string expression, TemplateContext context)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            LexerOptions lexerOption = new LexerOptions() {Mode = ScriptMode.ScriptOnly};
            Template template = Parse(expression, lexerOptions: lexerOption);
            return template.Evaluate(context);
        }

        /// <summary>
        /// Parse and evaluates a code only expression (without enclosing `{{` and `}}`) within the specified context.
        /// </summary>
        /// <param name="expression">A code only expression (without enclosing `{{` and `}}`)</param>
        /// <param name="model">An object instance used as a model for evaluating this expression</param>
        /// <param name="memberRenamer">This is a custom function to modify the imported object and transitive objects. See the documentation for details.</param>
        /// <param name="memberFilter">This is a custom function to filter members for the objects being accessed through the template, including the model being passed in.</param>
        /// <returns>The result of the evaluation of the expression</returns>
        public static object Evaluate(string expression, object model, MemberRenamerDelegate memberRenamer = null, MemberFilterDelegate memberFilter = null)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            LexerOptions lexerOption = new LexerOptions() { Mode = ScriptMode.ScriptOnly };
            Template template = Parse(expression, lexerOptions: lexerOption);

            return template.Evaluate(model, memberRenamer, memberFilter);
        }

        /// <summary>
        /// Evaluates the template using the specified context. See remarks.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <exception cref="ArgumentNullException">If context is null</exception>
        /// <exception cref="InvalidOperationException">If the template <see cref="HasErrors"/>. Check the <see cref="Messages"/> property for more details</exception>
        /// <returns>Returns the result of the last statement</returns>
        public object Evaluate(TemplateContext context)
        {
            bool previousOutput = context.EnableOutput;
            try
            {
                context.EnableOutput = false;
                return EvaluateAndRender(context, false);
            }
            finally
            {
                context.EnableOutput = previousOutput;
            }        
        }

        /// <summary>
        /// Evaluates the template using the specified context
        /// </summary>
        /// <param name="model">An object model to use with the evaluation.</param>
        /// <param name="memberRenamer">This is a custom function to modify the imported object and transitive objects. See the documentation for details.</param>
        /// <param name="memberFilter">This is a custom function to filter members for the objects being accessed through the template, including the model being passed in.</param>
        /// <exception cref="InvalidOperationException">If the template <see cref="HasErrors"/>. Check the <see cref="Messages"/> property for more details</exception>
        /// <returns>Returns the result of the last statement</returns>
        public object Evaluate(object model = null, MemberRenamerDelegate memberRenamer = null, MemberFilterDelegate memberFilter = null)
        {
            ScriptObject scriptObject = new ScriptObject();
            if (model != null)
                scriptObject.Import(model, memberFilter, memberRenamer);

            TemplateContext context = new TemplateContext
            {
                EnableOutput = false,
                MemberRenamer = memberRenamer,
                MemberFilter = memberFilter
            };
            context.PushGlobal(scriptObject);
            var result = Evaluate(context);
            context.PopGlobal();
            return result;
        }
        
        /// <summary>
        /// Renders this template using the specified context. See remarks.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <exception cref="ArgumentNullException">If context is null</exception>
        /// <exception cref="InvalidOperationException">If the template <see cref="HasErrors"/>. Check the <see cref="Messages"/> property for more details</exception>
        /// <remarks>
        /// When using this method, the result of rendering this page is output to <see cref="TemplateContext.Output"/>
        /// </remarks>
        public string Render(TemplateContext context)
        {
            EvaluateAndRender(context, true);
            string result = context.Output.ToString();
            StringBuilderOutput output = context.Output as StringBuilderOutput;
            if (output != null)
                output.Builder.Length = 0;

            return result;
        }

        /// <summary>
        /// Renders this template using the specified object model.
        /// </summary>
        /// <param name="model">The object model.</param>
        /// <param name="memberRenamer">This is a custom function to modify the imported object and transitive objects. See the documentation for details.</param>
        /// <param name="memberFilter">This is a custom function to filter members for the objects being accessed through the template, including the model being passed in.</param>
        /// <returns>A rendering result as a string </returns>
        public string Render(object model = null, MemberRenamerDelegate memberRenamer = null, MemberFilterDelegate memberFilter = null)
        {
            ScriptObject scriptObject = new ScriptObject();
            if (model != null)
                scriptObject.Import(model, memberFilter, memberRenamer);

            var context = _lexerOptions.Mode == ScriptMode.Liquid ? new LiquidTemplateContext() : new TemplateContext();
            context.MemberRenamer = memberRenamer;
            context.MemberFilter = memberFilter;
            context.PushGlobal(scriptObject);
            return Render(context);
        }

        /// <summary>
        /// Converts back this template to a textual representation. This is the inverse of <see cref="Parse"/>.
        /// </summary>
        /// <param name="options">The rendering options</param>
        /// <returns>The template converted back to a textual representation of the template</returns>
        public string ToText(TemplateRewriterOptions options = default(TemplateRewriterOptions))
        {
            CheckErrors();
            TextWriterOutput writer = new TextWriterOutput();
            TemplateRewriterContext renderContext = new TemplateRewriterContext(writer, options);
            renderContext.Write(Page);

            return writer.ToString();
        }

        /// <summary>
        /// Evaluates the template using the specified context. See remarks.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="render"><c>true</c> to render the output to the <see cref="TemplateContext.Output"/></param>
        /// <exception cref="ArgumentNullException">If context is null</exception>
        /// <exception cref="InvalidOperationException">If the template <see cref="HasErrors"/>. Check the <see cref="Messages"/> property for more details</exception>
        private object EvaluateAndRender(TemplateContext context, bool render)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            CheckErrors();

            // Make sure that we are using the same parserOptions
            if (SourceFilePath != null)
                context.PushSourceFile(SourceFilePath);

            try
            {
                var result = context.Evaluate(Page);
                if (render)
                {
                    if (Page != null && context.EnableOutput && result != null)
                        context.Write(Page.Span, result);
                }
                return result;
            }
            finally
            {
                if (SourceFilePath != null)
                    context.PopSourceFile();
            }
        }

        private void CheckErrors()
        {
            if (HasErrors)
                throw new InvalidOperationException(string.Format(RS.TemplateHasErrors, StringHelper.Join("\n", Messages)));
        }

        private void ParseInternal(string text, string sourceFilePath)
        {
            // Early exit
            if (string.IsNullOrEmpty(text))
            {
                HasErrors = false;
                Messages = new List<LogMessage>();
                Page = new ScriptPage() { Span = new SourceSpan(sourceFilePath, new TextPosition(), TextPosition.Eof) };
                return;
            }

            Lexer lexer = new Lexer(text, sourceFilePath, _lexerOptions);
            Parser parser = new Parser(lexer, _parserOptions);

            Page = parser.Run();

            HasErrors = parser.HasErrors;
            Messages = parser.Messages;
        }
    }
}