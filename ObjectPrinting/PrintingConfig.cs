using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner> : IPrintingConfigurationHolder
    {
        private readonly Dictionary<Type, CultureInfo> cultureInfos = new Dictionary<Type, CultureInfo>();
        private readonly HashSet<string> excludedProperties = new HashSet<string>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly List<Type> finalTypes;
        private int nestingLevel = int.MaxValue;
        private readonly Dictionary<string, Delegate> propertySerializers = new Dictionary<string, Delegate>();
        private readonly Dictionary<string, int> trimedParams = new Dictionary<string, int>();
        private readonly Dictionary<Type, Delegate> typeSerilizers = new Dictionary<Type, Delegate>();

        public PrintingConfig()
        {
            finalTypes = new List<Type>(new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            });
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var matchCollection = new Regex(string.Format("{0}.(\\w*)", memberSelector.Parameters[0].Name));
            var excludedProperty = matchCollection.Match(memberSelector.Body.ToString()).Value
                .Replace(memberSelector.Parameters[0].Name + ".", "");
            excludedProperties.Add(excludedProperty);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        internal PrintingConfig<TOwner> WithNestingLevel(int level)
        {
            nestingLevel = level;
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }


        private string FinilizeSerialization(object obj)
        {
            var culture = CultureInfo.InvariantCulture;
            if (cultureInfos.ContainsKey(obj.GetType()))
                culture = cultureInfos[obj.GetType()];
            var formatable = obj as IFormattable;
            if (formatable == null)
                return obj.ToString();
            return (obj as IFormattable).ToString(null, culture);
        }
        private string TrimmProperty(string str, PropertyInfo propertyInfo)
        {
            if (trimedParams.ContainsKey(propertyInfo.Name))
                str = string.Join("", str.Take(trimedParams[propertyInfo.Name]));
            return str;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel >= this.nestingLevel)
                return string.Empty;

            if (obj == null)
                return "null";


            if (finalTypes.Contains(obj.GetType()))
                return FinilizeSerialization(obj);

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo.Name))
                    continue;
                
                sb.Append(identation + propertyInfo.Name + "=");
                var pushValue = string.Empty;
                
                if (propertySerializers.ContainsKey(propertyInfo.Name))                
                    pushValue = propertySerializers[propertyInfo.Name]
                        .DynamicInvoke(propertyInfo.GetValue(obj)) as string;                
                else if (typeSerilizers.ContainsKey(propertyInfo.PropertyType))
                    pushValue = (typeSerilizers[propertyInfo.PropertyType]
                                  .DynamicInvoke(propertyInfo.GetValue(obj)) as string);                
                else
                    pushValue = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                
                sb.Append(TrimmProperty(pushValue, propertyInfo) + Environment.NewLine);
            }

            return sb.ToString();
        }

        #region InterfaceRealization

        HashSet<Type> IPrintingConfigurationHolder.excludedTypes
        {
            get { return excludedTypes; }
        }

        HashSet<string> IPrintingConfigurationHolder.excludedProperties
        {
            get { return excludedProperties; }
        }

        Dictionary<string, Delegate> IPrintingConfigurationHolder.propertySerializers
        {
            get { return propertySerializers; }
        }

        Dictionary<Type, Delegate> IPrintingConfigurationHolder.typeSerilizers
        {
            get { return typeSerilizers; }
        }

        Dictionary<Type, CultureInfo> IPrintingConfigurationHolder.cultureInfos
        {
            get { return cultureInfos; }
        }

        Dictionary<string, int> IPrintingConfigurationHolder.trimedParams
        {
            get { return trimedParams; }
        }

        #endregion
    }
}