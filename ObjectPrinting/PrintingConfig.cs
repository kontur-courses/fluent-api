using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private List<string> excludedProperties = new List<string>();
        private List<Type> excludedTypes = new List<Type>();

        private Dictionary<Type, Func<object, string>>
            typeRepresentation = new Dictionary<Type, Func<object, string>>();

        private Dictionary<string, Func<object, string>> propertyRepresentation =
            new Dictionary<string, Func<object, string>>();

        private CultureInfo cultureInfo = CultureInfo.CurrentCulture;

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
            /*if (finalTypes.Contains(obj.GetType()))
                return Convert.ToString(obj, cultureInfo) + Environment.NewLine;*/

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedProperties.Contains(propertyInfo.Name)) continue;
                if (excludedTypes.Contains(propertyInfo.PropertyType)) continue;


                sb.Append(GetPropertyRepresentaion(obj, propertyInfo, identation, nestingLevel));
            }

            return sb.ToString();
        }

        private string GetPropertyRepresentaion(object obj, PropertyInfo propertyInfo, string identation,
            int nestingLevel)
        {
            if (typeRepresentation.ContainsKey(propertyInfo.PropertyType))
            {
                return identation + typeRepresentation[propertyInfo.PropertyType](propertyInfo.GetValue(obj));
            }

            if (propertyRepresentation.ContainsKey(propertyInfo.Name))
            {
                return identation + propertyRepresentation[propertyInfo.Name](propertyInfo.GetValue(obj));
            }

            return identation + propertyInfo.Name + " = " +
                   PrintToString(propertyInfo.GetValue(obj),
                       nestingLevel + 1);
        }

        /*public PrintingConfig<TOwner> Excluding(Expression<Func<TOw>>)
        {
            excludedProperties.Add(propertyName);
            return this;
        }*/

        public PrintingConfig<TOwner> Excluding(Type propertyName)
        {
            excludedTypes.Add(propertyName);
            return this;
        }

        public PrintingConfig<TOwner> SetTypeSerialization<T>(Func<T, string> func)
        {
            string Converted(object x) => func((T)x);
            typeRepresentation.Add(typeof(T), Converted);
            return this;
        }

        public PrintingConfig<TOwner> SetCultureInfo(CultureInfo cultureInfo)
        {
            this.cultureInfo = cultureInfo;
            return this;
        }

        public PrintingConfig<TOwner> SetPropertySerialization<TResult>(Func<TOwner, TResult> getProperty,
            Func<TResult, string> serializationFunc)
        {
            var propName = typeof(TResult).Name;
            var objectSerialization = serializationFunc as Func<object, string>;
            propertyRepresentation[propName] = objectSerialization;
            return this;
        }
    }
}