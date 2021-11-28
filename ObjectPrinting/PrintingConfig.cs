using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public interface INestingPrintingConfig<TOwner, out TType>
    {
        PrintingConfig<TOwner> Use(Func<TType, string> transformer);
    }
    public class PrintingConfig<TOwner> : BasePrintingConfig
    {
        private readonly HashSet<Type> excludedTypes = new();
        private readonly Dictionary<Type, Func<object, string>> typeTransformers = new();
        private readonly Dictionary<MemberInfo, Func<object, string>> memberTransformers = new();
        
        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return $"null{Environment.NewLine}";
            var type = obj.GetType();

            if (typeTransformers.ContainsKey(type))
                return $"{typeTransformers[type](obj)}{Environment.NewLine}";

            if (FinalTypes.Contains(type))
                return $"{obj}{Environment.NewLine}";

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            var members = GetPropertiesAndFields(type)
                .Where(x => !excludedTypes.Contains(GetMemberType(x)));
            foreach (var memberInfo in members)
            {
                sb.Append($"{identation}{PrintMember(memberInfo, obj, nestingLevel)}");
            }
            return sb.ToString();
        }

        private static IEnumerable<MemberInfo> GetPropertiesAndFields(IReflect type) =>
            type
                .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.MemberType is MemberTypes.Property or MemberTypes.Field);

        private string PrintMember(MemberInfo memberInfo, object obj, int nestingLevel)
        {
            var memberValue = GetValue(memberInfo, obj);
            var value = memberTransformers.TryGetValue(memberInfo, out var transformer)
                ? transformer(memberValue) + Environment.NewLine
                : PrintToString(memberValue, nestingLevel + 1);
            return $"{memberInfo.Name} = {value}";
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

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        private PrintingConfig<TOwner> UseTransform<TType>(Func<TType, string> transformer)
        {
            typeTransformers[typeof(TType)] = obj => transformer((TType)obj);
            return this;
        }

        private PrintingConfig<TOwner> UseTransform<TType>(MemberInfo memberInfo, Func<TType, string> transformer)
        {
            memberTransformers[memberInfo] = obj => transformer((TType)obj);
            return this;
        }

        public NestingPrintingConfig<TType> When<TType>() => new(this);

        public NestingPropertyPrintingConfig<TType> When<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException("Cannot resolve member expression");
            var memberInfo = memberExpression.Member;
            if (memberInfo is null)
                throw new ArgumentException("Cannot resolve member type");
            if (memberInfo.MemberType is not MemberTypes.Field and not MemberTypes.Property)
                throw new ArgumentException($"Expected Field or Property, but actual {memberInfo.MemberType}");
            return new NestingPropertyPrintingConfig<TType>(this, memberInfo);
        }

        public class NestingPropertyPrintingConfig<TType> : INestingPrintingConfig<TOwner, TType>
        {
            private readonly PrintingConfig<TOwner> parent;
            private readonly MemberInfo memberInfo;

            public NestingPropertyPrintingConfig(PrintingConfig<TOwner> parent, MemberInfo memberInfo)
            {
                this.parent = parent;
                this.memberInfo = memberInfo;
            }

            public PrintingConfig<TOwner> Use(Func<TType, string> transformer) =>
                parent.UseTransform(memberInfo, transformer);
        }

        public class NestingPrintingConfig<TType> : INestingPrintingConfig<TOwner, TType>
        {
            private readonly PrintingConfig<TOwner> parent;

            public NestingPrintingConfig(PrintingConfig<TOwner> parent)
            {
                this.parent = parent;
            }

            public PrintingConfig<TOwner> Use(Func<TType, string> transformer) => parent.UseTransform(transformer);
        }
    }
}