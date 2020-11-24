using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class Configurator<TOwner>
    {
        internal readonly Dictionary<PropertyInfo, Delegate> PropPrintingMethods =
            new Dictionary<PropertyInfo, Delegate>();

        internal readonly HashSet<PropertyInfo> PropsToExclude = new HashSet<PropertyInfo>();
        internal readonly Dictionary<Type, CultureInfo> TypePrintingCultureInfo = new Dictionary<Type, CultureInfo>();
        internal readonly Dictionary<Type, Delegate> TypePrintingMethods = new Dictionary<Type, Delegate>();
        internal readonly HashSet<Type> TypesToExclude = new HashSet<Type>();

        private PropertyInfo propertyToConfig;

        public Configurator<TOwner> AddPrintingMethod<TPropType>(Func<TPropType, string> method)
        {
            if (propertyToConfig == null)
            {
                TypePrintingMethods[typeof(TPropType)] = method;
            }
            else
            {
                PropPrintingMethods[propertyToConfig] = method;
                propertyToConfig = null;
            }

            return this;
        }

        public Configurator<TOwner> AddPrintingCulture<TPropType>(CultureInfo cultureInfo)
        {
            TypePrintingCultureInfo[typeof(TPropType)] = cultureInfo;
            return this;
        }

        public IPropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public IPropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
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
                PropsToExclude.Add((PropertyInfo) body.Member);
            }

            return this;
        }

        public Configurator<TOwner> Excluding<TPropType>()
        {
            TypesToExclude.Add(typeof(TPropType));
            return this;
        }
    }
}