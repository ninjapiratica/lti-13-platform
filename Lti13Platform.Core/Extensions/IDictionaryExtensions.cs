﻿using System.Diagnostics.CodeAnalysis;

namespace NP.Lti13Platform.Core.Extensions
{
    public static class IDictionaryExtensions
    {
        [return: NotNullIfNotNull(nameof(dict))]
        [return: NotNullIfNotNull(nameof(merge))]
        public static IDictionary<string, string>? Merge(this IDictionary<string, string>? dict, IDictionary<string, string>? merge) => dict == null ? merge?.ToDictionary() : merge == null ? dict?.ToDictionary() : dict.Concat(merge).GroupBy(c => c.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Last().Value);
    }
}
