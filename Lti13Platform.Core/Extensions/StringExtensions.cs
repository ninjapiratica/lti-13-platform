namespace NP.Lti13Platform.Extensions
{
    internal static class StringExtensions
    {
        internal static string? ToNullIfEmpty(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
