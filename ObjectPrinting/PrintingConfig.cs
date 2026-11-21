using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Runtime.CompilerServices;
using System.Collections;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private HashSet<Type> ExcludedTypes = new HashSet<Type>();
        internal HashSet<(Type, string)> ExcludedMembers = new HashSet<(Type, string)> ();
        private Dictionary<Type, Func<object, string>> TypeSerializers = new Dictionary<Type, Func<object, string>>();
        internal Dictionary<(Type, string), Func<object, string>> MemberSerializers = new Dictionary<(Type, string), Func<object, string>>();
        private CultureInfo Culture = CultureInfo.CurrentCulture;
        private int? TruncateLength = null;

        public string PrintToString(TOwner obj)
        {
            var visited = new HashSet<object>(ReferenceComparer.Instance);
            return PrintToString(obj, 0, visited);
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> visited)
        {
            if (obj == null) return "null";

            var type = obj.GetType();
            if (TypeSerializers.TryGetValue(type, out var custom)) return custom(obj);

            if (obj is string str)
            {
                return TruncateLength.HasValue && str.Length > TruncateLength.Value
                    ? str.Substring(0, TruncateLength.Value) : str;
            }

            if (type.IsEnum) return obj.ToString();            
            if (obj is IFormattable f) return f.ToString(null, Culture);
            if (type.IsPrimitive) return Convert.ToString(obj, Culture);

            var isRef = !type.IsValueType;
            if (isRef && !visited.Add(obj)) return "<cycle>";
            try
            {
                if (obj is IDictionary dict) return SerializeDictionary(dict, nestingLevel, visited);
                if (obj is IEnumerable en) return SerializeEnumerable(en, nestingLevel, visited);
                return SerializeObject(obj, nestingLevel, visited);
            }
            finally
            {
                if (isRef) visited.Remove(obj);
            }
        }

        private string SerializeObject(object obj, int nestingLevel, HashSet<object> visited)
        {
            var type = obj.GetType();
            var indent = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            var members = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Cast<MemberInfo>()
                .Concat(type.GetFields(BindingFlags.Public | BindingFlags.Instance));

            foreach (var member in members)
            {
                var memberType = member is PropertyInfo pi ? pi.PropertyType : ((FieldInfo)member).FieldType;
                var underlying = Nullable.GetUnderlyingType(memberType) ?? memberType;

                if (ExcludedTypes.Contains(underlying)) continue;
                if (ExcludedMembers.Contains((member.DeclaringType, member.Name))) continue;

                object value = member is PropertyInfo p
                    ? p.GetValue(obj) : ((FieldInfo)member).GetValue(obj);

                string valueStr;
                if (MemberSerializers.TryGetValue((member.DeclaringType, member.Name), out var ms))
                {
                    valueStr = ms(value);
                }
                else valueStr = PrintToString(value, nestingLevel + 1, visited);

                sb.AppendLine($"{indent}{member.Name} = {valueStr}");
            }
            return sb.ToString().TrimEnd();
        }

        private string SerializeEnumerable(IEnumerable en, int nestingLevel, HashSet<object> visited)
        {
            var indent = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine("Collection:");
            foreach (var item in en)
            {
                sb.AppendLine($"{indent}{PrintToString(item, nestingLevel + 1, visited)}");
            }
            return sb.ToString().TrimEnd();
        }

        private string SerializeDictionary(IDictionary dict, int nestingLevel, HashSet<object> visited)
        {
            var indent = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine("Dictionary:");
            
            foreach (DictionaryEntry entry in dict)
            {
                var key = PrintToString(entry.Key, nestingLevel + 1, visited);
                var value = PrintToString(entry.Value, nestingLevel + 1, visited);
                sb.AppendLine($"{indent}{key} = {value}");
            }
            return sb.ToString().TrimEnd();
        }

        public PrintingConfig<TOwner> ExcludeType(Type type)
        {
            ExcludedTypes.Add(type);
            return this;
        }

        public PrintingConfig<TOwner> AlternativeSerialization(Type type, Func<dynamic, dynamic> func)
        {
            TypeSerializers[type] = o => func(o)?.ToString();
            return this;
        }

        public PrintingConfig<TOwner> SetNumericCulture(CultureInfo culture)
        {
            Culture = culture;
            return this;
        }

        public PropertyPrintingConfig<TOwner, TProp> SetMemberSerialization<TProp>(
            Expression<Func<TOwner, TProp>> selector) => new(this, GetMemberInfo(selector));

        public PrintingConfig<TOwner> SetMemberSerialization<TProp>(
            Expression<Func<TOwner, TProp>> selector, Func<TProp, string> serializer)
        {
            var m = GetMemberInfo(selector);
            MemberSerializers[(m.DeclaringType, m.Name)] = o => serializer((TProp)o);
            return this;
        }

        public PrintingConfig<TOwner> Truncate(int length)
        {
            TruncateLength = length;
            return this;
        }

        public PrintingConfig<TOwner> ExcludeMember<TProp>(Expression<Func<TOwner, TProp>> selector)
        {
            var m = GetMemberInfo(selector);
            ExcludedMembers.Add((m.DeclaringType, m.Name));
            return this;
        }

        public PrintingConfig<TOwner> StandardSerialization()
        {
            return this;
        }

        private static MemberInfo GetMemberInfo(LambdaExpression expr)
        {
            var body = expr.Body;
            if (body is UnaryExpression ue && ue.NodeType == ExpressionType.Convert)
            {
                body = ue.Operand;
            }
            if (body is MemberExpression me) return me.Member;
            throw new ArgumentException("Selector must be a simple member access expression");
        }

        private class ReferenceComparer : IEqualityComparer<object>
        {
            public static ReferenceComparer Instance = new ReferenceComparer();
            bool IEqualityComparer<object>.Equals(object? x, object? y) => ReferenceEquals(x, y);
            int IEqualityComparer<object>.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}