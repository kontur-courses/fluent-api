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
        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(string), typeof(DateTime), typeof(TimeSpan)
        };

        private readonly Dictionary<MemberInfo, Delegate> printingFunctionsForMembers =
            new Dictionary<MemberInfo, Delegate>();

        private readonly Dictionary<Type, Delegate> printingFunctionsForTypes =
            new Dictionary<Type, Delegate>();

        private readonly Dictionary<Type, CultureInfo> cultureTypes =
            new Dictionary<Type, CultureInfo>();

        public void AddCultureForType<TPropType>(CultureInfo culture)
        {
            if (typeof(IFormattable).IsAssignableFrom(typeof(TPropType)))
                cultureTypes[typeof(TPropType)] = culture;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this,
                func => printingFunctionsForTypes[typeof(TPropType)] = func);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, 
                func => printingFunctionsForMembers[member] = func);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null || obj.GetType().IsPrimitive || finalTypes.Contains(obj.GetType()))
                return GetSerializedObject(obj);

            var indentation = new string('\t', nestingLevel + 1);
            var resultString = new StringBuilder();
            var type = obj.GetType();
            resultString.AppendLine(type.Name);

            foreach (var member in type.GetProperties().Cast<MemberInfo>()
                .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                .Where(prop => prop is PropertyInfo propertyInfo && !excludingTypes.Contains(propertyInfo.PropertyType)
                               || prop is FieldInfo fieldInfo && !excludingTypes.Contains(fieldInfo.FieldType)))
                resultString.Append(indentation + member.Name + " = " + GetSerializedValue(member, obj, nestingLevel));

            return resultString.ToString();
        }

        private string GetSerializedObject(object obj)
        {
            return obj != null && cultureTypes.ContainsKey(obj.GetType())
                ? string.Format(cultureTypes[obj.GetType()], "{0}" + Environment.NewLine, obj)
                : obj + Environment.NewLine;
        }

        private string GetSerializedValue(MemberInfo member, object obj, int nestingLevel)
        {
            var memberValue = member is PropertyInfo info
                ? info.GetValue(obj)
                : ((FieldInfo)member).GetValue(obj);
            var memberType = member is PropertyInfo propertyInfo
                ? propertyInfo.PropertyType
                : ((FieldInfo)member).FieldType;

            if (printingFunctionsForMembers.ContainsKey(member))
                return printingFunctionsForMembers[member].DynamicInvoke(memberValue)?.ToString();
            if (printingFunctionsForTypes.ContainsKey(memberType))
                return printingFunctionsForTypes[memberType].DynamicInvoke(memberValue)?.ToString();
            return PrintToString(memberValue, nestingLevel + 1);
        }
    }
}