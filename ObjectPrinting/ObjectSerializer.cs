using System;
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
                    FormatValue(value, settings.GetTypeCulture(type)));
            }

            if (!type.IsValueType && !objectsInPath.Add(value))
                return AppendLine(PrintingFormat.CircularReferenceMarker);

            try
            {
                var indentation = new string(
                    PrintingFormat.IndentationCharacter,
                    nestingLevel + 1);
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
            finally
            {
                if (!type.IsValueType)
                    objectsInPath.Remove(value);
            }
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
            if (value is null)
                return PrintValue(null, nestingLevel, objectsInPath);

            var formatter = settings.GetMemberFormatter(member);
            var culture = settings.GetMemberCulture(member);

            string result;

            if (formatter is not null)
            {
                result = AppendLine(formatter(value));
            }
            else if (culture is not null)
            {
                result = AppendLine(FormatValue(value, culture));
            }
            else
            {
                result = PrintValue(value, nestingLevel, objectsInPath);
            }

            var maxLength = settings.GetMemberMaxLength(member);

            if (maxLength is null)
                return result;

            var lineEnding = Environment.NewLine;

            if (result.EndsWith(lineEnding, StringComparison.Ordinal))
                result = result[..^lineEnding.Length];

            return AppendLine(Truncate(result, maxLength.Value));
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
            return value + Environment.NewLine;
        }
    }
}