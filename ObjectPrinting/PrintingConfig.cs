using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new();
        private readonly Dictionary<Type, Func<object, string>> serializers = new();

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Using<T>(Func<T, string> serializer)
        {
            serializers[typeof(T)] = value => serializer((T)value);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            if (serializers.TryGetValue(obj.GetType(), out var serializer))
                return serializer(obj) + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
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

                sb.Append(identation)
                    .Append(member.Name)
                    .Append(" = ")
                    .Append(PrintToString(value, nestingLevel + 1));
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
                _ => throw new ArgumentException("Неизвестный тип члена")
            };
        }
    }
}