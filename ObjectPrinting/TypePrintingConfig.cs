using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, T> : IMemberPrintingConfig<TOwner, T>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        public readonly MemberInfo SelectedMember;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo selectedMember)
        {
            this.printingConfig = printingConfig;
            SelectedMember = selectedMember;
        }

        public PrintingConfig<TOwner> Using(Func<T, string> print)
        {
            printingConfig.AddOwnSerializationForSelectedType(print, SelectedMember);
            return printingConfig;
        }
    }
}