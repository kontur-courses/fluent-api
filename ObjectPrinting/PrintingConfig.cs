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
        private readonly List<Type> typesToBeExcluded = new List<Type>();
        private readonly List<string> propertiesToBeExcluded = new List<string>();

        public Dictionary<Type, Delegate> TypesToBeAlternativelySerialized = new Dictionary<Type, Delegate>();

        public Dictionary<string, Delegate> PropertiesToBeAlternativelySerialized = new Dictionary<string, Delegate>();

        public Dictionary<Type, CultureInfo> NumericTypesToBeAlternativelySerializedUsingCultureInfo = new Dictionary<Type, CultureInfo>();

        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            MemberExpression selectorBody;
            try
            {
                selectorBody = (MemberExpression)memberSelector.Body;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("»спользованное выражение не €вл€етс€ допустимым");
            }
            var propName = selectorBody.Member;

            return new PropertyPrintingConfig<TOwner, TPropType>(this, (PropertyInfo)propName);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            MemberExpression memberExpression;
            try
            {
                memberExpression = (MemberExpression)memberSelector.Body;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("»спользованное выражение не €вл€етс€ допустимым");
            }

            propertiesToBeExcluded.Add(memberExpression.Member.Name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToBeExcluded.Add(typeof(TPropType));

            return this;
        }

        public string PrintToString(TOwner obj, char indentSymbol = '\t', int maxNestingLevel = 10)
        {
            return PrintToString(obj, 0, indentSymbol, maxNestingLevel);
        }

        private string PrintToString(object obj, int nestingLevel, char indentSymbol, int maxNestingLevel)
        {
            if (nestingLevel >= maxNestingLevel)
                throw new OverflowException("ѕревышен максимальный уровень вложенности");

            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string(indentSymbol, nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();

            sb.AppendLine(type.Name);

            var members = new List<MemberInfo>();
            
            members.AddRange(type.GetFields());
            members.AddRange(type.GetProperties());

            foreach (var memberInfo in members)
            {
                var propType = memberInfo.GetUnderlyingType();

                var propName = memberInfo.Name;

                if (!typesToBeExcluded.Contains(propType)
                    && !propertiesToBeExcluded.Contains(propName))
                {
                    var value = memberInfo.GetValue(obj);

                    if (TypesToBeAlternativelySerialized.ContainsKey(propType)
                        && !PropertiesToBeAlternativelySerialized.ContainsKey(propName))
                        value = TypesToBeAlternativelySerialized[propType].DynamicInvoke(value);

                    if (NumericTypesToBeAlternativelySerializedUsingCultureInfo.ContainsKey(propType))
                        value = Convert.ToString(value,
                            NumericTypesToBeAlternativelySerializedUsingCultureInfo[propType]);

                    if (PropertiesToBeAlternativelySerialized.ContainsKey(propName))
                        value = PropertiesToBeAlternativelySerialized[propName].DynamicInvoke(value);

                    sb.Append(indentation + propName + " = " +
                              PrintToString(value, nestingLevel + 1, indentSymbol, maxNestingLevel));
                }
            }

            return sb.ToString();
        }
    }
}