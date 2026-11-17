using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private static readonly HashSet<Type> FinalTypes =
        [
            typeof(int), typeof(double), typeof(float),
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        ];

        private readonly HashSet<Type> excludedTypes = [];
        private readonly HashSet<MemberInfo> excludedProperties = [];

        private readonly Dictionary<Type, Func<object, string>> typeSerializers =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<MemberInfo, Func<object, string>> propertySerializers =
            new Dictionary<MemberInfo, Func<object, string>>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = ((MemberExpression)memberSelector.Body).Member;

            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = ((MemberExpression)memberSelector.Body).Member;
            excludedProperties.Add(memberInfo);

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        internal void SetSerializer<TPropType>(Func<TPropType, string> serializer, MemberInfo? memberInfo = null)
        {
            if (memberInfo != null)
                propertySerializers[memberInfo] = value => serializer((TPropType)value);
            else
                typeSerializers[typeof(TPropType)] = value => serializer((TPropType)value);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if(excludedTypes.Contains(propertyInfo.PropertyType) ||
                   excludedProperties.Contains(propertyInfo))
                    continue;

                if(typeSerializers.TryGetValue(propertyInfo.PropertyType, out var serializer) ||
                   propertySerializers.TryGetValue(propertyInfo, out serializer))
                {
                    sb.Append(identation
                              + propertyInfo.Name + " = "
                              + serializer(propertyInfo.GetValue(obj))
                              + Environment.NewLine);
                    continue;
                }

                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}