﻿// -----------------------------------------------------------------------
// <copyright file="ReflectionHelper.cs" repo="TextScript">
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

namespace Lizoc.TextScript
{
    internal static class ReflectionHelper
    {
#if NETFX
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }

        public static bool IsPrimitiveOrDecimal(this Type type)
        {
            return type.IsPrimitive || type == typeof(decimal);
        }

        public static Type GetBaseOrInterface(this Type type, Type lookInterfaceType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (lookInterfaceType == null)
                throw new ArgumentNullException(nameof(lookInterfaceType));

            if (lookInterfaceType.IsGenericTypeDefinition)
            {
                if (lookInterfaceType.IsInterface)
                {
                    foreach (Type interfaceType in type.GetInterfaces())
                    {
                        if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == lookInterfaceType)
                            return interfaceType;
                    }
                }

                for (Type t = type; t != null; t = t.BaseType)
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == lookInterfaceType)
                        return t;
                }
            }
            else
            {
                if (lookInterfaceType.IsAssignableFrom(type))
                    return lookInterfaceType;
            }

            return null;
        }

        public static Delegate CreateDelegate(this MethodInfo method, Type type)
        {
            return Delegate.CreateDelegate(type, method);
        }

        public static IEnumerable<FieldInfo> GetDeclaredFields(this Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static);
        }
        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static);
        }
        public static object GetValue(this PropertyInfo propInfo, object obj)
        {
            return propInfo.GetValue(obj, null);
        }
        public static void SetValue(this PropertyInfo propInfo, object obj, object value)
        {
            propInfo.SetValue(obj, value, null);
        }
        public static MethodInfo GetDeclaredMethod(this Type type, string name)
        {
            return type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }
        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type)
        {
            return type.GetMethods(BindingFlags.Public| BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly);
        }
        public static T GetCustomAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            foreach (var attribute in memberInfo.GetCustomAttributes(true))
            {
                var attributeT = attribute as T;
                if (attributeT != null)
                    return attributeT;
            }
            return (T)null;
        }
        public static MethodInfo GetMethodInfo(this Delegate del)
        {
            return del.Method;
        }

#elif NETSTANDARD || NETSTANDARD2 || UAP
        public static bool IsPrimitiveOrDecimal(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive || type == typeof(decimal);
        }

        public static Type GetBaseOrInterface(this Type type, Type lookInterfaceTypeArg)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (lookInterfaceTypeArg == null)
                throw new ArgumentNullException(nameof(lookInterfaceTypeArg));

            TypeInfo lookInterfaceType = lookInterfaceTypeArg.GetTypeInfo();

            if (lookInterfaceType.IsGenericTypeDefinition)
            {
                if (lookInterfaceType.IsInterface)
                {
                    foreach (var interfaceType in type.GetTypeInfo().ImplementedInterfaces)
                    {
                        if (interfaceType.GetTypeInfo().IsGenericType &&
                            interfaceType.GetTypeInfo().GetGenericTypeDefinition() == lookInterfaceTypeArg)
                        {
                            return interfaceType;
                        }
                    }
                }

                for (Type t = type; t != null; t = t.GetTypeInfo().BaseType)
                {
                    if (t.GetTypeInfo().IsGenericType &&
                        t.GetTypeInfo().GetGenericTypeDefinition() == lookInterfaceTypeArg)
                    {
                        return t;
                    }
                }
            }
            else
            {
                if (lookInterfaceType.IsAssignableFrom(type.GetTypeInfo()))
                    return lookInterfaceTypeArg;
            }

            return null;
        }

        public static Type[] GetGenericArguments(this TypeInfo type)
        {
            return type.GenericTypeArguments;
        }

        public static IEnumerable<FieldInfo> GetDeclaredFields(this TypeInfo type)
        {
            return type.DeclaredFields;
        }
        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this TypeInfo type)
        {
            return type.DeclaredProperties;
        }
        public static IEnumerable<MethodInfo> GetDeclaredMethods(this TypeInfo type)
        {
            return type.DeclaredMethods;
        }
#endif
#if NETSTANDARD && !UAP
        public static MethodInfo GetGetMethod(this PropertyInfo prop)
        {
            return prop.GetMethod;
        }
        public static MethodInfo GetSetMethod(this PropertyInfo prop)
        {
            return prop.SetMethod;
        }
#endif
    }
}
