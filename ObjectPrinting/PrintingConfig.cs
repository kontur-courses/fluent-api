using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
        public PropertyConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyConfig<TOwner, TPropType>(this);
        }

        public PropertyConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludingPropeties.Add(((MemberExpression)memberSelector.Body).Member as PropertyInfo);
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

        private string GetValueString(object obj, int nestingLevel)
        {
            if (ReturnDefaultString(obj, out var valueString)) 
                return valueString;
            var stringBuilder = new StringBuilder();
            var type = obj.GetType();
            stringBuilder.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if(excludingTypes.Contains(propertyInfo.PropertyType) || excludingPropeties.Contains(propertyInfo))
                    continue;
                var value = GetValueString(propertyInfo.GetValue(obj),nestingLevel + 1);
                if (string.IsNullOrEmpty(value)) continue;
                stringBuilder.Append(new string('\t', nestingLevel+1));
                stringBuilder.Append(propertyInfo.Name);
                stringBuilder.Append(" = ");
                stringBuilder.Append(value);
            }
            return stringBuilder.ToString();
        }

        private bool ReturnDefaultString(object obj, out string valueString)
        {
            valueString = null;
            if (obj is null) 
                return true;

            var objType = obj.GetType();
            if (!FinalTypes.Contains(objType)) 
                return false;
            if (typesForPrintWithSpec.ContainsKey(objType))
                valueString = (string)typesForPrintWithSpec[objType].DynamicInvoke(obj) + Environment.NewLine;
            else 
                valueString = obj + Environment.NewLine;
            return true;
        }
    }
}