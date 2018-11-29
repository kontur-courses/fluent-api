using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly Stack<object> currentObjects = new Stack<object>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<(Type, string)> excludedProperties = new HashSet<(Type, string)>();
        private readonly Dictionary<Type, Func<object, string>> typeSerializationSettings =
            new Dictionary<Type, Func<object, string>>();
        private readonly Dictionary<(Type, string), Func<object, string>> propertySerializationSettings = 
            new Dictionary<(Type, string), Func<object, string>>();
        private readonly Dictionary<(Type, string), int> propertyTrimmingSettings = new Dictionary<(Type, string), int>();
        private readonly Dictionary<Type, CultureInfo> cultureInfoSettings = 
            new Dictionary<Type, CultureInfo>();

        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.TypeSerializationSettings => typeSerializationSettings;
        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.CultureInfoSettings => cultureInfoSettings;
        Dictionary<(Type, string), Func<object, string>> IPrintingConfig<TOwner>.PropertySerializationSettings =>
            propertySerializationSettings;
        Dictionary<(Type, string), int> IPrintingConfig<TOwner>.PropertyTrimmingSettings => propertyTrimmingSettings;

        public PrintingConfig<TOwner> ExcludingType<TType>()
        {
            excludedTypes.Add(typeof(TType));
            return this;
        }

        public PrintingConfig<TOwner> ExcludingProperty<TType>(Expression<Func<TOwner, TType>> propertySelector)
        {
            var propertyExpression = (MemberExpression)propertySelector.Body;
            excludedProperties.Add((propertyExpression.Member.DeclaringType, propertyExpression.Member.Name));
            return this;
        }

        public TypeSerializingContext<TOwner, TType> Serializing<TType>()
        {
            return new TypeSerializingContext<TOwner, TType>(this);
        }

        public PropertySerializingContext<TOwner, TType> Serializing<TType>(Expression<Func<TOwner, TType>> propertySelector)
        {
            var propertyExpression = (MemberExpression) propertySelector.Body;
            return new PropertySerializingContext<TOwner, TType>(this,
                (propertyExpression.Member.DeclaringType, propertyExpression.Member.Name));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            var finalValue = TryGetObjectStringValue(obj);
            if (finalValue != null)
                return finalValue;
            currentObjects.Push(obj);
            var type = obj.GetType();
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            if (obj is IEnumerable collection)
                sb.Append(PrintElements(collection, nestingLevel));
            else
                sb.Append(PrintMembers(obj, nestingLevel));
            currentObjects.Pop();
            return sb.ToString();
        }

        private string PrintMembers(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var memberInfo in type.GetProperties().Cast<MemberInfo>().Concat(type.GetFields()))
            {
                var memberRepresentation = TryGetMemberStringValue(obj, memberInfo, nestingLevel);
                if (memberRepresentation == null)
                    continue;
                sb.Append(indentation + memberInfo.Name + " = " + memberRepresentation);
            }

            return sb.ToString();
        }

        private string PrintElements(IEnumerable collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);
            var counter = 0;
            foreach (var element in collection)
            {
                var value = PrintToString(element, nestingLevel + 1);
                sb.Append(indentation + $"{counter++}: " + value);
            }

            return sb.ToString();
        }

        private string TryGetObjectStringValue(object obj)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();
            if (currentObjects.Contains(obj))
                return type.Name + " (already printed)" + Environment.NewLine;
            if (cultureInfoSettings.ContainsKey(type))
                return ((IFormattable)obj).ToString(null, cultureInfoSettings[type]) + Environment.NewLine;
            if (typeSerializationSettings.ContainsKey(type))
                return typeSerializationSettings[type].Invoke(obj) + Environment.NewLine;
            switch (obj)
            {
                case IFormattable formattableObj:
                    return formattableObj.ToString(null, CultureInfo.InvariantCulture) + Environment.NewLine;
                case string _:
                    return obj + Environment.NewLine;
                default:
                    return null;
            }
        }

        private string TryGetMemberStringValue(object obj, MemberInfo memberInfo, int nestingLevel)
        {
            var type = obj.GetType();
            var member = (type, memberInfo.Name);
            var memberType = (memberInfo as PropertyInfo)?.PropertyType ?? (memberInfo as FieldInfo)?.FieldType;
            var memberValue = (memberInfo as PropertyInfo)?.GetValue(obj) ?? (memberInfo as FieldInfo)?.GetValue(obj);
            if (excludedTypes.Contains(memberType) || excludedProperties.Contains(member))
                return null;
            var value = propertySerializationSettings.ContainsKey(member) ?
                propertySerializationSettings[member].Invoke(memberValue) + Environment.NewLine
                : PrintToString(memberValue, nestingLevel + 1);

            if (propertyTrimmingSettings.ContainsKey(member))
                value = value.Remove(propertyTrimmingSettings[member]) +
                                         Environment.NewLine;
            return value;
        }
    }
}