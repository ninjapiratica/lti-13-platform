using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace NP.Lti13Platform.Test
{
    public class FirstTest
    {
        [Fact]
        public void Test()
        {
            Type[] interfaces = [];

            var invalidMethods = interfaces.SelectMany(i => i.GetMethods()).Where(m => !m.IsSpecialName).ToArray();
            if (invalidMethods.Length > 0)
            {
                throw new Exception("Interfaces may only have properties. " + string.Join(", ", invalidMethods.Select(m => m.DeclaringType).Distinct().Select(t => t?.FullName)));
            }

            var typeBuilder = AssemblyBuilder
                .DefineDynamicAssembly(new AssemblyName(Assembly.GetExecutingAssembly() + ".DynamicAssembly"), AssemblyBuilderAccess.Run)
                .DefineDynamicModule("DynamicModule")
                .DefineType("DynamicType", TypeAttributes.Public, null);

            foreach (var iface in interfaces)
            {
                typeBuilder.AddInterfaceImplementation(iface);

                foreach (var propertyInfo in iface.GetProperties())
                {
                    var fieldBuilder = typeBuilder.DefineField("_" + propertyInfo.Name.ToLower(), propertyInfo.PropertyType, FieldAttributes.Private);
                    var propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);

                    foreach (var customAttribute in propertyInfo.CustomAttributes)
                    {
                        var propertyArguments = customAttribute.NamedArguments.Where(a => !a.IsField).Select(a => new { PropertyInfo = (PropertyInfo)a.MemberInfo, a.TypedValue.Value }).ToArray();
                        var fieldArguments = customAttribute.NamedArguments.Where(a => a.IsField).Select(a => new { FieldInfo = (FieldInfo)a.MemberInfo, a.TypedValue.Value }).ToArray();

                        propertyBuilder.SetCustomAttribute(new CustomAttributeBuilder(
                            customAttribute.Constructor,
                            customAttribute.ConstructorArguments.Select(a => a.Value).ToArray(),
                            propertyArguments.Select(a => a.PropertyInfo).ToArray(),
                            propertyArguments.Select(a => a.Value).ToArray(),
                            fieldArguments.Select(a => a.FieldInfo).ToArray(),
                            fieldArguments.Select(a => a.Value).ToArray()));
                    }

                    var getter = typeBuilder.DefineMethod("get_" + propertyInfo.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, propertyInfo.PropertyType, Type.EmptyTypes);
                    var getGenerator = getter.GetILGenerator();
                    getGenerator.Emit(OpCodes.Ldarg_0);
                    getGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                    getGenerator.Emit(OpCodes.Ret);
                    propertyBuilder.SetGetMethod(getter);
                    var getMethod = propertyInfo.GetGetMethod();
                    if (getMethod != null)
                    {
                        typeBuilder.DefineMethodOverride(getter, getMethod);
                    }

                    var setter = typeBuilder.DefineMethod("set_" + propertyInfo.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, null, [propertyInfo.PropertyType]);
                    var setGenerator = setter.GetILGenerator();
                    setGenerator.Emit(OpCodes.Ldarg_0);
                    setGenerator.Emit(OpCodes.Ldarg_1);
                    setGenerator.Emit(OpCodes.Stfld, fieldBuilder);
                    setGenerator.Emit(OpCodes.Ret);
                    propertyBuilder.SetSetMethod(setter);
                    var setMethod = propertyInfo.GetSetMethod();
                    if (setMethod != null)
                    {
                        typeBuilder.DefineMethodOverride(setter, setMethod);
                    }
                }
            }

            var type = typeBuilder.CreateType();

        }
    }
}

/**
 *
 * Copyright (c) 2016 Jean-Dominique Nguele
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute,
 * sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial
 *  portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */
namespace IamNguele.Utils
{
    public static class TypeMixer<T>
    {
        public static readonly BindingFlags visibilityFlags = BindingFlags.Public | BindingFlags.Instance;
        public static K ExtendWith<K>(T source = default(T))
        {
            var assemblyName = new Guid().ToString();
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule("Module");
            var type = module.DefineType(typeof(T).Name + "_" + typeof(K).Name, TypeAttributes.Public, typeof(T));
            var fieldsList = new List<string>();
            type.AddInterfaceImplementation(typeof(K));
            foreach (var v in typeof(K).GetProperties())
            {
                fieldsList.Add(v.Name);
                var field = type.DefineField("_" + v.Name.ToLower(), v.PropertyType, FieldAttributes.Private);
                var property = type.DefineProperty(v.Name, PropertyAttributes.None, v.PropertyType, new Type[0]);
                var getter = type.DefineMethod("get_" + v.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, v.PropertyType, new Type[0]);
                var setter = type.DefineMethod("set_" + v.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, null, new Type[] { v.PropertyType });
                var getGenerator = getter.GetILGenerator();
                var setGenerator = setter.GetILGenerator();
                getGenerator.Emit(OpCodes.Ldarg_0);
                getGenerator.Emit(OpCodes.Ldfld, field);
                getGenerator.Emit(OpCodes.Ret);
                setGenerator.Emit(OpCodes.Ldarg_0);
                setGenerator.Emit(OpCodes.Ldarg_1);
                setGenerator.Emit(OpCodes.Stfld, field);
                setGenerator.Emit(OpCodes.Ret);
                property.SetGetMethod(getter);
                property.SetSetMethod(setter);
                type.DefineMethodOverride(getter, v.GetGetMethod());
                type.DefineMethodOverride(setter, v.GetSetMethod());
            }
            if (source != null)
            {
                foreach (var v in source.GetType().GetProperties())
                {
                    if (fieldsList.Contains(v.Name))
                    {
                        continue;
                    }
                    fieldsList.Add(v.Name);
                    var field = type.DefineField("_" + v.Name.ToLower(), v.PropertyType, FieldAttributes.Private);
                    var property = type.DefineProperty(v.Name, PropertyAttributes.None, v.PropertyType, new Type[0]);
                    var getter = type.DefineMethod("get_" + v.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, v.PropertyType, new Type[0]);
                    var setter = type.DefineMethod("set_" + v.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, null, new Type[] { v.PropertyType });
                    var getGenerator = getter.GetILGenerator();
                    var setGenerator = setter.GetILGenerator();
                    getGenerator.Emit(OpCodes.Ldarg_0);
                    getGenerator.Emit(OpCodes.Ldfld, field);
                    getGenerator.Emit(OpCodes.Ret);
                    setGenerator.Emit(OpCodes.Ldarg_0);
                    setGenerator.Emit(OpCodes.Ldarg_1);
                    setGenerator.Emit(OpCodes.Stfld, field);
                    setGenerator.Emit(OpCodes.Ret);
                    property.SetGetMethod(getter);
                    property.SetSetMethod(setter);
                }
            }
            var newObject = (K)Activator.CreateInstance(type.CreateType());
            return source == null ? newObject : CopyValues(source, newObject);
        }
        private static K CopyValues<K>(T source, K destination)
        {
            foreach (PropertyInfo property in source.GetType().GetProperties(visibilityFlags))
            {
                var prop = destination.GetType().GetProperty(property.Name, visibilityFlags);
                if (prop != null && prop.CanWrite)
                    prop.SetValue(destination, property.GetValue(source), null);
            }
            return destination;
        }
    }
}




public static class ObjectFactory
{
    private static readonly ConcurrentDictionary<Type, Type> TypeCache = new ConcurrentDictionary<Type, Type>();

    public static T CreateInstance<T>()
    {
        if (!typeof(T).IsInterface) throw new ArgumentException($"Type {typeof(T).Name} must be an interface.");
        var newType = TypeCache.GetOrAdd(typeof(T), t => BuildType(typeof(T)));
        return (T)Activator.CreateInstance(newType);
    }

    private static Type BuildType(Type interfaceType)
    {
        var assemblyName = new AssemblyName($"DynamicAssembly_{Guid.NewGuid():N}");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
        var typeName = $"{RemoveInterfacePrefix(interfaceType.Name)}_{Guid.NewGuid():N}";
        var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

        typeBuilder.AddInterfaceImplementation(interfaceType);

        var properties = interfaceType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (var property in properties)
        {
            BuildProperty(typeBuilder, property);
        }

        return typeBuilder.CreateType();

        string RemoveInterfacePrefix(string name) => Regex.Replace(name, "^I", string.Empty);
    }

    private static PropertyBuilder BuildProperty(TypeBuilder typeBuilder, PropertyInfo property)
    {
        var fieldName = $"<{property.Name}>k__BackingField";

        var propertyBuilder = typeBuilder.DefineProperty(property.Name, System.Reflection.PropertyAttributes.None, property.PropertyType, Type.EmptyTypes);

        // Build backing-field.
        var fieldBuilder = typeBuilder.DefineField(fieldName, property.PropertyType, FieldAttributes.Private);

        var getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;

        var getterBuilder = BuildGetter(typeBuilder, property, fieldBuilder, getSetAttributes);
        var setterBuilder = BuildSetter(typeBuilder, property, fieldBuilder, getSetAttributes);

        propertyBuilder.SetGetMethod(getterBuilder);
        propertyBuilder.SetSetMethod(setterBuilder);

        return propertyBuilder;
    }

    private static MethodBuilder BuildGetter(TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder fieldBuilder, MethodAttributes attributes)
    {
        var getterBuilder = typeBuilder.DefineMethod($"get_{property.Name}", attributes, property.PropertyType, Type.EmptyTypes);
        var ilGenerator = getterBuilder.GetILGenerator();

        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);

        if (property.GetCustomAttribute<NotNullAttribute>() != null)
        {
            // Build null check
            ilGenerator.Emit(OpCodes.Dup);

            var isFieldNull = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Brtrue_S, isFieldNull);
            ilGenerator.Emit(OpCodes.Pop);
            ilGenerator.Emit(OpCodes.Ldstr, $"{property.Name} isn't set.");

            var invalidOperationExceptionConstructor = typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) });
            ilGenerator.Emit(OpCodes.Newobj, invalidOperationExceptionConstructor);
            ilGenerator.Emit(OpCodes.Throw);

            ilGenerator.MarkLabel(isFieldNull);
        }
        ilGenerator.Emit(OpCodes.Ret);

        return getterBuilder;
    }

    private static MethodBuilder BuildSetter(TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder fieldBuilder, MethodAttributes attributes)
    {
        var setterBuilder = typeBuilder.DefineMethod($"set_{property.Name}", attributes, null, new Type[] { property.PropertyType });

        var ilGenerator = setterBuilder.GetILGenerator();

        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldarg_1);

        // Build null check

        if (property.GetCustomAttribute<NotNullAttribute>() != null)
        {
            var isValueNull = ilGenerator.DefineLabel();

            ilGenerator.Emit(OpCodes.Dup);
            ilGenerator.Emit(OpCodes.Brtrue_S, isValueNull);
            ilGenerator.Emit(OpCodes.Pop);
            ilGenerator.Emit(OpCodes.Ldstr, property.Name);

            var argumentNullExceptionConstructor = typeof(ArgumentNullException).GetConstructor(new Type[] { typeof(string) });
            ilGenerator.Emit(OpCodes.Newobj, argumentNullExceptionConstructor);
            ilGenerator.Emit(OpCodes.Throw);

            ilGenerator.MarkLabel(isValueNull);
        }
        ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
        ilGenerator.Emit(OpCodes.Ret);

        return setterBuilder;
    }
}