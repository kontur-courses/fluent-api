using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
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

        private readonly HashSet<Type> excludedTypes = new();
        private readonly Dictionary<Type, Func<object?, string>> typeSerializers = new();
        private readonly Dictionary<Type, CultureInfo> typeCultures = new();
        private readonly Dictionary<MemberInfo, Func<object?, string>> memberSerializers = new();

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Using<T>(Func<T, string> serializer)
        {
            typeSerializers[typeof(T)] = value => serializer((T)value);
            return this;
        }

        public PrintingConfig<TOwner> Using<TProperty>(
            Expression<Func<TOwner, TProperty>> memberExpression,
            Func<TProperty, string> serializer)
        {
            if (memberExpression.Body is not MemberExpression memberExpressionBody)
                throw new ArgumentException(
                    "Ожидалось выражение, обращающееся к свойству или полю",
                    nameof(memberExpression));

            var member = memberExpressionBody.Member;

            memberSerializers[member] = value => serializer((TProperty)value);
            return this;
        }

        public PrintingConfig<TOwner> WithCulture<T>(CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(culture);

            typeCultures[typeof(T)] = culture;
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object? obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (typeSerializers.TryGetValue(obj.GetType(), out var serializer))
                return serializer(obj) + Environment.NewLine;

            if (typeCultures.TryGetValue(obj.GetType(), out var culture) && obj is IFormattable formattable)
                return formattable.ToString(null, culture) + Environment.NewLine;

            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var member in GetPublicMembers(type))
            {
                if (excludedTypes.Contains(GetMemberType(member)))
                    continue;

                var value = member switch
                {
                    PropertyInfo property => property.GetValue(obj),
                    FieldInfo field => field.GetValue(obj),
                    _ => null
                };

                sb.Append(indentation)
                    .Append(member.Name)
                    .Append(" = ");
                if (memberSerializers.TryGetValue(member, out var memberSerializer))
                {
                    sb.Append(memberSerializer(value))
                        .Append(Environment.NewLine);
                }
                else
                {
                    sb.Append(PrintToString(value, nestingLevel + 1));
                }
            }
            return sb.ToString();
        }

        private static IEnumerable<MemberInfo> GetPublicMembers(Type type)
        {
            var properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.GetIndexParameters().Length == 0);
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
