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
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();

        private  Dictionary<Type, CultureInfo> culturesProperties =
            new Dictionary<Type, CultureInfo>();

        private  Dictionary<MemberInfo, Func<object, string>> memberConverters =
            new Dictionary<MemberInfo, Func<object, string>>();

        private Dictionary<Type, Func<object, string>> typeConverters =
            new Dictionary<Type, Func<object, string>>();

        public Dictionary<MemberInfo, Func<object, string>> MemberConverters
        {
            get => memberConverters;
            set => memberConverters = value;
        }

        public Dictionary<Type, Func<object, string>> TypeConverters
        {
            get => typeConverters;
            set => typeConverters = value;
        }

        public Dictionary<Type, CultureInfo> CulturesProperties
        {
            get => culturesProperties;
            set => culturesProperties = value;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var selectedProp = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(selectedProp, this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var selectedProp = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            excludedMembers.Add(selectedProp);
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
            {
                if (typeConverters.ContainsKey(obj.GetType()))
                {
                    var value = typeConverters[obj.GetType()].Invoke(obj);
                    return value + Environment.NewLine;
                }
                return obj + Environment.NewLine;
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                if (excludedMembers.Contains(propertyInfo))
                    continue;

                if (typeConverters.ContainsKey(propertyInfo.PropertyType))
                {
                    var value = typeConverters[propertyInfo.PropertyType].Invoke(propertyInfo.GetValue(obj));

                    sb.Append(identation + propertyInfo.Name + " = " + value);

                    continue;
                }

                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}