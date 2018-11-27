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
        private int nestingLevel;
        #endregion

        public PrintingConfig()
        {
            nestingLevel = 1;

        }

        private PrintingConfig(int nestingLevel)
        {
            this.nestingLevel = nestingLevel;
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
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var nextLevel = nestingLevel+1;
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!IsExcludingType(propertyInfo.PropertyType) && !IsExludedProperty(propertyInfo.Name))
                {
                    var currentObj = propertyInfo.GetValue(obj);
                    sb.Append(
                        $"{identation}{propertyInfo.Name} = {currentObj.PrintToString(config => new PrintingConfig<object>(nextLevel))}");
                }
                
            }
            return sb.ToString();
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
        List<string> NamesExludedProperties => namesExcludedProperties;

        List<Type> ExcludingTypes => exludedTypes;

        Dictionary<string, Expression<Func<object, string>>> PrintMethodsForProperties => printMethodsForProperties;

        Dictionary<Type, Expression<Func<object, string>>> PrintMethodsForTypes { get; } = new Dictionary<Type, Expression<Func<object, string>>>();

        Dictionary<Type, CultureInfo> CulturesForNumbers => culturiesForNumbers;

        int NestingLevel => nestingLevel;
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