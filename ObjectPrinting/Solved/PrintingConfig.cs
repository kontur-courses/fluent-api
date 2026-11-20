using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private HashSet<Type> _excludeTypes = new();
        private HashSet<PropertyInfo> _excludeProperties = new();
        private Dictionary<Type, Func<object, string>> _typeSerializersDict = new();
        private Dictionary<PropertyInfo, Func<object, string>> _propertySerializersDict = new();

        //serializeType
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        //serializeProp
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        //excludeProperty
        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            _excludeProperties.Add(propertyInfo);
            return this;
        }

        //excludeType
        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _excludeTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public void SetTypeSerialize<TPropType>(Func<TPropType, string> print)
        {
            _typeSerializersDict[typeof(TPropType)] = x => print((TPropType)x);
        }

        public void SetPropertySerialize<TPropType>(PropertyInfo property, Func<TPropType, string> print)
        {
            _propertySerializersDict[property] = x => print((TPropType)x);
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
                if (_excludeTypes.Contains(propertyInfo.PropertyType)) continue;

                if (_excludeProperties.Contains(propertyInfo)) continue;

                if (_typeSerializersDict.TryGetValue(propertyInfo.PropertyType, out var typeSerializer))
                {
                    sb.Append(
                        identation +
                        propertyInfo.Name +
                        " = " +
                        typeSerializer(propertyInfo.GetValue(obj)));
                    continue;
                }

                if (_propertySerializersDict.TryGetValue(propertyInfo, out var propertySerializer))
                {
                    sb.Append(
                        identation +
                        propertyInfo.Name +
                        " = " +
                        propertySerializer(propertyInfo.GetValue(obj)));
                    continue;
                }

                sb.Append(
                    identation + 
                    propertyInfo.Name + 
                    " = " +
                    PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}