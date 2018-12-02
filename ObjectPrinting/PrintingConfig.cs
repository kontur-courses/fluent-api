using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions.Common;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        #region privateFields

        private readonly int nestingLevel;
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private int maxRecursiveLevel = 10;
        private readonly HashSet<string> namesExcludedProperties = new HashSet<string>();

        private readonly Dictionary<string, List<Func<object, string>>> printMethodsForProperties =
            new Dictionary<string, List<Func<object, string>>>();

        private readonly Dictionary<Type, List<Func<object, string>>> printMethodsForTypes =
            new Dictionary<Type, List<Func<object, string>>>();


        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid),
        };

        #endregion

        #region ctor
        public PrintingConfig()
        {
            nestingLevel = 1;
        }

        private PrintingConfig(int nestingLevel, int maxRecursiveLevel)
        {
            this.nestingLevel = nestingLevel;
            this.maxRecursiveLevel = maxRecursiveLevel;
        }
        #endregion

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var propName = ((MemberExpression)propertySelector.Body).Member.Name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propName);
        }

        public PrintingConfig<TOwner> SetMaxNestingLevel(int maxNestingLevel)
        {
            maxRecursiveLevel = maxNestingLevel;
            return this;
        }



        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            namesExcludedProperties.Add(((MemberExpression)memberSelector.Body).Member.Name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
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
                return "null";
            if (printMethodsForTypes.TryGetValue(obj.GetType(), out var finalPrinters))
                return ApplyAllMethods(finalPrinters, obj);
            if (finalTypes.Contains(obj.GetType()))
                return obj.ToString();
            if (obj is IEnumerable)
                return CollectionProcessing((IEnumerable)obj);
            return PrintProperties(obj);

        }

        private string PrintProperties(object obj)
        {
            if (nestingLevel > maxRecursiveLevel)
                return $"{obj.GetType().Name} (Max nesting level!)";
            var identation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.Append(type.Name);
            var nextLevel = nestingLevel + 1;
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) ||
                    namesExcludedProperties.Contains(propertyInfo.Name)) continue;


                if (propertyInfo.IsIndexer())
                {
                    sb.Append($"{Environment.NewLine}{identation}{CollectionProcessing((IEnumerable) obj)}");
                    continue;
                }

                string serializeProperty;
                var currentObj = propertyInfo.GetValue(obj);
                var printMethods = GetPrintMethods(propertyInfo);
                if (printMethods.Count == 0)
                    serializeProperty = " = " + currentObj.PrintToString(config => new PrintingConfig<object>(nextLevel, maxRecursiveLevel));
                else
                    serializeProperty = " = " + ApplyAllMethods(printMethods, currentObj);

                sb.Append($"{Environment.NewLine}{identation}{propertyInfo.Name}{serializeProperty}");
            }
            return sb.ToString();
        }

        private List<Func<object, string>> GetPrintMethods(PropertyInfo propertyInfo)
        {
            List<Func<object, string>> printers = new List<Func<object, string>>();
            if (printMethodsForProperties.TryGetValue(propertyInfo.Name, out var printersProperties))
                printers.AddRange(printersProperties);
            if (printMethodsForTypes.TryGetValue(propertyInfo.PropertyType, out var printersTypes))
                printers.AddRange(printersTypes);
            return printers;

        }

        private string CollectionProcessing(IEnumerable collection)
        {
            var nextLevel = nestingLevel + 1;
            var ident = new string('\t', nestingLevel);
            var sb = new StringBuilder($"{collection.GetType().Name} :{Environment.NewLine}{ident}");           
            foreach (var item in collection)
            {
                var serialItem = item.PrintToString(config => new PrintingConfig<object>(nextLevel, maxRecursiveLevel));
                sb.Append($"   {serialItem},{Environment.NewLine}{ident}");
            }

            return sb.ToString();
        }

        private string ApplyAllMethods(List<Func<object, string>> methods, object obj)
        {
            if (methods.Count == 0)
                return obj.ToString();
            var prevObj = methods[0].Invoke(obj);
            foreach (var method in methods.Skip(1))
            {
                prevObj = method.Invoke(prevObj);
            }
            return prevObj;
        }


        #region InterfaceProperties
        Dictionary<string, List<Func<object, string>>> IPrintingConfig<TOwner>.PrintMethodsForProperties => printMethodsForProperties;

        Dictionary<Type, List<Func<object, string>>> IPrintingConfig<TOwner>.PrintMethodsForTypes => printMethodsForTypes;
        #endregion
    }

    interface IPrintingConfig<TOwner>
    {
        Dictionary<string, List<Func<object, string>>> PrintMethodsForProperties { get; }
        Dictionary<Type, List<Func<object, string>>> PrintMethodsForTypes { get; }
    }
}