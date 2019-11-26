using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        public PrintingConfig()
        {
            exclusions = new HashSet<Type>();
            typePrintingFunctions = new Dictionary<Type, Delegate>();
            typeCultures = new Dictionary<Type, CultureInfo>();
            propertyPrintingFunctions = new Dictionary<string, Delegate>();
        }
        
        private readonly HashSet<Type> exclusions;
        private readonly Dictionary<Type, Delegate> typePrintingFunctions;
        private readonly Dictionary<Type, CultureInfo> typeCultures;
        private readonly Dictionary<string, Delegate> propertyPrintingFunctions;
        Dictionary<Type, Delegate> IPrintingConfig.TypePrintingFunctions => typePrintingFunctions;
        Dictionary<Type, CultureInfo> IPrintingConfig.TypeCultures => typeCultures;
        Dictionary<string, Delegate> IPrintingConfig.PropertyPrintingFunctions => propertyPrintingFunctions; 

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            exclusions.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (typePrintingFunctions.TryGetValue(obj.GetType(), out var printingFunc))
                return (string) printingFunc.DynamicInvoke(obj) + Environment.NewLine;
            
            if (finalTypes.Contains(obj.GetType()))
                if (typeCultures.ContainsKey(obj.GetType()))
                    return string.Format(typeCultures[obj.GetType()], obj.ToString()) + Environment.NewLine;
                else
                    return obj + Environment.NewLine;


            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (exclusions.Contains(propertyInfo.PropertyType)) continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
            }
            return sb.ToString();
        }
    }

    interface IPrintingConfig
    {
        Dictionary<Type, Delegate> TypePrintingFunctions { get; }
        Dictionary<string, Delegate> PropertyPrintingFunctions { get; }
        Dictionary<Type, CultureInfo> TypeCultures { get; }
    }
}