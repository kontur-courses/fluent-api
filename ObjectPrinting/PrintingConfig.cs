using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {

        private HashSet<Type> finalTypes = new()
        {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(bool),
                typeof(long), typeof(decimal), typeof(byte),
                typeof(sbyte), typeof(short)
        };

        internal HashSet<MemberInfo> _excludingMember = new();
        internal HashSet<Type> _excludingType = new();

        internal Dictionary<MemberInfo, Func<object, string>> _formaterMember = new();
        internal Dictionary<Type, Func<object, string>> _formaterType = new();


        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetMember(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            _excludingMember.Add(GetMember(memberSelector));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _excludingType.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
            => PrintToString(obj, 0, 
                new HashSet<object>(ReferenceEqualityComparer.Instance));

        private string PrintToString(object obj, int nestingLevel, HashSet<object> path)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if(_formaterType.TryGetValue(type, out var typeFormater))
                return typeFormater(obj) + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (!path.Add(obj))
                return $"{type.Name} (cycle){Environment.NewLine}";

            var result = obj is IEnumerable collectino
                ? SerializeCollection(collectino, nestingLevel, path)
                : SerializeProperty(obj, nestingLevel, path);
            path.Remove(obj);
            return result;
            
        }

        private string SerializeCollection(IEnumerable collection, int nestingLevel, HashSet<object> path)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(collection.GetType().Name);

            if (collection is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    var key = PrintToString(entry.Key, nestingLevel + 1, path).TrimEnd();
                    sb.Append(identation + "[" + key + "] = " +
                              PrintToString(entry.Value, nestingLevel + 1, path));
                }
            }
            else
            {
                var index = 0;
                foreach (var item in collection)
                    sb.Append(identation + "[" + index++ + "] = " +
                              PrintToString(item, nestingLevel + 1, path));
            }

            return sb.ToString();
        }

        private string SerializeProperty(object obj, int nestingLevel, HashSet<object> path)
        {
            var type = obj.GetType();
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);

            foreach (var member in GetMembers(type))
            {
                if (IsMemberExcluded(member))
                    continue;

                var value = GetValue(member, obj);
                var printed = value != null && _formaterMember.TryGetValue(member, out var propertyFormater)
                    ? propertyFormater(value) + Environment.NewLine
                    : PrintToString(value, nestingLevel + 1, path);

                sb.Append(identation + member.Name + " = " + printed);

            }
            return sb.ToString();
        }


        private MemberInfo GetMember<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
            => ((MemberExpression)memberSelector.Body).Member;

        private static IEnumerable<MemberInfo> GetMembers(Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            var properties = type.GetProperties(flags)
                .Where(p => p.GetIndexParameters().Length == 0);

            return properties.Cast<MemberInfo>().Concat(type.GetFields(flags));
        }

        private static Type GetMemberType(MemberInfo member)
            => member is PropertyInfo property 
            ? property.PropertyType 
            : ((FieldInfo)member).FieldType;

        private static object? GetValue(MemberInfo member, object obj)
            => member is PropertyInfo property 
            ? property.GetValue(obj) 
            : ((FieldInfo)member).GetValue(obj);

        private bool IsMemberExcluded(MemberInfo member)
        {
            return _excludingMember.Contains(member)
                || _excludingType.Contains(GetMemberType(member));
        }

    }
}