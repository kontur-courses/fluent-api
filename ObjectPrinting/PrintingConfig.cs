using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly List<Delegate> excludedFields = new List<Delegate>();
        public readonly Dictionary<Type, CultureInfo> Cultures = new Dictionary<Type, CultureInfo>();
        public readonly Dictionary<Type, Delegate> Serializations = new Dictionary<Type, Delegate>();
        public readonly Dictionary<Delegate, int> TrimForStringProperties = new Dictionary<Delegate, int>();

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedFields.Add(memberSelector.Compile());
            return this;
        }
        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector.Compile());
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }
        
        private readonly Type[] finalTypes =  new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private string GetNumberWithCultureOrNull(object obj)
        {
            if (Cultures.TryGetValue(obj.GetType(), out var culture))
            {
                switch (obj)
                {
                    case int i:
                        return i.ToString(culture) + Environment.NewLine;
                    case double d:
                        return d.ToString(culture) + Environment.NewLine;
                    case float f:
                        return f.ToString(culture) + Environment.NewLine;
                }
            }

            return null;
        }

        private bool IsExcludedField(object obj, PropertyInfo propertyInfo)
        {
            foreach (var func in excludedFields)
            {
                if (func.DynamicInvoke(obj) == propertyInfo.GetValue(obj))
                {
                    return true;
                }
            }

            return false;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            
            if (Serializations.TryGetValue(obj.GetType(), out var func))
            {
                return func.DynamicInvoke(obj) + Environment.NewLine;
            }
            
            if (finalTypes.Contains(obj.GetType()))
            {
                return GetNumberWithCultureOrNull(obj) ?? obj + Environment.NewLine;
            }

            return PrintObject(obj, nestingLevel);
        }

        private bool TryAddSpecialSerialization(Object obj, PropertyInfo propertyInfo, StringBuilder sb, string identation)
        {
            if (propertyInfo.PropertyType == typeof(string))
            {
                foreach (var memberSelector in TrimForStringProperties.Keys)
                {
                    if (memberSelector.DynamicInvoke(obj) == propertyInfo.GetValue(obj))
                    {
                        sb.Append(identation + propertyInfo.Name + " = ");
                        sb.Append(propertyInfo.GetValue(obj)?.ToString()
                            ?.Substring(0, TrimForStringProperties[memberSelector]));
                        sb.Append(Environment.NewLine);
                        return true;
                    }
                }
            }

            return false;
        }

        private string PrintObject(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType))
                {
                    continue;
                }

                if (IsExcludedField(obj, propertyInfo))
                {
                    continue;
                }

                if (TryAddSpecialSerialization(obj, propertyInfo, sb, identation))
                {
                    continue;
                }
                
                sb.Append(identation + propertyInfo.Name + " = ");
                sb.Append(PrintToString(propertyInfo.GetValue(obj),nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}