﻿// -----------------------------------------------------------------------
// <copyright file="CodeGenerator.cs" repo="TextScript">
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
using Mono.Cecil;

namespace TextScriptCodeGen
{
    internal class CodeGenerator
    {
        private readonly AssemblyDefinition _assemblyDefinition;
        private readonly Dictionary<string, MethodDefinition> _methods;
        private TextWriter _writer;

        private const string BaseNamespace = "Lizoc.TextScript";
        private const string TargetNamespacePrefix = "Lizoc.TextScript.Functions";
        private List<string> ExposeClassNames = new List<string>()
        {
        	"ArrayFunctions",
        	"DateTimeFunctions",
        	"HtmlFunctions",
        	"MathFunctions",
        	"ObjectFunctions",
        	"RegexFunctions",
        	"StringFunctions",
        	"TimeSpanFunctions",
        	"FileSystemFunctions",
        };

        private string _assemblyPath;
        private string _outputCodePath;

        public CodeGenerator(string assemblyPath, string outputCodePath)
        {
            _assemblyPath = assemblyPath;
            _outputCodePath = outputCodePath;

            _assemblyDefinition = AssemblyDefinition.ReadAssembly(_assemblyPath);
            _methods = new Dictionary<string, MethodDefinition>();
        }

        public void GenerateCode(string assemblyPath, string outputCodePath)
        {
        	foreach (string className in ExposeClassNames)
        	{
        		CollectMethods(TargetNamespacePrefix + "." + className);
        	}

        	_writer = new StreamWriter(outputCodePath);

        	string headerComment = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 4.0.30319.42000
//     Custom Build Tool: TextScriptCodeGen
//     Last Updated: {0}
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//            
//     WARNING! DO NOT EDIT MANUALLY. To generate this file, you need to build 
//     Lizoc.TextScript.dll first. then use TextScriptCodeGen to generate this 
//     file. Replace this file with the generated output and rebuild the project.
//     You only need to update this file if you made changes to any file in 
//     the 'Lizoc.TextScript.Functions' namespace.
// </auto-generated>
//------------------------------------------------------------------------------
";

			string classPrefix = @"using System;
using System.Collections;
using System.Reflection;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Syntax;
using TSF = Lizoc.TextScript.Functions;
using RS = Lizoc.TextScript.RS;

namespace Lizoc.TextScript.Runtime
{
    public abstract partial class DynamicCustomFunction
    {
";

			string classSuffix = @"
	}
}
";

			string ctorPrefix = @"		static DynamicCustomFunction()
        {
";

			string ctorSuffix = @"
		}
";

			string ctorAddDelegate = "			BuiltinFunctions.Add(typeof({0}).GetTypeInfo().GetDeclaredMethod(nameof({0}.{1})), method => new {2}(method));";

            _writer.WriteLine(string.Format(headerComment, DateTime.Now));
            _writer.WriteLine(classPrefix);

            _writer.WriteLine(ctorPrefix);           
            foreach (var keyPair in _methods.OrderBy(s => s.Key))
            {
                var method = keyPair.Value;
                var name = "Function_" + GetSignature(method, SignatureMode.Name);
                var methodName = method.Name;

                _writer.WriteLine(ctorAddDelegate, "TSF." + method.DeclaringType.Name, methodName, name);
            }
            _writer.WriteLine(ctorSuffix);

            foreach (var keyPair in _methods.OrderBy(s => s.Key))
            {
                DumpMethod(keyPair.Key, keyPair.Value);
            }

            _writer.WriteLine(classSuffix);
            _writer.WriteLine();
            _writer.Flush();
            _writer.Close();
        }

        private void CollectMethods(string type)
        {
            var typeDefinition = _assemblyDefinition.MainModule.GetType(type);
            if (typeDefinition == null)
                throw new InvalidOperationException(string.Format("Unable to find type {0}", type));
            
            foreach (var method in typeDefinition.Methods)
            {
                if (method.IsConstructor || !method.IsPublic || !method.IsStatic || method.IsGetter)
                    continue;

                if (method.Parameters.Any(p => p.ParameterType.IsGenericInstance) || method.ReturnType.IsGenericInstance)
                    continue;

                if (method.CustomAttributes.Any(attr => attr.Constructor.Name == "ScriptMemberIgnore"))
                    continue;

                var signature = GetSignature(method, SignatureMode.Verbose);
                if (!_methods.ContainsKey(signature))
                    _methods.Add(signature, method);
            }
        }

		private void DumpMethod(string signature, MethodDefinition method)
        {
            var name = "Function_" + GetSignature(method, SignatureMode.Name);
            var delegateSignature = GetSignature(method, SignatureMode.Delegate);

            var arguments = new StringBuilder();
            var caseArguments = new StringBuilder();
            var delegateCallArgs = new StringBuilder();
            var defaultParamDeclaration = new StringBuilder();
            var defaultParamConstructors = new StringBuilder();
            
            int argumentCount = 0;
            int argOffset = 0;
            for (var i = 0; i < method.Parameters.Count; i++)
            {
                var arg = method.Parameters[i];
                var type = arg.ParameterType;
                if (type.Name == "TemplateContext" || type.Name == "SourceSpan")
                {
                    argOffset++;
                    continue;
                }
                argumentCount++;
            }
            int minimunArg = argumentCount;

            string argCheckMask = string.Format("argMask != (1 << {0}) - 1", argumentCount);

            int argIndex = 0;
            for (var paramIndex = 0; paramIndex < method.Parameters.Count; paramIndex++)
            {
                var arg = method.Parameters[paramIndex];
                var type = arg.ParameterType;

                if (paramIndex > 0)
                    delegateCallArgs.Append(", ");

                if (type.Name == "TemplateContext")
                {
                    delegateCallArgs.Append("context");
                    continue;
                }

                if (type.Name == "SourceSpan")
                {
                    delegateCallArgs.Append("callerContext.Span");
                    continue;
                }

                arguments.Append(string.Format("                var arg{0} = ", argIndex));
                caseArguments.AppendLine(string.Format("                        case {0}:", argIndex));
                caseArguments.Append(string.Format("                            arg{0} = ", argIndex));

                if (arg.IsOptional)
                {
                    if (argIndex < minimunArg)
                        minimunArg = argIndex;

                    arguments.Append(string.Format("defaultArg{0}", argIndex));
                }
                else
                {
                    arguments.Append(string.Format("default({0})", PrettyType(type)));
                }
                arguments.AppendLine(";");

                if (type.MetadataType == MetadataType.String)
                {
                    caseArguments.Append("context.ToString(callerContext.Span, arg)");
                }
                else if (type.MetadataType == MetadataType.Int32)
                {
                    caseArguments.Append("context.ToInt(callerContext.Span, arg)");
                }
                else 
                {
                    if (type.MetadataType != MetadataType.Object)
                    {
                        if (type.Name == "IList")
                            caseArguments.Append("context.ToList(callerContext.Span, arg)");
                        else
                            caseArguments.Append(string.Format("({0})context.ToObject(callerContext.Span, arg, typeof({0}))", PrettyType(type)));
                    }
                    else
                    {
                        caseArguments.Append("arg");
                    }
                }
                caseArguments.AppendLine(";");

                // If argument is optional, we don't need to update the mask as it is aslready taken into account into the mask init
                if (!arg.IsOptional)
                    caseArguments.AppendLine(string.Format("                            argMask |= (1 << {0});", argIndex));

                caseArguments.AppendLine("                            break;");

                delegateCallArgs.Append(string.Format("arg{0}", argIndex));
                argIndex++;
            }

            // Outptu default argument masking
            var defaultArgMask = 0;
            for (int i = minimunArg; i < argumentCount; i++)
            {
                defaultArgMask |= (1 << i);
            }
            arguments.AppendLine(string.Format("                int argMask = {0};", defaultArgMask));

            var argCheck = "arguments.Count";
            string errorResName = "BadFunctionInvokeArgCountExact"; // exactly n parameters must be specified
            if (minimunArg != argumentCount)
            {
                errorResName = "BadFunctionInvokeArgCountMin"; // at least n parameters must be specified
                argCheck += " < " + minimunArg;
                argCheck += " || arguments.Count > " + argumentCount;

                for (var i = 0; i < method.Parameters.Count; i++)
                {
                    var arg = method.Parameters[i];

                    if (arg.IsOptional)
                    {
                        argIndex = i - argOffset;
                        defaultParamDeclaration.AppendLine();
                        defaultParamDeclaration.Append(string.Format("            private readonly {0} defaultArg{1};", PrettyType(arg.ParameterType), argIndex));
                        defaultParamConstructors.AppendLine();
                        defaultParamConstructors.Append(string.Format("                defaultArg{0} = ({1})Parameters[{2}].DefaultValue;", argIndex, PrettyType(arg.ParameterType), i));
                    }
                }
            }
            else
            {
                argCheck += " != " + argumentCount;
            }

            string caseBlockBegin = @"
                    switch (argIndex)
                    {
";
            string caseBlockEnd = @"
                    }
";

            if (caseArguments.Length == 0)
            {
                caseBlockBegin = string.Empty;
                caseBlockEnd = string.Empty;
            }

            var template = $@"
        /// <summary>
        /// Optimized custom function for: {signature}
        /// </summary>
        private class {name} : DynamicCustomFunction
        {{
            private delegate {delegateSignature};

            private readonly InternalDelegate _delegate;{defaultParamDeclaration}

            public {name}(MethodInfo method) 
            	: base(method)
            {{
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));{defaultParamConstructors}
            }}

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {{
                if ({argCheck})
                {{
                    string argCheckConditionError = string.Format(RS.{errorResName}, callerContext, arguments.Count, {minimunArg});
                    throw new ScriptRuntimeException(callerContext.Span, argCheckConditionError);
                }}
{arguments}
                int argOrderedIndex = 0;
                for (int i = 0; i < arguments.Count; i++)
                {{
                    int argIndex = 0;
                    var arg = arguments[i];
                    var namedArg = arg as ScriptNamedArgument;
                    if (namedArg != null)
                    {{
                        Type argType;
                        arg = GetNamedArgument(context, callerContext, namedArg, out argIndex, out argType);
                        argIndex -= {argOffset};
                    }}
                    else
                    {{
                        argIndex = argOrderedIndex;
                        argOrderedIndex++;
                    }}

{caseBlockBegin}
{caseArguments}
{caseBlockEnd}
                }}

                if ({argCheckMask})
                {{
                    string argCheckMaskError = string.Format(RS.{errorResName}, callerContext, arguments.Count, {minimunArg});
                    throw new ScriptRuntimeException(callerContext.Span, argCheckMaskError);
                }}

                return _delegate({delegateCallArgs});
            }}
        }}
";

            _writer.Write(template);
        }

        private string GetSignature(MethodDefinition method, SignatureMode mode)
        {
            var text = new StringBuilder();
            text.Append(PrettyType(method.ReturnType));

            if (mode == SignatureMode.Verbose)
                text.Append(" (");
            else if (mode == SignatureMode.Delegate)
                text.Append(" InternalDelegate(");

            for (var i = 0; i < method.Parameters.Count; i++)
            {
                var parameter = method.Parameters[i];
                if (i > 0)
                {
                    if (mode != SignatureMode.Name)
                        text.Append(", ");
                }

                if (mode == SignatureMode.Name)
                    text.Append("_");

                text.Append(PrettyType(parameter.ParameterType));

                if (mode == SignatureMode.Delegate)
                    text.Append($" arg{i}");

                if (parameter.IsOptional)
                {
                    if (mode == SignatureMode.Verbose)
                        text.Append(" = ...");
                    else if (mode == SignatureMode.Name)
                        text.Append("___Opt");
                }

                // TODO: Handle params
            }

            if (mode != SignatureMode.Name)
                text.Append(")");

            return text.ToString();
        }

        private string PrettyType(TypeReference typeReference)
        {
            switch (typeReference.MetadataType)
            {
                case MetadataType.String:
                    return "string";
                case MetadataType.Double:
                    return "double";
                case MetadataType.Single:
                    return "float";
                case MetadataType.Byte:
                    return "byte";
                case MetadataType.SByte:
                    return "sbyte";
                case MetadataType.Int16:
                    return "short";
                case MetadataType.UInt16:
                    return "ushort";
                case MetadataType.Int32:
                    return "int";
                case MetadataType.UInt32:
                    return "uint";
                case MetadataType.Int64:
                    return "long";
                case MetadataType.UInt64:
                    return "ulong";
                case MetadataType.Boolean:
                    return "bool";
                case MetadataType.Object:
                    return "object";
                case MetadataType.Void:
                    return "void";
            }

            return typeReference.Namespace.StartsWith(BaseNamespace) || typeReference.Namespace == "System" || typeReference.Namespace == "System.Collections"
                ? typeReference.Name
                : typeReference.FullName;
        }
    }

    internal enum SignatureMode
    {
        Verbose,
        Name,
        Delegate,
    }
}
