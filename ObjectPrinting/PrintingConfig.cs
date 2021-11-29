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
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
        private readonly Dictionary<Type, Delegate> customTypeSerializers = new();
        private readonly Dictionary<MemberInfo, Delegate> customMemberSerializers = new();


        public PrintingConfig<TOwner> Excluding<TMember>()
        {
            excludedTypes.Add(typeof(TMember));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TMember>(Expression<Func<TOwner, TMember>> memberSelector)
        {
            var member = GetMemberInfoFromSelector(memberSelector);
            excludedMembers.Add(member);
            return this;
        }

        public INestedPrintingConfig<TOwner,TMember> Printing<TMember>()
        {
            return new TypePrintingConfig<TOwner, TMember>(this);
        }

        public INestedPrintingConfig<TOwner, TMember> Printing<TMember>(Expression<Func<TOwner, TMember>> memberSelector)
        {
            return new MemberPrintingConfig<TOwner, TMember>(this, GetMemberInfoFromSelector(memberSelector));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        internal void AddCustomTypeSerializer<TMember>(Type type, Func<TMember, string> serializer)
        {
            if (!customTypeSerializers.TryAdd(type, serializer))
                customTypeSerializers[type] = serializer;
            
        }

        internal void AddCustomMemberSerializer<TMember>(MemberInfo memberInfo, Func<TMember, string> serializer)
        {
            if (!customMemberSerializers.TryAdd(memberInfo, serializer))
                customMemberSerializers[memberInfo] = serializer;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var memberInfo in type.GetPublicPropertiesAndFields().Where(m => !IsExcluded(m)))
            {
                var isCustomSerialization = TryUseCustomSerialization(memberInfo, obj, out var customSerialization);
                sb.Append(identation + memberInfo.Name + " = " +
                          (!isCustomSerialization 
                              ? PrintToString(memberInfo.GetMemberValue(obj),
                              nestingLevel + 1) 
                              : customSerialization));
            }
            return sb.ToString();
        }

        private bool TryUseCustomSerialization(MemberInfo member, object obj, out string customSerialization)
        {
            var memberType = member.GetMemberType();
            if (customTypeSerializers.ContainsKey(memberType))
            {
                customSerialization = customTypeSerializers[memberType]
                    .DynamicInvoke(member.GetMemberValue(obj))
                    + Environment.NewLine;
                return true;
            }

            if (customMemberSerializers.ContainsKey(member))
            {
                customSerialization = customMemberSerializers[member]
                    .DynamicInvoke(member.GetMemberValue(obj))
                    +Environment.NewLine;
                return true;
            }
            customSerialization = null;
            return false;
        }

        private bool IsExcluded(MemberInfo member)
        {
            return excludedMembers.Contains(member) || excludedTypes.Contains(member.GetMemberType());
        }

        private static MemberInfo GetMemberInfoFromSelector<TMember>(Expression<Func<TOwner, TMember>> memberSelector)
        {
            var member = memberSelector.Body as MemberExpression;
            return member?.Member;
        }
    }
}