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
            {
                return AppendLine(PrintingFormat.NullValue);
            }

            var type = value.GetType();
            var formatter = settings.GetTypeFormatter(type);

            if (formatter is not null)
            {
                return AppendLine(formatter(value));
            }

            if (IsSimpleType(type))
            {
                return AppendLine(FormatValue(value, null));
            }

            return PrintComplexValue(
                value,
                type,
                nestingLevel,
                objectsInPath);
        }

        private string PrintComplexValue(
            object value,
            Type type,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            if (!type.IsValueType && !objectsInPath.Add(value))
            {
                return AppendLine(PrintingFormat.CircularReferenceMarker);
            }

            try
            {
                return PrintComplexValueContent(
                    value,
                    type,
                    nestingLevel,
                    objectsInPath);
            }
            finally
            {
                if (!type.IsValueType)
                {
                    objectsInPath.Remove(value);
                }
            }
        }

        private string PrintComplexValueContent(
            object value,
            Type type,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            if (value is IDictionary dictionary)
            {
                return PrintDictionary(dictionary, nestingLevel, objectsInPath);
            }

            if (value is Array array)
            {
                return PrintArray(array, nestingLevel, objectsInPath);
            }

            if (value is IEnumerable collection)
            {
                return PrintCollection(collection, nestingLevel, objectsInPath);
            }

            return PrintObject(value, type, nestingLevel, objectsInPath);
        }

        private string PrintArray(
            Array array,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            var builder = new StringBuilder();
            var indentation = GetIndentation(nestingLevel);

            if (array.Length == 0)
            {
                AppendEmptyCollection(builder, indentation);
                return builder.ToString();
            }

            var indices = new int[array.Rank];

            AppendArrayEntries(
                array,
                0,
                indices,
                builder,
                indentation,
                nestingLevel,
                objectsInPath);

            return builder.ToString();
        }

        private void AppendArrayEntries(
            Array array,
            int dimension,
            int[] indices,
            StringBuilder builder,
            string indentation,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            var index = array.GetLowerBound(dimension);
            var upperBound = array.GetUpperBound(dimension);

            while (true)
            {
                indices[dimension] = index;

                AppendArrayDimensionEntry(
                    array,
                    dimension,
                    indices,
                    builder,
                    indentation,
                    nestingLevel,
                    objectsInPath);

                if (index == upperBound)
                {
                    break;
                }

                index++;
            }
        }

        private void AppendArrayDimensionEntry(
            Array array,
            int dimension,
            int[] indices,
            StringBuilder builder,
            string indentation,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            if (dimension == array.Rank - 1)
            {
                AppendArrayEntry(
                    array,
                    indices,
                    builder,
                    indentation,
                    nestingLevel,
                    objectsInPath);

                return;
            }

            AppendArrayEntries(
                array,
                dimension + 1,
                indices,
                builder,
                indentation,
                nestingLevel,
                objectsInPath);
        }

        private void AppendArrayEntry(
            Array array,
            int[] indices,
            StringBuilder builder,
            string indentation,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            AppendCollectionEntry(
                builder,
                indentation,
                FormatArrayIndices(indices),
                array.GetValue(indices),
                nestingLevel,
                objectsInPath);
        }

        private static string FormatArrayIndices(int[] indices)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < indices.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(',');
                }

                builder.Append(indices[i].ToString(CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private string PrintObject(
            object value,
            Type type,
            int nestingLevel,
            HashSet<object> objectsInPath)
        {
            var indentation = GetIndentation(nestingLevel);
            var builder = new StringBuilder();

            builder.Append(type.Name)
                .Append(PrintingFormat.NewLine);

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
            {
                AppendEmptyCollection(builder, indentation);
            }

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
            {
                AppendEmptyCollection(builder, indentation);
            }

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
                .Append(PrintingFormat.NewLine);
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
                if (!ShouldPrintProperty(property))
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

        private bool ShouldPrintProperty(PropertyInfo property)
        {
            return property.GetGetMethod() is not null
                   && property.GetIndexParameters().Length == 0
                   && !settings.IsTypeExcluded(property.PropertyType)
                   && !settings.IsMemberExcluded(property);
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
                if (!ShouldPrintField(field))
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

        private bool ShouldPrintField(FieldInfo field)
        {
            return !settings.IsTypeExcluded(field.FieldType)
                   && !settings.IsMemberExcluded(field);
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
            var formatter = GetMemberFormatter(member, value);
            var stringFormatter = settings.GetMemberStringFormatter(member);

            if (formatter is not null)
            {
                return PrintFormattedMemberValue(
                    formatter,
                    value,
                    stringFormatter);
            }

            if (value is null)
            {
                return PrintNullMemberValue(stringFormatter);
            }

            var result = PrintValue(value, nestingLevel, objectsInPath);

            return ApplyStringFormatter(result, stringFormatter);
        }

        private Func<object?, string>? GetMemberFormatter(
            MemberInfo member,
            object? value)
        {
            var formatter = settings.GetMemberFormatter(member);

            formatter ??= settings.GetTypeFormatter(GetMemberType(member));

            if (formatter is null && value is not null)
            {
                formatter = settings.GetTypeFormatter(value.GetType());
            }

            return formatter;
        }

        private string PrintFormattedMemberValue(
            Func<object?, string> formatter,
            object? value,
            Func<string, string>? stringFormatter)
        {
            var result = formatter(value);

            if (stringFormatter is not null)
            {
                result = stringFormatter(result);
            }

            return AppendLine(result);
        }

        private string PrintNullMemberValue(Func<string, string>? stringFormatter)
        {
            var result = stringFormatter is null
                ? PrintingFormat.NullValue
                : stringFormatter(PrintingFormat.NullValue);

            return AppendLine(result);
        }

        private string ApplyStringFormatter(
            string value,
            Func<string, string>? stringFormatter)
        {
            if (stringFormatter is null)
            {
                return value;
            }

            return AppendLine(stringFormatter(RemoveNewLine(value)));
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
            {
                return PrintingFormat.NullValue;
            }

            var type = key.GetType();
            var formatter = settings.GetTypeFormatter(type);

            if (formatter is not null)
            {
                return formatter(key);
            }

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

        private static string AppendLine(string value)
        {
            return value + PrintingFormat.NewLine;
        }
    }
}