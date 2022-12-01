using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<Type> excludingTypes;
        private readonly HashSet<MemberInfo> excludingMembers;
        public Dictionary<Type, Delegate> AlternativePrintingForTypes { get; }
        public Dictionary<MemberInfo, Delegate> AlternativePrintForProperty { get; }
        public Dictionary<Type, CultureInfo> Cultures { get; }
        public Dictionary<MemberInfo, int> MaxStringLength;
        public PrintingConfig()
        {
            AlternativePrintForProperty = new Dictionary<MemberInfo, Delegate>();
            MaxStringLength = new Dictionary<MemberInfo, int>();
            excludingMembers = new HashSet<MemberInfo>();
            excludingTypes = new HashSet<Type>();
            AlternativePrintingForTypes = new Dictionary<Type, Delegate>();
            Cultures = new Dictionary<Type, CultureInfo>();
        }

        internal TypePrintingConfig<TOwner, TPropType> Print<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Print<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = memberSelector.Body as MemberExpression;
            var propertyInfo = member!.Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        internal PrintingConfig<TOwner> Excluding<T>()
        {
            excludingTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = memberSelector.Body as MemberExpression;
            var memberInfo = member!.Member;
            excludingMembers.Add(memberInfo);
            return this;
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

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            var properties = type.GetProperties().Where(p =>
                !excludingTypes.Contains(p.PropertyType) || !excludingMembers.Contains(p));

            foreach (var propertyInfo in properties)
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}