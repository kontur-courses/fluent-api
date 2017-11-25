﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
	public class PrintingConfig<TOwner>
	{
	    private List<Type> excludingTypes;
	    private List<string> excludingPropertiesNames;
	    private Dictionary<Type, Func<TOwner, string>> typePrintingFuncs;
        private Dictionary<string, Func<TOwner, string>> propertiesPrintingFuncs;

        public PrintingConfig()
	    {
	        excludingTypes = new List<Type>();
            excludingPropertiesNames = new List<string>();
            typePrintingFuncs = new Dictionary<Type, Func<TOwner, string>>();
            propertiesPrintingFuncs = new Dictionary<string, Func<TOwner, string>>();
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
	}
}