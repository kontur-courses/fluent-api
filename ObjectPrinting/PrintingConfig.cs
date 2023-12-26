using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;
using ObjectPrinting.InnerPrintingConfig;
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
        internal readonly Dictionary<Type, Func<object, string>> TypeSerializers = new();
        internal readonly Dictionary<MemberInfo, Func<object, string>> MemberSerializers = new();
        private int MaxNestingLevel { get; }
        public PrintingConfig(int maxNestingLevel = 10)
        {
            MaxNestingLevel = maxNestingLevel;
        }

        public TypePrintingConfig<TOwner, TType> Printing<TType>()
        {
            return new TypePrintingConfig<TOwner, TType>(this);
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            var memberInfo = ((MemberExpression) memberSelector.Body).Member;
            return new MemberPrintingConfig<TOwner, TMemberType>(this, memberInfo);
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            var memberInfo = ((MemberExpression) memberSelector.Body).Member;
            excludedMembers.Add(memberInfo);
            return Excluding<TMemberType>();
        }

        public PrintingConfig<TOwner> Excluding<TType>()
        {
            excludedTypes.Add(typeof(TType));
            return this;
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

            if (TypeSerializers.TryGetValue(obj.GetType(), out var serializer))
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
            foreach (var propertyOrField in GetNonExcludedPropertyOrFields(type))
            {
                sb.Append(identation + propertyOrField.Name + " = " +
                          (MemberSerializers.TryGetValue(propertyOrField.UnderlyingMember, out var propertyOrFieldSerializer) 
                              ? propertyOrFieldSerializer(propertyOrField.GetValue(obj)) + Environment.NewLine
                              : PrintToString(propertyOrField.GetValue(obj), nestingLevel + 1)));
            }

            return sb.ToString();
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