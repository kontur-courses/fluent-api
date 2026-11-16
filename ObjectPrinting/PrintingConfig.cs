using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework.Internal;
using ObjectPrinting.Configuration;
using ObjectPrinting.Solved;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        internal readonly HashSet<Type> excludedTypes = new();
        internal readonly HashSet<string> excludedProperties = new();
        internal readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
        internal readonly Dictionary<string, Func<object, string>> propertySerializers = new();
        internal readonly Dictionary<Type, CultureInfo> typeCultures = new();
        internal readonly Dictionary<string, int> trimLengths = new();

        internal readonly HashSet<object> visitedObjects = new();

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Using<T>(Func<T, string> serializer)
        {
            typeSerializers[typeof(T)] = obj => serializer((T)obj);
            return this;
        }

        public PrintingConfig<TOwner> UsingCulture<T>(CultureInfo culture)
        {
            typeCultures[typeof(T)] = culture;
            return this;
        }

        public IPropertyConfig<TOwner> Property(string propertyName)
        {
            return new PropertyConfigImpl<TOwner>(this, propertyName);
        }

        public IStringPropertyConfig<TOwner> Property(Expression<Func<TOwner, object>> propertySelector)
        {
            var name = GetPropertyName(propertySelector);
            return new StringPropertyConfigImpl<TOwner>(this, name);
        }

        public string PrintToString(TOwner obj)
        {
            visitedObjects.Clear();
            return PrintToStringInternal(obj, 0);
        }

        private string PrintToStringInternal(object? obj, int nestingLevel, string? propertyName = null)
        {
            var type = obj?.GetType();

            if (propertyName != null && propertySerializers.TryGetValue(propertyName, out var propSerializer))
            {
                return propSerializer(obj) + Environment.NewLine;
            }

            if (obj == null)
                return "null" + Environment.NewLine;

            if (typeSerializers.ContainsKey(type) || (propertyName != null && propertySerializers.ContainsKey(propertyName)))
            {
                return SerializeValue(obj, type, propertyName) + Environment.NewLine;
            }

            if (IsFinalType(type))
                return SerializeValue(obj, type, propertyName) + Environment.NewLine;

            if (!type.IsValueType)
            {
                if (visitedObjects.Contains(obj))
                    return "<Circular Reference>" + Environment.NewLine;

                visitedObjects.Add(obj);
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            if (obj is IEnumerable enumerable)
            {
                sb.AppendLine(type.Name);
                foreach (var item in enumerable)
                {
                    sb.Append(identation);
                    sb.Append(PrintToStringInternal(item, nestingLevel + 1, propertyName).TrimEnd());
                    sb.AppendLine();
                }
                if (!type.IsValueType) visitedObjects.Remove(obj);
                return sb.ToString();
            }

            sb.AppendLine(type.Name);

            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field);

            foreach (var member in members)
            {
                if (excludedProperties.Contains(member.Name) || excludedTypes.Contains(GetMemberType(member)))
                    continue;

                object? value = null;
                if (member is PropertyInfo p) value = p.GetValue(obj);
                if (member is FieldInfo f) value = f.GetValue(obj);

                sb.Append(identation + member.Name + " = " + 
                          PrintToStringInternal(value, nestingLevel + 1, member.Name).TrimEnd());
                sb.AppendLine();
            }

            if (!type.IsValueType) visitedObjects.Remove(obj);
            return sb.ToString();
        }

        private string SerializeValue(object obj, Type type, string? propertyName)
        {
            if (propertyName != null && propertySerializers.TryGetValue(propertyName, out var propSerializer))
                return propSerializer(obj);

            if (obj is string str && propertyName != null && trimLengths.TryGetValue(propertyName, out var length))
            {
                return str.Length <= length ? str : str[..length];
            }

            if (typeSerializers.TryGetValue(type, out var typeSerializer))
                return typeSerializer(obj);

            if (typeCultures.TryGetValue(type, out var culture) && obj is IFormattable formattable)
                return formattable.ToString(null, culture);

            if (obj is IFormattable f)
                return f.ToString(null, CultureInfo.InvariantCulture);

            return obj.ToString()!;
        }

        private bool IsFinalType(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) ||
                   type == typeof(DateTime) || type == typeof(TimeSpan) || type.IsEnum;
        }

        private Type GetMemberType(MemberInfo member)
        {
            return member switch
            {
                PropertyInfo p => p.PropertyType, FieldInfo f => f.FieldType, _ => throw new NotSupportedException()
            };
        }

        private string GetPropertyName(Expression<Func<TOwner, object>> selector)
        {
            var body = selector.Body;

            if (body is UnaryExpression unary)
                body = unary.Operand;

            if (body is MemberExpression member)
                return member.Member.Name;

            throw new ArgumentException("Expression must be a property access");
        }
    }
}


