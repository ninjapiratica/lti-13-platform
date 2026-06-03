using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace NP.Lti13Platform.Core.Utilities;

/// <summary>
/// Provides utilities for dynamically creating types that implement multiple interfaces on top of a base type.
/// </summary>
/// <remarks>
/// This utility uses reflection and dynamic code generation to create wrapper types at runtime.
/// Generated types inherit from base type T and implement all specified interfaces by delegating to the base type.
/// Generated types are cached to avoid redundant reflection and IL generation on subsequent calls.
/// </remarks>
public static class DynamicTypeBuilder
{
    private static readonly ModuleBuilder _moduleBuilder;
    private static int _typeCounter = 0;
    private static readonly Lock _lockObject = new();
    private static readonly ConcurrentDictionary<string, Type> _typeCache = new();

    static DynamicTypeBuilder()
    {
        var assemblyName = new AssemblyName($"DynamicTypes_{Guid.NewGuid():N}");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            assemblyName,
            AssemblyBuilderAccess.Run);
        _moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
    }

    /// <summary>
    /// Creates a new type that implements all specified interfaces while inheriting from the base type T.
    /// Generated types are cached to improve performance on subsequent calls with the same base type and interface set.
    /// </summary>
    /// <remarks>
    /// The generated type will:
    /// - Inherit from the base type T
    /// - Implement all interfaces in the provided list
    /// - Preserve all public members from the base type
    /// - Delegate interface implementations to the base type where it already has them
    /// - Copy attributes from all interface members (both methods and properties)
    /// 
    /// If no new interfaces are provided but T already implements interfaces, a dynamic type is still created
    /// to allow their attributes to be copied to the dynamic type.
    /// Returns T unchanged only if both no new interfaces are provided and T implements no interfaces.
    /// </remarks>
    /// <typeparam name="T">The base type that the generated type will inherit from.</typeparam>
    /// <param name="interfaces">A collection of interfaces that the generated type should implement.
    /// If null or empty and T has no existing interfaces, returns the base type T unchanged.</param>
    /// <returns>A new type that inherits from T and implements all specified interfaces,
    /// with attributes copied from all interface members. Returns T unchanged only if both
    /// no new interfaces are provided and T implements no interfaces.</returns>
    /// <exception cref="ArgumentException">Thrown if T is sealed or if any interface is not actually an interface type.</exception>
    public static Type CreateTypeImplementingInterfaces<T>(IEnumerable<Type>? interfaces) where T : class
    {
        var interfaceList = interfaces?.ToList() ?? [];
        var baseType = typeof(T);
        var baseTypeInterfaces = baseType.GetInterfaces();

        // No need to create a dynamic type if no new interfaces and no existing interfaces
        if (interfaceList.Count == 0 && baseTypeInterfaces.Length == 0)
        {
            return baseType;
        }

        ValidateInputs<T>(baseType, interfaceList);

        var cacheKey = GenerateCacheKey<T>(interfaceList);
        if (_typeCache.TryGetValue(cacheKey, out var cachedType))
        {
            return cachedType;
        }

        var allInterfaces = new HashSet<Type>([.. baseTypeInterfaces, .. interfaceList]);
        var newType = CreateTypeInternal<T>(allInterfaces);
        _typeCache.TryAdd(cacheKey, newType);

        return newType;
    }

    private static void ValidateInputs<T>(Type baseType, List<Type> interfaceList) where T : class
    {
        if (baseType.IsSealed)
        {
            throw new ArgumentException($"Base type {baseType.Name} cannot be sealed.", nameof(T));
        }

        foreach (var interfaceType in interfaceList)
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException(
                    $"Type {interfaceType.Name} is not an interface.",
                    nameof(interfaceList));
            }
        }
    }

    private static string GenerateCacheKey<T>(List<Type> interfaceList) where T : class
    {
        var baseTypeName = typeof(T).FullName ?? typeof(T).Name;
        var interfaceNames = string.Join("|", interfaceList
            .OrderBy(i => i.FullName ?? i.Name)
            .Select(i => i.FullName ?? i.Name));
        return $"{baseTypeName}::{interfaceNames}";
    }

    private static Type CreateTypeInternal<T>(HashSet<Type> allInterfaces) where T : class
    {
        using (_lockObject.EnterScope())
        {
            _typeCounter++;
            var typeName = $"Dynamic_{typeof(T).Name}_{_typeCounter}";

            var typeBuilder = _moduleBuilder.DefineType(
                typeName,
                TypeAttributes.Public,
                typeof(T),
                [.. allInterfaces]);

            CreateDefaultConstructor<T>(typeBuilder);

            var implementedMembers = new ImplementedMembers();
            foreach (var interfaceType in allInterfaces)
            {
                ImplementInterfaceMembers(typeBuilder, typeof(T), interfaceType, implementedMembers);
            }

            return typeBuilder.CreateType()
                ?? throw new InvalidOperationException("Failed to create dynamic type.");
        }
    }

    private static void CreateDefaultConstructor<T>(TypeBuilder typeBuilder) where T : class
    {
        var constructorBuilder = typeBuilder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            Type.EmptyTypes);

        var il = constructorBuilder.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call,
            typeof(T).GetConstructor(Type.EmptyTypes)
            ?? throw new InvalidOperationException(
                $"Base type {typeof(T).Name} must have a parameterless constructor."));
        il.Emit(OpCodes.Ret);
    }

    private static void ImplementInterfaceMembers(
        TypeBuilder typeBuilder,
        Type baseType,
        Type interfaceType,
        ImplementedMembers implementedMembers)
    {
        foreach (var property in interfaceType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase))
        {
            ImplementInterfaceProperty(typeBuilder, baseType, property, implementedMembers);
        }
    }

    private static void ImplementInterfaceProperty(
        TypeBuilder typeBuilder,
        Type baseType,
        PropertyInfo interfaceProperty,
        ImplementedMembers implementedMembers)
    {
        var getMethod = interfaceProperty.GetGetMethod();
        var setMethod = interfaceProperty.GetSetMethod();

        var propSig = GetPropertySignature(interfaceProperty);

        // If already implemented in this dynamic type, just copy additional attributes
        if (implementedMembers.Properties.TryGetValue(propSig, out var existingPropertyBuilder))
        {
            CopyCustomAttributes(interfaceProperty, attr => existingPropertyBuilder.SetCustomAttribute(attr));
            return;
        }

        var baseProperty = baseType.GetProperty(
            interfaceProperty.Name,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        var hasDefaultImplementation = getMethod?.GetMethodBody() is not null
            || setMethod?.GetMethodBody() is not null;

        // If the base type already has this property with the same type,
        // do NOT create a new PropertyBuilder to avoid shadowing/duplicate JSON properties.
        // Interface attributes will be applied via InterfaceAttributeJsonTypeInfoResolver instead.
        if (!hasDefaultImplementation
            && baseProperty is not null
            && baseProperty.PropertyType == interfaceProperty.PropertyType)
        {
            return;
        }

        var propertyBuilder = typeBuilder.DefineProperty(
            interfaceProperty.Name,
            PropertyAttributes.None,
            interfaceProperty.PropertyType,
            null);

        CopyCustomAttributes(interfaceProperty, attr => propertyBuilder.SetCustomAttribute(attr));
        implementedMembers.Properties[propSig] = propertyBuilder;

        if (hasDefaultImplementation)
        {
            CreateDelegatingPropertyAccessors(
                typeBuilder,
                propertyBuilder,
                interfaceProperty);

            return;
        }

        // No base property (or type mismatch) — create backing field
        CreateBackingFieldPropertyAccessors(
            typeBuilder,
            propertyBuilder,
            interfaceProperty);
    }

    private static void CreateDelegatingPropertyAccessors(
        TypeBuilder typeBuilder,
        PropertyBuilder propertyBuilder,
        PropertyInfo interfaceProperty)
    {
        if (interfaceProperty.GetMethod is { IsAbstract: false } getter)
        {
            var getterBuilder = typeBuilder.DefineMethod(
                getter.Name,
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig,
                getter.ReturnType,
                Type.EmptyTypes);

            var il = getterBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, getter);
            il.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterBuilder);
        }

        if (interfaceProperty.SetMethod is { IsAbstract: false } setter)
        {
            var setterBuilder = typeBuilder.DefineMethod(
                setter.Name,
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig,
                null,
                [interfaceProperty.PropertyType]);

            var il = setterBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, setter);
            il.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setterBuilder);
        }
    }

    private static void CreateBackingFieldPropertyAccessors(
        TypeBuilder typeBuilder,
        PropertyBuilder propertyBuilder,
        PropertyInfo interfaceProperty)
    {
        var backingField = typeBuilder.DefineField(
            $"<{interfaceProperty.Name}>k__BackingField",
            interfaceProperty.PropertyType,
            FieldAttributes.Private);

        if (interfaceProperty.CanRead)
        {
            var getMethodBuilder = typeBuilder.DefineMethod(
                $"get_{interfaceProperty.Name}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.NewSlot,
                interfaceProperty.PropertyType,
                Type.EmptyTypes);

            var interfaceGetter = interfaceProperty.GetGetMethod();
            if (interfaceGetter != null)
            {
                CopyCustomAttributes(interfaceGetter, attr => getMethodBuilder.SetCustomAttribute(attr));
            }

            var il = getMethodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, backingField);
            il.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);
            typeBuilder.DefineMethodOverride(getMethodBuilder, interfaceGetter
                ?? throw new InvalidOperationException("Cannot find interface get method."));
        }

        if (interfaceProperty.CanWrite)
        {
            var setMethodBuilder = typeBuilder.DefineMethod(
                $"set_{interfaceProperty.Name}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.NewSlot,
                null,
                [interfaceProperty.PropertyType]);

            var interfaceSetter = interfaceProperty.GetSetMethod();
            if (interfaceSetter != null)
            {
                CopyCustomAttributes(interfaceSetter, attr => setMethodBuilder.SetCustomAttribute(attr));
            }

            var il = setMethodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, backingField);
            il.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setMethodBuilder);
            typeBuilder.DefineMethodOverride(setMethodBuilder, interfaceSetter
                ?? throw new InvalidOperationException("Cannot find interface set method."));
        }
    }

    private static void CopyCustomAttributes(ICustomAttributeProvider source, Action<CustomAttributeBuilder> setAttributeAction)
    {
        foreach (var attributeData in GetCustomAttributeData(source))
        {
            try
            {
                var constructor = attributeData.Constructor;
                var constructorArgs = attributeData.ConstructorArguments.Select(ca => ca.Value).ToArray();

                var namedProperties = attributeData.NamedArguments
                    .Where(na => na.MemberInfo is PropertyInfo)
                    .Select(na => (PropertyInfo)na.MemberInfo)
                    .ToArray();
                var propertyValues = attributeData.NamedArguments
                    .Where(na => na.MemberInfo is PropertyInfo)
                    .Select(na => na.TypedValue.Value)
                    .ToArray();

                var namedFields = attributeData.NamedArguments
                    .Where(na => na.MemberInfo is FieldInfo)
                    .Select(na => (FieldInfo)na.MemberInfo)
                    .ToArray();
                var fieldValues = attributeData.NamedArguments
                    .Where(na => na.MemberInfo is FieldInfo)
                    .Select(na => na.TypedValue.Value)
                    .ToArray();

                var attributeBuilder = new CustomAttributeBuilder(
                    constructor,
                    constructorArgs,
                    namedProperties,
                    propertyValues,
                    namedFields,
                    fieldValues);

                setAttributeAction(attributeBuilder);
            }
            catch
            {
                // Skip attributes that cannot be copied (e.g., attributes with complex types)
            }
        }
    }

    private static List<CustomAttributeData> GetCustomAttributeData(ICustomAttributeProvider source)
    {
        return source switch
        {
            MethodInfo methodInfo => [.. CustomAttributeData.GetCustomAttributes(methodInfo)],
            PropertyInfo propertyInfo => [.. CustomAttributeData.GetCustomAttributes(propertyInfo)],
            FieldInfo fieldInfo => [.. CustomAttributeData.GetCustomAttributes(fieldInfo)],
            Type typeInfo => [.. CustomAttributeData.GetCustomAttributes(typeInfo)],
            Assembly assemblyInfo => [.. CustomAttributeData.GetCustomAttributes(assemblyInfo)],
            ParameterInfo parameterInfo => [.. CustomAttributeData.GetCustomAttributes(parameterInfo)],
            _ => []
        };
    }

    private static string GetPropertySignature(PropertyInfo property)
    {
        return $"{property.Name}:{property.PropertyType.FullName ?? property.PropertyType.Name}";
    }

    private class ImplementedMembers
    {
        public HashSet<string> Methods { get; } = [];
        public Dictionary<string, PropertyBuilder> Properties { get; } = [];
    }
}

