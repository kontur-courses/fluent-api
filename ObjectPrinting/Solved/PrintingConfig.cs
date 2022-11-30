using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> _excludedTypes;
        private readonly List<MemberInfo> _excludedMembers;
        private readonly Dictionary<string, Func<object, string>> _propertiesPrintSettings;
        private readonly Dictionary<Type, Func<object, string>> _typesPrintSettings;
        private List<object> _printedObjects;

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

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
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
            _printedObjects = new List<object>();
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            _printedObjects.Add(obj);
            if (obj == null)
                return "null" + Environment.NewLine;
            
            if (obj is ICollection collection)
                return GenerateStringFromCollection(collection);

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
                if (!IsValidMember(obj, propertyInfo))
                    continue;
                
                string toPrint = GetPrintMember(obj, propertyInfo, nestingLevel);
                sb.Append(identation + propertyInfo.Name + " = " + toPrint);
            }

            return sb.ToString();
        }
        
        
        private string GenerateStringFromCollection(ICollection collection)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            foreach (var element in collection)
                sb.Append($" {element}");
            sb.Append(" ]");
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        private bool IsValidMember(object obj, PropertyInfo propertyInfo)
        {
            if (_excludedTypes.Contains(propertyInfo.PropertyType))
                return false;
            if(_excludedMembers.Contains(propertyInfo))
                return false;
            return !_printedObjects.Contains(propertyInfo.GetValue(obj));
        }
        
        private string GetPrintMember(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            if (_typesPrintSettings.ContainsKey(propertyInfo.PropertyType))
                return _typesPrintSettings[propertyInfo.PropertyType](propertyInfo.GetValue(obj))
                        + Environment.NewLine;
            if (_propertiesPrintSettings.ContainsKey(propertyInfo.Name))
                return _propertiesPrintSettings[propertyInfo.Name](propertyInfo)
                                           + Environment.NewLine;
            return PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
        }
    }
}