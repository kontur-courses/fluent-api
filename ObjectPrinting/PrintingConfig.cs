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
        private readonly List<Type> exludedTypes = new List<Type>();
        private readonly List<string> namesExcludedProperties = new List<string>();

        private readonly Dictionary<Type, CultureInfo> culturiesForNumbers =
            new Dictionary<Type, CultureInfo>();

        private readonly Dictionary<string, Expression<Func<object, string>>> printMethodsForProperties =
            new Dictionary<string, Expression<Func<object, string>>>();

        private readonly Dictionary<Type, Expression<Func<object, string>>> printMethodsForTypes =
            new Dictionary<Type, Expression<Func<object, string>>>();

        private readonly int nestingLevel;
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
            exludedTypes.Add(typeof(TPropType));
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
            if (TryGetMethodForType(obj.GetType(), out var finalPrinter))
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
                if (!IsExcludingType(propertyInfo.PropertyType) && !IsExludedProperty(propertyInfo.Name))
                {
                    var currentObj = propertyInfo.GetValue(obj);
                    var propertyName = propertyInfo.Name;
                    Func<object, string> printer;


                    if (TryGetMethodForProperty(propertyName, out var printerProperties))
                        printer = printerProperties;
                    else if (TryGetMethodForType(propertyInfo.PropertyType, out var printerTypes))
                        printer = printerTypes;
                    else
                        printer = o => o.PrintToString(config => new PrintingConfig<object>(nextLevel));

                    sb.Append($"\r\n{identation}{propertyInfo.Name} = {printer.Invoke(currentObj)}");
                }

            }
            return sb.ToString();
        }

        private bool TryGetMethodForType(Type type, out Func<object, string> expr)
        {
            if (printMethodsForTypes.ContainsKey(type))
            {
                expr = printMethodsForTypes[type].Compile();
                return true;
            }
            else
            {
                expr = null;
                return false;
            }
        }

        private bool TryGetMethodForProperty(string propertyName, out Func<object, string> expr)
        {
            if (printMethodsForProperties.ContainsKey(propertyName))
            {
                expr = printMethodsForProperties[propertyName].Compile();
                return true;
            }
            else
            {
                expr = null;
                return false;
            }
        }
   
        private bool IsExcludingType(Type type)
        {
            return exludedTypes.Contains(type);
        }

        private bool IsExludedProperty(string propertyName)
        {
            return namesExcludedProperties.Contains(propertyName);
        }

        #region InterfaceProperties
        List<string> IPrintingConfig<TOwner>.NamesExludedProperties => namesExcludedProperties;

        List<Type> IPrintingConfig<TOwner>.ExcludingTypes => exludedTypes;

        Dictionary<string, Expression<Func<object, string>>> IPrintingConfig<TOwner>.PrintMethodsForProperties => printMethodsForProperties;

        Dictionary<Type, Expression<Func<object, string>>> IPrintingConfig<TOwner>.PrintMethodsForTypes => printMethodsForTypes;
        #endregion
    }

    interface IPrintingConfig<TOwner>
    {
        List<string> NamesExludedProperties { get; }
        List<Type> ExcludingTypes { get; }

        Dictionary<string, Expression<Func<object, string>>> PrintMethodsForProperties { get; }
        Dictionary<Type, Expression<Func<object, string>>> PrintMethodsForTypes { get; }
    }
}