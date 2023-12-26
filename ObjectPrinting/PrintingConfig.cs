using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;
using ObjectPrinting.InnerPrintingConfigs;
using ObjectPrinting.PropertyOrField;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
        protected readonly Dictionary<Type, Func<object, string>> TypeSerializers = new();
        protected readonly Dictionary<MemberInfo, Func<object, string>> MemberSerializers = new();
        private int MaxNestingLevel { get; }

        public PrintingConfig(int maxNestingLevel = 10)
        {
            MaxNestingLevel = maxNestingLevel;
        }

        protected PrintingConfig(PrintingConfig<TOwner> parent)
        {
            excludedTypes = new HashSet<Type>(parent.excludedTypes);
            excludedMembers = new HashSet<MemberInfo>(parent.excludedMembers);
            TypeSerializers = new Dictionary<Type, Func<object, string>>(parent.TypeSerializers);
            MemberSerializers = new Dictionary<MemberInfo, Func<object, string>>(parent.MemberSerializers);
            MaxNestingLevel = parent.MaxNestingLevel;
        }

        public IInnerPrintingConfig<TOwner, TType> Printing<TType>()
        {
            var config = new PrintingConfig<TOwner>(this);
            return new TypePrintingConfig<TOwner, TType>(config);
        }

        public IInnerPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException("memberSelector should me MemberExpression");
            var memberInfo = memberExpression.Member;
            if (memberInfo.DeclaringType != typeof(TOwner))
                throw new ArgumentException($"You should access {typeof(TOwner)} property or field");
            
            var config = new PrintingConfig<TOwner>(this);
            return new MemberPrintingConfig<TOwner, TMemberType>(config, memberInfo);
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException("memberSelector should me MemberExpression");
            var memberInfo = memberExpression.Member;
            if (memberInfo.DeclaringType != typeof(TOwner))
                throw new ArgumentException($"You should access {typeof(TOwner)} property or field");
            var config = new PrintingConfig<TOwner>(this);
            config.excludedMembers.Add(memberInfo);
            return config;
        }

        public PrintingConfig<TOwner> Excluding<TType>()
        {
            var config = new PrintingConfig<TOwner>(this);
            config.excludedTypes.Add(typeof(TType));
            return config;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (TypeSerializers.TryGetValue(type, out var serializer))
                return serializer(obj) + Environment.NewLine;
            if (finalTypes.Contains(type) || nestingLevel == MaxNestingLevel)
                return obj + Environment.NewLine;
            if (obj is ICollection collection)
                return Environment.NewLine + SerializeCollection(collection, nestingLevel);
            
            return SerializeByFieldsAndProperties(obj, nestingLevel);
        }

        private string SerializeByFieldsAndProperties(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder(type.Name + Environment.NewLine);
            
            foreach (var member in GetNonExcludedPropertyOrFields(type))
                sb.Append(identation + member.Name + " = " + GetSerializedMemberOrDefault(obj, member, nestingLevel + 1));
            
            return sb.ToString();
        }

        private string GetSerializedMemberOrDefault(object obj, IPropertyOrField member, int nestingLevel)
        {
            var isMemberSerialized = MemberSerializers.ContainsKey(member.UnderlyingMember);
            var memberValue = member.GetValue(obj);
            
            return isMemberSerialized
                ? MemberSerializers[member.UnderlyingMember](memberValue) + Environment.NewLine
                : PrintToString(memberValue, nestingLevel);
        }

        private IEnumerable<IPropertyOrField> GetNonExcludedPropertyOrFields(Type type)
        {
            return type
                .GetFieldsAndProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => !excludedMembers.Contains(x.UnderlyingMember)
                            && !excludedTypes.Contains(x.DataType));
        }

        private string SerializeCollection(ICollection collection, int nestingLevel)
        {
            var serialization = new StringBuilder();
            var index = 0;

            if (collection is IDictionary dictionary)
                foreach (var key in dictionary.Keys)
                    serialization.Append(PrintCollectionItem(key, dictionary[key], nestingLevel));
            else foreach (var item in collection)
                serialization.Append(PrintCollectionItem(index++, item , nestingLevel));

            return serialization.ToString();
        }

        private string PrintCollectionItem(object key, object value, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel);
            var isFinalOrCollection = finalTypes.Contains(value.GetType()) || value is ICollection;
            
            var serialization = new StringBuilder();
            serialization.Append($"{identation}[{PrintToString(key, nestingLevel).TrimEnd()}]:{(isFinalOrCollection ? "" : Environment.NewLine)}");
            serialization.Append($"{(isFinalOrCollection ? "  " : identation + '\t')}{PrintToString(value, nestingLevel + 1)}");

            return serialization.ToString();
        }
    }
}