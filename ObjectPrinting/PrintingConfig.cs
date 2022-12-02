using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : ISerializer<TOwner>
    {
        private readonly Dictionary<string, PropertySetting<TOwner>> propertiesOptions
            = new Dictionary<string, PropertySetting<TOwner>>();
        private readonly Dictionary<string, StringSetting<TOwner>> stringOptions
            = new Dictionary<string, StringSetting<TOwner>>();

        private readonly Dictionary<Type, Func<object, string>> optionsTypes
            = new Dictionary<Type, Func<object, string>>();

        private readonly List<Type> exceptTypes
            = new List<Type>();

        private Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(bool),
            typeof(DateTime), typeof(TimeSpan)
        };

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PropertySetting<TOwner> SelectProperty<P>(Expression<Func<TOwner, P>> properties)
        {
            var memberExpression = (MemberExpression)properties.Body;
            var propertySetting = new PropertySetting<TOwner>(this);
            propertiesOptions[memberExpression.Member.Name] = propertySetting;

            return propertySetting;
        }
        public StringSetting<TOwner> SelectProperty(Expression<Func<TOwner, string>> properties)
        {
            var memberExpression = (MemberExpression)properties.Body;
            var stringSetting = new StringSetting<TOwner>(this);
            stringOptions[memberExpression.Member.Name] = stringSetting;
            propertiesOptions[memberExpression.Member.Name] = stringSetting;

            return stringSetting;
        }
        public ISerializer<TOwner> ChangeTypeOutput<T2>(Func<object, string> method)
        {
            var type = typeof(T2);
            optionsTypes.Add(type, method);

            return this;
        }

        public ISerializer<TOwner> Except<T2>()
        {
            var type = typeof(T2);
            exceptTypes.Add(type);

            return this;
        }
        public PrintingConfig<TOwner> Except<P>(Expression<Func<TOwner, P>> properties)
        {
            var memberExpression = (MemberExpression)properties.Body;
            var propertySetting = new PropertySetting<TOwner>(this, true);
            propertiesOptions[memberExpression.Member.Name] = propertySetting;

            return this;
        }
        private string PrintToString(object obj, int nestingLevel, string name = "")
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (finalTypes.Contains(obj.GetType()) || nestingLevel > 1000)
            {
                CultureInfo cultureInfo = CultureInfo.CurrentCulture;
                if (propertiesOptions.ContainsKey(name))
                    cultureInfo = propertiesOptions[name].Culture;
                return String.Format(cultureInfo, "{0}", obj) + Environment.NewLine;
            }

            if (obj is string)
            {
                int maxLength = -1;
                if (stringOptions.ContainsKey(name))
                    maxLength = stringOptions[name].MaxLength;
                if(maxLength < 0)
                    return obj + Environment.NewLine;
                return ((string)obj).Substring(0, maxLength) + Environment.NewLine;
            }

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name + ":");
            var identation = new string('\t', nestingLevel + 1);
            foreach (var propertyInfo in type.GetProperties())
            {
                var variable = propertyInfo.GetValue(obj);
                if (exceptTypes.Contains(propertyInfo.PropertyType) ||
                    propertiesOptions.ContainsKey(propertyInfo.Name) && propertiesOptions[propertyInfo.Name].IsExcept)
                    continue;

                if (propertiesOptions.ContainsKey(propertyInfo.Name) &&
                    propertiesOptions[propertyInfo.Name].OutputMethod != null)
                    sb.AppendLine(identation + propertyInfo.Name + " = " +
                                  propertiesOptions[propertyInfo.Name].OutputMethod.Invoke(variable));

                else if (optionsTypes.ContainsKey(propertyInfo.PropertyType))
                    sb.AppendLine(identation + propertyInfo.Name + " = " +
                                  optionsTypes[propertyInfo.PropertyType].Invoke(variable));
                else if (variable is IList)
                {
                    var list = (IList)variable;
                    for (int i = 0; i < list.Count; i++)
                        sb.Append(identation + i + " = " + PrintToString(list[i], nestingLevel + 1,
                            propertyInfo.Name));
                }
                else
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1,
                                  propertyInfo.Name));
                }
            }

            return sb.ToString();
        }
    }
}