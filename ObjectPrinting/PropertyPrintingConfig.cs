using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public sealed class PropertyPrintingConfig<TRoot, TValue>
    {
        private readonly PrintingConfig<TRoot> printingConfig;
        private readonly MemberInfo member;

        internal PropertyPrintingConfig(
            PrintingConfig<TRoot> printingConfig,
            MemberInfo member)
        {
            ArgumentNullException.ThrowIfNull(printingConfig);
            ArgumentNullException.ThrowIfNull(member);

            this.printingConfig = printingConfig;
            this.member = member;
        }

        public PrintingConfig<TRoot> Using(Func<TValue, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);

            printingConfig.Settings.SetMemberFormatter(
                member,
                value => formatter((TValue)value!));

            return printingConfig;
        }

        internal PrintingConfig<TRoot> UsingCulture(CultureInfo culture)
        {
            printingConfig.Settings.SetMemberCulture(member, culture);

            return printingConfig;
        }

        internal PrintingConfig<TRoot> SetMaxLength(int maxLength)
        {
            printingConfig.Settings.SetMemberMaxLength(member, maxLength);

            return printingConfig;
        }
    }
}