using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new ();

        private readonly HashSet<MemberInfo> excludedMembers = new ();

        private HashSet<Type> finalTypes = new ()
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan),
        };

        private Dictionary<Type, CultureInfo> culturesProperties =
            new ();

        private Dictionary<MemberInfo, Func<object, string>> memberConverters =
            new ();

        private Dictionary<Type, Func<object, string>> typeConverters =
            new ();

        public Dictionary<MemberInfo, Func<object, string>> MemberConverters
        {
            get => memberConverters;
            set => memberConverters = value;
        }

        public Dictionary<Type, Func<object, string>> TypeConverters
        {
            get => typeConverters;
            set => typeConverters = value;
        }

        public Dictionary<Type, CultureInfo> CulturesProperties
        {
            get => culturesProperties;
            set => culturesProperties = value;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var selectedProp = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(selectedProp, this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var selectedProp = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            excludedMembers.Add(selectedProp);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new HashSet<object>());
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> objectsCashe)
        {
            if (obj == null)
            {
                return "null" + Environment.NewLine;
            }

            if (objectsCashe.Contains(obj))
            {
                return "circular references" + Environment.NewLine;
            }

            if (finalTypes.Contains(obj.GetType()) || excludedTypes.Contains(obj.GetType()))
            {
                return SerializeFinalTypes(obj);
            }

            objectsCashe.Add(obj);
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedMembers.Contains(propertyInfo) || excludedTypes.Contains(propertyInfo.PropertyType))
                {
                    continue;
                }

                var value = SerializeType(obj) ?? SerializeMember(propertyInfo, obj);

                if (!string.IsNullOrEmpty(value))
                {
                    sb.Append(identation + propertyInfo.Name + " = " + value);
                    sb.Append(Environment.NewLine);
                    continue;
                }

                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(
                              propertyInfo.GetValue(obj),
                              nestingLevel + 1, objectsCashe));
            }

            return sb.ToString();
        }

        private string SerializeFinalTypes(object obj)
        {
            if (typeConverters.ContainsKey(obj.GetType()))
            {
                var value = typeConverters[obj.GetType()].Invoke(obj);
                return value + Environment.NewLine;
            }

            return obj + Environment.NewLine;
        }

        private string SerializeType(object obj)
        {
            if (typeConverters.ContainsKey(obj.GetType()))
            {
                return typeConverters[obj.GetType()].Invoke(obj);
            }

            return null;
        }

        private string SerializeMember(PropertyInfo propertyInfo, object obj)
        {
            if (memberConverters.ContainsKey(propertyInfo))
            {
                return memberConverters[propertyInfo].Invoke(propertyInfo.GetValue(obj));
            }

            return null;
        }
    }
}