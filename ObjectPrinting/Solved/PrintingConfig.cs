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
	    Action<Type, Func<object, string>> IPrintingConfig<TOwner>.AddTypeSerialisation => AddTypeSerialisation;

	    Action<string, Func<object, string>> IPrintingConfig<TOwner>.AddPropertySerialisation
	        => AddPropertySerialisation;
        Action<Type, CultureInfo> IPrintingConfig<TOwner>.AddTypeCulture => AddTypeCulture;


        public PrintingConfig()
	    {
	        ignoringTypes = new List<Type>();
            ignoringPropertiesNames = new List<string>();
            typeSerialisations = new Dictionary<Type, Func<object, string>>();
            propertiesSerialisations = new Dictionary<string, Func<object, string>>();
            typeCultures = new Dictionary<Type, CultureInfo>();
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
			//TODO apply configurations
		    if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

		    if (typeSerialisations.ContainsKey(type))
		        return typeSerialisations[type](obj);

            var finalTypes = new[]
		    {
		        typeof(int), typeof(double), typeof(float), typeof(string),
		        typeof(DateTime), typeof(TimeSpan)
		    };
            var numericTypes = new[]
            {
                typeof(int), typeof(double), typeof(float)
            };
            if (numericTypes.Contains(type) && typeCultures.ContainsKey(type))
                // ReSharper disable once PossibleNullReferenceException
                return (string)type.GetMethod("ToString", new [] {typeof(CultureInfo)})
                    .Invoke(obj, new object[] {typeCultures[type]}) + Environment.NewLine;
            if (finalTypes.Contains(type))
				return obj + Environment.NewLine;

			var identation = new string('\t', nestingLevel + 1);
			var sb = new StringBuilder();
			sb.AppendLine(type.Name);
			foreach (var propertyInfo in type.GetProperties().Where(i => !ignoringTypes.Contains(i.PropertyType)))
			{
			    var propName = propertyInfo.Name;
                if(ignoringPropertiesNames.Contains(propName) || ignoringTypes.Contains(propertyInfo.PropertyType))
                    continue;

			    if (propertiesSerialisations.ContainsKey(propName))
			    {
			        sb.Append(identation + propName + " = " +
                              propertiesSerialisations[propName](propertyInfo.GetValue(obj)) + Environment.NewLine);
                    continue;
                }

                if (typeSerialisations.ContainsKey(propertyInfo.PropertyType))
                {
                    sb.Append(identation + propName + " = " +
                              typeSerialisations[propertyInfo.PropertyType](propertyInfo.GetValue(obj)) + Environment.NewLine);
                    continue;
                }

                sb.Append(identation + propName + " = " +
			              PrintToString(propertyInfo.GetValue(obj),
			                  nestingLevel + 1));
			}
			return sb.ToString();
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
	}
}