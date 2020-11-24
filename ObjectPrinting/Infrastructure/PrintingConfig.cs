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
        private readonly Dictionary<MemberInfo, Settings> membersSettings = new Dictionary<MemberInfo, Settings>();
        private readonly Dictionary<Type, Settings> typeSettings = new Dictionary<Type, Settings>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, typeof(TPropType));
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetMemberInfo(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = GetMemberInfo(memberSelector);
            GetSettings(memberInfo).IsExcluded = true;
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            GetSettings(typeof(TPropType)).IsExcluded = true;
            return this;
        }
        
        private MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> propertyLambda) =>
            propertyLambda.Body is MemberExpression memberExpression
                ? memberExpression.Member
                : throw new ArgumentException("Member is not selected");

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
                sb.Append(identation).Append(memberInfo.Name).Append(" = ");
                if (TryAlternatePrint(memberInfo, obj, out var toPrint))
                {
                    sb.Append(toPrint).Append(Environment.NewLine);
                    continue;
                }
                sb.Append(PrintToString(GetValue(obj, memberInfo), nestingLevel + 1));
            }
            return sb.ToString();
        }

        private bool TryAlternatePrint(MemberInfo memberInfo, object owner, out string printed)
        {
            if (membersSettings.TryGetValue(memberInfo, out var settings))
            {
                printed = ApplySettings(settings, memberInfo, owner);
                return true;
            }

            if (typeSettings.TryGetValue(GetType(memberInfo), out settings))
            {
                printed = ApplySettings(settings, memberInfo, owner);
                return true;
            }

            printed = null;
            return false;
        }

        private string ApplySettings(Settings settings, MemberInfo memberInfo, object owner)
        {
            var toPrint = GetValue(owner, memberInfo)?.ToString();
            if (settings.Printer != null)
                toPrint = settings.Printer.DynamicInvoke(GetValue(owner, memberInfo)).ToString();
            if (settings.MaxLength != null)
                toPrint = toPrint.Substring(0, Math.Min(toPrint.Length, settings.MaxLength ?? default));
            return toPrint;
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
            if (typeSettings.TryGetValue(GetType(memberInfo), out var settings) && settings.IsExcluded)
                return true;
            if (membersSettings.TryGetValue(memberInfo, out settings) && settings.IsExcluded)
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

        public Settings GetSettings(MemberInfo memberInfo)
        {
            return GetSettings(membersSettings, memberInfo);
        }
        
        public Settings GetSettings(Type type)
        {
            return GetSettings(typeSettings, type);
        }

        private Settings GetSettings<TKey>(Dictionary<TKey, Settings> settingsDictionary, TKey key)
        {
            if (settingsDictionary.TryGetValue(key, out var settings))
                return settings;
            
            settings = new Settings();
            settingsDictionary.Add(key, settings);
            return settings;
        }
    }
}