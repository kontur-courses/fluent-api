using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly SerializingOptions options = new SerializingOptions();
        private List<object> used = new List<object>();

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return $"null{Environment.NewLine}";

            var type = obj.GetType();

            if (options.TryGetRule(type, out var rule))
                return $"{rule(obj)}{Environment.NewLine}";

            if (finalTypes.Contains(type) || type.IsPrimitive)
                return $"{obj}{Environment.NewLine}";

            if (typeof(ICollection).IsAssignableFrom(type))
                return PrintCollection((ICollection)obj, nestingLevel);

            used.Add(obj);
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            
            var members = GetPropertiesAndFields(type)
                .Where(memberInfo => !options.IsExcluded(GetMemberType(memberInfo)))
                .Where(memberInfo => !options.IsExcluded(memberInfo));
            
            foreach (var memberInfo in members)
            {
                sb.Append($"{indentation}{PrintMember(memberInfo, obj, nestingLevel)}");
            }

            return sb.ToString();
        }

        private string PrintCollection(ICollection collection, int nestingLevel)
        {
            if (collection.Count == 0)
            {
                return $"[]{Environment.NewLine}";
            }

            var indentation = new string('\t', nestingLevel);

            var sb = new StringBuilder();

            sb.AppendLine($"{indentation}[");

            foreach (var obj in collection)
            {
                sb.Append(indentation + "\t" + PrintToString(obj, nestingLevel + 1));
            }

            sb.AppendLine($"{indentation}]");

            return sb.ToString();
        }

        private string PrintMember(MemberInfo memberInfo, object obj, int nestingLevel)
        {
            return $"{memberInfo.Name} = {ToString(memberInfo, obj, nestingLevel)}";
        }

        private string ToString(MemberInfo memberInfo, object obj, int nestingLevel)
        {
            var memberValue = GetValue(memberInfo, obj);
            if (!GetMemberType(memberInfo).IsValueType && memberValue is not null)
            {
                if (used.Exists(o => ReferenceEquals(o, memberValue)))
                {
                    return options.AllowCyclicReferences
                        ? $"{{...}}{Environment.NewLine}"
                        : throw new InvalidOperationException("Сyclic reference detected");
                }

                used.Add(memberValue);
            }

            return options.TryGetRule(memberInfo, out var rule)
                ? $"{rule(memberValue)}{Environment.NewLine}"
                : PrintToString(memberValue, nestingLevel + 1);
        }

        private static IEnumerable<MemberInfo> GetPropertiesAndFields(Type type)
        {
            return type
                .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
        }

        private static object GetValue(MemberInfo memberInfo, object obj)
        {
            return memberInfo switch
            {
                PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
                FieldInfo fieldInfo => fieldInfo.GetValue(obj),
                _ => throw new Exception($"Cannot resolve member info type {memberInfo.GetType()}")
            };
        }

        private static Type GetMemberType(MemberInfo memberInfo)
        {
            return memberInfo switch
            {
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                FieldInfo fieldInfo => fieldInfo.FieldType,
                _ => throw new Exception($"Cannot resolve member info type {memberInfo.GetType()}")
            };
        }

        public PrintingConfig<TOwner> Excluding<TType>()
        {
            options.Exclude(typeof(TType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var memberInfo = GetMemberInfo(expression);

            options.Exclude(memberInfo);

            return this;
        }


        public PropertyPrintingConfig<TOwner, TType> Serialize<TType>()
        {
            return new PropertyPrintingConfig<TOwner, TType>(this, options);
        }

        public PropertyPrintingConfig<TOwner, TType> Serialize<TType>(Expression<Func<TOwner, TType>> expression)
        {
            if (expression is null)
            {
                throw new ArgumentException(nameof(expression));
            }

            var memberInfo = GetMemberInfo(expression);
            return new PropertyPrintingConfig<TOwner, TType>(this, options, memberInfo);
        }

        public PrintingConfig<TOwner> IgnoringCyclicReferences()
        {
            options.AllowCyclicReferences = true;
            return this;
        }

        private MemberInfo GetMemberInfo<TType>(Expression<Func<TOwner, TType>> expression)
        {
            if (expression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException(nameof(expression));
            }

            var memberInfo = memberExpression.Member;

            if (memberInfo.MemberType != MemberTypes.Property && memberInfo.MemberType != MemberTypes.Field)
            {
                throw new ArgumentException("MemberType is not property or field");
            }

            return memberInfo;
        }
    }
}