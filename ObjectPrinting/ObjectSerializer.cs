using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    internal sealed class ObjectSerializer
    {
        private readonly PrintingSettings settings;

        internal ObjectSerializer(PrintingSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            this.settings = settings;
        }

        internal string PrintToString(object? value)
        {
            var objectsInPath = new HashSet<object>(
                ReferenceEqualityComparer.Instance);

            return PrintValue(value, 0, objectsInPath);
        }

        private string PrintValue(
            object? value,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            if (value is null)
                return AppendLine(PrintingFormat.NullValue);

            var type = value.GetType();
            var formatter = settings.GetTypeFormatter(type);

            if (formatter is not null)
                return AppendLine(formatter(value));

            if (IsSimpleType(type))
            {
                return AppendLine(
                    FormatValue(value, null));
            }

            if (!type.IsValueType && !objectsInPath.Add(value))
            {
                return AppendLine(
                    PrintingFormat.CircularReferenceMarker);
            }

            try
            {
                if (value is IDictionary dictionary)
                {
                    return PrintDictionary(
                        dictionary,
                        nestingLevel,
                        objectsInPath);
                }

                if (value is IEnumerable collection)
                {
                    return PrintCollection(
                        collection,
                        nestingLevel,
                        objectsInPath);
                }

                return PrintObject(
                    value,
                    type,
                    nestingLevel,
                    objectsInPath);
            }
            finally
            {
                if (!type.IsValueType)
                    objectsInPath.Remove(value);
            }
        }

        private string PrintObject(
            object value,
            Type type,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            var indentation = GetIndentation(nestingLevel);
            var builder = new StringBuilder();

            builder.AppendLine(type.Name);

            AppendProperties(
                builder,
                value,
                type,
                indentation,
                nestingLevel,
                objectsInPath);

            AppendFields(
                builder,
                value,
                type,
                indentation,
                nestingLevel,
                objectsInPath);

            return builder.ToString();
        }

        private string PrintCollection(
            IEnumerable collection,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            var builder = new StringBuilder();
            var indentation = GetIndentation(nestingLevel);
            var index = 0;

            foreach (var item in collection)
            {
                AppendCollectionEntry(
                    builder,
                    indentation,
                    index.ToString(CultureInfo.InvariantCulture),
                    item,
                    nestingLevel,
                    objectsInPath);

                index++;
            }

            if (index == 0)
                AppendEmptyCollection(builder, indentation);

            return builder.ToString();
        }

        private string PrintDictionary(
            IDictionary dictionary,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            var builder = new StringBuilder();
            var indentation = GetIndentation(nestingLevel);

            foreach (DictionaryEntry entry in dictionary)
            {
                AppendCollectionEntry(
                    builder,
                    indentation,
                    FormatDictionaryKey(entry.Key),
                    entry.Value,
                    nestingLevel,
                    objectsInPath);
            }

            if (dictionary.Count == 0)
                AppendEmptyCollection(builder, indentation);

            return builder.ToString();
        }

        private void AppendCollectionEntry(
            StringBuilder builder,
            string indentation,
            string key,
            object? value,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            builder.Append(indentation)
                .Append(PrintingFormat.CollectionEntryPrefix)
                .Append(key)
                .Append(PrintingFormat.CollectionEntrySuffix)
                .Append(PrintingFormat.MemberAssignmentSeparator)
                .Append(PrintValue(
                    value,
                    nestingLevel + 1,
                    objectsInPath));
        }

        private static void AppendEmptyCollection(
            StringBuilder builder,
            string indentation)
        {
            builder.Append(indentation)
                .Append(PrintingFormat.EmptyCollectionMarker)
                .AppendLine();
        }

        private void AppendProperties(
            StringBuilder builder,
            object obj,
            Type type,
            string indentation,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            var properties = type.GetProperties(
                BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in properties)
            {
                if (property.GetGetMethod() is null)
                    continue;

                if (property.GetIndexParameters().Length > 0)
                    continue;

                if (settings.IsTypeExcluded(property.PropertyType) ||
                    settings.IsMemberExcluded(property))
                {
                    continue;
                }

                AppendMember(
                    builder,
                    indentation,
                    property,
                    property.GetValue(obj),
                    nestingLevel,
                    objectsInPath);
            }
        }

        private void AppendFields(
            StringBuilder builder,
            object obj,
            Type type,
            string indentation,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            var fields = type.GetFields(
                BindingFlags.Instance | BindingFlags.Public);

            foreach (var field in fields)
            {
                if (settings.IsTypeExcluded(field.FieldType) ||
                    settings.IsMemberExcluded(field))
                {
                    continue;
                }

                AppendMember(
                    builder,
                    indentation,
                    field,
                    field.GetValue(obj),
                    nestingLevel,
                    objectsInPath);
            }
        }

        private void AppendMember(
            StringBuilder builder,
            string indentation,
            MemberInfo member,
            object? value,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            builder.Append(indentation)
                .Append(member.Name)
                .Append(PrintingFormat.MemberAssignmentSeparator)
                .Append(PrintMemberValue(
                    member,
                    value,
                    nestingLevel + 1,
                    objectsInPath));
        }

        private string PrintMemberValue(
            MemberInfo member,
            object? value,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            var formatter = settings.GetMemberFormatter(member);
            formatter ??= settings.GetTypeFormatter(GetMemberType(member));

            if (formatter is null && value is not null)
            {
                formatter = settings.GetTypeFormatter(value.GetType());
            }

            var stringFormatter = settings.GetMemberStringFormatter(member);

            if (formatter is not null)
            {
                var formattedValue = formatter(value);

                if (stringFormatter is not null)
                {
                    formattedValue = stringFormatter(formattedValue);
                }

                return AppendLine(formattedValue);
            }

            if (value is null)
            {
                var nullValue = stringFormatter is null
                    ? PrintingFormat.NullValue
                    : stringFormatter(PrintingFormat.NullValue);

                return AppendLine(nullValue);
            }

            var result = PrintValue(value, nestingLevel, objectsInPath);

            if (stringFormatter is null)
            {
                return result;
            }

            return AppendLine(stringFormatter(RemoveNewLine(result)));
        }
        
        private static Type GetMemberType(MemberInfo member)
        {
            return member switch
            {
                PropertyInfo property => property.PropertyType,
                FieldInfo field => field.FieldType,
                _ => throw new ArgumentException(
                    "Member must be a property or field.",
                    nameof(member))
            };
        }

        private static string RemoveNewLine(string value)
        {
            return value.EndsWith(PrintingFormat.NewLine, StringComparison.Ordinal)
                ? value[..^PrintingFormat.NewLine.Length]
                : value;
        }

        private string FormatDictionaryKey(object? key)
        {
            if (key is null)
                return PrintingFormat.NullValue;

            var type = key.GetType();
            var formatter = settings.GetTypeFormatter(type);

            if (formatter is not null)
                return formatter(key);

            return FormatValue(key, null);
        }

        private static string GetIndentation(int nestingLevel)
        {
            return new string(
                PrintingFormat.IndentationCharacter,
                nestingLevel + 1);
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(string) ||
                   typeof(IFormattable).IsAssignableFrom(type);
        }

        private static string FormatValue(
            object value,
            CultureInfo? culture)
        {
            if (value is IFormattable formattable)
            {
                return formattable.ToString(
                    null,
                    culture ?? CultureInfo.CurrentCulture) ?? string.Empty;
            }

            return value.ToString() ?? string.Empty;
        }

        private static string Truncate(string value, int maxLength)
        {
            return value.Length <= maxLength
                ? value
                : value[..maxLength];
        }

        private static string AppendLine(string value)
        {
            return value + PrintingFormat.NewLine;
        }
    }
}