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
        private readonly HashSet<Type> excludedTypes = new();
        private readonly Dictionary<Type, Func<object, string>> typeTransformers = new();
        private readonly Dictionary<MemberInfo, Func<object, string>> memberTransformers = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
        private bool isAllowCycleReference;

        public string PrintToString(TOwner obj) => PrintToString(obj, 0, new List<object>());

        private string PrintToString(object obj, int nestingLevel, List<object> usedObjects)
        {
            if (obj == null)
                return $"null{Environment.NewLine}";

            var type = obj.GetType();

            if (typeTransformers.TryGetValue(type, out var typeTransformer))
                return $"{typeTransformer(obj)}{Environment.NewLine}";

            if (FinalTypes.Contains(type))
                return $"{obj}{Environment.NewLine}";

            if (typeof(ICollection).IsAssignableFrom(type))
                return PrintCollection((ICollection)obj, nestingLevel, usedObjects);

            usedObjects.Add(obj);
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            var members = type
                .GetPropertiesAndFields()
                .Where(memberInfo => !excludedTypes.Contains(memberInfo.GetMemberType()))
                .Where(memberInfo => !excludedMembers.Contains(memberInfo));
            foreach (var memberInfo in members)
                sb.Append($"{identation}{PrintMember(memberInfo, obj, nestingLevel, usedObjects)}");
            return sb.ToString();
        }

        private string PrintCollection(ICollection collection, int nestingLevel, List<object> usedObjects)
        {
            if (collection.Count == 0) return $"[]{Environment.NewLine}";
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
                    return !isAllowCycleReference
                        ? throw new InvalidOperationException("Unexpected cycle reference")
                        : $"{{...}}{Environment.NewLine}";
                usedObjects.Add(memberValue);
            }

            return memberTransformers.TryGetValue(memberInfo, out var transformer)
                ? $"{transformer(memberValue)}{Environment.NewLine}"
                : PrintToString(memberValue, nestingLevel + 1, usedObjects);
        }

        public PrintingConfig<TOwner> Exclude<TType>()
        {
            excludedTypes.Add(typeof(TType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            if (memberSelector == null) throw new ArgumentNullException(nameof(memberSelector));
            excludedMembers.Add(SelectMember(memberSelector));
            return this;
        }

        private PrintingConfig<TOwner> UseTransform<TType>(Func<TType, string> transformer)
        {
            if (transformer == null) throw new ArgumentNullException(nameof(transformer));
            typeTransformers[typeof(TType)] = obj => transformer((TType)obj);
            return this;
        }

        private PrintingConfig<TOwner> UseTransform<TType>(MemberInfo memberInfo, Func<TType, string> transformer)
        {
            if (transformer == null) throw new ArgumentNullException(nameof(transformer));
            memberTransformers[memberInfo] = obj => transformer((TType)obj);
            return this;
        }

        public NestingPrintingConfig<TType> When<TType>() => new(this);

        public NestingPropertyPrintingConfig<TType> When<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            if (memberSelector == null) throw new ArgumentNullException(nameof(memberSelector));
            return new NestingPropertyPrintingConfig<TType>(this, SelectMember(memberSelector));
        }

        public PrintingConfig<TOwner> SetAllowCycleReference(bool allow)
        {
            isAllowCycleReference = allow;
            return this;
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

            public PrintingConfig<TOwner> Use(Func<TType, string> transformer)
            {
                if (transformer == null) throw new ArgumentNullException(nameof(transformer));
                return parent.UseTransform(memberInfo, transformer);
            }
        }

        public class NestingPrintingConfig<TType> : INestingPrintingConfig<TOwner, TType>
        {
            private readonly PrintingConfig<TOwner> parent;

            public NestingPrintingConfig(PrintingConfig<TOwner> parent)
            {
                this.parent = parent;
            }

            public PrintingConfig<TOwner> Use(Func<TType, string> transformer)
            {
                if (transformer == null) throw new ArgumentNullException(nameof(transformer));
                return parent.UseTransform(transformer);
            }
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