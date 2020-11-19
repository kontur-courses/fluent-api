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
        private readonly HashSet<Type> typesToExclude = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> propertiesToExclude = new HashSet<PropertyInfo>();

        private bool IsExcluded(PropertyInfo property) => typesToExclude.Contains(property.PropertyType) ||
                                                          propertiesToExclude.Contains(property);
        
        private readonly Dictionary<Type, Delegate> alternativeTypeRules = new Dictionary<Type, Delegate>();
        private readonly Dictionary<PropertyInfo, Delegate> alternativePropertyRules =
            new Dictionary<PropertyInfo, Delegate>();
        private readonly Dictionary<Type, CultureInfo> cultures = new Dictionary<Type, CultureInfo>();

        private PropertyInfo parameterToAdd;

        public void AddRule<TPropType>(Func<TPropType, string> print)
        {
            if (parameterToAdd == null)
            {
                alternativeTypeRules[typeof(TPropType)] = print;
            }
            else
            {
                alternativePropertyRules[parameterToAdd] = print;
                parameterToAdd = null;
            }
        }

        public void AddCulture<TPropType>(CultureInfo cultureInfo)
        {
            cultures[typeof(TPropType)] = cultureInfo;
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression)memberSelector.Body;
            parameterToAdd = (PropertyInfo)body.Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression) memberSelector.Body;
            propertiesToExclude.Add((PropertyInfo) body.Member);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToExclude.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private readonly Type[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        
        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties().Where(t => !IsExcluded(t)))
                sb.Append(indentation + propertyInfo.Name + " = " + PropertyToString(propertyInfo, obj, nestingLevel));
            return sb.ToString();
        }

        private string PropertyToString(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            dynamic value = propertyInfo.GetValue(obj);
            var type = propertyInfo.PropertyType;
            
            if (alternativePropertyRules.TryGetValue(propertyInfo, out var propertyRule))
                return (string) propertyRule.DynamicInvoke(value) + Environment.NewLine;
            
            if (alternativeTypeRules.TryGetValue(type, out var typeRule))
                return (string) typeRule.DynamicInvoke(value) + Environment.NewLine;
            
            if (cultures.TryGetValue(type, out var cultureInfo))
                return value.ToString(cultureInfo) + Environment.NewLine;
            
            return PrintToString(value, nestingLevel + 1);
        }
    }
}