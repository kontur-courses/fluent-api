using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly IDictionary<Type, CultureInfo> cultureLookup;
        private readonly ISet<MemberInfo> excludedProperties;
        private readonly ISet<Type> excludedTypes;
        private readonly IDictionary<MemberInfo, Delegate> propertyPrinters;
        private readonly IDictionary<Type, Delegate> typePrinters;

        internal PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<MemberInfo>();
            typePrinters = new Dictionary<Type, Delegate>();
            propertyPrinters = new Dictionary<MemberInfo, Delegate>();
            cultureLookup = new Dictionary<Type, CultureInfo>();
        }

        ISet<MemberInfo> IPrintingConfig.ExcludedProperties => excludedProperties;
        ISet<Type> IPrintingConfig.ExcludedTypes => excludedTypes;
        IDictionary<Type, Delegate> IPrintingConfig.TypePrinters => typePrinters;
        IDictionary<MemberInfo, Delegate> IPrintingConfig.PropertyPrinters => propertyPrinters;
        IDictionary<Type, CultureInfo> IPrintingConfig.CultureLookup => cultureLookup;

        public PropertyPrintingConfig<TOwner, T> Printing<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> memberSelector)
        {
            var memberInfo = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, T>(this, memberInfo);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> memberSelector)
        {
            var memberInfo = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            excludedProperties.Add(memberInfo ?? throw new ArgumentException("Property expected"));
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return new ObjectPrinter(this).PrintToString(obj);
        }
    }

    internal interface IPrintingConfig
    {
        ISet<MemberInfo> ExcludedProperties { get; }
        ISet<Type> ExcludedTypes { get; }
        IDictionary<Type, Delegate> TypePrinters { get; }
        IDictionary<MemberInfo, Delegate> PropertyPrinters { get; }
        IDictionary<Type, CultureInfo> CultureLookup { get; }
    }
}