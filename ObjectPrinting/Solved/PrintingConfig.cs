using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> _excludedTypes;
        private readonly List<MemberInfo> _excludedMembers;
        private readonly Dictionary<string, Func<object, string>> _propertiesPrintSettings;
        private readonly Dictionary<Type, Func<object, string>> _typesPrintSettings;
        public PrintingConfig()
        {
            _excludedTypes = new List<Type>();
            _excludedMembers = new List<MemberInfo>();
            _propertiesPrintSettings = new Dictionary<string, Func<object, string>>();
            _typesPrintSettings = new Dictionary<Type, Func<object, string>>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var type = typeof(TPropType);
            var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            _typesPrintSettings.Add(type, propertyConfig.UseSettings);
            return propertyConfig;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpression))
                throw new ArgumentException();

            var memberName = memberExpression.Member.Name;
            var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this); 
            _propertiesPrintSettings.Add(memberName, propertyConfig.UseSettings);
            return propertyConfig;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpression))
                throw new ArgumentException();
            
            _excludedMembers.Add(memberExpression.Member);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _excludedTypes.Add(typeof(TPropType));
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

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if(_excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if(_excludedMembers.Contains(propertyInfo))
                    continue;

                string toPrint;
                if (_typesPrintSettings.ContainsKey(propertyInfo.PropertyType))
                    toPrint = _typesPrintSettings[propertyInfo.PropertyType](propertyInfo.GetValue(obj)) 
                              + Environment.NewLine;
                else if (_propertiesPrintSettings.ContainsKey(propertyInfo.Name))
                    toPrint = _propertiesPrintSettings[propertyInfo.Name](propertyInfo)
                              + Environment.NewLine;
                else
                    toPrint = PrintToString(propertyInfo.GetValue(obj),
                            nestingLevel + 1);
                sb.Append(identation + propertyInfo.Name + " = " + toPrint);
            }
            return sb.ToString();
        }
    }
}