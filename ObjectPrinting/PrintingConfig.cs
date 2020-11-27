using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly List<string> excludedFields = new List<string>();
        private readonly Dictionary<Type, CultureInfo> cultures = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<Type, Delegate> serializationsForType = new Dictionary<Type, Delegate>();
        private readonly Dictionary<string, Delegate> serializationForProperty = new Dictionary<string, Delegate>();
        private TOwner startObject;

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedFields.Add(((MemberExpression)memberSelector.Body).Member.Name);
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
            return new PropertyPrintingConfig<TOwner, TPropType>(this,((MemberExpression) memberSelector.Body).Member.Name);
        }

        public string PrintToString(TOwner obj)
        {
            startObject = obj;
            return obj is ICollection collection ? PrintToStringICollectable(collection) : PrintToString(obj, 0);
        }
        
        public void AddSerializationForType<TPropType>(Type type, Func<TPropType, string> serializaton)
        {
            serializationsForType[type] = serializaton;
        }

        public void AddSerializationForProperty<TPropType>(string propertyName, Func<TPropType, string> serialization)
        {
            serializationForProperty[propertyName] = serialization;
        }

        public void AddCultureForMember(Type type, CultureInfo culture)
        {
            cultures[type] = culture;
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

            if (finalTypes.Contains(obj.GetType()))
            {
                return GetMemberWithCultureOrNull(obj) ?? obj + Environment.NewLine;
            }

            return PrintObject(obj, nestingLevel);
        }

        private string PrintObject(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                PrintMember(propertyInfo.Name, propertyInfo.GetValue(obj),propertyInfo.PropertyType,
                    obj, sb, nestingLevel);
            }

            foreach (var fieldInfo in type.GetFields())
            {
                PrintMember(fieldInfo.Name, fieldInfo.GetValue(obj), fieldInfo.FieldType,
                    obj, sb, nestingLevel);
            }

            return sb.ToString();
        }

        private void PrintMember(string name, object value, Type type, object parent, StringBuilder sb, int nestingLevel)
        {
            if (excludedTypes.Contains(type) || (parent is TOwner && excludedFields.Contains(name)))
            {
                return;
            }
            
            var identation = new string('\t', nestingLevel + 1);
            sb.Append(identation + name + " = ");
            
            if (parent is TOwner && serializationForProperty.TryGetValue(name, out var serializator))
            {
                sb.Append(serializator.DynamicInvoke(value) + Environment.NewLine);
                return;
            }
            
            if (serializationsForType.TryGetValue(type, out var func))
            {
                sb.Append(func.DynamicInvoke(value) + Environment.NewLine);
                return;
            }

            sb.Append(PrintToString(value, nestingLevel + 1));
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

        private string GetMemberWithCultureOrNull(object obj)
        {
            if (cultures.TryGetValue(obj.GetType(), out var culture))
            {
                return String.Format(culture, "{0}", obj);
            }

            return null;
        }
    }
}