using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace NP.Lti13Platform.Core.Utilities;

/// <summary>
/// Provides utilities for dynamically creating types that implement multiple interfaces on top of a base type.
/// </summary>
/// <remarks>This utility uses reflection and dynamic code generation to create wrapper types at runtime.
/// The generated types inherit from a base type T and implement all specified interfaces by delegating to the base type.
/// Generated types are cached to avoid redundant reflection and IL generation on subsequent calls.</remarks>
public static class DynamicTypeBuilder
{
    private static readonly ModuleBuilder _moduleBuilder;
    private static int _typeCounter = 0;
    private static readonly Lock _lockObject = new();
    
    /// <summary>
    /// Cache for dynamically created types, keyed by base type and interface set.
    /// </summary>
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
    /// - Delegate interface implementations to the base type where applicable
    /// - Copy attributes from all interfaces (both provided and already implemented by T)
    /// 
    /// This is useful for runtime scenarios where you need to compose types dynamically with multiple interface implementations.
    /// Generated types are cached with a key based on the base type and sorted interface names to ensure consistent cache lookups.
    /// 
    /// Even if no new interfaces are provided, a dynamic type will be created if the base type already implements interfaces,
    /// allowing their attributes to be copied to the dynamic type.
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
        
        // Check if we need to create a dynamic type:
        // 1. If new interfaces are being added, OR
        // 2. If the base type already implements interfaces (to copy their attributes)
        var baseTypeInterfaces = typeof(T).GetInterfaces();
        if (interfaceList.Count == 0 && baseTypeInterfaces.Length == 0)
        {
            // No new interfaces and no existing interfaces to process
            return typeof(T);
        }

        if (typeof(T).IsSealed)
        {
            throw new ArgumentException($"Base type {typeof(T).Name} cannot be sealed.", nameof(T));
        }

        foreach (var interfaceType in interfaceList)
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException(
                    $"Type {interfaceType.Name} is not an interface.",
                    nameof(interfaces));
            }
        }

        // Create cache key from base type and sorted interface names
        var cacheKey = GenerateCacheKey<T>(interfaceList);

        // Check cache first - if found, return immediately
        if (_typeCache.TryGetValue(cacheKey, out var cachedType))
        {
            return cachedType;
        }

        // Create type if not in cache
        var newType = CreateTypeInternal<T>([.. baseTypeInterfaces, .. interfaceList]);
        
        // Store in cache
        _typeCache.TryAdd(cacheKey, newType);

        return newType;
    }

    /// <summary>
    /// Generates a cache key based on the base type and interface set.
    /// </summary>
    private static string GenerateCacheKey<T>(List<Type> interfaceList) where T : class
    {
        var baseTypeName = typeof(T).FullName ?? typeof(T).Name;
        var interfaceNames = string.Join("|", interfaceList
            .OrderBy(i => i.FullName ?? i.Name)
            .Select(i => i.FullName ?? i.Name));

        return $"{baseTypeName}::{interfaceNames}";
    }

    /// <summary>
    /// Creates the dynamic type with IL generation (internal implementation).
    /// </summary>
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

            // Create a default constructor that calls the base constructor
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);

            var constructorIL = constructorBuilder.GetILGenerator();
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Call,
                typeof(T).GetConstructor(Type.EmptyTypes)
                ?? throw new InvalidOperationException(
                    $"Base type {typeof(T).Name} must have a parameterless constructor."));
            constructorIL.Emit(OpCodes.Ret);

            // Implement all interface members through a unified flow
            foreach (var interfaceType in allInterfaces)
            {
                ImplementInterface(typeBuilder, typeof(T), interfaceType);
            }

            return typeBuilder.CreateType()
                ?? throw new InvalidOperationException("Failed to create dynamic type.");
        }
    }

    /// <summary>
    /// Copies custom attributes from a source member to a target builder using the provided setter action.
    /// </summary>
    private static void CopyCustomAttributes(ICustomAttributeProvider source, Action<CustomAttributeBuilder> setAttributeAction)
    {
        var customAttributes = source.GetCustomAttributes(false);
        foreach (var customAttribute in customAttributes)
        {
            try
            {
                var attributeType = customAttribute.GetType();
                
                // Use reflection to get the actual constructor and arguments used
                var customAttributeDataList = GetCustomAttributeData(source);
                if (customAttributeDataList.Count == 0)
                    continue;

                var customAttributeData = customAttributeDataList[0];
                var constructor = customAttributeData.Constructor;
                var constructorArgs = customAttributeData.ConstructorArguments.Select(ca => ca.Value).ToArray();

                var namedProperties = customAttributeData.NamedArguments
                    .Where(na => na.MemberInfo is PropertyInfo)
                    .Select(na => (PropertyInfo)na.MemberInfo)
                    .ToArray();
                var propertyValues = customAttributeData.NamedArguments
                    .Where(na => na.MemberInfo is PropertyInfo)
                    .Select(na => na.TypedValue.Value)
                    .ToArray();
                var namedFields = customAttributeData.NamedArguments
                    .Where(na => na.MemberInfo is FieldInfo)
                    .Select(na => (FieldInfo)na.MemberInfo)
                    .ToArray();
                var fieldValues = customAttributeData.NamedArguments
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

    /// <summary>
    /// Gets custom attribute data for a given attribute type from a member.
    /// </summary>
    private static List<CustomAttributeData> GetCustomAttributeData(ICustomAttributeProvider source)
    {
        if (source is MethodInfo methodInfo)
            return [.. CustomAttributeData.GetCustomAttributes(methodInfo)];
        if (source is PropertyInfo propertyInfo)
            return [.. CustomAttributeData.GetCustomAttributes(propertyInfo)];
        if (source is FieldInfo fieldInfo)
            return [.. CustomAttributeData.GetCustomAttributes(fieldInfo)];
        if (source is Type typeInfo)
            return [.. CustomAttributeData.GetCustomAttributes(typeInfo)];
        if (source is Assembly assemblyInfo)
            return [.. CustomAttributeData.GetCustomAttributes(assemblyInfo)];
        if (source is ParameterInfo parameterInfo)
            return [.. CustomAttributeData.GetCustomAttributes(parameterInfo)];

        return [];
    }

    private static void ImplementInterface(TypeBuilder typeBuilder, Type baseType, Type interfaceType)
    {
        var interfaceMethods = interfaceType.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        foreach (var interfaceMethod in interfaceMethods)
        {
            // Skip if the base type already implements this method
            var baseMethod = baseType.GetMethod(
                interfaceMethod.Name,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase,
                null,
                [.. interfaceMethod.GetParameters().Select(p => p.ParameterType)],
                null);

            if (baseMethod != null && baseMethod.ReturnType == interfaceMethod.ReturnType)
            {
                continue; // Base type already implements this method
            }

            ImplementMethod(typeBuilder, baseType, interfaceMethod);
        }

        var interfaceProperties = interfaceType.GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        foreach (var interfaceProperty in interfaceProperties)
        {
            var baseProperty = baseType.GetProperty(
                interfaceProperty.Name,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (baseProperty != null && baseProperty.PropertyType == interfaceProperty.PropertyType)
            {
                continue; // Base type already implements this property
            }

            ImplementProperty(typeBuilder, baseType, interfaceProperty);
        }
    }

    private static void ImplementMethod(TypeBuilder typeBuilder, Type baseType, MethodInfo interfaceMethod)
    {
        var parameters = interfaceMethod.GetParameters();
        var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

        var methodBuilder = typeBuilder.DefineMethod(
            interfaceMethod.Name,
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.NewSlot,
            interfaceMethod.ReturnType,
            parameterTypes);

        // Copy attributes from the interface method
        CopyCustomAttributes(interfaceMethod, attr => methodBuilder.SetCustomAttribute(attr));

        var methodIL = methodBuilder.GetILGenerator();

        // Load 'this'
        methodIL.Emit(OpCodes.Ldarg_0);

        // Load all parameters
        for (int i = 0; i < parameters.Length; i++)
        {
            switch (i)
            {
                case 0:
                    methodIL.Emit(OpCodes.Ldarg_1);
                    break;
                case 1:
                    methodIL.Emit(OpCodes.Ldarg_2);
                    break;
                case 2:
                    methodIL.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    methodIL.Emit(OpCodes.Ldarg, i + 1);
                    break;
            }
        }

        // Try to call the base type's implementation
        var baseImplementation = baseType.GetMethod(
            interfaceMethod.Name,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase,
            null,
            parameterTypes,
            null);

        if (baseImplementation != null)
        {
            methodIL.Emit(OpCodes.Callvirt, baseImplementation);
        }
        else
        {
            // If base type doesn't implement it, throw NotImplementedException
            var notImplementedConstructor = typeof(NotImplementedException).GetConstructor(Type.EmptyTypes)
                ?? throw new InvalidOperationException("Cannot find NotImplementedException constructor.");
            methodIL.Emit(OpCodes.Newobj, notImplementedConstructor);
            methodIL.Emit(OpCodes.Throw);
        }

        methodIL.Emit(OpCodes.Ret);

        typeBuilder.DefineMethodOverride(methodBuilder, interfaceMethod);
    }

    private static void ImplementProperty(TypeBuilder typeBuilder, Type baseType, PropertyInfo interfaceProperty)
    {
        var baseProperty = baseType.GetProperty(
            interfaceProperty.Name,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        var propertyBuilder = typeBuilder.DefineProperty(
            interfaceProperty.Name,
            PropertyAttributes.None,
            interfaceProperty.PropertyType,
            null);

        // Copy attributes from the interface property
        CopyCustomAttributes(interfaceProperty, attr => propertyBuilder.SetCustomAttribute(attr));

        // If base type has this property, delegate to it
        if (baseProperty != null && baseProperty.PropertyType == interfaceProperty.PropertyType)
        {
            if (interfaceProperty.CanRead && baseProperty.CanRead)
            {
                var getMethodBuilder = typeBuilder.DefineMethod(
                    $"get_{interfaceProperty.Name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.NewSlot,
                    interfaceProperty.PropertyType,
                    Type.EmptyTypes);

                // Copy attributes from the interface property's getter
                var interfaceGetMethod = interfaceProperty.GetGetMethod();
                if (interfaceGetMethod != null)
                {
                    CopyCustomAttributes(interfaceGetMethod, attr => getMethodBuilder.SetCustomAttribute(attr));
                }

                var getIL = getMethodBuilder.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Callvirt, baseProperty.GetGetMethod()
                    ?? throw new InvalidOperationException($"Cannot find getter for property {baseProperty.Name}."));
                getIL.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getMethodBuilder);
                typeBuilder.DefineMethodOverride(getMethodBuilder, interfaceProperty.GetGetMethod()
                    ?? throw new InvalidOperationException("Cannot find interface get method."));
            }

            if (interfaceProperty.CanWrite && baseProperty.CanWrite)
            {
                var setMethodBuilder = typeBuilder.DefineMethod(
                    $"set_{interfaceProperty.Name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.NewSlot,
                    null,
                    [interfaceProperty.PropertyType]);

                // Copy attributes from the interface property's setter
                var interfaceSetMethod = interfaceProperty.GetSetMethod();
                if (interfaceSetMethod != null)
                {
                    CopyCustomAttributes(interfaceSetMethod, attr => setMethodBuilder.SetCustomAttribute(attr));
                }

                var setIL = setMethodBuilder.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Callvirt, baseProperty.GetSetMethod()
                    ?? throw new InvalidOperationException($"Cannot find setter for property {baseProperty.Name}."));
                setIL.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(setMethodBuilder);
                typeBuilder.DefineMethodOverride(setMethodBuilder, interfaceProperty.GetSetMethod()
                    ?? throw new InvalidOperationException("Cannot find interface set method."));
            }
        }
        else
        {
            // Create a backing field for the property
            var backingFieldName = $"<{interfaceProperty.Name}>k__BackingField";
            var backingField = typeBuilder.DefineField(
                backingFieldName,
                interfaceProperty.PropertyType,
                FieldAttributes.Private);

            // Create getter
            if (interfaceProperty.CanRead)
            {
                var getMethodBuilder = typeBuilder.DefineMethod(
                    $"get_{interfaceProperty.Name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.NewSlot,
                    interfaceProperty.PropertyType,
                    Type.EmptyTypes);

                // Copy attributes from the interface property's getter
                var interfaceGetMethod = interfaceProperty.GetGetMethod();
                if (interfaceGetMethod != null)
                {
                    CopyCustomAttributes(interfaceGetMethod, attr => getMethodBuilder.SetCustomAttribute(attr));
                }

                var getIL = getMethodBuilder.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, backingField);
                getIL.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getMethodBuilder);
                typeBuilder.DefineMethodOverride(getMethodBuilder, interfaceProperty.GetGetMethod()
                    ?? throw new InvalidOperationException("Cannot find interface get method."));
            }

            // Create setter
            if (interfaceProperty.CanWrite)
            {
                var setMethodBuilder = typeBuilder.DefineMethod(
                    $"set_{interfaceProperty.Name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.NewSlot,
                    null,
                    [interfaceProperty.PropertyType]);

                // Copy attributes from the interface property's setter
                var interfaceSetMethod = interfaceProperty.GetSetMethod();
                if (interfaceSetMethod != null)
                {
                    CopyCustomAttributes(interfaceSetMethod, attr => setMethodBuilder.SetCustomAttribute(attr));
                }

                var setIL = setMethodBuilder.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, backingField);
                setIL.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(setMethodBuilder);
                typeBuilder.DefineMethodOverride(setMethodBuilder, interfaceProperty.GetSetMethod()
                    ?? throw new InvalidOperationException("Cannot find interface set method."));
            }
        }
    }
}
