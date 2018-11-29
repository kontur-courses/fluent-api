using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        #region privateFields
        private readonly int nestingLevel;
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<string> namesExcludedProperties = new HashSet<string>();

        private readonly Dictionary<string, Func<object, string>> printMethodsForProperties =
            new Dictionary<string, Func<object, string>>();

        private readonly Dictionary<Type, Func<object, string>> printMethodsForTypes =
            new Dictionary<Type, Func<object, string>>();

        
        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        #endregion

        #region ctor
        public PrintingConfig()
        {
            nestingLevel = 1;
        }

        private PrintingConfig(int nestingLevel)
        {
            this.nestingLevel = nestingLevel;
        }
        #endregion

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propName = ((MemberExpression) memberSelector.Body).Member.Name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propName);
        }



        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            namesExcludedProperties.Add(((MemberExpression)memberSelector.Body).Member.Name);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToStringAccordingConfiguration(obj);
        }

        private string PrintToStringAccordingConfiguration(object obj)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (printMethodsForTypes.TryGetValue(obj.GetType(), out var finalPrinter))
               return finalPrinter.Invoke(obj);
            if (finalTypes.Contains(obj.GetType()))
                return obj.ToString();
            return PrintProperties(obj);

        }

        private string PrintProperties(object obj)
        {
            var identation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.Append(type.Name);
            var nextLevel = nestingLevel + 1;
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!excludedTypes.Contains(propertyInfo.PropertyType) && !namesExcludedProperties.Contains(propertyInfo.Name))
                {
                    var currentObj = propertyInfo.GetValue(obj);
                    var propertyName = propertyInfo.Name;
                    Func<object, string> printer;


                    if (printMethodsForProperties.TryGetValue(propertyName, out var printerProperties))
                        printer = printerProperties;
                    else if (printMethodsForTypes.TryGetValue(propertyInfo.PropertyType, out var printerTypes))
                        printer = printerTypes;
                    else
                        printer = o => o.PrintToString(config => new PrintingConfig<object>(nextLevel));

                    sb.Append($"{Environment.NewLine}{identation}{propertyInfo.Name} = {printer.Invoke(currentObj)}");
                }

            }
            return sb.ToString();
        }
   
        private bool IsExcludingType(Type type)
        {
            return excludedTypes.Contains(type);
        }

        private bool IsExcludedProperty(string propertyName)
        {
            return namesExcludedProperties.Contains(propertyName);
        }

        #region InterfaceProperties
        Dictionary<string, Func<object, string>> IPrintingConfig<TOwner>.PrintMethodsForProperties => printMethodsForProperties;

        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.PrintMethodsForTypes => printMethodsForTypes;
        #endregion
    }

    interface IPrintingConfig<TOwner>
    {
        Dictionary<string, Func<object, string>> PrintMethodsForProperties { get; }
        Dictionary<Type, Func<object, string>> PrintMethodsForTypes { get; }
    }
}