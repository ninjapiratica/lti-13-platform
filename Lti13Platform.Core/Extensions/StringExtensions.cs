namespace NP.Lti13Platform.Core.Extensions
{
    public static class StringExtensions
    {
        public static string? ToNullIfEmpty(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
