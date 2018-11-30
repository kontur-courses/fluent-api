using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> mutedTypes = new HashSet<Type>();
        private readonly HashSet<string> mutedMembers = new HashSet<string>();
        private readonly Dictionary<Type, Delegate> alternativeTypeSerializers = new Dictionary<Type, Delegate>();
        private readonly Dictionary<string, Delegate> alternativePropertySerializers = new Dictionary<string, Delegate>();
        private readonly Dictionary<Type, CultureInfo> cultures = new Dictionary<Type, CultureInfo>();

        private static int maxNestingLevel = 10;
        private static readonly Type[] additionalFinalTypes = { typeof(string), typeof(DateTime), typeof(TimeSpan) };
        
        public void AddAlternativeTypeSerializer<TProperty>(Func<TProperty, string> printingFunc)
        {
            alternativeTypeSerializers[typeof(TProperty)] = printingFunc;
        }

        public void AddAlternativePropertySerializer<TProperty>(Func<TProperty, string> printingFunc, string propertyName)
        {
            alternativePropertySerializers[propertyName] = printingFunc;
        }

        public void AddCultureInfo(Type type, CultureInfo culture)
        {
            cultures[type] = culture;
        }

        public PrintingConfig<TOwner> Excluding<TProperty>()
        {
            mutedTypes.Add(typeof(TProperty));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            var memberInfo = ((MemberExpression)memberSelector.Body).Member;
            var memberName = memberInfo.Name;
            mutedMembers.Add(memberName);
            return this;
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>()
        {
            return new PropertyPrintingConfig<TOwner, TProperty>(this);
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            var memberInfo = ((MemberExpression)memberSelector.Body).Member;
            var memberName = memberInfo.Name;
            return new PropertyPrintingConfig<TOwner, TProperty>(this, memberName);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public string PrintToString(IEnumerable<TOwner> collection)
        {
            var sb = new StringBuilder();
            sb.AppendLine(collection.GetType().Name);
            foreach (var item in collection)
            {
                var itemRepresentation = PrintToString(item, 1);
                sb.Append('\t');
                sb.Append(itemRepresentation);
            }
            return sb.ToString();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (obj.GetType().IsPrimitive || additionalFinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            if (nestingLevel > maxNestingLevel)
                return "..." + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            MemberInfo[] properties = type.GetProperties();
            MemberInfo[] fields = type.GetFields();
            var allMemberInfo = properties.Concat(fields);
            foreach (var memberInfo in allMemberInfo)
            {
                var memberName = memberInfo.Name;
                var (memberType, memberValueForObj) = GetTypeAndValueFor(memberInfo, obj);

                if (mutedTypes.Contains(memberType) || mutedMembers.Contains(memberName)) continue;

                var valueRepresentation = GetSpecialRepresentationForValue(memberValueForObj, memberName, memberType);
                if (valueRepresentation == null)
                {
                    var valueRepresentationWithNewLine = PrintToString(memberValueForObj, nestingLevel + 1);
                    sb.Append(indentation)
                        .Append($"{memberName} = {valueRepresentationWithNewLine}");
                }
                else
                {
                    sb.Append(indentation)
                        .Append($"{memberName} = {valueRepresentation}")
                        .Append(Environment.NewLine);
                }
            }
            return sb.ToString();
        }

        private (Type, object) GetTypeAndValueFor(MemberInfo memberInfo, object obj)
        {
            Type type;
            object value;
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    value = fieldInfo.GetValue(obj);
                    type = fieldInfo.FieldType;
                    return (type, value);
                case PropertyInfo propertyInfo:
                    value = propertyInfo.GetValue(obj);
                    type = propertyInfo.PropertyType;
                    return (type, value);
                default:
                    throw new Exception($"Unexpected memberInfo: {nameof(memberInfo.Name)}");
            }
        }

        private object GetSpecialRepresentationForValue(object value, string name, Type type)
        {
            if (alternativeTypeSerializers.ContainsKey(type))
                return alternativeTypeSerializers[type].DynamicInvoke(value);
            if (alternativePropertySerializers.ContainsKey(name))
                return alternativePropertySerializers[name].DynamicInvoke(value);
            if (cultures.ContainsKey(type))
                return ((IFormattable)value).ToString(null, cultures[type]);
            return null;
        }
    }
}