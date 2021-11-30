using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.PrintingConfig
{
    public class PrintingConfig<TOwner> : BasePrintingConfig
    {
        private readonly SerializationSettings settings = new();

        public string PrintToString(TOwner obj) => PrintToString(obj, 0, new List<object>());

        private string PrintToString(object obj, int nestingLevel, List<object> usedObjects)
        {
            if (obj == null)
                return $"null{NewLine}";

            var type = obj.GetType();

            if (settings.TryGetTypeTransformer(type, out var typeTransformer))
                return $"{typeTransformer(obj)}{NewLine}";

            if (FinalTypes.Contains(type))
                return $"{obj}{NewLine}";

            if (typeof(ICollection).IsAssignableFrom(type))
                return PrintCollection((ICollection)obj, nestingLevel, usedObjects);

            usedObjects.Add(obj);
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            var members = type
                .GetPropertiesAndFields()
                .Where(memberInfo => !settings.IsExcluded(memberInfo.GetMemberType()))
                .Where(memberInfo => !settings.IsExcluded(memberInfo));
            foreach (var memberInfo in members)
                sb.Append($"{identation}{PrintMember(memberInfo, obj, nestingLevel, usedObjects)}");
            return sb.ToString();
        }

        private string PrintCollection(ICollection collection, int nestingLevel, List<object> usedObjects)
        {
            if (collection.Count == 0) return $"[]{NewLine}";
            var identation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"{identation}[");
            foreach (var el in collection)
                sb.Append(identation + "\t" + PrintToString(el, nestingLevel + 1, usedObjects));
            sb.AppendLine($"{identation}]");

            return sb.ToString();
        }

        private string PrintMember(MemberInfo memberInfo, object obj, int nestingLevel, List<object> usedObjects) =>
            $"{memberInfo.Name} = {ToString(memberInfo, obj, nestingLevel, usedObjects)}";

        private string ToString(MemberInfo memberInfo, object obj, int nestingLevel, List<object> usedObjects)
        {
            var memberValue = memberInfo.GetValue(obj);
            if (!memberInfo.GetMemberType().IsValueType && memberValue is not null)
            {
                if (usedObjects.Exists(o => ReferenceEquals(o, memberValue)))
                    return !settings.IsAllowCycleReference
                        ? throw new InvalidOperationException("Unexpected cycle reference")
                        : $"{{...}}{NewLine}";
                usedObjects.Add(memberValue);
            }

            return settings.TryGetMemberTransformer(memberInfo, out var transformer)
                ? $"{transformer(memberValue)}{NewLine}"
                : PrintToString(memberValue, nestingLevel + 1, usedObjects);
        }

        public PrintingConfig<TOwner> Exclude<TType>()
        {
            settings.Exclude(typeof(TType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            if (memberSelector == null) throw new ArgumentNullException(nameof(memberSelector));
            settings.Exclude(SelectMember(memberSelector));
            return this;
        }

        public NestingPrintingConfig<TOwner, TType> When<TType>() => new(this, settings);

        public NestingPropertyPrintingConfig<TOwner, TType> When<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            if (memberSelector == null) throw new ArgumentNullException(nameof(memberSelector));
            return new NestingPropertyPrintingConfig<TOwner, TType>(this, settings, SelectMember(memberSelector));
        }

        public PrintingConfig<TOwner> SetAllowCycleReference(bool allow)
        {
            settings.IsAllowCycleReference = allow;
            return this;
        }

        private static MemberInfo SelectMember<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException("Cannot resolve member expression");
            var memberInfo = memberExpression.Member;
            if (memberInfo is null)
                throw new ArgumentException("Cannot resolve member type");
            if (memberInfo.MemberType is not MemberTypes.Field and not MemberTypes.Property)
                throw new ArgumentException($"Expected Field or Property, but actual {memberInfo.MemberType}");
            return memberInfo;
        }
    }
}