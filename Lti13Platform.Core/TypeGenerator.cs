using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace NP.Lti13Platform.Core
{
    public static partial class TypeGenerator
    {
        private static readonly ModuleBuilder dynamicModule = AssemblyBuilder
            .DefineDynamicAssembly(new AssemblyName(Assembly.GetExecutingAssembly().GetName().Name + ".DynamicAssembly"), AssemblyBuilderAccess.Run)
            .DefineDynamicModule("DynamicModule");

        [GeneratedRegex("[^a-zA-Z0-9]")]
        private static partial Regex LettersAndNumbersRegex();

        public static Type CreateType(string typeName, IEnumerable<Type> interfaces, Type? baseType = null)
        {
            var typeBuilder = dynamicModule.DefineType("Dynamic" + LettersAndNumbersRegex().Replace(typeName.Trim(), "_") + Guid.NewGuid().ToString("N"), TypeAttributes.Public, baseType);

            foreach (var iFace in interfaces)
            {
                typeBuilder.AddInterfaceImplementation(iFace);

                foreach (var propertyInfo in iFace.GetProperties())
                {
                    var fieldBuilder = typeBuilder.DefineField("_" + propertyInfo.Name.ToLower(), propertyInfo.PropertyType, FieldAttributes.Private);
                    var propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes);

                    foreach (var customAttribute in propertyInfo.CustomAttributes)
                    {
                        var propertyArguments = customAttribute.NamedArguments.Where(a => !a.IsField).Select(a => new { PropertyInfo = (PropertyInfo)a.MemberInfo, a.TypedValue.Value }).ToArray();
                        var fieldArguments = customAttribute.NamedArguments.Where(a => a.IsField).Select(a => new { FieldInfo = (FieldInfo)a.MemberInfo, a.TypedValue.Value }).ToArray();

                        var constructorArgs = customAttribute.ConstructorArguments.Select(a => a.Value is ReadOnlyCollection<CustomAttributeTypedArgument> collection ? collection.Select(c => c.Value).ToArray() : a.Value).ToArray();
                        var propertyArgs = propertyArguments.Select(a => a.PropertyInfo).ToArray();
                        var propertyValues = propertyArguments.Select(a => a.Value).ToArray();
                        var fieldArgs = fieldArguments.Select(a => a.FieldInfo).ToArray();
                        var fieldValues = fieldArguments.Select(a => a.Value).ToArray();

                        var customBuilder = new CustomAttributeBuilder(
                            customAttribute.Constructor,
                            constructorArgs,
                            propertyArgs,
                            propertyValues,
                            fieldArgs,
                            fieldValues);

                        propertyBuilder.SetCustomAttribute(customBuilder);
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

            return typeBuilder.CreateType();
        }
    }
}
