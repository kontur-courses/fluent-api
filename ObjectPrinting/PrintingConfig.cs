using System;
using System.Collections;
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
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();

        private readonly Dictionary<Type, Func<object, string>> typePrintingRules =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<MemberInfo, Func<object, string>> memberPrintingRules =
            new Dictionary<MemberInfo, Func<object, string>>();

        private readonly Dictionary<Type, CultureInfo> typeCultures = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<MemberInfo, CultureInfo> memberCultures = new Dictionary<MemberInfo, CultureInfo>();

        private readonly Dictionary<MemberInfo, Func<string, string>> memberTrimToLength =
            new Dictionary<MemberInfo, Func<string, string>>();

        private Func<string, string> globalTrimToLength = x => x;
        private MemberInfo currentMember;

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            currentMember = ((MemberExpression) memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            currentMember = ((MemberExpression) memberSelector.Body).Member;
            excludedMembers.Add(currentMember);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }


        public void SetPrintingRule<TPropType>(Func<TPropType, string> print)
        {
            if (currentMember == null)
                typePrintingRules[typeof(TPropType)] = x => print((TPropType) x);
            else
                memberPrintingRules[currentMember] = x => print((TPropType) x);
            currentMember = null;
        }

        public void SetCulture<TPropType>(CultureInfo culture)
        {
            if (currentMember == null)
                typeCultures[typeof(TPropType)] = culture;
            else
                memberCultures[currentMember] = culture;
            currentMember = null;
        }

        public void SetMaxLength(int maxLen)
        {
            if (currentMember == null)
                globalTrimToLength = x => x.Substring(0, Math.Min(x.Length, maxLen));

            else
                memberTrimToLength[currentMember] = x => x.Substring(0, Math.Min(x.Length, maxLen));


            currentMember = null;
        }

        public string PrintToString(TOwner obj)
        {
            var serialization = new Serialization(new SerializationConfig(excludedTypes,
                excludedMembers,
                typePrintingRules,
                memberPrintingRules,
                typeCultures,
                memberCultures,
                memberTrimToLength,
                globalTrimToLength));
            return serialization.Serialize(obj);
        }
    }
}