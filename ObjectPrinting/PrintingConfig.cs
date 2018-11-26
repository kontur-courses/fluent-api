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
        private readonly List<Type> typesToBeExcluded = new List<Type>();
        private readonly List<string> propertiesToBeExcluded = new List<string>();

        public Dictionary<Type, Delegate> TypesToBeAlternativelySerialized = new Dictionary<Type, Delegate>();

        public Dictionary<Type, CultureInfo> NumericTypesToBeAlternativelySerializedUsingCultureInfo = new Dictionary<Type, CultureInfo>();

        
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
            var memberExpression = (MemberExpression)memberSelector.Body;
            propertiesToBeExcluded.Add(memberExpression.Member.Name);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToBeExcluded.Add(typeof(TPropType));

            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();

            sb.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties())
            {
                var propType = propertyInfo.PropertyType;

                if (!typesToBeExcluded.Contains(propType)
                    && !propertiesToBeExcluded.Contains(propertyInfo.Name))
                {
                    var value = propertyInfo.GetValue(obj);

                    if (TypesToBeAlternativelySerialized.ContainsKey(propType))
                        value = TypesToBeAlternativelySerialized[propType].DynamicInvoke(value);

                    if (NumericTypesToBeAlternativelySerializedUsingCultureInfo.ContainsKey(propType))
                        value = Convert.ToString(value, NumericTypesToBeAlternativelySerializedUsingCultureInfo[propType]);

                    sb.Append(indentation + propertyInfo.Name + " = " +
                          PrintToString(value,
                              nestingLevel + 1));
                }

                    
            }
            return sb.ToString();
        }
    }

}