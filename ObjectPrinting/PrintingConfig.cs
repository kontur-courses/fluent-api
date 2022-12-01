using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : ISerializer<TOwner>
    {
        private readonly Dictionary<string, PropertySetting<TOwner>> propertiesOptions
            = new Dictionary<string, PropertySetting<TOwner>>();

        private readonly Dictionary<Type, Func<object, string>> optionsTypes
            = new Dictionary<Type, Func<object, string>>();

        private readonly List<Type> exceptTypes
            = new List<Type>();

        private int stringMaxSize = -1;

        private Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float),
            typeof(DateTime), typeof(TimeSpan)
        };

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public ISerializer<TOwner> SetStringMaxSize(int length)
        {
            stringMaxSize = length;
            return this;
        }

        public ISerializer<TOwner> SetupDefaultStringSize()
        {
            stringMaxSize = -1;
            return this;
        }

        public PropertySetting<TOwner> SelectProperty<P>(Expression<Func<TOwner, P>> properties)
        {
            var memberExpression = (MemberExpression)properties.Body;
            var propertySetting = new PropertySetting<TOwner>(this);
            propertiesOptions[memberExpression.Member.Name] = propertySetting;

            return propertySetting;
        }

        public ISerializer<TOwner> ChangeTypeOutput(Type type, Func<object, string> method)
        {
            optionsTypes.Add(type, method);

            return this;
        }

        public ISerializer<TOwner> ExceptType(Type type)
        {
            exceptTypes.Add(type);

            return this;
        }

        private string PrintToString(object obj, int nestingLevel, bool isArray = false)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()) || obj is string && stringMaxSize < 0 || nestingLevel > 1000)
                return obj + "\r\n";
            if (obj is string)
                return ((string)obj).Substring(0, stringMaxSize) + "\r\n";

            var identation = new string('\t', nestingLevel);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(identation + type.Name + ":");
            identation = new string('\t', nestingLevel + 1);
            foreach (var propertyInfo in type.GetProperties())
            {
                var variable = propertyInfo.GetValue(obj);
                if (exceptTypes.Contains(propertyInfo.PropertyType) ||
                    propertiesOptions.ContainsKey(propertyInfo.Name) && propertiesOptions[propertyInfo.Name].IsExcept)
                    continue;

                if (propertiesOptions.ContainsKey(propertyInfo.Name) &&
                    propertiesOptions[propertyInfo.Name].OutputMethod != null)
                    sb.AppendLine(identation + propertiesOptions[propertyInfo.Name].OutputMethod.Invoke(variable));
                
                else if (optionsTypes.ContainsKey(propertyInfo.PropertyType))
                    sb.AppendLine(identation + optionsTypes[propertyInfo.PropertyType].Invoke(variable));
                else if (variable is IList)
                {
                    var list = (IList)variable;
                    for (int i = 0; i < list.Count; i++)
                        sb.Append(identation + i + " = " + PrintToString(list[i], nestingLevel + 1));
                }
                else
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
                }
            }

            return sb.ToString();
        }
    }
}