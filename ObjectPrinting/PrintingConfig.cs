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
        private readonly HashSet<MemberInfo> excludingMembers;

        private readonly HashSet<Type> excludingTypes;

        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig()
        {
            AlternativePrintForMembers = new Dictionary<MemberInfo, Delegate>();
            MaxStringLength = new Dictionary<MemberInfo, int>();
            excludingMembers = new HashSet<MemberInfo>();
            excludingTypes = new HashSet<Type>();
            AlternativePrintingForTypes = new Dictionary<Type, Delegate>();
            Cultures = new Dictionary<Type, CultureInfo>();
        }

        public Dictionary<MemberInfo, int> MaxStringLength { get; }

        public Dictionary<Type, Delegate> AlternativePrintingForTypes { get; }

        public Dictionary<MemberInfo, Delegate> AlternativePrintForMembers { get; }

        public Dictionary<Type, CultureInfo> Cultures { get; }

        public TypePrintingConfig<TOwner, TPropType> Print<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Print<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = memberSelector.Body as MemberExpression;
            var propertyInfo = member!.Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<T>()
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
            return PrintToString(obj, 0, new HashSet<object>());
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> objects)
        {
            if (objects.Count > 10)
                return obj.GetType().Name;

            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (AlternativePrintingForTypes.ContainsKey(type))
                return AlternativePrintingForTypes[obj.GetType()].DynamicInvoke(obj)!.ToString();

            if (Cultures.ContainsKey(type))
            {
                var result = obj as IFormattable;
                var culture = Cultures[type];
                return result!.ToString(null, culture);
            }

            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            if (objects.Any(o => ReferenceEquals(o, obj)))
                return type.Name;

            objects.Add(obj);

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            var properties = type.GetProperties().Where(p =>
                !excludingTypes.Contains(p.PropertyType) && !excludingMembers.Contains(p));

            var fields = type.GetFields()
                .Where(f => !excludingTypes.Contains(f.FieldType) && !excludingMembers.Contains(f) && f.IsPublic &&
                            (type.IsClass || !f.IsStatic));

            foreach (var propertyInfo in properties)
                sb.Append(indentation + propertyInfo.Name + " = " +
                          GetMemberInfoStrings(propertyInfo.GetValue(obj), propertyInfo, objects));

            foreach (var fieldsInfo in fields)
                sb.Append(indentation + fieldsInfo.Name + " = " +
                          GetMemberInfoStrings(fieldsInfo.GetValue(obj), fieldsInfo, objects));

            return sb.ToString();
        }

        public string GetMemberInfoStrings(object obj, MemberInfo memberInfo, HashSet<object> objects)
        {
            var str = "";
            if (AlternativePrintForMembers.ContainsKey(memberInfo))
                return AlternativePrintForMembers[memberInfo].DynamicInvoke(obj)! + "\r\n";
            str += PrintToString(obj, objects.Count + 1, objects);

            if (MaxStringLength.ContainsKey(memberInfo))
                str = str.Substring(0, MaxStringLength[memberInfo]) + "\r\n";

            return str;
        }
    }
}