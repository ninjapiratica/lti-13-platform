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
    /// 
    /// This is useful for runtime scenarios where you need to compose types dynamically with multiple interface implementations.
    /// Generated types are cached with a key based on the base type and sorted interface names to ensure consistent cache lookups.
    /// </remarks>
    /// <typeparam name="T">The base type that the generated type will inherit from.</typeparam>
    /// <param name="interfaces">A collection of interfaces that the generated type should implement.
    /// If null or empty, returns the base type T unchanged.</param>
    /// <returns>A new type that inherits from T and implements all specified interfaces.
    /// If no interfaces are provided, returns T itself. Results are cached for performance.</returns>
    /// <exception cref="ArgumentException">Thrown if T is sealed or if any interface is not actually an interface type.</exception>
    public static Type CreateTypeImplementingInterfaces<T>(IEnumerable<Type>? interfaces) where T : class
    {
        if (interfaces == null)
        {
            return typeof(T);
        }

        var interfaceList = interfaces.ToList();
        if (interfaceList.Count == 0)
        {
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
        var newType = CreateTypeInternal<T>(interfaceList);
        
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
    private static Type CreateTypeInternal<T>(List<Type> interfaceList) where T : class
    {
        using (_lockObject.EnterScope())
        {
            _typeCounter++;
            var typeName = $"Dynamic_{typeof(T).Name}_{_typeCounter}";

            var typeBuilder = _moduleBuilder.DefineType(
                typeName,
                TypeAttributes.Public,
                typeof(T),
                [.. interfaceList]);

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

            // Implement all interface members
            foreach (var interfaceType in interfaceList)
            {
                ImplementInterface(typeBuilder, typeof(T), interfaceType);
            }

            return typeBuilder.CreateType()
                ?? throw new InvalidOperationException("Failed to create dynamic type.");
        }
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
