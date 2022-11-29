using System;
using System.Linq.Expressions;
using ObjectPrinting.Abstractions.Configs;
using ObjectPrinting.Abstractions.Printers;
using ObjectPrinting.Infrastructure;

namespace ObjectPrinting.Implementations.Configs
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly IRootObjectPrinter _printer;

        public PrintingConfig(IRootObjectPrinter printer) =>
            _printer = printer;

        public string PrintToString(TOwner? obj) =>
            _printer.PrintToString(obj);

        public IPrintingConfig<TOwner> Exclude<TMember>()
        {
            _printer.ExcludingRules.Exclude<TMember>();
            return this;
        }

        public IPrintingConfig<TOwner> Exclude<TMember>(Expression<Func<TOwner, TMember>> selector)
        {
            _printer.ExcludingRules.Exclude(selector.GetMemberPath());
            return this;
        }

        public IMemberPrintingConfig<TOwner, TMember> Printing<TMember>()
        {
            var memberCfg = new MemberPrintingConfig<TOwner, TMember>(this);
            _printer.PrintersCollector.AddPrinterFor<TMember>(memberCfg);
            return memberCfg;
        }

        public IMemberPrintingConfig<TOwner, TMember> Printing<TMember>(Expression<Func<TOwner, TMember>> selector)
        {
            var memberCfg = new MemberPrintingConfig<TOwner, TMember>(this);
            _printer.PrintersCollector.AddPrinterFor(selector.GetMemberPath(), memberCfg);
            return memberCfg;
        }
    }
}