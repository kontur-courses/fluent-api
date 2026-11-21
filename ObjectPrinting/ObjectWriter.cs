using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    internal sealed class ObjectWriter
    {
        private static readonly HashSet<Type> FinalTypes = new()
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
        };

        private readonly IReadOnlyDictionary<Type, TypeSettings> typeSettings;
        private readonly IReadOnlyDictionary<MemberInfo, MemberSettings> memberSettings;

        internal ObjectWriter(
            IReadOnlyDictionary<Type, TypeSettings> typeSettings,
            IReadOnlyDictionary<MemberInfo, MemberSettings> memberSettings)
        {
            this.typeSettings = typeSettings;
            this.memberSettings = memberSettings;
        }

        internal string Print(object? obj)
        {
            var builder = new StringBuilder();
            AppendValue(
                builder,
                obj,
                0,
                new HashSet<object>(ReferenceEqualityComparer.Instance));

            return builder.ToString();
        }

        private void AppendValue(
            StringBuilder builder,
            object? obj,
            int nestingLevel,
            HashSet<object> activeReferences)
        {
            if (obj == null)
            {
                builder.Append("null");
                return;
            }

            var type = obj.GetType();
            if (typeSettings.TryGetValue(type, out var settings))
            {
                var typeSerializer = settings.Serializer;
                if (typeSerializer is not null)
                {
                    builder.Append(typeSerializer(obj));
                    return;
                }

                var culture = settings.Culture;
                if (culture is not null && obj is IFormattable formattable)
                {
                    builder.Append(formattable.ToString(null, culture));
                    return;
                }
            }

            if (FinalTypes.Contains(type))
            {
                builder.Append(obj.ToString());
                return;
            }

            var isReferenceType = !type.IsValueType;
            if (isReferenceType && !activeReferences.Add(obj))
            {
                builder.Append($"[Циклическая ссылка: {type.Name}]");
                return;
            }

            try
            {
                if (obj is IDictionary dictionary)
                {
                    AppendDictionary(builder, dictionary, nestingLevel, activeReferences);
                    return;
                }

                if (obj is IEnumerable collection)
                {
                    AppendCollection(builder, collection, nestingLevel, activeReferences);
                    return;
                }

                AppendObjectMembers(builder, obj, type, nestingLevel, activeReferences);
            }
            finally
            {
                if (isReferenceType)
                    activeReferences.Remove(obj);
            }
        }

        private void AppendDictionary(
            StringBuilder builder,
            IDictionary dictionary,
            int nestingLevel,
            HashSet<object> activeReferences)
        {
            var indentation = new string('\t', nestingLevel);
            var childIndentation = new string('\t', nestingLevel + 1);

            builder.Append('{');
            var hasEntries = false;
            foreach (DictionaryEntry entry in dictionary)
            {
                if (hasEntries)
                    builder.Append(',');

                builder.AppendLine().Append(childIndentation);
                AppendValue(builder, entry.Key, nestingLevel + 1, activeReferences);
                builder.Append(": ");
                AppendValue(builder, entry.Value, nestingLevel + 1, activeReferences);
                hasEntries = true;
            }

            if (hasEntries)
                builder.AppendLine().Append(indentation);

            builder.Append('}');
        }

        private void AppendCollection(
            StringBuilder builder,
            IEnumerable collection,
            int nestingLevel,
            HashSet<object> activeReferences)
        {
            var indentation = new string('\t', nestingLevel);
            var childIndentation = new string('\t', nestingLevel + 1);

            builder.Append('[');
            var hasItems = false;
            foreach (var item in collection)
            {
                if (hasItems)
                    builder.Append(',');

                builder.AppendLine().Append(childIndentation);
                AppendValue(builder, item, nestingLevel + 1, activeReferences);
                hasItems = true;
            }

            if (hasItems)
                builder.AppendLine().Append(indentation);

            builder.Append(']');
        }

        private void AppendObjectMembers(
            StringBuilder builder,
            object obj,
            Type type,
            int nestingLevel,
            HashSet<object> activeReferences)
        {
            var childIndentation = new string('\t', nestingLevel + 1);
            builder.Append(type.Name).Append(':');

            foreach (var member in GetPublicMembers(type))
            {
                var memberType = GetMemberType(member);
                memberSettings.TryGetValue(member, out var memberConfiguration);

                if ((typeSettings.TryGetValue(memberType, out var typeConfiguration)
                        && typeConfiguration.IsExcluded)
                    || memberConfiguration?.IsExcluded == true)
                {
                    continue;
                }

                var value = member switch
                {
                    PropertyInfo property => property.GetValue(obj),
                    FieldInfo field => field.GetValue(obj),
                    _ => throw new ArgumentException("Неизвестный тип", nameof(member))
                };

                if (value is string text
                    && memberConfiguration?.MaxStringLength is int maxLength
                    && text.Length > maxLength)
                {
                    value = text[..maxLength];
                }

                builder.AppendLine()
                    .Append(childIndentation)
                    .Append(member.Name)
                    .Append(" = ");

                var memberSerializer = memberConfiguration?.Serializer;
                if (memberSerializer is not null)
                {
                    builder.Append(memberSerializer(value));
                }
                else
                {
                    AppendValue(builder, value, nestingLevel + 1, activeReferences);
                }
            }
        }

        private static IEnumerable<MemberInfo> GetPublicMembers(Type type)
        {
            var properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.GetIndexParameters().Length == 0
                    && property.GetGetMethod() is not null);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

            return properties.Cast<MemberInfo>().Concat(fields);
        }

        private static Type GetMemberType(MemberInfo member)
        {
            return member switch
            {
                PropertyInfo property => property.PropertyType,
                FieldInfo field => field.FieldType,
                _ => throw new ArgumentException("Неизвестный тип")
            };
        }
    }
}
