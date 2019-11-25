using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private Dictionary<Type, Func<Type, string>> cultureRules = new Dictionary<Type, Func<Type, string>>();
        private HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();

        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        private Dictionary<PropertyInfo, Func<object, string>> propertyRules =
            new Dictionary<PropertyInfo, Func<object, string>>();

        private Dictionary<Type, Func<Type, string>> typeRules = new Dictionary<Type, Func<Type, string>>();

        //protected void addPropertyRule(Func<object, string> rule)
        //{
        //}

        //protected void addTypeRule(Func<object, string> rule)
        //{
        //}

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }


        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PropertySerializationConfig<TOwner, TPropType> For<TPropType>(Expression<Func<TOwner, TPropType>> propType)
        {
            return new PropertySerializationConfig<TOwner, TPropType>(this);
        }

        public PropertySerializationConfig<TOwner, TPropType> For<TPropType>()
        {
            return new PropertySerializationConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> func)
        {
            var propInfo = ((MemberExpression)func.Body).Member as PropertyInfo;

            return this;
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
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            return sb.ToString();
        }
    }
}