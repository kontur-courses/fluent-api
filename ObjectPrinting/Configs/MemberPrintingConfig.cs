using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Configs
{
    public class MemberPrintingConfig<TOwner, TMemberType>
    {
        internal PrintingConfig<TOwner> ParentConfig { get; }
        private readonly MemberInfo member;
        private readonly GlobalConfig config;

        public MemberPrintingConfig(PrintingConfig<TOwner> parentConfig, MemberInfo member)
        {
            ParentConfig = parentConfig;
            config = parentConfig.Config;
            this.member = member;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> serializeFunc)
        {
            config.AlternativeMemberSerializations[member] = 
                obj => serializeFunc.Invoke((TMemberType)obj);
            return ParentConfig;
        }

        [FormattableType]
        private PrintingConfig<TOwner> UsingCulture(CultureInfo culture)
        {
            config.Cultures[member] = culture;
            return ParentConfig;
        }

        [TrimmableType]
        private PrintingConfig<TOwner> WithCut(int length)
        {
            config.CutLengths[member] = length;
            return ParentConfig;
        }
    }
}