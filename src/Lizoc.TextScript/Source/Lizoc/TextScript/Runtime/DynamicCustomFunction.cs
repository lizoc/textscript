// -----------------------------------------------------------------------
// <copyright file="DynamicCustomFunction.cs" repo="TextScript">
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
using System.Reflection;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// Creates a reflection based <see cref="IScriptCustomFunction"/> from a <see cref="MethodInfo"/>.
    /// </summary>
    public abstract partial class DynamicCustomFunction : IScriptCustomFunction
    {
        private static readonly Dictionary<MethodInfo, Func<MethodInfo, DynamicCustomFunction>> BuiltinFunctions = new Dictionary<MethodInfo, Func<MethodInfo, DynamicCustomFunction>>(MethodComparer.Default);

        /// <summary>
        /// Gets the reflection method associated to this dynamic call.
        /// </summary>
        public readonly MethodInfo Method;

        protected readonly ParameterInfo[] Parameters;

        protected DynamicCustomFunction(MethodInfo method)
        {
            Method = method;
            Parameters = method.GetParameters();
        }

        protected object GetNamedArgument(TemplateContext context, ScriptNode callerContext, ScriptNamedArgument namedArg, out int argIndex, out Type argType)
        {
            for (int j = 0; j < Parameters.Length; j++)
            {
                ParameterInfo arg = Parameters[j];
                if (arg.Name == namedArg.Name)
                {
                    argIndex = j;
                    argType = arg.ParameterType;
                    return context.Evaluate(namedArg);
                }
            }
            throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.ArgNotFoundForFunc, namedArg.Name, callerContext));
        }

        public abstract object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement);

        /// <summary>
        /// Returns a <see cref="DynamicCustomFunction"/> from the specified object target and <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="target">A target object - might be null</param>
        /// <param name="method">A MethodInfo</param>
        /// <returns>A custom <see cref="DynamicCustomFunction"/></returns>
        public static DynamicCustomFunction Create(object target, MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            Func<MethodInfo, DynamicCustomFunction> newFunction;
            if (target == null && method.IsStatic && BuiltinFunctions.TryGetValue(method, out newFunction))
                return newFunction(method);

            return new GenericFunctionWrapper(target, method);
        }

        /// <summary>
        /// Generic function wrapper handling any kind of function parameters.
        /// </summary>
        private class GenericFunctionWrapper : DynamicCustomFunction
        {
            private readonly object _target;
            private readonly bool _hasObjectParams;
            private readonly int _lastParamsIndex;
            private readonly bool _hasTemplateContext;
            private readonly bool _hasSpan;
            private readonly object[] _arguments;
            private readonly int _optionalParameterCount;
            private readonly Type _paramsElementType;

            public GenericFunctionWrapper(object target, MethodInfo method) : base(method)
            {
                _target = target;
                _lastParamsIndex = Parameters.Length - 1;
                if (Parameters.Length > 0)
                {
                    // Check if we have TemplateContext+SourceSpan as first parameters
                    if (typeof(TemplateContext).GetTypeInfo().IsAssignableFrom(Parameters[0].ParameterType.GetTypeInfo()))
                    {
                        _hasTemplateContext = true;
                        if (Parameters.Length > 1)
                            _hasSpan = typeof(SourceSpan).GetTypeInfo().IsAssignableFrom(Parameters[1].ParameterType.GetTypeInfo());
                    }

                    ParameterInfo lastParam = Parameters[_lastParamsIndex];
                    if (lastParam.ParameterType.IsArray)
                    {
                        foreach (var param in lastParam.GetCustomAttributes(typeof(ParamArrayAttribute), false))
                        {
                            _hasObjectParams = true;
                            _paramsElementType = lastParam.ParameterType.GetElementType();
                            break;
                        }
                    }
                }

                if (!_hasObjectParams)
                {
                    for (int i = 0; i < Parameters.Length; i++)
                    {
                        if (Parameters[i].IsOptional)
                            _optionalParameterCount++;
                    }
                }

                _arguments = new object[Parameters.Length];
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                int expectedNumberOfParameters = Parameters.Length;
                if (_hasTemplateContext)
                {
                    expectedNumberOfParameters--;
                    if (_hasSpan)
                        expectedNumberOfParameters--;
                }

                int minimumRequiredParameters = expectedNumberOfParameters - _optionalParameterCount;

                // Check parameters
                if ((_hasObjectParams && arguments.Count < minimumRequiredParameters - 1) || (!_hasObjectParams && arguments.Count < minimumRequiredParameters))
                {
                    if (minimumRequiredParameters != expectedNumberOfParameters)
                        throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.FuncMinArgCountMismatch, arguments.Count, callerContext, minimumRequiredParameters));
                    else
                        throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.FuncArgCountMismatch, arguments.Count, callerContext, expectedNumberOfParameters));
                }

                // Convert arguments
                object[] paramArguments = null;
                if (_hasObjectParams)
                {
                    paramArguments = new object[arguments.Count - _lastParamsIndex];
                    _arguments[_lastParamsIndex] = paramArguments;
                }

                // Copy TemplateContext/SourceSpan parameters
                int argOffset = 0;
                int argMask = 0;
                if (_hasTemplateContext)
                {
                    _arguments[0] = context;
                    argOffset++;
                    argMask |= 1;
                    if (_hasSpan)
                    {
                        _arguments[1] = callerContext.Span;
                        argOffset++;
                        argMask |= 2;
                    }
                }

                int argOrderedIndex = argOffset;

                // Setup any default parameters
                if (_optionalParameterCount > 0)
                {
                    for (int i = Parameters.Length - 1; i >= Parameters.Length - _optionalParameterCount; i--)
                    {
                        _arguments[i] = Parameters[i].DefaultValue;
                        argMask |= 1 << i;
                    }
                }

                int paramsIndex = 0;
                for (int i = 0; i < arguments.Count; i++)
                {
                    Type argType = null;
                    try
                    {
                        int argIndex;
                        var arg = arguments[i];
                        var namedArg = arg as ScriptNamedArgument;
                        if (namedArg != null)
                        {
                            arg = GetNamedArgument(context, callerContext, namedArg, out argIndex, out argType);
                            if (_hasObjectParams && argIndex == _lastParamsIndex)
                            {
                                argType = _paramsElementType;
                                argIndex = argIndex + paramsIndex;
                                paramsIndex++;
                            }
                        }
                        else
                        {
                            argIndex = argOrderedIndex;
                            if (_hasObjectParams && argIndex == _lastParamsIndex)
                            {
                                argType = _paramsElementType;
                                argIndex = argIndex + paramsIndex;
                                paramsIndex++;
                            }
                            else
                            {
                                argType = Parameters[argIndex].ParameterType;
                                argOrderedIndex++;
                            }
                        }

                        var argValue = context.ToObject(callerContext.Span, arg, argType);
                        if (paramArguments != null && argIndex >= _lastParamsIndex)
                        {
                            paramArguments[argIndex - _lastParamsIndex] = argValue;
                            argMask |= 1 << _lastParamsIndex;
                        }
                        else
                        {
                            _arguments[argIndex] = argValue;
                            argMask |= 1 << argIndex;
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.FuncCastArgTypeError, i, arguments[i]?.GetType(), argType), exception);
                    }
                }

                // In case we have named arguments we need to verify that all arguments were set
                if (argMask != (1 << Parameters.Length) - 1)
                {
                    if (minimumRequiredParameters != expectedNumberOfParameters)
                        throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.FuncMinArgCountMismatch, arguments.Count, callerContext, minimumRequiredParameters));
                    else
                        throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.FuncArgCountMismatch, arguments.Count, callerContext, expectedNumberOfParameters));
                }

                // Call method
                try
                {
                    var result = Method.Invoke(_target, _arguments);
                    return result;
                }
                catch (TargetInvocationException exception)
                {
                    throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.CallContextError, callerContext), exception.InnerException);
                }
            }
        }

        private class MethodComparer : IEqualityComparer<MethodInfo>
        {
            public static readonly MethodComparer Default = new MethodComparer();

            public bool Equals(MethodInfo method, MethodInfo otherMethod)
            {
                if (method != null && otherMethod != null && method.ReturnType == otherMethod.ReturnType && method.IsStatic == otherMethod.IsStatic)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    ParameterInfo[] otherParameters = otherMethod.GetParameters();
                    int length = parameters.Length;
                    if (length == otherParameters.Length)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            ParameterInfo param = parameters[i];
                            ParameterInfo otherParam = otherParameters[i];
                            if (param.ParameterType != otherParam.ParameterType || param.IsOptional != otherParam.IsOptional)
                                return false;
                        }
                        return true;
                    }
                }
                return false;
            }

            public int GetHashCode(MethodInfo method)
            {
                int hash = method.ReturnType.GetHashCode();
                if (!method.IsStatic)
                    hash = (hash * 397) ^ 1;

                ParameterInfo[] parameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    hash = (hash * 397) ^ parameters[i].ParameterType.GetHashCode();
                }
                return hash;
            }
        }
    }
}