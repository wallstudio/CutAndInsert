using System.Collections.Generic;
using System.Linq;


namespace CAI
{
    static class CollectionExtentions
    {
        public static IEnumerable<T> ToDebugArray<T>(this IEnumerable<T> src)
        {
            #if UNITY_EDITOR
            return src.ToArray();
            #else
            return src;
            #endif 
        }
    }
}