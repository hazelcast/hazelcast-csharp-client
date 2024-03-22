// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#nullable enable

using System;
using System.Reflection;
using System.Reflection.Emit;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;

namespace Hazelcast.Tests.Serialization.Compact
{
    internal static class ReflectionHelper
    {
        public static Type CreateObjectType(params Type[] propertyTypes)
        {
            var assemblyName = new AssemblyName("dynamic");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleName = assemblyName.Name;
            if (moduleName == null) throw new Exception("panic: null module name.");
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
            var typeBuilder = moduleBuilder.DefineType("Thing", TypeAttributes.Public);

            var index = 0;
            foreach (var propertyType in propertyTypes)
            {
                var fieldBuilder = typeBuilder.DefineField("_value", propertyType, FieldAttributes.Private);

                var propertyBuilder = typeBuilder.DefineProperty($"Value{index}", PropertyAttributes.HasDefault, propertyType, null);
                var getsetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

                var getBuilder = typeBuilder.DefineMethod($"get_Value{index}", getsetAttributes, propertyType, Type.EmptyTypes);
                var setBuilder = typeBuilder.DefineMethod($"set_Value{index}", getsetAttributes, null, new[] { propertyType });

                var ilgen = getBuilder.GetILGenerator();
                ilgen.Emit(OpCodes.Ldarg_0);
                ilgen.Emit(OpCodes.Ldfld, fieldBuilder);
                ilgen.Emit(OpCodes.Ret);

                ilgen = setBuilder.GetILGenerator();
                ilgen.Emit(OpCodes.Ldarg_0);
                ilgen.Emit(OpCodes.Ldarg_1);
                ilgen.Emit(OpCodes.Stfld, fieldBuilder);
                ilgen.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getBuilder);
                propertyBuilder.SetSetMethod(setBuilder);

                index++;
            }

            var type = typeBuilder.CreateType();
            if (type == null) throw new Exception("panic: null type.");
            return type;
        }

        public static Type CreateObjectSerializerType(Type objectType, string propertyName, FieldKind fieldKind)
        {
            var property = objectType.GetProperty(propertyName);
            if (property == null) throw new ArgumentException("Not a valid property name.", nameof(propertyName));
            var propertyType = property.PropertyType;

            var assemblyName = new AssemblyName("Hazelcast.Net.Tests"); // to access internals!
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleName = assemblyName.Name;
            if (moduleName == null) throw new Exception("panic: null module name.");
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
            var typeBuilder = moduleBuilder.DefineType("ThingSerializer", TypeAttributes.Public);
            var interfaceType = typeof(ICompactSerializer<>).MakeGenericType(objectType);
            typeBuilder.AddInterfaceImplementation(interfaceType);

            var fieldBuilder = typeBuilder.DefineField("_defaultValue", propertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty("DefaultValue", PropertyAttributes.HasDefault, propertyType, null);
            var getsetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            var getBuilder = typeBuilder.DefineMethod("get_DefaultValue", getsetAttributes, propertyType, Type.EmptyTypes);
            var setBuilder = typeBuilder.DefineMethod("set_DefaultValue", getsetAttributes, null, new[] { propertyType });
            var ilgen = getBuilder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldfld, fieldBuilder);
            ilgen.Emit(OpCodes.Ret);
            ilgen = setBuilder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.Stfld, fieldBuilder);
            ilgen.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getBuilder);
            propertyBuilder.SetSetMethod(setBuilder);
            var getDefaultValue = getBuilder.GetBaseDefinition();

            fieldBuilder = typeBuilder.DefineField("_fieldName", typeof(string), FieldAttributes.Private);
            propertyBuilder = typeBuilder.DefineProperty("FieldName", PropertyAttributes.HasDefault, typeof(string), null);
            getsetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            getBuilder = typeBuilder.DefineMethod("get_FieldName", getsetAttributes, typeof(string), Type.EmptyTypes);
            setBuilder = typeBuilder.DefineMethod("set_FieldName", getsetAttributes, null, new[] { typeof(string) });
            ilgen = getBuilder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldfld, fieldBuilder);
            ilgen.Emit(OpCodes.Ret);
            ilgen = setBuilder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.Stfld, fieldBuilder);
            ilgen.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getBuilder);
            propertyBuilder.SetSetMethod(setBuilder);
            var getFieldName = getBuilder.GetBaseDefinition();

            propertyBuilder = typeBuilder.DefineProperty("TypeName", PropertyAttributes.HasDefault, typeof(string), null);
            getsetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            getBuilder = typeBuilder.DefineMethod("get_TypeName", getsetAttributes, typeof(string), Type.EmptyTypes);
            ilgen = getBuilder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldstr, "thing");
            ilgen.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getBuilder);

            var write = typeof(ICompactWriter).GetMethod($"Write{fieldKind}")!;
            if (fieldKind == FieldKind.Compact) write = write.MakeGenericMethod(propertyType);
            var writeBuilder = typeBuilder.DefineMethod("Write", MethodAttributes.Public | MethodAttributes.Virtual, null, new[] { typeof(ICompactWriter), objectType });
            ilgen = writeBuilder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Call, getFieldName);
            ilgen.Emit(OpCodes.Ldarg_2);
            ilgen.Emit(OpCodes.Callvirt, property.GetMethod!);
            ilgen.Emit(OpCodes.Callvirt, write);
            ilgen.Emit(OpCodes.Ret);

            var type = typeBuilder.CreateType();
            if (type == null) throw new Exception("panic: null type.");
            return type;
        }

        public static void SetPropertyValue(object obj, string propertyName, object? value)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null) throw new ArgumentException("Property not found.", nameof(propertyName));
            try
            {
                property.SetValue(obj, value);
            }
            catch
            {
                throw new InvalidOperationException(value == null
                    ? $"Failed to assign value null to property of type {property!.PropertyType}."
                    : $"Failed to assign value of type {value.GetType()} to property of type {property.PropertyType}.");
            }
        }

        public static object? GetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null) throw new ArgumentException("Property not found.", nameof(propertyName));
            try
            {
                return property.GetValue(obj);
            }
            catch
            {
                throw new InvalidOperationException($"Failed to get property value of type {property!.PropertyType}.");
            }
        }
    }
}
