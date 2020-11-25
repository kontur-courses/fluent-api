using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.VisualBasic;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly List<Delegate> excludedFields = new List<Delegate>();
        private readonly Dictionary<Type, CultureInfo> Cultures = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<Type, Delegate> SerializationsForType = new Dictionary<Type, Delegate>();
        private readonly Dictionary<string, Delegate> SerializationForProperty = new Dictionary<string, Delegate>();
        private TOwner startObject;

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedFields.Add(memberSelector.Compile());
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            //это пока не так работает, тут надо поправить
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector.Compile().GetMethodInfo().ReturnParameter.Name);
        }

        public string PrintToString(TOwner obj)
        {
            startObject = obj;
            return obj is ICollection collection ? PrintToStringICollectable(collection) : PrintToString(obj, 0);
        }

        public void AddSerializationForType<TPropType>(Type type, Func<TPropType, string> serializaton)
        {
            SerializationsForType[type] = serializaton;
        }

        public void AddSerializationForProperty<TPropType>(string propertyName, Func<TPropType, string> serialization)
        {
            SerializationForProperty[propertyName] = serialization;
        }

        public void AddCultureForNumber(Type type, CultureInfo culture)
        {
            Cultures[type] = culture;
        }

        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (obj is TOwner && ReferenceEquals(obj, startObject) && nestingLevel > 0)
            {
                return "circle ref" + Environment.NewLine;
            }

            if (SerializationsForType.TryGetValue(obj.GetType(), out var func))
            {
                return func.DynamicInvoke(obj) + Environment.NewLine;
            }

            if (finalTypes.Contains(obj.GetType()))
            {
                return GetNumberWithCultureOrNull(obj) ?? obj + Environment.NewLine;
            }

            return PrintObject(obj, nestingLevel);
        }

        private string PrintObject(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) || IsExcludedField(obj, propertyInfo))
                {
                    continue;
                }

                if (TryGetSerializationByProperty(obj, propertyInfo, sb, identation))
                {
                    continue;
                }

                sb.Append(identation + propertyInfo.Name + " = ");
                sb.Append(PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }

            return sb.ToString();
        }

        private string PrintToStringICollectable(IEnumerable collection)
        {
            var sb = new StringBuilder();
            var index = 0;
            foreach (var obj in collection)
            {
                sb.Append(index + ": ");
                sb.Append(PrintToString(obj, 1));
                index++;
            }

            return sb.ToString();
        }

        private bool TryGetSerializationByProperty(object obj, PropertyInfo propertyInfo, StringBuilder sb,
            string identation)
        {
            if (SerializationForProperty.ContainsKey(propertyInfo.Name))
            {
                sb.Append(identation + propertyInfo.Name + " = ");
                sb.Append(SerializationForProperty[propertyInfo.Name]
                    .DynamicInvoke(propertyInfo.GetValue(obj)));
                sb.Append(Environment.NewLine);
                return true;
            }

            return false;
        }

        private string GetNumberWithCultureOrNull(object obj)
        {
            if (Cultures.TryGetValue(obj.GetType(), out var culture))
            {
                return String.Format(culture, "{0}", obj);
            }

            return null;
        }

        private bool IsExcludedField(object obj, PropertyInfo propertyInfo)
        {
            foreach (var func in excludedFields)
            {
                if (func.DynamicInvoke(obj) == propertyInfo.GetValue(obj))
                {
                    return true;
                }
            }

            return false;
        }
    }
}