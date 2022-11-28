using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.PropertyPrintingConfig;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();
        internal readonly Dictionary<MemberInfo, Delegate> MemberSerializationRule = new Dictionary<MemberInfo, Delegate>();
        internal readonly Dictionary<Type, Delegate> TypeSerializationRule = new Dictionary<Type, Delegate>();
        private readonly Type[] finalTypes = new[]
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
        };

        public PrintingConfig()
        {

        }
        private PrintingConfig(PrintingConfig<TOwner> parent)
        {
            excludedTypes.AddRange(parent.excludedTypes);
            excludedMembers.AddRange(parent.excludedMembers);
            MemberSerializationRule.AddRange(parent.MemberSerializationRule);
            TypeSerializationRule.AddRange(parent.TypeSerializationRule);
        }
            
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var config = new PrintingConfig<TOwner>(this);
            return new PropertyPrintingConfig<TOwner, TPropType>(config);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var config = new PrintingConfig<TOwner>(this);
            var memberInfo = GetMemberInfo(memberSelector);
            return new PropertyPrintingConfig<TOwner, TPropType>(config, memberInfo);
        }

        private MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector) =>
            (memberSelector.Body as MemberExpression)?.Member;

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var config = new PrintingConfig<TOwner>(this);
            var memberInfo = GetMemberInfo(memberSelector);
            if(memberInfo != null)
                config.excludedMembers.Add(memberInfo);
            return config;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var config = new PrintingConfig<TOwner>(this);
            config.excludedTypes.Add(typeof(TPropType));
            return config;
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
            foreach (var propertyInfo in type.GetProperties()
                .Where(AreNotExcludedProperty))
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(GetValue(obj, propertyInfo), nestingLevel + 1));
            }
            return sb.ToString();
        }

        private bool AreNotExcludedProperty(PropertyInfo propertyInfo) =>
            !excludedTypes.Contains(propertyInfo.PropertyType) &&
            !excludedMembers.Contains(propertyInfo);

        private object GetValue(object obj, PropertyInfo propertyInfo)
        {
            var val = propertyInfo.GetValue(obj);
            return MemberSerializationRule.ContainsKey(propertyInfo) ?
                MemberSerializationRule[propertyInfo].DynamicInvoke(val) :
                TypeSerializationRule.ContainsKey(propertyInfo.PropertyType) ?
                TypeSerializationRule[propertyInfo.PropertyType].DynamicInvoke(val) :
                val;
        }
    }
}