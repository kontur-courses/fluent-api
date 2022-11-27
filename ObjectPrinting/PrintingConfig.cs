using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> FinalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan),typeof(bool), typeof(decimal),
        };
        private HashSet<Type> excludingTypes = new HashSet<Type>();
        private HashSet<PropertyInfo> excludingPropeties = new HashSet<PropertyInfo>();
        //private HashSet<object> printed= new HashSet<object>();
        public readonly Dictionary<Type, Delegate> typesForPrintWithSpec = new Dictionary<Type, Delegate>();
        public readonly Dictionary<PropertyInfo, int> PropertyLenForString=new Dictionary<PropertyInfo, int>();
        public PropertyConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyConfig<TOwner, TPropType>(this);
        }
 
        public PropertyConfig<TOwner,TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = GetPropertyInfoFromExpression(memberSelector);
            return new PropertyConfig<TOwner, TPropType>(this, propertyInfo);
        }
        private static PropertyInfo GetPropertyInfoFromExpression<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludingPropeties.Add(GetPropertyInfoFromExpression(memberSelector));
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;    
        }

        public string PrintToString(TOwner obj)
        {
            if (obj is null)
                return "null" + Environment.NewLine;
            return GetValueString(obj, 0);
        }

        private string GetValueString(object obj, int nestingLevel,PropertyInfo property=null)
        {
            if (ReturnDefaultString(obj,property,out var valueString)) 
                return valueString;
            var stringBuilder = new StringBuilder();
            var type = obj.GetType();
            stringBuilder.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if(excludingTypes.Contains(propertyInfo.PropertyType) || excludingPropeties.Contains(propertyInfo))
                    continue;
                var value = GetValueString(propertyInfo.GetValue(obj),nestingLevel + 1, propertyInfo);
                if (string.IsNullOrEmpty(value)) continue;
                stringBuilder.Append(new string('\t', nestingLevel+1));
                stringBuilder.Append(propertyInfo.Name);
                stringBuilder.Append(" = ");
                stringBuilder.Append(value);
            }
            return stringBuilder.ToString();
        }

        private bool ReturnDefaultString(object obj, PropertyInfo property, out string valueString)
        {
            valueString = null;
            if (obj is null) 
                return true;
            var objType = obj.GetType();
            if (!FinalTypes.Contains(objType)) 
                return false;
            var isTrimmed = TrimString(obj, property, ref valueString);
            valueString= GetString(obj, valueString, objType, isTrimmed);
            valueString += Environment.NewLine;
            return true;
        }

        private string GetString(object obj, string valueString, Type objType, bool isTrimmed)
        {
            if (typesForPrintWithSpec.ContainsKey(objType))
            {
                if (isTrimmed)
                    valueString = (string)typesForPrintWithSpec[objType].DynamicInvoke(valueString);
                else
                    valueString = (string)typesForPrintWithSpec[objType].DynamicInvoke(obj);
            }
            else
                valueString = obj.ToString();
            return valueString;
        }

        private bool TrimString(object obj, PropertyInfo property, ref string valueString)
        {
            if (property == null || !PropertyLenForString.ContainsKey(property) || !(obj is string s) ||
                s.Length <= PropertyLenForString[property]) return false;
            valueString = s.Substring(0, PropertyLenForString[property]);
            return true;
        }
    }
}