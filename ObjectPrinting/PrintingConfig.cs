using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public record PrintingConfig<TOwner>
    {
        private readonly Type[] FinalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly HashSet<object> visited = new();

        private ImmutableHashSet<Type> excludedTypes = ImmutableHashSet<Type>.Empty;
        private ImmutableHashSet<MemberInfo> excludedMembers = ImmutableHashSet<MemberInfo>.Empty;
        private ImmutableDictionary<Type, Delegate> customTypeSerializers = ImmutableDictionary<Type, Delegate>.Empty;
        private ImmutableDictionary<MemberInfo, Delegate> customMemberSerializers = ImmutableDictionary<MemberInfo, Delegate>.Empty;
        private bool throwIfCycleReference = false;
        
        public CultureInfo CurrentCulture { get; init; } = CultureInfo.CurrentCulture;
        private readonly FinalTypesPrinterHelper finalTypesHelper = new();
        private readonly CollectionPrinterHelper collectionHelper = new();
        private readonly DictionaryPrinterHelper dictionaryHelper = new();

        public PrintingConfig<TOwner> Excluding<TMember>()
        {
            return this with {excludedTypes = excludedTypes.Add(typeof(TMember))};
        }

        public PrintingConfig<TOwner> Excluding<TMember>(Expression<Func<TOwner, TMember>> memberSelector)
        {
            var member = GetMemberInfoFromSelector(memberSelector);
            return this with {excludedMembers = excludedMembers.Add(member)};
        }

        public PrintingConfig<TOwner> ThrowingIfCycleReference(bool isConfirmed)
        {
            return this with {throwIfCycleReference = isConfirmed};
        }

        public PrintingConfig<TOwner> UsingCulture(CultureInfo culture)
        {
            return this with {CurrentCulture = culture};
        }

        public INestedPrintingConfig<TOwner, TMember> Printing<TMember>()
        {
            return new TypePrintingConfig<TOwner, TMember>(this);
        }

        public INestedPrintingConfig<TOwner, TMember> Printing<TMember>(Expression<Func<TOwner, TMember>> memberSelector)
        {
            return new MemberPrintingConfig<TOwner, TMember>(this, GetMemberInfoFromSelector(memberSelector));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);

        }

        internal PrintingConfig<TOwner> AddCustomTypeSerializer<TMember>(Type type, Func<TMember, string> serializer)
        {
            return this with {customTypeSerializers = customTypeSerializers.SetItem(type, serializer)};
        }

        internal PrintingConfig<TOwner> AddCustomMemberSerializer<TMember>(MemberInfo memberInfo, Func<TMember, string> serializer)
        {
            return this with {customMemberSerializers = customMemberSerializers.SetItem(memberInfo, serializer)};
        }


        internal string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (FinalTypes.Contains(obj.GetType()))
                return finalTypesHelper.Print(this, obj);
            if (obj is IDictionary dictionary)
                return dictionaryHelper.Print(this, dictionary, nestingLevel);
            if (obj is IEnumerable collection)
                return collectionHelper.Print(this, collection, nestingLevel);
            if (visited.Contains(obj))
                return TryPrintCycleMember(obj);
            visited.Add(obj);
            var sb = new StringBuilder();
            var type = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            sb.AppendLine(type.Name);
            foreach (var memberInfo in type.GetPublicPropertiesAndFields().Where(m => !IsExcluded(m)))
            {
                var isCustomSerialization = TryUseCustomSerialization(memberInfo, obj, out var customSerialization);
                var memberValue = memberInfo.GetMemberValue(obj);
                sb.Append(identation + memberInfo.Name + " = " +
                          (!isCustomSerialization
                              ? PrintToString(memberValue,
                                  nestingLevel + 1)
                              : customSerialization));
            }
            visited.Clear();
            return sb.ToString();
        }

        private string TryPrintCycleMember(object obj)
        {
            if (throwIfCycleReference) throw new Exception($"Member with type {obj.GetType()} has cycle reference");
            var objType = obj.GetType();
            return $"cycle link detected on object with hashcode: {obj.GetHashCode()}{Environment.NewLine}";
        }

        private bool TryUseCustomSerialization(MemberInfo member, object obj, out string customSerialization)
        {
            var memberType = member.GetMemberType();
            if (customTypeSerializers.ContainsKey(memberType))
            {
                customSerialization = customTypeSerializers[memberType]
                    .DynamicInvoke(member.GetMemberValue(obj))
                    + Environment.NewLine;
                return true;
            }

            if (customMemberSerializers.ContainsKey(member))
            {
                customSerialization = customMemberSerializers[member]
                    .DynamicInvoke(member.GetMemberValue(obj))
                    + Environment.NewLine;
                return true;
            }
            customSerialization = null;
            return false;
        }

        private bool IsExcluded(MemberInfo member)
        {
            return excludedMembers.Contains(member) || excludedTypes.Contains(member.GetMemberType());
        }

        private static MemberInfo GetMemberInfoFromSelector<TMember>(Expression<Func<TOwner, TMember>> memberSelector)
        {
            var member = memberSelector.Body as MemberExpression;
            return member?.Member;
        }
    }
}