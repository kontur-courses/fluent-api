using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
	{
	    private readonly List<Type> ignoringTypes;
	    private readonly List<string> ignoringPropertiesNames;
	    private readonly Dictionary<Type, Func<object, string>> typeSerialisations;
        private readonly Dictionary<string, Func<object, string>> propertiesSerialisations;
	    private readonly Dictionary<Type, CultureInfo> typeCultures;
	    private readonly Dictionary<string, int> strPropsLengths;
	    private int averageStringLength;
	    Action<Type, Func<object, string>> IPrintingConfig<TOwner>.AddTypeSerialisation => AddTypeSerialisation;
	    Action<string, Func<object, string>> IPrintingConfig<TOwner>.AddPropertySerialisation
	        => AddPropertySerialisation;
        Action<Type, CultureInfo> IPrintingConfig<TOwner>.AddTypeCulture => AddTypeCulture;
        Action<int> IPrintingConfig<TOwner>.SetAverageStringLength => SetAverageStringLength;
        Action<string, int> IPrintingConfig<TOwner>.SetStringPropertyLength => SetStringPropertyLength;


        public PrintingConfig()
	    {
	        ignoringTypes = new List<Type>();
            ignoringPropertiesNames = new List<string>();
            typeSerialisations = new Dictionary<Type, Func<object, string>>();
            propertiesSerialisations = new Dictionary<string, Func<object, string>>();
            typeCultures = new Dictionary<Type, CultureInfo>();
            strPropsLengths = new Dictionary<string, int>();
	        averageStringLength = -1;
	    }

	    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
		{
			return new PropertyPrintingConfig<TOwner, TPropType>(this);
		}

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = ((MemberExpression) memberSelector.Body).Member.Name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
		}

		public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
		{
            ignoringPropertiesNames.Add(((MemberExpression)memberSelector.Body).Member.Name);
            return this;
		}

		internal PrintingConfig<TOwner> Excluding<TPropType>()
		{
            ignoringTypes.Add(typeof(TPropType));
			return this;
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

		    if (typeSerialisations.ContainsKey(type))
		        return typeSerialisations[type](obj) + Environment.NewLine;

		    if (obj is string && averageStringLength > 0)
		        return ((string) obj).Substring(0, averageStringLength) + Environment.NewLine;

            var numericTypes = new[]
            {
                typeof(int), typeof(double), typeof(float)
            };
            if (numericTypes.Contains(type) && typeCultures.ContainsKey(type))
                return NumericToString(obj) + Environment.NewLine;

            var finalTypes = new[]
		    {
		        typeof(int), typeof(double), typeof(float), typeof(string),
		        typeof(DateTime), typeof(TimeSpan)
		    };
            if (finalTypes.Contains(type))
				return obj + Environment.NewLine;
            
			var sb = new StringBuilder(type.Name + Environment.NewLine);

		    var properPropertiesStrings = type.GetProperties()
		        .Where(p => !(ignoringTypes.Contains(p.PropertyType) || ignoringPropertiesNames.Contains(p.Name)))
		        .Select(p => GetPropertyString(obj, p, nestingLevel));
		    foreach (var propertyStr in properPropertiesStrings)
		        sb.Append(propertyStr);

			return sb.ToString();
		}

	    private string GetPropertyString(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            var prefix = new string('\t', nestingLevel + 1) + propertyInfo.Name + " = ";
            if (propertiesSerialisations.ContainsKey(propertyInfo.Name))
                return prefix + propertiesSerialisations[propertyInfo.Name](propertyInfo.GetValue(obj)) + Environment.NewLine;

            if(strPropsLengths.ContainsKey(propertyInfo.Name))
                return prefix + ((string)propertyInfo.GetValue(obj)).Substring(0, strPropsLengths[propertyInfo.Name]) + Environment.NewLine;

            return prefix + PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
        }

	    private string NumericToString(object printingObj)
	    {
	        // ReSharper disable once PossibleNullReferenceException
	        return (string) printingObj.GetType()
	            .GetMethod("ToString", new[] {typeof(CultureInfo)})
	            .Invoke(printingObj, new object[] {typeCultures[printingObj.GetType()]});
	    }

	    private void AddTypeSerialisation(Type type, Func<object, string> serialisation)
	    {
	        typeSerialisations.Add(type, serialisation);
	    }

	    private void AddPropertySerialisation(string propertyName, Func<object, string> serialisation)
	    {
	        propertiesSerialisations.Add(propertyName, serialisation);
	    }

	    private void AddTypeCulture(Type type, CultureInfo culture)
	    {
	        typeCultures.Add(type, culture);
        }

        private void SetStringPropertyLength(string propertyName, int maxLength)
        {
            strPropsLengths.Add(propertyName, maxLength);
        }

        private void SetAverageStringLength(int maxLength)
        {
            averageStringLength = maxLength;
        }
    }
}