using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class SerializationConfig<TOwner>
    {
        protected PrintingConfig<TOwner> ParentConfig;
        
        private readonly HashSet<Type> typesToExclude = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> propertiesToExclude = new HashSet<PropertyInfo>();
        
        private readonly Dictionary<PropertyInfo, Delegate> alternativePropertyRules =
            new Dictionary<PropertyInfo, Delegate>();
        private readonly Dictionary<Type, Delegate> alternativeTypeRules = new Dictionary<Type, Delegate>();
        private readonly Dictionary<Type, CultureInfo> cultures = new Dictionary<Type, CultureInfo>();
        
        private PropertyInfo parameterToAdd;
        
        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression) memberSelector.Body;
            propertiesToExclude.Add((PropertyInfo) body.Member);
            return ParentConfig;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToExclude.Add(typeof(TPropType));
            return ParentConfig;
        }

        protected bool IsExcluded(PropertyInfo property) => typesToExclude.Contains(property.PropertyType) ||
                                                            propertiesToExclude.Contains(property);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() =>
            new PropertyPrintingConfig<TOwner, TPropType>(ParentConfig);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression)memberSelector.Body;
            parameterToAdd = (PropertyInfo)body.Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(ParentConfig);
        }
        
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

        public void AddCulture<TPropType>(CultureInfo cultureInfo) => cultures[typeof(TPropType)] = cultureInfo;

        protected string GetAlternativeSerialization(PropertyInfo propertyInfo, dynamic value)
        {
            if (value == null) return null;
            var type = propertyInfo.PropertyType;
            
            if (alternativePropertyRules.TryGetValue(propertyInfo, out var propertyRule))
                return (string) propertyRule.DynamicInvoke(value) + Environment.NewLine;
            
            if (alternativeTypeRules.TryGetValue(type, out var typeRule))
                return (string) typeRule.DynamicInvoke(value) + Environment.NewLine;
            
            if (cultures.TryGetValue(type, out var cultureInfo))
                return value.ToString(cultureInfo) + Environment.NewLine;

            return null;
        }
    }
}