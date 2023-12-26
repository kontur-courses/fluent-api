using System;
using System.Reflection;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.InnerPrintingConfigs
{
    public class MemberPrintingConfig<TOwner, TMemberType> : PrintingConfig<TOwner>, IInnerPrintingConfig<TOwner, TMemberType>
    {
        private readonly MemberInfo memberInfo;

        internal MemberPrintingConfig(PrintingConfig<TOwner> parent, MemberInfo memberInfo) : base(parent)
        {
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> print)
        {
            MemberSerializers[memberInfo] = obj => print((TMemberType)obj);
            return this;
        }
        
        public PrintingConfig<TOwner> TrimmedToLength(int maxLen)
        {
            var isSerialized = MemberSerializers.TryGetValue(memberInfo, out var prevSerializer);
            MemberSerializers[memberInfo] = isSerialized 
                ? obj => prevSerializer(obj).Truncate(maxLen) 
                : obj => obj.ToString().Truncate(maxLen);

            return this;
        }
    }
}