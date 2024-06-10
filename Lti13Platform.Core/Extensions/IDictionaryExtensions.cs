using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Lti13Platform.Extensions
{
    internal static class IDictionaryExtensions
    {
        internal static IDictionary<string, string>? Merge(this IDictionary<string, string>? dict, IDictionary<string, string>? merge) => dict == null ? merge?.ToDictionary() : merge == null ? dict?.ToDictionary() : dict.Concat(merge).GroupBy(c => c.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Last().Value);
    }
}
