using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public static class IDictionaryExtension
    {
        public static IEnumerable<DictionaryEntry> Cast(this IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
                yield return entry;
        }
    }
}
