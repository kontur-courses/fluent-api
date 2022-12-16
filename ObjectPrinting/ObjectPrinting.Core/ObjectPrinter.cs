using System.Text;
using System.Reflection;
using System.Collections;
using ObjectPrinting.Core.Configs;

namespace ObjectPrinting.Core
{
    public class ObjectPrinter<TOwner>
    {        
        private readonly Config? _config;
        private const char Separator = '\t';
        private readonly Dictionary<object, Guid> _alreadyPrinted = new();
        public static int MaxCollectionSize { get; set; } = 1024;
        private ObjectPrinter(Config? config)
        {
            this._config = config;
        }

        public string PrintToString(TOwner obj) 
            => PrintToString(obj, 0, _config);

        public static ObjectPrinter<TOwner> Configure(Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> options) 
            => new(options(new PrintingConfig<TOwner>()).Config);

        public static ObjectPrinter<TOwner> Default => new(null);

        public static string Print(TOwner obj) => Default.PrintToString(obj);
        
        private string PrintToString(object? obj, int nestingLevel, Config? config = null, MemberInfo? currentMember = null)
        {
            if (obj == null)
                return "null";

            if (_alreadyPrinted.TryGetValue(obj, out var id))
                return $"Already was printed : {id}";

            config ??= new Config();
            var serialized = TrySerializeWithMemberInfo(obj, currentMember, config);
            if (serialized != null)
                return serialized;

            var type = obj.GetType();

            serialized = (obj is not IEnumerable or not ICollection) 
                            ? null 
                            : TrySerializeCollection((IEnumerable)obj, nestingLevel, config);

            if (serialized != null)
                return serialized;

            serialized = TrySerializeByType(obj, type, config);
            if (serialized != null)
                return serialized;

            return SerializeProperties(obj, type, nestingLevel, config);
        }

        private string SerializeProperties(object obj, Type type, int nestingLevel, Config config)
        {
            var id = Guid.NewGuid();
            _alreadyPrinted.Add(obj, id);
            var identation = new string(Separator, nestingLevel + 1);
            var sb = new StringBuilder();
            sb.Append($"{type.Name} - {id}");

            foreach (var propertyInfo in type.GetProperties())
            {
                if (config.ExcludedMembers.Contains(propertyInfo))
                    continue;

                if (config.ExcludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                var serialized = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, config, propertyInfo);
                sb.Append($"{Environment.NewLine}{identation}{propertyInfo.Name} = {serialized}");
            }

            return sb.ToString();
        }

        private string? TrySerializeCollection(IEnumerable collection, int nestingLevel, Config config)
        {
            var identation = new string(Separator, nestingLevel + 1);

            if (collection is IDictionary dictionary)
                return SerializeDictionary(dictionary, nestingLevel, identation, config);

            return SerializeCollection(collection, nestingLevel, identation, config);
        }

        private string SerializeCollection(IEnumerable collection, int nestingLevel, string identation, Config config)
        {
            var sb = new StringBuilder($"[{Environment.NewLine}");
            var count = 0;
            bool overflow = false;

            foreach (var obj in collection)
            {
                if (count == MaxCollectionSize)
                {
                    overflow = true;
                    break;
                }

                count++;
                sb.Append($"{identation}{PrintToString(obj, nestingLevel + 1, config)},{Environment.NewLine}");
            }

            sb.Remove(sb.Length - 2, 2);

            if(overflow)
                sb.Append($"{Environment.NewLine}{identation}...");
            
            sb.Append($"{Environment.NewLine}{identation}]");

            return sb.ToString();
        }

        private string SerializeDictionary(IDictionary dictionary, int nestingLevel, string identation, Config config)
        {
            var sb = new StringBuilder($"{{{Environment.NewLine}");
            var count = 0;
            bool overflow = false;

            foreach (DictionaryEntry obj in dictionary)
            {
                if (count == MaxCollectionSize)
                {
                    overflow = true;
                    break;
                }

                count++;
                sb.Append($"{identation}{PrintToString(obj.Key, nestingLevel + 1, config)} : " +
                          $"{PrintToString(obj.Value, nestingLevel + 1, config)},{Environment.NewLine}");
            }

            if (overflow)
                sb.Append($"{Environment.NewLine}{identation}...");

            sb.Append($"{identation}}}");
            return sb.ToString();
        }

        private static string? TrySerializeByType(object obj, Type type, Config config)
        {
            if (config.TypeSpecificSerializers.TryGetValue(type, out var serializer))
                return serializer(obj);

            if (obj is string strValue)
                return ToTrimmedString(strValue, config.StringTrimLength);

            if (config.FinalTypes.Contains(type) || type.IsEnum && config.FinalTypes.Contains(typeof(Enum)))
            {
                return config.TypeCultureSettings.TryGetValue(type, out var culture)
                    ? ToStringAsFormattable(obj, culture)
                    : obj.ToString();
            }

            return null;
        }

        private static string? TrySerializeWithMemberInfo(object? obj, MemberInfo? memberInfo, Config config)
        {
            if (memberInfo == null || obj == null)
                return null;

            if (config.MemberSpecificSerializers.TryGetValue(memberInfo, out var serializer))
                return serializer(obj);

            if (config.MemberCultureSettings.TryGetValue(memberInfo, out var culture))
                return ToStringAsFormattable(obj, culture);

            return config.MemberTrimLengths.TryGetValue(memberInfo, out var trimLength) 
                ? ToTrimmedString((string)obj, trimLength) 
                : null;
        }

        private static string ToTrimmedString(string str, int trimLength)
        {
            if (trimLength == -1 || str.Length <= trimLength)
                return str;

            if (trimLength < 0)
                throw new InvalidOperationException($"TrimLength, set for type or Member, is invalid: {trimLength}");

            return $"{str[..trimLength]}...";
        }

        private static string ToStringAsFormattable(object obj, IFormatProvider culture) => ((IFormattable)obj).ToString(null, culture);
    }
}