using System.Reflection;
using System.Globalization;

namespace ObjectPrinting.Core.Configs
{
    internal class Config
    {
        internal readonly HashSet<Type> FinalTypes = new();
        internal readonly HashSet<Type> ExcludedTypes = new();
        internal readonly HashSet<MemberInfo> ExcludedMembers = new();
        
        internal readonly Dictionary<MemberInfo, int> MemberTrimLengths = new();

        internal readonly Dictionary<Type, CultureInfo> TypeCultureSettings = new();
        internal readonly Dictionary<MemberInfo, CultureInfo> MemberCultureSettings = new();

        internal readonly Dictionary<Type, Func<object, string>> TypeSpecificSerializers = new();
        internal readonly Dictionary<MemberInfo, Func<object, string>> MemberSpecificSerializers = new();

        internal int StringTrimLength = -1;

        internal Config()
        {              
            FinalTypes.Add(typeof(Enum));         
            FinalTypes.Add(typeof(string));
            FinalTypes.UnionWith(GetSystemTypes());
        }

        private static IEnumerable<Type> GetSystemTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes().Where(x => x.Namespace == nameof(System)))
                .Where(x => x.GetInterface(nameof(IFormattable)) != null);
        }
    }
}
