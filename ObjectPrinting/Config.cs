using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting
{
    internal class Config
    {
        internal readonly HashSet<Type> FinalTypes = new();

        internal readonly HashSet<Type> ExcludedTypes = new();
        internal readonly HashSet<MemberInfo> ExcludedMembers = new();
        internal readonly Dictionary<Type, Func<object, string>> TypeSpecificSerializers = new();
        internal readonly Dictionary<Type, CultureInfo> TypeCultureSettings = new();
        internal readonly Dictionary<MemberInfo, Func<object, string>> MemberSpecificSerializers = new();
        internal readonly Dictionary<MemberInfo, CultureInfo> MemberCultureSettings = new();
        internal readonly Dictionary<MemberInfo, int> MemberTrimLengths = new();
        internal int StringTrimLength = -1;

        internal Config()
        {
            var systemTypes = AppDomain.CurrentDomain.GetAssemblies()
                           .SelectMany(t => t.GetTypes().Where(x => x.Namespace == nameof(System)));

            var primitives = systemTypes.Where(x => x.GetInterface(nameof(IFormattable)) != null);
            FinalTypes.UnionWith(primitives);
            FinalTypes.Add(typeof(string));
            FinalTypes.Add(typeof(Enum));
        }
    }
}

