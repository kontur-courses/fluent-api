using System;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TMemberType> : IMemberPrintingConfig<TOwner, TMemberType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private Func<object, string> printMember = obj => obj.ToString();

        public MemberPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> print)
        {
            printMember = obj => print((TMemberType) obj);
            return printingConfig;
        }

        PrintingConfig<TOwner> IMemberPrintingConfig<TOwner, TMemberType>.ParentConfig => printingConfig;
        Func<object, string> IMemberPrintingConfig<TOwner, TMemberType>.PrintMember => printMember;
    }

    public interface IMemberPrintingConfig<TOwner, TMemberType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        Func<object, string> PrintMember { get; }
    }
}