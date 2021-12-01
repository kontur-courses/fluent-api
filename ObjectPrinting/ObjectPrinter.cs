using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class ObjectPrinter<TOwner>
    {
        protected const char identationElement = '\t';
        protected readonly Dictionary<object, Guid> alreadyPrinted = new();
        internal Config? Config;

        private ObjectPrinter(Config? config)
        {
            Config = config;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, Config);
        }

        public static ObjectPrinter<TOwner> Configure(Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> options)
        {
            var configurator = options(new PrintingConfig<TOwner>());
            return new ObjectPrinter<TOwner>(configurator.Config);
        }

        public static ObjectPrinter<TOwner> Default => new(null);

        public static string Print(TOwner obj)
        {
            return Default.PrintToString(obj);
        }

        private string PrintToString(object? obj, int nestingLevel, Config? config = null, MemberInfo? currentMember = null)
        {
            if (obj == null)
                return "null";

            if (alreadyPrinted.TryGetValue(obj, out var id))
                return $"was printed before : {id}";

            config ??= new Config();
            var serialized = TrySerializeWithMemberInfo(obj, currentMember, config);
            if (serialized != null)
                return serialized;

            var type = obj.GetType();

            serialized = TrySerializeCollection(obj, nestingLevel, config);
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
            alreadyPrinted.Add(obj, id);
            var identation = new string(identationElement, nestingLevel + 1);
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

        private string? TrySerializeCollection(object collection, int nestingLevel, Config config)
        {
            if (collection is not ICollection)
                return null;

            var identation = new string(identationElement, nestingLevel + 1);
            if (collection is IDictionary dictionary)
                return SerializeDictionary(dictionary, nestingLevel, identation, config);

            return SerializeCollection(collection as ICollection, nestingLevel, identation, config);
        }

        private string SerializeCollection(ICollection collection, int nestingLevel, string identation, Config config)
        {
            var sb = new StringBuilder($"[{Environment.NewLine}");
            foreach (var obj in collection)
            {
                sb.Append($"{identation}{PrintToString(obj, nestingLevel + 1, config)},{Environment.NewLine}");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append($"{Environment.NewLine}{identation}]");

            return sb.ToString();
        }

        private string SerializeDictionary(IDictionary dictionary, int nestingLevel, string identation, Config config)
        {
            var sb = new StringBuilder($"{{{Environment.NewLine}");
            foreach (DictionaryEntry obj in dictionary)
            {
                sb.Append($"{identation}{PrintToString(obj.Key, nestingLevel + 1, config)} : {PrintToString(obj.Value, nestingLevel + 1, config)},{Environment.NewLine}");
            }

            sb.Append($"{identation}}}");
            return sb.ToString();
        }

        private string? TrySerializeByType(object obj, Type type, Config config)
        {
            if (config.TypeSpecificSerializers.TryGetValue(type, out var serializer))
                return serializer(obj);

            if (obj is string)

                return ToTrimmedString(obj as string, config.StringTrimLength);

            if (config.FinalTypes.Contains(type) || type.IsEnum && config.FinalTypes.Contains(typeof(Enum)))
            {
                return config.TypeCultureSettings.TryGetValue(type, out var culture)
                    ? ToStringAsFormattable(obj, culture)
                    : obj.ToString();
            }

            return null;
        }

        private string? TrySerializeWithMemberInfo(object? obj, MemberInfo? memberInfo, Config config)
        {
            if (memberInfo == null)
                return null;

            if (config.MemberSpecificSerializers.TryGetValue(memberInfo, out var serializer))
                return serializer(obj);

            if (config.MemberCultureSettings.TryGetValue(memberInfo, out var culture))
                return ToStringAsFormattable(obj, culture);

            if (config.MemberTrimLengths.TryGetValue(memberInfo, out var trimLength))
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
    }
}