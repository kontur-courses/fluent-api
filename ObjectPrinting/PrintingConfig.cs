using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;


namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : ISerializer<TOwner>
    {
        private readonly Dictionary<string, PropertySetting<TOwner>> propertiesOptions = new();

        private readonly Dictionary<string, StringSetting<TOwner>> stringOptions = new();

        private readonly Dictionary<Type, Func<object, string>> optionsTypes = new();

        private readonly HashSet<Type> exceptTypes = new();

        private readonly Type[] types =
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(bool),
            typeof(DateTime),
            typeof(TimeSpan)
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

        public ISerializer<TOwner> ChangeProperty<T2>(Func<object, string> method)
        {
            optionsTypes.Add(typeof(T2), method);
            return this;
        }

        public ISerializer<TOwner> Exclude<T2>()
        {
            exceptTypes.Add(typeof(T2));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TP>(Expression<Func<TOwner, TP>> properties)
        {
            var memberExpression = (MemberExpression)properties.Body;
            var propertySetting = new PropertySetting<TOwner>(this, true);
            propertiesOptions[memberExpression.Member.Name] = propertySetting;
            return this;
        }

        private string PrintToString(object obj, int id, string name = "")
        {
            if (obj == null)
                return "null";
            
            if (types.Contains(obj.GetType()) || id > 1)
            {
                var cultureInfo = CultureInfo.CurrentCulture;
                if (propertiesOptions.TryGetValue(name, out var option))
                    cultureInfo = option.Culture;
                return string.Format(cultureInfo, "{0}", obj);
            }

            if (obj is string s)
            {
                var maxLength = -1;
                if (stringOptions.TryGetValue(name, out var option))
                    maxLength = option.MaxLength;
                if (maxLength < 0)
                    return obj as string;
                return s[..maxLength];
            }

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.Append(type.Name + ":");
            foreach (var propertyInfo in type.GetProperties())
            {
                var variable = propertyInfo.GetValue(obj);
                if (exceptTypes.Contains(propertyInfo.PropertyType) ||
                    propertiesOptions.ContainsKey(propertyInfo.Name) &&
                    propertiesOptions[propertyInfo.Name].IsExcept)
                    continue;
                
                if (propertiesOptions.ContainsKey(propertyInfo.Name) &&
                    propertiesOptions[propertyInfo.Name].OutputMethod != null)
                    sb.Append(propertyInfo.Name + " = " +
                                  propertiesOptions[propertyInfo.Name].OutputMethod.Invoke(variable));
                
                else if (optionsTypes.TryGetValue(propertyInfo.PropertyType, out var optionsType))
                    sb.Append(propertyInfo.Name + " = " +
                                  optionsType.Invoke(variable));
                
                else if (variable is IList list)
                {
                    for (var i = 0; i < list.Count; i++)
                        sb.Append(i + " = " +
                                  PrintToString(list[i], id + 1, propertyInfo.Name));
                }

                else
                {
                    sb.Append(propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj), id + 1,
                                  propertyInfo.Name));
                }
            }
            
            return sb.ToString();
        }
    }
}