using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly SerializerSettings settings = new();
        private readonly HashSet<object> visited = new();
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float),
            typeof(string), typeof(DateTime), typeof(TimeSpan)
        };

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);
        
        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (visited.Contains(obj))
            {
                return settings.IsAllowCyclingReference
                    ? $"Cycle" + Environment.NewLine
                    : throw new Exception("Unexpected cycle reference");
            }
            
            var type = obj.GetType();
            
            if (settings.HasTypeSerializer(type))
                return settings.GetTypeSerializer(type).DynamicInvoke(obj) + Environment.NewLine;
            
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            visited.Add(obj);
            
            var ident = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            var members = 
                type
                .GetFieldsAndProperties()
                .Where(NotExcluded);
            foreach (var member in members)
            {
                var str = $"{ident}{ToString(member, obj, nestingLevel)}";
                sb.Append(str);
            }
            return sb.ToString();
        }

        private bool NotExcluded(MemberInfo memberInfo) =>
            !settings.IsExcluded(memberInfo.GetMemberType()) && !settings.IsExcluded(memberInfo);

        private string ToString(MemberInfo memberInfo, object obj, int nestingLevel) =>
            $"{memberInfo.Name} = {MemberValueToString(memberInfo, obj, nestingLevel)}";

        private string MemberValueToString(MemberInfo memberInfo, object obj, int nestingLevel)
        {
            var memberValue = memberInfo.GetValue(obj);
            if (settings.HasMemberSerializer(memberInfo))
                return settings.GetMemberSerializer(memberInfo).DynamicInvoke(memberValue) + Environment.NewLine;
            return PrintToString(memberValue, nestingLevel + 1);
        }
        
        public PrintingConfig<TOwner> AllowCyclingReference()
        {
            settings.IsAllowCyclingReference = true;
            return this;
        }
        
        public PrintingConfig<TOwner> Excluding<TType>()
        {
            settings.AddExcludedType(typeof(TType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            var memberInfo = GetMember(memberSelector);
            settings.AddExcludedMember(memberInfo);
            return this;
        }
        
        public IInnerTypeConfig<TOwner, TType> Printing<TType>() => new TypeConfig<TOwner, TType>(this, settings);

        public IInnerTypeConfig<TOwner, TType> Printing<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            var memberInfo = GetMember(memberSelector);
            return new PropertyPrintingConfig<TOwner, TType>(this, memberInfo, settings);
        }
        
        private MemberInfo GetMember<TType>(Expression<Func<TOwner, TType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException("Cannot resolve member expression");
            var memberInfo = memberExpression.Member;
            if (memberInfo is null)
                throw new ArgumentException("Cannot resolve member type");
            return memberInfo;
        }
    }
}