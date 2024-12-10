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
        private readonly HashSet<object> visitedHouse = [];

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
                var value = propertyInfo.GetValue(obj);
                if (GetConfig.ExcludedTypes.Contains(propertyInfo.PropertyType) ||
                    GetConfig.ExcludedProperties.Contains(propertyInfo) ||
                    value == null || !visitedHouse.Add(value)) continue;


                if (GetFinalTypes().Contains(value.GetType()))
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

                if (propertyInfo.PropertyType.IsArray)
                {
                    WriteArrayElements(obj, nestingLevel, propertyInfo, sb, identation);
                    continue;
                }


                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }

            return sb.ToString();
        }

        private void WriteArrayElements(object obj, int nestingLevel, PropertyInfo propertyInfo, StringBuilder sb,
            string identation)
        {
            var property = propertyInfo.PropertyType.GetProperty("Length");
            var datArray = propertyInfo.GetValue(obj);
            var lenght = (int)property.GetValue(datArray);


            sb.AppendLine(identation + propertyInfo.Name + " [");
            var identationInArray = identation + '\t';
            for (int i = 0; i < lenght; i++)
            {
                var data = propertyInfo.PropertyType.GetMethod("GetValue", [typeof(int)])!
                    .Invoke(datArray, [i]);

                if (data == null) continue;

                sb.Append(identationInArray + PrintToString(data, nestingLevel + 2));
            }


            sb.AppendLine(identation + "]");
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
            s = null;
            var changed = false;
            var value = o.GetValue(obj);
            s = null;
            if (GetConfig.TypeCultures.TryGetValue(o.PropertyType, out var cultureInfo))
            {
                s = ((IFormattable)value).ToString(null, cultureInfo);
                changed = true;
            }

            if (GetConfig.TypeSerializers.TryGetValue(o.PropertyType, out var formatter))
            {
                s = formatter(s ?? value);
                changed = true;
            }

            if (GetConfig.PropertyTrim.TryGetValue(o, out var len))
            {
                s = s?[..len] ?? ((string)value)[..len];

                changed = true;
            }

            if (GetConfig.PropertySerializers.TryGetValue(o, out formatter))
            {
                s = formatter(s ?? value);

                changed = true;
            }


            return changed;
        }

        internal DataConfig GetConfig { get; } = new DataConfig();
    }
}