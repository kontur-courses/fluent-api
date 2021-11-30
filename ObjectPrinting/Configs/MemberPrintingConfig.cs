using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
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
            config.AlternativeMemberSerializations
                .AddOrUpdate(member, obj => serializeFunc.Invoke((TMemberType)obj));
            return ParentConfig;
        }

        [FormattableType]
        private PrintingConfig<TOwner> UsingCulture(CultureInfo culture)
        {
            config.Cultures.AddOrUpdate(member, culture);
            return ParentConfig;
        }

        [TrimmableType]
        private PrintingConfig<TOwner> WithCut(int length)
        {
            config.CutLengths.AddOrUpdate(member, length);
            return ParentConfig;
        }
    }
}