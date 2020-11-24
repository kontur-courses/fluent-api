using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class Configurator<TOwner>
    {
        internal readonly Dictionary<PropertyInfo, Delegate> propPrintingMethods =
            new Dictionary<PropertyInfo, Delegate>();

        internal readonly HashSet<PropertyInfo> propsToExclude = new HashSet<PropertyInfo>();
        internal readonly Dictionary<Type, CultureInfo> typePrintingCultureInfo = new Dictionary<Type, CultureInfo>();
        internal readonly Dictionary<Type, Delegate> typePrintingMethods = new Dictionary<Type, Delegate>();
        internal readonly HashSet<Type> typesToExclude = new HashSet<Type>();

        private PropertyInfo propertyToConfig;

        public Configurator<TOwner> AddPrintingMethod<TPropType>(Func<TPropType, string> method)
        {
            if (propertyToConfig == null)
            {
                typePrintingMethods[typeof(TPropType)] = method;
            }
            else
            {
                propPrintingMethods[propertyToConfig] = method;
                propertyToConfig = null;
            }

            return this;
        }

        public Configurator<TOwner> AddPrintingCulture<TPropType>(CultureInfo cultureInfo)
        {
            typePrintingCultureInfo[typeof(TPropType)] = cultureInfo;
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression) memberSelector.Body;
            propertyToConfig = (PropertyInfo) body.Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public Configurator<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression body)
            {
                propsToExclude.Add((PropertyInfo) body.Member);
            }

            return this;
        }

        public Configurator<TOwner> Excluding<TPropType>()
        {
            typesToExclude.Add(typeof(TPropType));
            return this;
        }
    }
}