using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetConfig.TypeSerializers);
        }

        public PropertyConfigMember<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression) throw new ArgumentException();

            return new PropertyConfigMember<TOwner, TPropType>(this, GetConfig.PropertySerializers,
                memberExpression.Member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression) throw new ArgumentException();

            GetConfig.ExcludedProperties.Add(memberExpression.Member);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            GetConfig.ExcludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null) return "null" + Environment.NewLine;


            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (GetConfig.ExcludedTypes.Contains(propertyInfo.PropertyType) ||
                    GetConfig.ExcludedProperties.Contains(propertyInfo)) continue;


                if (GetFinalTypes().Contains(propertyInfo.GetValue(obj).GetType()))
                {
                    if (TryFormater(propertyInfo, obj, out var newLine))
                    {
                        sb.AppendLine(identation + propertyInfo.Name + " = " + newLine);
                    }
                    else
                    {
                        sb.AppendLine(identation + propertyInfo.Name + " = " + propertyInfo.GetValue(obj));
                    }

                    continue;
                }

                sb.Append(identation + propertyInfo.Name + " = " + PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }

            return sb.ToString();
        }

        private static Type[] GetFinalTypes()
        {
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            return finalTypes;
        }

        private bool TryFormater(PropertyInfo o, object obj, out string s)
        {
            var value = o.GetValue(obj);
            s = null;
            if (GetConfig.TypeCultures.TryGetValue(o.PropertyType, out var cultureInfo))
            {
                s = ((IFormattable)value).ToString(null, cultureInfo);
                return true;
            }

            if (GetConfig.TypeSerializers.TryGetValue(value.GetType(), out var formatter))
            {
                s = formatter(value);
                return true;
            }

            if (GetConfig.PropertySerializers.TryGetValue(o, out formatter))
            {
                s = formatter(value);

                return true;
            }

            if (GetConfig.PropertyTrim.TryGetValue(o, out var len))
            {
                s = s?[..len] ?? ((string)value)[..len];

                return true;
            }

            s = null;
            return false;
        }

        internal DataConfig GetConfig { get; } = new DataConfig();
    }
}