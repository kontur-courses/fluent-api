using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class ObjectSerializer<TOwner>
    {
        private readonly PrintingConfig<TOwner> config;
        private readonly int maxDepth;
        private readonly HashSet<object> visited = new(ReferenceEqualityComparer.Instance);

        public ObjectSerializer(PrintingConfig<TOwner> config, int maxDepth)
        {
            this.config = config;
            this.maxDepth = maxDepth;
        }

        public string Serialize(object? obj) => Print(obj, 0);

        private string Print(object? obj, int level)
        {
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (config.ExcludedTypes.Contains(type))
                return string.Empty;

            if (TypeInspector.IsFinalType(type))
                return PrintFinal(obj, type);

            if (obj is IDictionary dict)
                return PrintDictionary(dict, level);

            if (obj is IEnumerable enumerable and not string)
                return PrintEnumerable(enumerable, level);

            return PrintComplex(obj, type, level);
        }

        private string PrintFinal(object obj, Type type)
        {
            if (config.TypeSerializers.TryGetValue(type, out var ser))
                return ser(obj) + Environment.NewLine;

            if (config.TypeCultures.TryGetValue(type, out var culture) && obj is IFormattable f)
                return f.ToString(null, culture) + Environment.NewLine;

            return obj + Environment.NewLine;
        }

        private string PrintComplex(object obj, Type type, int level)
        {
            if (!visited.Add(obj))
                return "<cycle>" + Environment.NewLine;

            if (level >= maxDepth)
            {
                visited.Remove(obj);
                return $"<{type.Name}>" + Environment.NewLine;
            }

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(type.Name);
                var indentation = new string('\t', level + 1);

                foreach (var member in TypeInspector.GetMembers(type))
                {
                    if (config.ExcludedTypes.Contains(member.MemberType)) continue;
                    if (config.ExcludedMembers.Contains(member.Name)) continue;

                    var value = member.GetValue(obj);
                    sb.Append(indentation + member.Name + " = ");
                    sb.Append(PrintMember(member.Name, value, level + 1));
                }

                return sb.ToString();
            }
            finally
            {
                visited.Remove(obj);
            }
        }

        private string PrintMember(string memberName, object? value, int level)
        {
            if (config.MemberSerializers.TryGetValue(memberName, out var ser))
                return ser(value) + Environment.NewLine;

            if (value is not null
                && config.MemberCultures.TryGetValue(memberName, out var culture) && value is IFormattable f)
                return f.ToString(null, culture) + Environment.NewLine;

            return Print(value, level);
        }

        private string PrintEnumerable(IEnumerable enumerable, int level)
        {
            if (!visited.Add(enumerable))
                return "<cycle>" + Environment.NewLine;

            if (level >= maxDepth)
            {
                visited.Remove(enumerable);
                return $"<{enumerable.GetType().Name}>" + Environment.NewLine;
            }

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(enumerable.GetType().Name);
                var indentation = new string('\t', level + 1);

                foreach (var item in enumerable)
                    sb.Append(indentation + "- " + Print(item, level + 1));

                return sb.ToString();
            }
            finally
            {
                visited.Remove(enumerable);
            }
        }

        private string PrintDictionary(IDictionary dict, int level)
        {
            if (!visited.Add(dict))
                return "<cycle>" + Environment.NewLine;

            if (level >= maxDepth)
            {
                visited.Remove(dict);
                return $"<{dict.GetType().Name}>" + Environment.NewLine;
            }

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(dict.GetType().Name);
                var indentation = new string('\t', level + 1);

                foreach (DictionaryEntry entry in dict)
                {
                    sb.Append(indentation + "[");
                    sb.Append(Print(entry.Key, level + 1).TrimEnd());
                    sb.Append("] = ");
                    sb.Append(Print(entry.Value, level + 1));
                }

                return sb.ToString();
            }
            finally
            {
                visited.Remove(dict);
            }
        }
    }
}