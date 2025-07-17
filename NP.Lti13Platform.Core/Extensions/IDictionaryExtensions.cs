using System.Diagnostics.CodeAnalysis;

namespace NP.Lti13Platform.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    public static class IDictionaryExtensions
    {
        /// <summary>
        /// Merges two dictionaries, with the second dictionary's values overwriting the first's in case of key conflicts.
        /// </summary>
        /// <param name="dict">The first dictionary.</param>
        /// <param name="merge">The second dictionary.</param>
        /// <returns>A new dictionary containing the merged key-value pairs.</returns>
        [return: NotNullIfNotNull(nameof(dict))]
        [return: NotNullIfNotNull(nameof(merge))]
        public static IDictionary<string, string>? Merge(this IDictionary<string, string>? dict, IDictionary<string, string>? merge) => dict == null ? merge?.ToDictionary() : merge == null ? dict?.ToDictionary() : dict.Concat(merge).GroupBy(c => c.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Last().Value);
    }
}
