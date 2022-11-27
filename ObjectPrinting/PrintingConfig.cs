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
            typeof(DateTime), typeof(TimeSpan)
        };
        private HashSet<Type> excludingTypes = new HashSet<Type>();
        private HashSet<PropertyInfo> excludingPropeties = new HashSet<PropertyInfo>();
        private HashSet<object> printed= new HashSet<object>();

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
            return GetFinalString(obj, 0);
        }

        private string GetFinalString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;
            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            var identation = new string('\t', nestingLevel + 1);
            var stringBuilder = new StringBuilder();
            var type = obj.GetType();
            stringBuilder.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {

                if(excludingTypes.Contains(propertyInfo.PropertyType)|| excludingPropeties.Contains(propertyInfo))
                    continue;
                stringBuilder.Append(identation);
                stringBuilder.Append(propertyInfo.Name);
                stringBuilder.Append(" = ");
                stringBuilder.Append(GetFinalString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }

            return stringBuilder.ToString();
        }
    }
}