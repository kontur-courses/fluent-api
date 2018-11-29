using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Config.Member;
using ObjectPrinting.Config.Type;

namespace ObjectPrinting.Config
{
    public class PrintingConfig<TOwner>
    {
        private readonly System.Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(decimal),
            typeof(string), typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<System.Type> typesToExclude;
        private readonly Dictionary<System.Type, Func<object, string>> printingOverridedTypes;
        private readonly Dictionary<System.Type, CultureInfo> cultureOverridedTypes;

        private readonly Dictionary<MemberInfo, Func<object, string>> printingOverridedMembers;
        private readonly HashSet<MemberInfo> membersToExclude;

        private readonly HashSet<object> printedObjects;

        public PrintingConfig()
        {
            typesToExclude = new HashSet<System.Type>();
            printingOverridedTypes = new Dictionary<System.Type, Func<object, string>>();
            cultureOverridedTypes = new Dictionary<System.Type, CultureInfo>();

            printingOverridedMembers = new Dictionary<MemberInfo, Func<object, string>>();
            membersToExclude = new HashSet<MemberInfo>();

            printedObjects = new HashSet<object>();
        }

        #region Types Handling

        public void OverrideTypePrinting<TPropType>(Func<TPropType, string> print)
        {
            printingOverridedTypes[typeof(TPropType)] = obj => print((TPropType) obj);
        }

        public void OverrideTypeCulture<TPropType>(CultureInfo culture)
        {
            cultureOverridedTypes[typeof(TPropType)] = culture;
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToExclude.Add(typeof(TPropType));

            return this;
        }

        #endregion


        #region Properties/Fields Handling

        public void OverrideMember(MemberInfo memberInfo, Func<object, string> print)
        {
            printingOverridedMembers[memberInfo] = print;
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            var member = GetMember(memberSelector);

            if (!IsPropertyOrField(member))
                throw new ArgumentException("Can't exclude non properties/fields objects", nameof(memberSelector));

            return new MemberPrintingConfig<TOwner, TMemberType>(this, member);
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            var memberToExlude = GetMember(memberSelector);

            if (!IsPropertyOrField(memberToExlude))
                throw new ArgumentException("Can't exclude non properties/fields objects", nameof(memberSelector));

            membersToExclude.Add(memberToExlude);

            return this;
        }

        #endregion


        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (printedObjects.Contains(obj))
                return "...";

            printedObjects.Add(obj);

            if (obj == null)
                return "null";

            var type = obj.GetType();

            if (finalTypes.Contains(type))
            {
                if (cultureOverridedTypes.TryGetValue(type, out var cultureInfo))
                    return PrintWithCulture(obj, cultureInfo);

                return PrintWithCulture(obj, CultureInfo.InvariantCulture);
            }
            
            var sb = new StringBuilder();
            sb.Append(type.Name);
            sb.Append(PropertiesAndFieldsToString(obj, nestingLevel));

            return sb.ToString();
        }

        private string PropertiesAndFieldsToString(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            var sb = new StringBuilder();

            var propertiesAndFields = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>()
                .Concat(type.GetFields(BindingFlags.Public | BindingFlags.Instance));

            foreach (var memberInfo in propertiesAndFields)
            {
                if (typesToExclude.Contains(GetValueType(memberInfo)) || membersToExclude.Contains(memberInfo))
                    continue;

                var propertyString = MemberToString(memberInfo, obj, nestingLevel);
                sb.Append(FormatMember(memberInfo.Name, propertyString, nestingLevel));
            }

            return sb.ToString();
        }

        private string MemberToString(MemberInfo memberInfo, object container, int nestingLevel)
        {
            var type = GetValueType(memberInfo);
            var value = GetValue(memberInfo, container);

            if (printingOverridedMembers.ContainsKey(memberInfo))
                return printingOverridedMembers[memberInfo](value);

            if (printingOverridedTypes.ContainsKey(type))
                return printingOverridedTypes[type](value);

            return PrintToString(value, nestingLevel + 1);
        }

        #region static helpers

        private static string FormatMember(string memberName, string memeberValue, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            return Environment.NewLine + identation + memberName + " = " + memeberValue;
        }

        private static string PrintWithCulture(object obj, CultureInfo cultureInfo)
        {
            var objAsFormattable = obj as IFormattable;

            if (objAsFormattable == null)
                return obj.ToString();

            return objAsFormattable.ToString(null, cultureInfo);
        }

        private static MemberInfo GetMember<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return ((MemberExpression) memberSelector.Body).Member;
        }

        private static bool IsPropertyOrField(MemberInfo memberInfo)
        {
            return memberInfo.MemberType == MemberTypes.Property || memberInfo.MemberType == MemberTypes.Field;
        }

        private static System.Type GetValueType(MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo) memberInfo).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo) memberInfo).PropertyType;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static object GetValue(MemberInfo memberInfo, object container)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo) memberInfo).GetValue(container);
                case MemberTypes.Property:
                    return ((PropertyInfo) memberInfo).GetValue(container);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}