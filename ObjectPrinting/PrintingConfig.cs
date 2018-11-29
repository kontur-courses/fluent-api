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
        private readonly List<Type> typesToBeExcluded = new List<Type>();
        private readonly List<string> propertiesToBeExcluded = new List<string>();

        private readonly Dictionary<Type, Delegate> typesToBeAlternativelySerialized = new Dictionary<Type, Delegate>();

        private readonly Dictionary<string, Delegate> propertiesToBeAlternativelySerialized = new Dictionary<string, Delegate>();

        private readonly Dictionary<Type, CultureInfo> numericTypesToBeAlternativelySerializedUsingCultureInfo
            = new Dictionary<Type, CultureInfo>();

        public void AddTypeToBeAlternativelySerialized(Type type, Delegate del)
            => typesToBeAlternativelySerialized[type] = del;

        public void AddPropertyToBeAlternativelySerialized(string propertyName, Delegate del)
            => propertiesToBeAlternativelySerialized[propertyName] = del;

        public void AddNumericTypeToBeAlternativelySerializedUsingCultureInfo(Type type, CultureInfo cultureInfo)
            => numericTypesToBeAlternativelySerializedUsingCultureInfo[type] = cultureInfo;

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpression))
                throw new ArgumentException("»спользованное выражение не €вл€етс€ допустимым");
            
            var propName = memberExpression.Member;

            return new PropertyPrintingConfig<TOwner, TPropType>(this, (PropertyInfo)propName);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if(!(memberSelector.Body is MemberExpression memberExpression))
                throw new ArgumentException("»спользованное выражение не €вл€етс€ допустимым");

            propertiesToBeExcluded.Add(memberExpression.Member.Name);

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToBeExcluded.Add(typeof(TPropType));

            return this;
        }

        public string PrintToString(TOwner obj, char indentSymbol = '\t', int maxNestingLevel = 10,
            HashSet<MemberTypes> requiredMemberTypes = null)
        {
            return PrintToString(obj, 0, indentSymbol, maxNestingLevel,
                requiredMemberTypes ?? new HashSet<MemberTypes> { MemberTypes.Field, MemberTypes.Property });
        }

        private string PrintToString(object obj, int nestingLevel, char indentSymbol,
            int maxNestingLevel, HashSet<MemberTypes> requiredMemberTypes)
        {
            if (nestingLevel >= maxNestingLevel)
                throw new OverflowException("ѕревышен максимальный уровень вложенности");

            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (type.IsSimpleType())
                return obj + Environment.NewLine;

            var indentation = new string(indentSymbol, nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);

            var members = new List<MemberInfo>();

            foreach (var memberType in requiredMemberTypes)
            {
                switch (memberType)
                {
                    case MemberTypes.Field:
                        members.AddRange(type.GetFields());
                        break;
                    case MemberTypes.Property:
                        members.AddRange(type.GetProperties());
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            foreach (var memberInfo in members)
            {
                var propType = memberInfo.GetUnderlyingType();

                var propName = memberInfo.Name;

                if (!typesToBeExcluded.Contains(propType)
                    && !propertiesToBeExcluded.Contains(propName))
                {
                    var value = memberInfo.GetValue(obj);

                    if (typesToBeAlternativelySerialized.ContainsKey(propType)
                        && !propertiesToBeAlternativelySerialized.ContainsKey(propName))
                        value = typesToBeAlternativelySerialized[propType].DynamicInvoke(value);

                    if (numericTypesToBeAlternativelySerializedUsingCultureInfo.ContainsKey(propType))
                        value = Convert.ToString(value,
                            numericTypesToBeAlternativelySerializedUsingCultureInfo[propType]);

                    if (propertiesToBeAlternativelySerialized.ContainsKey(propName))
                        value = propertiesToBeAlternativelySerialized[propName].DynamicInvoke(value);

                    sb.Append(indentation + propName + " = " +
                              PrintToString(value, nestingLevel + 1, indentSymbol, maxNestingLevel, requiredMemberTypes));
                }
            }

            return sb.ToString();
        }
    }
}