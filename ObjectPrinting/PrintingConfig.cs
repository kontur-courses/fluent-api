using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new();
        private readonly Dictionary<Type, Delegate> customTypeSerializers = new();

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public INestedPrintingConfig<TOwner, TMemberType> Printing<TMemberType>()
        {
            return new TypePrintingConfig<TOwner, TMemberType>(this);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        internal void AddCustomTypeSerializer<TMember>(Type type, Func<TMember, string> serializer)
        {
            if (customTypeSerializers.ContainsKey(type))
                customTypeSerializers[type] = serializer;
            else
                customTypeSerializers.Add(type, serializer);
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
            foreach (var memberInfo in type.GetPublicPropertiesAndFields().Where(p => !excludedTypes.Contains(p.GetMemberType())))
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
                customSerialization = customTypeSerializers[memberType].DynamicInvoke(member.GetMemberValue(obj)) + Environment.NewLine;
                return true;
            }

            customSerialization = null;
            return false;
        }
    }
}