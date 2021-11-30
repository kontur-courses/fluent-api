using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Configs
{
    public class PrintingConfig<TOwner>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly HashSet<Type> FinalTypes = new()
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly Dictionary<MemberInfo, Func<object, string>> customMembersSerializers = new();
        private readonly Dictionary<Type, Func<object, string>> customTypesSerializers = new();

        private readonly HashSet<MemberInfo> excludedMembers = new();
        private readonly HashSet<Type> excludedTypes = new();

        private bool isCycleReferencesAllowed;

        private List<object> visited = new();

        public string PrintToString(TOwner obj)
        {
            visited = new List<object>();
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> Exclude<TType>()
        {
            excludedTypes.Add(typeof(TType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TMember>(Expression<Func<TOwner, TMember>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            excludedMembers.Add(SelectMember(selector));
            return this;
        }

        public MemberConfig<TOwner, TMember> Use<TMember>(Expression<Func<TOwner, TMember>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            return new MemberConfig<TOwner, TMember>(this, SelectMember(selector));
        }

        public TypeConfig<TOwner, TMember> Use<TMember>() => new(this);

        public PrintingConfig<TOwner> UseCycleReference(bool cycleReferencesAllowed = false)
        {
            isCycleReferencesAllowed = cycleReferencesAllowed;
            return this;
        }

        public void AddTypeSerializer<TMember>(Type type, Func<TMember, string> serializer)
        {
            customTypesSerializers[type] = o => serializer((TMember) o);
        }

        public void AddMemberSerializer<TMember>(MemberInfo info, Func<TMember, string> serializer)
        {
            customMembersSerializers[info] = o => serializer((TMember) o);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (obj is ICollection collection) 
                return PrintCollection(collection, nestingLevel);

            visited.Add(obj);
            var indentation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            var printedMembers = GetFieldsAndProperties(type)
                .Where(x => !IsExcluded(x))
                .Select(x => $"{indentation}{PrintMember(x, obj, nestingLevel + 1)}");

            return sb.AppendCollection(printedMembers).ToString();
        }

        private string PrintCollection(ICollection collection, int nestingLevel)
        {
            if (collection.Count == 0)
                return $"[]{Environment.NewLine}";
            var indentation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            sb.AppendLine($"{indentation}[");
            foreach (var el in collection) sb.Append(indentation + "\t" + PrintToString(el, nestingLevel + 1));
            sb.AppendLine($"{indentation}]");

            return sb.ToString();
        }

        private string PrintMember(MemberInfo memberInfo, object obj, int nestingLevel)
            => $"{memberInfo.Name} = {Print(memberInfo, obj, nestingLevel)}";

        private string Print(MemberInfo memberInfo, object obj, int nestingLevel)
        {
            var memberValue = memberInfo.GetValue(obj);

            if (!memberInfo.GetMemberType().IsValueType && memberValue is not null)
            {
                if (visited.Exists(o => ReferenceEquals(o, memberValue)))
                    return isCycleReferencesAllowed
                        ? $"![Cyclic reference]!{Environment.NewLine}"
                        : throw new Exception("Unexpected cycle reference");

                visited.Add(memberValue);
            }

            if (customMembersSerializers.TryGetValue(memberInfo, out var serializer)
                || customTypesSerializers.TryGetValue(memberInfo.GetMemberType(), out serializer))
                return $"{serializer(memberValue)}{Environment.NewLine}";

            return PrintToString(memberValue, nestingLevel + 1);
        }

        private bool IsExcluded(MemberInfo memberInfo)
            => excludedMembers.Contains(memberInfo) || excludedTypes.Contains(memberInfo.GetMemberType());

        private IEnumerable<MemberInfo> GetFieldsAndProperties(Type type) =>
            type.GetFields()
                .Cast<MemberInfo>()
                .Concat(type.GetProperties());

        private MemberInfo SelectMember<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException("Cannot resolve member expression");
            var memberInfo = memberExpression.Member;
            if (memberInfo is null)
                throw new ArgumentException("Cannot resolve member type");
            if (memberInfo.MemberType is not MemberTypes.Field and not MemberTypes.Property)
                throw new ArgumentException($"Expected Field or Property, but was {memberInfo.MemberType}");
            return memberInfo;
        }
    }
}