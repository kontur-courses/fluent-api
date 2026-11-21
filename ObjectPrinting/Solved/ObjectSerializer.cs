using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Solved
{
    internal sealed class ObjectSerializer<TOwner>
    {
        private readonly PrintingConfig<TOwner> config;
        private readonly int maxDepth;
        private readonly HashSet<object> visited =
            new(ReferenceEqualityComparer.Instance);

        public ObjectSerializer(PrintingConfig<TOwner> config, int maxDepth)
        {
            this.config = config;
            this.maxDepth = maxDepth;
        }

        public string Serialize(object? obj)
        {
            var builder = new StringBuilder();
            Print(builder, obj, 0);
            return builder.ToString();
        }

        private void Print(StringBuilder builder, object? obj, int level)
        {
            if (obj is null)
            {
                builder.Append("null").Append(Environment.NewLine);
                return;
            }

            var type = obj.GetType();

            if (config.ExcludedTypes.Contains(type))
            {
                return;
            }

            if (config.TypeSerializers.TryGetValue(type, out var serializer))
            {
                builder.Append(serializer(obj) ?? "null").Append(Environment.NewLine);
                return;
            }

            if (config.TypeCultures.TryGetValue(type, out var culture)
                && obj is IFormattable formattable)
            {
                builder.Append(formattable.ToString(null, culture)).Append(Environment.NewLine);
                return;
            }

            if (TypeInspector.IsFinalType(type))
            {
                builder.Append(obj).Append(Environment.NewLine);
                return;
            }

            if (obj is IDictionary dictionary)
            {
                PrintDictionary(builder, dictionary, level);
                return;
            }

            if (obj is IEnumerable enumerable and not string)
            {
                PrintEnumerable(builder, enumerable, level);
                return;
            }

            PrintComplex(builder, obj, type, level);
        }

        private void PrintWithGuards(
            StringBuilder builder, object obj, Type type, int level, Action body)
        {
            if (!visited.Add(obj))
            {
                builder.Append("<cycle>").Append(Environment.NewLine);
                return;
            }

            try
            {
                if (level >= maxDepth)
                {
                    builder.Append('<').Append(type.Name).Append('>').Append(Environment.NewLine);
                    return;
                }

                body();
            }
            finally
            {
                visited.Remove(obj);
            }
        }

        private void PrintComplex(StringBuilder builder, object obj, Type type, int level)
            => PrintWithGuards(builder, obj, type, level, () =>
            {
                builder.AppendLine(type.Name);
                var indentation = new string('\t', level + 1);

                foreach (var member in TypeInspector.GetMembers(type))
                {
                    if (config.ExcludedTypes.Contains(member.MemberType))
                    {
                        continue;
                    }

                    if (config.ExcludedMembers.Contains(member.Name))
                    {
                        continue;
                    }

                    var value = member.GetValue(obj);

                    if (value is not null && config.ExcludedTypes.Contains(value.GetType()))
                    {
                        continue;
                    }

                    builder.Append(indentation).Append(member.Name).Append(" = ");
                    PrintMember(builder, member.Name, value, level + 1);
                }
            });

        private void PrintMember(
            StringBuilder builder, string memberName, object? value, int level)
        {
            if (config.MemberSerializers.TryGetValue(memberName, out var serializer))
            {
                builder.Append(serializer(value) ?? "null").Append(Environment.NewLine);
                return;
            }

            if (value is not null
                && config.MemberCultures.TryGetValue(memberName, out var culture)
                && value is IFormattable formattable)
            {
                builder.Append(formattable.ToString(null, culture)).Append(Environment.NewLine);
                return;
            }

            Print(builder, value, level);
        }

        private void PrintEnumerable(StringBuilder builder, IEnumerable enumerable, int level)
            => PrintWithGuards(builder, enumerable, enumerable.GetType(), level, () =>
            {
                builder.Append('[');

                var first = true;
                foreach (var item in enumerable)
                {
                    if (!first)
                    {
                        builder.Append(", ");
                    }

                    first = false;

                    var start = builder.Length;
                    Print(builder, item, level + 1);
                    TrimTrailingNewLine(builder, start);
                }

                builder.Append(']').Append(Environment.NewLine);
            });

        private void PrintDictionary(StringBuilder builder, IDictionary dictionary, int level)
            => PrintWithGuards(builder, dictionary, dictionary.GetType(), level, () =>
            {
                builder.AppendLine(dictionary.GetType().Name);
                var indentation = new string('\t', level + 1);

                foreach (DictionaryEntry entry in dictionary)
                {
                    builder.Append(indentation).Append('[');

                    var start = builder.Length;
                    Print(builder, entry.Key, level + 1);
                    TrimTrailingNewLine(builder, start);

                    builder.Append("] = ");
                    Print(builder, entry.Value, level + 1);
                }
            });

        private static void TrimTrailingNewLine(StringBuilder builder, int since)
        {
            var newLineLength = Environment.NewLine.Length;
            if (builder.Length >= since + newLineLength
                && string.CompareOrdinal(
                    builder.ToString(builder.Length - newLineLength, newLineLength),
                    Environment.NewLine) == 0)
            {
                builder.Length -= newLineLength;
            }
        }
    }
}