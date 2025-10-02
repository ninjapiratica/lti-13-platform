namespace NP.Lti13Platform.Core;

/// <summary>
/// Specifies that the attributed class or struct is associated with a string-based identifier.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public class StringIdAttribute : Attribute { }
