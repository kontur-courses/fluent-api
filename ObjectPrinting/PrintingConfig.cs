using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>: IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();   
        private readonly Dictionary<Type, Delegate> alternativeTypePrinting=  new Dictionary<Type, Delegate>();
        private readonly Dictionary<Type, CultureInfo> culturesForPrinting = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<string, Delegate> alternativePropertyPrinting = new Dictionary<string, Delegate>();
        private readonly HashSet<string> excludedProperties = new HashSet<string>();
        private readonly int maxNestingLevel = 10; 

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        public PrintingConfig<TOwner> Excluding<TPropType>()
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
            
            var propertyInfo = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            var propertyName = propertyInfo.Name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
        }

        public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            var propertyName = propertyInfo.Name;
            excludedProperties.Add(propertyName);
            return this;
        }


        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (nestingLevel == maxNestingLevel)
                return "Max Nesting Level" + Environment.NewLine;

            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (finalTypes.Contains(type))
            {
                if (culturesForPrinting.ContainsKey(type))
                    return Convert.ToString(obj, culturesForPrinting[type]) + Environment.NewLine;
                return obj + Environment.NewLine;
            }

            var builder = new StringBuilder();
            builder.AppendLine(type.Name);
            
            foreach (var propertyPrintInfo in GetAllPropertiesAndItems(obj))
                AppendItemInRightForm(builder, propertyPrintInfo, nestingLevel);
            return builder.ToString();
        }

        private void AppendItemInRightForm(StringBuilder builder, PropertyPrintInfo info, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var start = identation + info.Definition;
            if (excludedTypes.Contains(info.ItemType))
                return;
            if (excludedProperties.Contains(info.Name) && nestingLevel == 0)
                return;
            if (alternativePropertyPrinting.ContainsKey(info.Name) && nestingLevel == 0)
                builder.Append(start + PrintToString(alternativePropertyPrinting[info.Name].DynamicInvoke(info.Item), nestingLevel + 1));
            else if (alternativeTypePrinting.ContainsKey(info.ItemType))
                builder.Append(start + PrintToString(alternativeTypePrinting[info.ItemType].DynamicInvoke(info.Item), nestingLevel + 1));
            else
                builder.Append(start + PrintToString(info.Item, nestingLevel + 1));
        }

        private IEnumerable<PropertyPrintInfo> GetAllPropertiesAndItems(object obj)
        {
            if (obj is IEnumerable)
            {
                var collection = (IEnumerable)obj;
                foreach (var item in collection)
                    yield return new PropertyPrintInfo(item, item.GetType());
            }
            else
            {
                var type = obj.GetType();
                foreach (var propertyInfo in type.GetProperties())
                    yield return new PropertyPrintInfo(propertyInfo.GetValue(obj),propertyInfo.PropertyType,
                        propertyInfo.Name, propertyInfo.Name + " = ");
            }
        }

        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.AlternativeTypePrinting => alternativeTypePrinting;

        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.CulturesForPrinting => culturesForPrinting;

        Dictionary<string, Delegate> IPrintingConfig<TOwner>.AlternativePropertyPrinting => alternativePropertyPrinting;
    }

    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Delegate> AlternativeTypePrinting { get; }

        Dictionary<Type, CultureInfo> CulturesForPrinting { get; }

        Dictionary<string, Delegate> AlternativePropertyPrinting { get; }
    }
}