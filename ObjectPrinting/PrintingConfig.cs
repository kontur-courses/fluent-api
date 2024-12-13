using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> FinalTypes = [
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        ];
        private readonly HashSet<Type> excludedTypes = [];
        private readonly HashSet<MemberInfo> excludedMembers = [];
        private readonly Dictionary<MemberInfo, Func<object, string>> customMemberSerializers = [];
        private readonly Dictionary<Type, Func<object, string>> customTypeSerializers = [];
        private int maxNestingDepth = 50;
        
        public PrintingConfig<TOwner> SetDepth(int depth)
        {
            maxNestingDepth = depth;
            return this;
        }

        // После долгих размышлений, как расширить FinalTypes, я только придумала дать возможность пользователю
        // расширить нужные ему типы, которые посложнее примитивов
        public PrintingConfig<TOwner> AddFinalTypes(IEnumerable<Type> types)
        {
            FinalTypes.UnionWith(types);
            return this;
        }

        public PrintingConfig<TOwner> AddFinalType<TFinalType>()
        {
            FinalTypes.Add(typeof(TFinalType));
            return this;
        }

        public IPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(
                this, 
                customTypeSerializers);
        }

        public IPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberInfo)
                throw new ArgumentException($"Expression '{memberSelector}' refers to a method, not a property/field.");
            return new MemberPrintingConfig<TOwner, TPropType>(
                this,
                customMemberSerializers,
                memberInfo.Member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberInfo)
                throw new ArgumentException($"Expression '{memberSelector}' refers to a method, not a property/field.");
            excludedMembers.Add(memberInfo.Member);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string? PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, []);
        }

        private string? PrintToString(object? obj, int nestingLevel, HashSet<object> visitedObjects)
        {
            if (maxNestingDepth < nestingLevel)
                return $"The maximum recursion depth {maxNestingDepth} has been reached";
            
            if (obj == null)
                return "null";

            if (visitedObjects.Contains(obj))
                return "Cycling reference!";
 
            var objType = obj.GetType();
            if (objType.IsPrimitive || FinalTypes.Contains(objType))
            {
                if (customTypeSerializers.TryGetValue(objType, out var serializer))
                    return serializer(obj);
                
                return obj is IFormattable formattable 
                    ? formattable.ToString(null, CultureInfo.InvariantCulture)
                    : obj.ToString();
            }
            
            return obj switch
            {
                IDictionary dictionary => PrintToStringIDictionary(dictionary, nestingLevel, visitedObjects),
                IEnumerable enumerable => PrintToStringIEnumerable(enumerable, nestingLevel, visitedObjects),
                _ => PrintToStringObject(obj, nestingLevel, visitedObjects)
            };
        }
        
        private bool TryUseCustomSerializer(object value, MemberInfo memberInfo, out string? str)
        {
            str = null;
            
            if (customMemberSerializers.TryGetValue(memberInfo, out var lambda) || 
                customTypeSerializers.TryGetValue(memberInfo.GetMemberType(), out lambda))
            {
                str = lambda(value);
                return true;
            }
            
            return false;
        }

        private IEnumerable<MemberInfo> GetIncludedMembers(Type type)
        {
            var members = type.GetMembers()
                .Where(m => m is PropertyInfo or FieldInfo)
                .Where(m => !IsExcluded(m));
            return members;
        }

        private bool IsExcluded(MemberInfo memberInfo)
        {
            return excludedMembers.Contains(memberInfo) || excludedTypes.Contains(memberInfo.GetMemberType());
        }

        private string PrintToStringObject(object obj, int nestingLevel, HashSet<object> visitedObjects)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();
            var type = obj.GetType();
            builder.Append(type.Name);
            visitedObjects.Add(obj);
            
            foreach (var memberInfo in GetIncludedMembers(type))
            {
                var value = memberInfo.GetValue(obj);
                var serializedStr = TryUseCustomSerializer(value, memberInfo, out var str)
                    ? str
                    : PrintToString(value, nestingLevel + 1, visitedObjects);
                builder.Append('\n')
                    .Append(indentation)
                    .Append(memberInfo.Name)
                    .Append(" = ")
                    .Append(serializedStr);
            }
            
            visitedObjects.Remove(obj);
            return builder.ToString();
        }

        private string PrintToStringIDictionary(IDictionary dictionary, int nestingLevel, HashSet<object> visitedObjects)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();
            builder.Append(dictionary.GetType().Name);
            visitedObjects.Add(dictionary);

            foreach (DictionaryEntry item in dictionary)
                builder.Append('\n')
                    .Append(indentation)
                    .Append(PrintToString(item.Key, nestingLevel + 1, visitedObjects))
                    .Append(" = ")
                    .Append(PrintToString(item.Value, nestingLevel + 1, visitedObjects));

            visitedObjects.Remove(dictionary);
            return builder.ToString();
        }
        
        private string PrintToStringIEnumerable(IEnumerable collection, int nestingLevel, HashSet<object> visitedObjects)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();
            var index = 0;
            builder.Append(collection.GetType().Name);
            visitedObjects.Add(collection);

            foreach (var item in collection)
                builder.Append('\n')
                    .Append(indentation)
                    .Append(index++)
                    .Append(" = ")
                    .Append(PrintToString(item, nestingLevel + 1, visitedObjects));

            visitedObjects.Remove(collection);
            return builder.ToString();
        }
    }
}