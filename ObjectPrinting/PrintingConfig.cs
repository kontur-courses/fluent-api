using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<PropertyInfo, Func<object, string>> customPropertyPrinters;
        private readonly Dictionary<Type, Func<object, string>> customTypePrinters;
        private readonly HashSet<PropertyInfo> excludedProperties;
        private readonly HashSet<Type> excludedPropTypes;

        private readonly Type[] finalNotPrimitiveTypes =
        {
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid),
            typeof(Point), typeof(Rectangle), typeof(Size), typeof(decimal), typeof(DateTimeOffset)
        };

        public PrintingConfig()
        {
            excludedPropTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            customTypePrinters = new Dictionary<Type, Func<object, string>>();
            customPropertyPrinters = new Dictionary<PropertyInfo, Func<object, string>>();
        }

        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add(memberSelector.GetPropertyInfo());
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedPropTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Using<TPropType>(
            Func<TPropType, string> print)
        {
            customTypePrinters[typeof(TPropType)] = obj => print((TPropType) obj);
            return this;
        }

        public PrintingConfig<TOwner> Using<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector,
            Func<TPropType, string> print)
        {
            customPropertyPrinters[memberSelector.GetPropertyInfo()] = obj => print((TPropType) obj);
            return this;
        }

        public bool IsFinal(Type type)
        {
            return type.IsPrimitive || finalNotPrimitiveTypes.Contains(type);
        }

        public bool IsExcluded(PropertyInfo propInfo)
        {
            return excludedPropTypes.Contains(propInfo.PropertyType) || excludedProperties.Contains(propInfo);
        }

        public bool TryGetCustomPrinter(PropertyInfo propInfo, out Func<object, string> result)
        {
            return customPropertyPrinters.TryGetValue(propInfo, out result) ||
                   customTypePrinters.TryGetValue(propInfo.PropertyType, out result);
        }
    }
}