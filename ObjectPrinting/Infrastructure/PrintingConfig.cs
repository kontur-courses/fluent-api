using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Infrastructure
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly List<MemberInfo> excludedMembers = new List<MemberInfo>();
        public readonly Dictionary<MemberInfo, Delegate> MemberPrinters = new Dictionary<MemberInfo, Delegate>();
        public readonly Dictionary<Type, Delegate> TypePrinters = new Dictionary<Type, Delegate>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, null);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetMemberInfo(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = GetMemberInfo(memberSelector);
            excludedMembers.Add(memberInfo);
            return this;
        }

        private MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> propertyLambda) =>
            propertyLambda.Body is MemberExpression memberExpression
                ? memberExpression.Member
                : throw new ArgumentException("Member is not selected");

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var memberInfo in type.GetMembers())
            {
                if (IsExcluded(memberInfo))
                    continue;
                if (TryAlternatePrint(memberInfo, obj, out var toPrint))
                {
                    sb.Append(identation).Append(memberInfo.Name).Append(" = ").Append(toPrint).Append(Environment.NewLine);
                    continue;
                }
                sb.Append(identation)
                    .Append(memberInfo.Name)
                    .Append(" = ")
                    .Append(PrintToString(GetValue(obj, memberInfo), nestingLevel + 1));
            }
            return sb.ToString();
        }

        private bool TryAlternatePrint(MemberInfo memberInfo, object owner, out string printed)
        {
            if (MemberPrinters.TryGetValue(memberInfo, out var printer))
            {
                printed = printer.DynamicInvoke(GetValue(owner, memberInfo)).ToString();
                return true;
            }

            if (TypePrinters.TryGetValue(GetType(memberInfo), out printer))
            {
                printed = printer.DynamicInvoke(GetValue(owner, memberInfo)).ToString();
                return true;
            }

            printed = null;
            return false;
        }

        private object GetValue(object owner, MemberInfo memberInfo) =>
            memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo) memberInfo).GetValue(owner),
                MemberTypes.Property => ((PropertyInfo) memberInfo).GetValue(owner),
                _ => throw new ArgumentException($"Cannot get value {memberInfo} from {owner}")
            };

        private bool IsExcluded(MemberInfo memberInfo)
        {
            if (!(memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property))
                return true;
            if (excludedTypes.Contains(GetType(memberInfo)))
                return true;
            if (excludedMembers.Contains(memberInfo))
                return true;
            return false;
        }
        
        public static Type GetType(MemberInfo member) =>
            member.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo) member).FieldType,
                MemberTypes.Property => ((PropertyInfo) member).PropertyType,
                _ => throw new ArgumentException()
            };
    }
}