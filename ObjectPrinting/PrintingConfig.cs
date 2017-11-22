using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    interface IPrintingConfig
    {
        Dictionary<PropertyInfo, int> TrimmingLenth { get; }
        Dictionary<Type, Func<object, string>> CustomSerialization { get; }
        Dictionary<Type, CultureInfo> NumericCulture { get; }
        Dictionary<PropertyInfo, Func<object, string>> CustomPropetrySerialization { get; }
    }

	public class PrintingConfig<TOwner> : IPrintingConfig
	{
	    private static readonly Type[] FinalTypes = {
	        typeof(int), typeof(double), typeof(float), typeof(string),
	        typeof(DateTime), typeof(TimeSpan)
	    };

	    private readonly List<Type> excludingTypes;
        private readonly List<PropertyInfo> excludingProprties;

	    Dictionary<PropertyInfo, int> IPrintingConfig.TrimmingLenth => trimmingLenth;
        private readonly Dictionary<PropertyInfo, int> trimmingLenth;

	    Dictionary<Type, Func<object, string>> IPrintingConfig.CustomSerialization => customSerialization;
        private readonly Dictionary<Type, Func<object, string>> customSerialization;

	    Dictionary<Type, CultureInfo> IPrintingConfig.NumericCulture => numericCulture;
        private readonly Dictionary<Type, CultureInfo> numericCulture;

	    Dictionary<PropertyInfo, Func<object, string>> IPrintingConfig.CustomPropetrySerialization => customPropertySerialization;
	    private readonly Dictionary<PropertyInfo, Func<object, string>> customPropertySerialization;

        public PrintingConfig()
	    {
	        excludingTypes = new List<Type>();
	        excludingProprties = new List<PropertyInfo>();
            trimmingLenth = new Dictionary<PropertyInfo, int>();
            customSerialization = new Dictionary<Type, Func<object, string>>();
            numericCulture = new Dictionary<Type, CultureInfo>();
            customPropertySerialization = new Dictionary<PropertyInfo, Func<object, string>>();
        }


	    internal PrintingConfig<TOwner> Excluding<TPropType>()
	    {
            excludingTypes.Add(typeof(TPropType));
	        return this;
	    }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
	    {
	        var propInfo = (PropertyInfo) ((MemberExpression)memberSelector.Body).Member;
	        excludingProprties.Add(propInfo);
            return this;
	    }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() 
            => new PropertyPrintingConfig<TOwner, TPropType>(this, typeof(TPropType));

	    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>
            (Expression<Func<TOwner, TPropType>> memberSelector)
		{
		    var propInfo = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propInfo);
		}

		public string PrintToString(TOwner obj)
		{
			return PrintToString(obj, 0);
		}

		private string PrintToString(object obj, int nestingLevel)
		{
		    if (obj == null)
                return "null" + Environment.NewLine;

		    var type = obj.GetType();

		    if (numericCulture.ContainsKey(type))
		        return ConverNumeric(obj, type);

            if (customSerialization.ContainsKey(type))
		        return customSerialization[type](obj);

		    if (FinalTypes.Contains(type))
				return obj + Environment.NewLine;

		    return ConvertPropertiesToString(obj, nestingLevel);
        }

	    private string ConverNumeric(object obj, Type type)
	    {
	        var toStringMethod = type.GetMethod("ToString", new[] {typeof(IFormatProvider)});
	        return toStringMethod.Invoke(obj, new object[] {numericCulture[type]})
                + Environment.NewLine;
	    }

	    private string ConvertPropertiesToString(object obj, int nestingLevel)
	    {
	        var identation = new string('\t', nestingLevel + 1);
	        var sb = new StringBuilder();
	        var type = obj.GetType();

	        sb.AppendLine(type.Name);
	        foreach (var propertyInfo in type.GetProperties())
	        {
	            if (IgnoreProperty(propertyInfo)) continue;
	            var stringProperty = ToStringProperty(obj, nestingLevel, propertyInfo);
                sb.Append(identation + propertyInfo.Name + " = " + stringProperty);
	        }
	        return sb.ToString();
	    }

	    private string ToStringProperty(object obj, int nestingLevel, PropertyInfo propertyInfo)
	    {
	        var stringProperty = customPropertySerialization.ContainsKey(propertyInfo) ? 
                customPropertySerialization[propertyInfo](propertyInfo.GetValue(obj)) :
                PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);

	        if (propertyInfo.PropertyType != typeof(string) || !trimmingLenth.ContainsKey(propertyInfo)) return stringProperty;

	        stringProperty = TrimmProperty(propertyInfo, stringProperty);
	        return stringProperty;
	    }

	    private string TrimmProperty(PropertyInfo property, string stringProperty)
	    {
	        var trimmingCount = Math.Min(trimmingLenth[property], stringProperty.Length);
	        stringProperty = stringProperty.Substring(0, trimmingCount);
	        return stringProperty;
	    }

	    private bool IgnoreProperty(PropertyInfo propertyInfo) 
            => excludingTypes.Contains(propertyInfo.PropertyType) 
            || excludingProprties.Contains(propertyInfo);
    }
}