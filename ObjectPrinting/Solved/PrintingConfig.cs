using System;
using System.Collections;
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
        private readonly HashSet<object> visitedElement = [];

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
                    value == null) continue;

                if (!visitedElement.Add(value))
                {
                    sb.AppendLine(identation + "here Cycle");
                    break;
                }

                if (GetFinalTypes().Contains(value.GetType()))
                {
                    if (TryFormater(propertyInfo, value, out var newLine))
                    {
                        sb.AppendLine(identation + propertyInfo.Name + " = " + newLine);
                    }

                    else
                    {
                        sb.AppendLine(identation + propertyInfo.Name + " = " + value);
                    }

                    continue;
                }

                if (value is IDictionary dictionary)
                {
                    WriteDict(dictionary, nestingLevel, propertyInfo, sb, identation);
                    continue;
                }

                if (value is IEnumerable vEnum)
                {
                    WriteArrayElements(vEnum, nestingLevel, propertyInfo, sb, identation);
                    continue;
                }


                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }

            return sb.ToString();
        }

        private void WriteDict(IDictionary dictionary, int nestingLevel, PropertyInfo propertyInfo,
            StringBuilder sb, string identation)
        {
            sb.AppendLine(identation + propertyInfo.Name + " {");
            var identationInArray = identation + '\t';

            foreach (DictionaryEntry o in dictionary)
            {
                if (GetFinalTypes().Contains(o.Value.GetType()))
                {
                    if (TryFormater(propertyInfo, o.Value, out var newLine))
                    {
                        sb.AppendLine($"{identationInArray} {o.Key} = {newLine}");
                    }
                    else
                    {
                        sb.AppendLine($"{identationInArray} {o.Key} = {o.Value}");
                    }
                }

                else
                {
                    sb.Append(identationInArray + PrintToString(o.Value, nestingLevel + 2));
                }
            }

            sb.AppendLine(identation + "} ");
        }

        private void WriteArrayElements(IEnumerable enumerable, int nestingLevel, PropertyInfo propertyInfo,
            StringBuilder sb,
            string identation)
        {
            sb.AppendLine(identation + propertyInfo.Name + " [");
            var identationInArray = identation + '\t';
            foreach (var element in enumerable) // у нас element может быть сложныс классом
            {
                if (TryAddAsPrimitiveType(propertyInfo, sb, element, identationInArray)) continue;

                sb.Append(identationInArray + PrintToString(element, nestingLevel + 2));
            }


            sb.AppendLine(identation + "]");
        }

        private bool TryAddAsPrimitiveType(PropertyInfo propertyInfo, StringBuilder sb, object element,
            string identationInArray)
        {
            if (GetFinalTypes().Contains(element.GetType()))
            {
                if (TryFormater(propertyInfo, element, out var newLine))
                {
                    sb.AppendLine(identationInArray + newLine);
                }
                else
                {
                    sb.AppendLine(identationInArray + element);
                }

                return true;
            }

            return false;
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

        private bool TryFormater(PropertyInfo o, object value, out string s)
        {
            s = null;
            var changed = false;
            if (GetConfig.TypeCultures.TryGetValue(value.GetType(), out var cultureInfo))
            {
                s = ((IFormattable)value).ToString(null, cultureInfo);
                changed = true;
            }

            if (GetConfig.TypeSerializers.TryGetValue(value.GetType(), out var formatter))
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