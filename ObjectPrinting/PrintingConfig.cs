using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
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
        private int nestingLevel = 16;
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
      
        private string PrintToString(object obj, int nestingLevel)
        {           
            
            if (nestingLevel >= this.nestingLevel)
                return "Nesting Level Overflow!";

            if (obj == null)
                return "null";

            var pushValue = string.Empty;
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var objectType = obj.GetType();
            
            if (TryFinalizeSerialization(obj,out pushValue))
            {
                sb.Append(new string('\t',nestingLevel) + objectType.Name + "=" + pushValue + Environment.NewLine);
                return sb.ToString();
            }

            sb.AppendLine(objectType.Name);
            if (obj is IEnumerable)
            {
                foreach (var item in obj as IEnumerable)
                {
                    sb.Append(PrintToString(item, nestingLevel + 1));
                }
            }
            
            foreach (var propertyInfo in objectType.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo.Name))
                    continue;               
               
                try
                {
                    if (TryGetPropertySerialization(propertyInfo, obj, out pushValue)){}
                    else if (TryGetTypeSerialization(propertyInfo, obj, out pushValue)){}
                    else if (TryFinalizeSerialization(propertyInfo, obj, out pushValue)){}
                    else
                        pushValue = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);                  
                    sb.Append(identation + propertyInfo.PropertyType + " "  + propertyInfo.Name + "=");
                    sb.Append(TrimmProperty(pushValue, propertyInfo) + Environment.NewLine);
                }
                catch (TargetParameterCountException e)
                {}
            }
            return sb.ToString(); 
        }

        private bool TryGetPropertySerialization(PropertyInfo propertyInfo,object obj, out string pushValue)
        {
            pushValue = string.Empty;
            if (!propertySerializers.ContainsKey(propertyInfo.Name))
                return false;
            pushValue = propertySerializers[propertyInfo.Name].DynamicInvoke(propertyInfo.GetValue(obj)) as string;
            return true;
        }
        private bool TryGetTypeSerialization(PropertyInfo propertyInfo,object obj, out string pushValue)
        {
            pushValue = string.Empty;
            if (!typeSerilizers.ContainsKey(propertyInfo.PropertyType))
                return false;
            pushValue = typeSerilizers[propertyInfo.PropertyType].DynamicInvoke(propertyInfo.GetValue(obj)) as string;
            return true;
        }
        private bool TryFinalizeSerialization(PropertyInfo propertyInfo,object obj,out string pushValue)
        {
            pushValue = string.Empty;

            if (!finalTypes.Contains(propertyInfo.PropertyType))
            {
                return false;
            }            
            var culture = CultureInfo.InvariantCulture;
            if (cultureInfos.ContainsKey(propertyInfo.PropertyType))
                culture = cultureInfos[propertyInfo.PropertyType];

            var propValue = propertyInfo.GetValue(obj);
            if (propValue == null)
            {
                pushValue = "null";
                return false;
            }            
            var formatable = propertyInfo.GetValue(obj) as IFormattable;
            if (formatable == null)
                pushValue = propValue.ToString();
            else
                pushValue = (propValue as IFormattable).ToString(null, culture);
            return true;
        }       
        private bool TryFinalizeSerialization(object obj,out string pushValue)
        {
            pushValue = string.Empty;
            if (!finalTypes.Contains(obj.GetType()))
                return false;
            if (cultureInfos.ContainsKey(obj.GetType()) && obj is IFormattable)
                pushValue = (obj as IFormattable).ToString(null,cultureInfos[obj.GetType()]);
            else
                pushValue = obj.ToString();            
            return true;
        }
        private string TrimmProperty(string str, PropertyInfo propertyInfo)
        {
            if (trimedParams.ContainsKey(propertyInfo.Name))
                str = string.Join("", str.Take(trimedParams[propertyInfo.Name]));
            return str;
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
           