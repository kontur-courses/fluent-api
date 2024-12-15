using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Helpers;
using ObjectPrinting.Serializers;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        internal readonly IMembersSerializer[] MembersSerializers =
        [
            new MembersSerializerByMember(),
            new MembersSerializerByType()
        ];
        
        private readonly HashSet<Type> primitiveTypes =
        [
            typeof(Guid),
            typeof(string),
            typeof(DateTime), 
            typeof(TimeSpan)
        ];
        private readonly HashSet<Type> excludingTypes = [];
        private readonly HashSet<MemberInfo> excludingMembers = [];

        public PrintingConfig<TOwner> Excluding<TPropertyType>()
        {
            excludingTypes.Add(typeof(TPropertyType));
            
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            excludingMembers.Add(GetMember(memberSelector));
            
            return this;
        }

        public TypePrintingConfig<TOwner, TPropertyType> For<TPropertyType>() => new(this);

        public MemberPrintingConfig<TOwner, TProperty> For<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector) => 
            new(this, GetMember(memberSelector));
        
        public string PrintToString(TOwner obj) => 
            PrintToString(obj, 0, []);
        
        private string PrintToString(object obj, int nestingLevel, Dictionary<object, int> parsedObjects)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            
            var type = obj.GetType();
            
            if (type.IsPrimitive || primitiveTypes.Contains(type))
                return obj + Environment.NewLine;
            
            if (parsedObjects.TryGetValue(obj, out var level))
                return $"cycled {type.Name} in level {level}" + Environment.NewLine;
            parsedObjects.Add(obj, nestingLevel);

            return obj switch
            {
                IDictionary dictionary => PrintDictionary(dictionary, nestingLevel, parsedObjects),
                IEnumerable collection => PrintCollection(collection, nestingLevel, parsedObjects),
                _ => PrintClassProperties(obj, nestingLevel, parsedObjects)
            };
        }

        private string PrintDictionary(IDictionary dictionary, int nestingLevel, Dictionary<object,int> parsedObjects)
        {
            var sb = new StringBuilder();
            var nextNestingLevel = nestingLevel + 1;
            var identation = new string('\t', nextNestingLevel);
            sb.AppendLine("Dictionary");

            foreach (DictionaryEntry kvp in dictionary)
            {
                var key = kvp.Key;
                var value = kvp.Value!;
                
                sb.Append(identation + PrintToString(key, nestingLevel, parsedObjects).Trim() + 
                          " : " +
                          PrintToString(value, nestingLevel, parsedObjects));
            }
            
            return sb.ToString();
        }

        private string PrintCollection(IEnumerable collection, int nestingLevel, Dictionary<object,int> parsedObjects)
        {
            var sb = new StringBuilder();
            var nextNestingLevel = nestingLevel + 1;
            var identation = new string('\t', nextNestingLevel);

            sb.AppendLine("Collection");
            foreach (var element in collection)
                sb.Append(identation + PrintToString(element, nextNestingLevel, parsedObjects));
            
            return sb.ToString();
        }
        
        private string PrintClassProperties(object obj, int nestingLevel, Dictionary<object, int> parsedObjects)
        {
            var type = obj.GetType();
            var sb = new StringBuilder();
            var nextNestingLevel = nestingLevel + 1;
            var identation = new string('\t', nextNestingLevel);
            sb.AppendLine(type.Name);
            
            foreach (var memberInfo in type.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!MemberHelper.TryGetTypeAndValueOfMember(obj, memberInfo, out var memberData))
                    continue;
                
                var (memberValue, memberType) = memberData!.Value;
                if (excludingMembers.Contains(memberInfo) || excludingTypes.Contains(memberType))
                    continue;
                
                sb.Append(identation + 
                          memberInfo.Name + 
                          " = " + 
                          PrintMember(memberValue, memberType, memberInfo, nextNestingLevel, parsedObjects));
            }
            
            return sb.ToString();
        }

        private static MemberInfo GetMember<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException("Expression refers to a method, not a member.");
            
            return memberExpression.Member;
        }

        private string PrintMember(
            object memberValue,
            Type memberType,
            MemberInfo memberInfo,
            int nextNestingLevel,
            Dictionary<object, int> parsedObjects)
        {
            string? result = null;

            foreach (var serializer in MembersSerializers)
                if (serializer.TrySerialize(memberValue, memberType, memberInfo, out result))
                    break;
            
            return result == null 
                ? PrintToString(memberValue, nextNestingLevel, parsedObjects) 
                : result + Environment.NewLine;
        }
    }
}