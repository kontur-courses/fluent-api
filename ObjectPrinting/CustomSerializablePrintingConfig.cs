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
    public class CustomSerializablePrintingConfig<TOwner> : ICustomSerializablePrintingConfig
    {
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<object> printed = new HashSet<object>();
        public Dictionary<Type, Delegate> TypeSerializers { get; } = new Dictionary<Type, Delegate>();
        public Dictionary<MemberInfo, Delegate> PropertySerializers { get; } = new Dictionary<MemberInfo, Delegate>();
        public Dictionary<Type, CultureInfo> TypesCultures { get; } = new Dictionary<Type, CultureInfo>();
        public Dictionary<MemberInfo, int> StringPropertyLengths { get; } = new Dictionary<MemberInfo, int>();
        
        private readonly Type[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PropertyPrintingConfig<TOwner, TProperty> ForProperties<TProperty>()
        {
           return new PropertyPrintingConfig<TOwner, TProperty>(this);
        }

        public PropertyPrintingConfig<TOwner, TProperty> ForProperty<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TProperty>(this, memberSelector);
        }
        
        public CustomSerializablePrintingConfig<TOwner> Excluding<TProperty>()
        {
            excludedTypes.Add(typeof(TProperty));
            return this;
        }
        
        public CustomSerializablePrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
                excludedMembers.Add(memberExpression.Member);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            var result = PrintToString(obj, 0);
            printed.Clear();
            return result;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null";

            if (printed.Contains(obj))
                return obj.GetType().FullName;
            
            printed.Add(obj);

            if (TypeSerializers.TryGetValue(obj.GetType(), out var serializer))
                return (string) serializer.DynamicInvoke(obj);
            if (TypesCultures.TryGetValue(obj.GetType(), out  var cultureInfo) && obj is IFormattable formattable)
                return formattable.ToString(null, cultureInfo);
            if (obj.GetType() != typeof(string) && TryPrintIEnumerable(obj, nestingLevel, out var result))
                return result;

            if (finalTypes.Contains(obj.GetType()))
                return obj.ToString();
            
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine();
            sb.Append(type.Name);
            foreach (var propertyInfo in type.GetProperties().Where(info => !IsPropertyExcluded(info)))
                sb.Append(PrintMember(propertyInfo.GetValue(obj), propertyInfo, nestingLevel));
            
            foreach (var fieldInfo in type.GetFields().Where(info => !IsFieldExcluded(info)))
                sb.Append(PrintMember(fieldInfo.GetValue(obj), fieldInfo, nestingLevel));
            return sb.ToString();
        }

        private bool IsPropertyExcluded(PropertyInfo info) => excludedTypes.Contains(info.PropertyType) || excludedMembers.Contains(info);

        private bool IsFieldExcluded(FieldInfo fieldInfo)
        {
            return fieldInfo.IsStatic 
                   || !fieldInfo.IsPublic 
                   || excludedTypes.Contains(fieldInfo.FieldType) 
                   || excludedMembers.Contains(fieldInfo);
        }
        
        private bool TryPrintIEnumerable(object obj, int nestingLevel, out string result)
        {
            result = string.Empty;
            if (!(obj is IEnumerable iEnumerable))
                return false;
            
            var sb = new StringBuilder();
            sb.Append("[ ");
            var hasElements = false;
            
            foreach (var i in iEnumerable)
            {
                sb.Append(PrintToString(i, nestingLevel + 1) + ", ");
                hasElements = true;
            }

            var removeCount = hasElements ? 2 : 1;
            sb.Remove(sb.Length - removeCount, removeCount);
            sb.Append(" ]");
            result = sb.ToString();
            return true;
        }
        
        private string PrintMember(object value, MemberInfo memberInfo, int nestingLevel)
        {
            string valueString;
            if (PropertySerializers.TryGetValue(memberInfo, out var printingFunc)) 
                valueString = (string)printingFunc.DynamicInvoke(value);
            else
                valueString = PrintToString(value, nestingLevel + 1);
            
            if (StringPropertyLengths.TryGetValue(memberInfo, out var length))
                valueString = valueString?[..length] + Environment.NewLine;

            return Environment.NewLine + new string('\t', nestingLevel + 1) + memberInfo.Name + " = " + valueString;
        }
    }
}