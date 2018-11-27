using System;
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
        private int nestingLevel;
        private Func<object, string> printMethod = o => o.ToString();
        #endregion

        #region ctor
        public PrintingConfig()
        {
            nestingLevel = 1;
        }

        private PrintingConfig(int nestingLevel, Func<object, string> printMethod)
        {
            this.nestingLevel = nestingLevel;
            this.printMethod = printMethod;
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

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

            if (finalTypes.Contains(obj.GetType()))
                return printMethod.Invoke(obj) + Environment.NewLine;

            var identation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var nextLevel = nestingLevel+1;
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!IsExcludingType(propertyInfo.PropertyType) && !IsExludedProperty(propertyInfo.Name))
                {
                    var expr1 = GetMethodForType(propertyInfo.PropertyType);
                    var expr2 = GetMethodForProperty(propertyInfo.Name);
                    Func<object, string> printMethod = o => expr1.Compile().Invoke(expr2.Compile().Invoke(o));
                    var currentObj = propertyInfo.GetValue(obj);

                    sb.Append(
                        $"{identation}{propertyInfo.Name} = {currentObj.PrintToString(config => new PrintingConfig<object>(nextLevel, printMethod))}");
                }
                
            }
            return sb.ToString();
        }

        private Expression<Func<object, string>> GetMethodForType(Type type)
        {
            if (printMethodsForTypes.ContainsKey(type))
            {
                return printMethodsForTypes[type];
            }
            else
            {
                return o => o.ToString();
            }
        }

        private Expression<Func<object, string>> GetMethodForProperty(string propertyName)
        {
            if (printMethodsForProperties.ContainsKey(propertyName))
            {
                return printMethodsForProperties[propertyName];
            }
            else
            {
                return o => o.ToString();
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

        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.CulturesForNumbers => culturiesForNumbers;

        int IPrintingConfig<TOwner>.NestingLevel => nestingLevel;
        #endregion
    }

    interface IPrintingConfig<TOwner>
    {
        int NestingLevel { get; }

        List<string> NamesExludedProperties { get; }
        List<Type> ExcludingTypes { get; }

        Dictionary<string, Expression<Func<object, string>>> PrintMethodsForProperties { get; }
        Dictionary<Type, Expression<Func<object, string>>> PrintMethodsForTypes { get; }

        Dictionary<Type, CultureInfo> CulturesForNumbers { get; }
    }
}