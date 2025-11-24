using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class Serializer<T>(PrintingConfig<T> config)
    {
        private readonly PrintingConfig<T> config = config;
        private StringBuilder sb;
        private HashSet<object> visited;

        public string Serialize(T root)
        {
            sb = new StringBuilder();
            visited = new HashSet<object>(new ReferenceEqualityComparer());

            PrintObject(root, 0, null);

            var result = sb.ToString();

            return result;
        }

        private void PrintObject(object? obj, int nestingLevel, MemberInfo? currentMember)
        {
            if (obj == null) { sb.AppendLine("null"); return; }

            var type = obj.GetType();

            if (IsExcluded(type, currentMember))
            {
                sb.AppendLine(string.Empty);
                return;
            }

            if (TryApplyMemberSerializer(obj, currentMember)) return;
            if (TryApplyTypeSerializer(type, obj)) return;

            if (HandleString(obj, currentMember)) return;
            if (HandleFormattable(type, obj)) return;
            if (HandleFinals(type, obj)) return;

            if (HandleReferenceTracking(type, obj)) return;

            if (HandleDictionary(obj, nestingLevel)) return;
            if (HandleEnumerable(obj, nestingLevel)) return;

            sb.AppendLine(type.Name);
            PrintProperties(type, obj, nestingLevel);
            PrintFields(type, obj, nestingLevel);
        }

        private static string Indent(int lvl) => new string('\t', lvl);

        private static object? GetValueSafely(MemberInfo member, object obj)
        {
            try
            {
                    switch (member)
                {
                        case PropertyInfo p: return p.GetValue(obj);
                        case FieldInfo f: return f.GetValue(obj);
                        default: return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private bool IsExcluded(Type type, MemberInfo? member) =>
            config.ExcludedTypes.Contains(type) || (member != null && config.ExcludedMembers.Contains(member));

        private bool TryApplyMemberSerializer(object obj, MemberInfo? member)
        {
            if (member == null || !config.MemberSerializers.TryGetValue(member, out var mser)) return false;
            var s = mser.DynamicInvoke(obj);
            sb.AppendLine(s?.ToString());
            return true;
        }

        private bool TryApplyTypeSerializer(Type type, object obj)
        {
            if (!config.TypeSerializers.TryGetValue(type, out var tser)) return false;
            var s = tser.DynamicInvoke(obj);
            sb.AppendLine(s?.ToString());
            return true;
        }

        private bool HandleString(object? obj, MemberInfo? member)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(string)) return false;

            var s = obj as string;
            if (member != null && config.MemberTrimLengths.TryGetValue(member, out var l) && s != null && s.Length > l)
                s = s.Substring(0, l);

            sb.AppendLine(s);
            return true;
        }

        private bool HandleFormattable(Type type, object obj)
        {
            if (obj is not IFormattable formattable || !config.TypeCultures.TryGetValue(type, out var culture))
                return false;
            sb.AppendLine(formattable.ToString(null, culture));
            return true;
        }

        private bool HandleFinals(Type type, object obj)
        {
            if (!config.FinalTypes.Contains(type)) return false;
            sb.AppendLine(obj.ToString());
            return true;
        }

        private bool HandleReferenceTracking(Type type, object obj)
        {
            if (type.IsValueType) return false;
            if (visited.Contains(obj))
            {
                sb.AppendLine($"<Циклическая ссылка {type.Name}>");
                return true;
            }
            visited.Add(obj);
            return false;
        }

        private bool HandleDictionary(object obj, int nestingLevel)
        {
            if (obj is not IDictionary dict) return false;

            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (DictionaryEntry e in dict)
            {
                sb.Append(Indent(nestingLevel + 1));
                sb.Append("Key = ");
                PrintObject(e.Key, nestingLevel + 1, null);

                sb.Append(Indent(nestingLevel + 1));
                sb.Append("Value = ");
                PrintObject(e.Value, nestingLevel + 1, null);
            }
            return true;
        }

        private bool HandleEnumerable(object obj, int nestingLevel)
        {
            if (obj is not IEnumerable enumerable || obj is string) return false;

            var type = obj.GetType();
            sb.AppendLine(type.Name);
            int i = 0;
            foreach (var item in enumerable)
            {
                sb.Append(Indent(nestingLevel + 1));
                sb.Append($"[{i}] = ");
                PrintObject(item, nestingLevel + 1, null);
                i++;
            }
            return true;
        }

        private void PrintProperties(Type type, object obj, int nestingLevel)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in props)
            {
                if (p.GetIndexParameters().Length > 0) continue;
                if (config.ExcludedTypes.Contains(p.PropertyType) || config.ExcludedMembers.Contains(p)) continue;

                sb.Append(Indent(nestingLevel + 1));
                sb.Append(p.Name);
                sb.Append(" = ");

                var value = GetValueSafely(p, obj);

                if (value is string sVal && config.MemberTrimLengths.TryGetValue(p, out var trim) && sVal.Length > trim)
                    value = sVal.Substring(0, trim);

                PrintObject(value, nestingLevel + 1, p);
            }
        }

        private void PrintFields(Type type, object obj, int nestingLevel)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var f in fields)
            {
                if (config.ExcludedTypes.Contains(f.FieldType) || config.ExcludedMembers.Contains(f)) continue;

                sb.Append(Indent(nestingLevel + 1));
                sb.Append(f.Name);
                sb.Append(" = ");

                var value = GetValueSafely(f, obj);

                if (value is string sf && config.MemberTrimLengths.TryGetValue(f, out var trimf) && sf.Length > trimf)
                    value = sf.Substring(0, trimf);

                PrintObject(value, nestingLevel + 1, f);
            }
        }

        private class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
