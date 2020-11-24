using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        public readonly object SelectedMember;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, object selectedMember)
        {
            this.printingConfig = printingConfig;
            SelectedMember = selectedMember;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            printingConfig.AddOwnSerializationForSelectedMember(print, SelectedMember);
            return printingConfig;
        }
    }
}