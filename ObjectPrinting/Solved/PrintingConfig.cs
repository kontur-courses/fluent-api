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
	    private readonly List<Type> excludingTypes;
	    private readonly List<string> excludingPropertiesNames;
	    private readonly Dictionary<Type, Func<object, string>> typeSerialisations;
        private readonly Dictionary<string, Func<object, string>> propertiesSerialisations;
	    private readonly Dictionary<Type, CultureInfo> typeCultures;
	    Action<Type, Func<object, string>> IPrintingConfig<TOwner>.AddTypeSerialisation => AddTypeSerialisation;

	    Action<string, Func<object, string>> IPrintingConfig<TOwner>.AddPropertySerialisation
	        => AddPropertySerialisation;
        Action<Type, CultureInfo> IPrintingConfig<TOwner>.AddTypeCulture => AddTypeCulture;


        public PrintingConfig()
	    {
	        excludingTypes = new List<Type>();
            excludingPropertiesNames = new List<string>();
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
			return new PropertyPrintingConfig<TOwner, TPropType>(this);
		}

		public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
		{
            excludingPropertiesNames.Add(((MemberExpression)memberSelector.Body).Member.Name);
            return this;
		}

		internal PrintingConfig<TOwner> Excluding<TPropType>()
		{
            excludingTypes.Add(typeof(TPropType));
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
            
            var finalTypes = new[]
		    {
		        typeof(int), typeof(double), typeof(float), typeof(string),
		        typeof(DateTime), typeof(TimeSpan)
		    };
		    if (finalTypes.Contains(obj.GetType()))
				return obj + Environment.NewLine;

			var identation = new string('\t', nestingLevel + 1);
			var sb = new StringBuilder();
			var type = obj.GetType();
			sb.AppendLine(type.Name);
			foreach (var propertyInfo in type.GetProperties().Where(i => !excludingTypes.Contains(i.PropertyType)))
			{
                if(excludingPropertiesNames.Contains(propertyInfo.Name))
                    continue;
			    
			    sb.Append(identation + propertyInfo.Name + " = " +
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