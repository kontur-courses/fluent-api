using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class ObjectPrinter : IObjectPrinter
    {
        private const char identationElement = '\t';
        private readonly HashSet<Type> finalTypes = new();

        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
        private readonly Dictionary<Type, Func<object, string>> typeSpecificSerializers = new();
        private readonly Dictionary<Type, CultureInfo> typeCultureSettings = new();
        private readonly Dictionary<MemberInfo, Func<object, string>> memberSpecificSerializers = new();
        private readonly Dictionary<MemberInfo, CultureInfo> memberCultureSettings = new();
        private readonly Dictionary<MemberInfo, int> memberTrimLengths = new();
        private readonly Dictionary<object, Guid> alreadyPrinted = new();
        private int stringTrimLength = -1;

        public ObjectPrinter()
        {
            var systemTypes = AppDomain.CurrentDomain.GetAssemblies()
                           .SelectMany(t => t.GetTypes().Where(x => x.Namespace == nameof(System)));

            var primitives = systemTypes.Where(x => x.GetInterface(nameof(IFormattable)) != null);
            finalTypes.UnionWith(primitives);
            finalTypes.Add(typeof(string));
            finalTypes.Add(typeof(Enum));
        }

        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>(new ObjectPrinter());
        }

        public string PrintToString(object obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object? obj, int nestingLevel, MemberInfo? currentMember = null)
        {
            if (obj == null)
                return "null";

            if (alreadyPrinted.TryGetValue(obj, out var id))
                return $"was printed before : {id}";

            var serialized = TrySerializeWithMemberInfo(obj, currentMember);
            if (serialized != null)
                return serialized;

            var type = obj.GetType();

            serialized = TrySerializeCollection(obj, nestingLevel);
            if (serialized != null)
                return serialized;

            serialized = TrySerializeByType(obj, type);
            if (serialized != null)
                return serialized;

            return SerializeProperties(obj, type, nestingLevel);
        }

        private string SerializeProperties(object obj, Type type, int nestingLevel)
        {
            var id = Guid.NewGuid();
            alreadyPrinted.Add(obj, id);
            var identation = new string(identationElement, nestingLevel + 1);
            var sb = new StringBuilder();
            sb.Append($"{type.Name} - {id}");
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedMembers.Contains(propertyInfo))
                    continue;

                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                var serialized = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, propertyInfo);
                sb.Append($"{Environment.NewLine}{identation}{propertyInfo.Name} = {serialized}");
            }

            return sb.ToString();
        }

        private string? TrySerializeCollection(object collection, int nestingLevel)
        {
            if (collection is not ICollection)
                return null;

            var identation = new string(identationElement, nestingLevel + 1);
            if (collection is IDictionary dictionary)
                return SerializeDictionary(dictionary, nestingLevel, identation);

            return SerializeCollection(collection as ICollection, nestingLevel, identation);
        }

        private string SerializeCollection(ICollection collection, int nestingLevel, string identation)
        {
            var sb = new StringBuilder($"[{Environment.NewLine}");
            foreach (var obj in collection)
            {
                sb.Append($"{identation}{PrintToString(obj, nestingLevel + 1)},{Environment.NewLine}");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append($"{Environment.NewLine}{identation}]");

            return sb.ToString();
        }

        private string SerializeDictionary(IDictionary dictionary, int nestingLevel, string identation)
        {
            var sb = new StringBuilder($"{{{Environment.NewLine}");
            foreach (DictionaryEntry obj in dictionary)
            {
                sb.Append($"{identation}{PrintToString(obj.Key, nestingLevel + 1)} : {PrintToString(obj.Value, nestingLevel + 1)},{Environment.NewLine}");
            }

            sb.Append($"{identation}}}");
            return sb.ToString();
        }

        private string? TrySerializeByType(object obj, Type type)
        {
            if (typeSpecificSerializers.TryGetValue(type, out var serializer))
                return serializer(obj);

            if (obj is string)

                return ToTrimmedString(obj as string, stringTrimLength);

            if (finalTypes.Contains(type) || type.IsEnum && finalTypes.Contains(typeof(Enum)))
            {
                return typeCultureSettings.TryGetValue(type, out var culture)
                    ? ToStringAsFormattable(obj, culture)
                    : obj.ToString();
            }

            return null;
        }

        private string? TrySerializeWithMemberInfo(object? obj, MemberInfo? memberInfo)
        {
            if (memberInfo == null)
                return null;

            if (memberSpecificSerializers.TryGetValue(memberInfo, out var serializer))
                return serializer(obj);

            if (memberCultureSettings.TryGetValue(memberInfo, out var culture))
                return ToStringAsFormattable(obj, culture);

            if (memberTrimLengths.TryGetValue(memberInfo, out var trimLength))
                return ToTrimmedString(obj as string, trimLength);

            return null;
        }

        private static string ToTrimmedString(string str, int trimLength)
        {
            if (trimLength == -1 || str.Length <= trimLength)
                return str;
            if (trimLength < 0)
                throw new InvalidOperationException($"TrimLength, set for type or member, is invalid: {trimLength}");
            return $"{str[..trimLength]}...";
        }

        private static string ToStringAsFormattable(object obj, CultureInfo culture)
        {
            return (obj as IFormattable).ToString(null, culture);
        }

        public static string Print<T>(T obj)
        {
            return new ObjectPrinter().PrintToString(obj);
        }

        void IObjectPrinter.SetTrimLength(int trimLength)
        {
            stringTrimLength = trimLength;
        }

        void IObjectPrinter.SetTrimLength(MemberInfo member, int trimLength)
        {
            memberTrimLengths[member] = trimLength;
        }

        void IObjectPrinter.SetCulture(Type type, CultureInfo cultureInfo)
        {
            typeCultureSettings[type] = cultureInfo;
        }

        void IObjectPrinter.SetCulture(MemberInfo member, CultureInfo cultureInfo)
        {
            memberCultureSettings[member] = cultureInfo;
        }

        void IObjectPrinter.SetSerializer(Type type, Func<object, string> serializer)
        {
            typeSpecificSerializers[type] = serializer;
        }

        void IObjectPrinter.SetSerializer(MemberInfo member, Func<object, string> serializer)
        {
            memberSpecificSerializers[member] = serializer;
        }

        void IObjectPrinter.Exclude(Type type)
        {
            excludedTypes.Add(type);
        }

        void IObjectPrinter.Exclude(MemberInfo member)
        {
            excludedMembers.Add(member);
        }
    }
}