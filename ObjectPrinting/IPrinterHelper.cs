namespace ObjectPrinting
{
    public interface IPrinterHelper<TMember>
    {
        public string Print<TOwner>(PrintingConfig<TOwner> printer, TMember obj, int nestingLevel);
    }
}