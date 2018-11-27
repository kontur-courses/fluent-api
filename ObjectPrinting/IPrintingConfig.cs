using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        HashSet<Type> ExcludeMembersByType { get; set; }
        HashSet<string> ExcludeMembersByName { get; set; }
        Dictionary<Type, Delegate> AlternativeSerializationByType { get; set; }
        Dictionary<string, Delegate> AlternativeSerializationByName { get; set; }
        HashSet<object> ViewedObjects { get; set; }
        Dictionary<string, Delegate> TrimmingFunctions { get; set; }
        Dictionary<Type, CultureInfo> CultureInfoForNumbers { get; set; }
    }
}