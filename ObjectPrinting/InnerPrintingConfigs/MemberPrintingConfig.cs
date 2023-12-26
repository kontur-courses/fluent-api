using System;
using System.Reflection;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.InnerPrintingConfigs
{
    public class MemberPrintingConfig<TOwner, TMemberType> : PrintingConfig<TOwner>
    {
        private readonly MemberInfo memberInfo;

        public MemberPrintingConfig(PrintingConfig<TOwner> parent, MemberInfo memberInfo) : base(parent)
        {
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> print)
        {
            memberSerializers[memberInfo] = obj => print((TMemberType)obj);
            return this;
        }
        
        public PrintingConfig<TOwner> TrimmedToLength(int maxLen)
        {
            var isSerialized = memberSerializers.TryGetValue(memberInfo, out var prevSerializer);
            memberSerializers[memberInfo] = isSerialized 
                ? obj => prevSerializer(obj).Truncate(maxLen) 
                : obj => obj.ToString().Truncate(maxLen);

            return this;
        }
    }
}