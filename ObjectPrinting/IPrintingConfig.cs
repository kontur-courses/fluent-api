namespace ObjectPrinting
{
    using System;

    public interface IPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> With<TPropType>(Func<TPropType, string> printer);
    }
}
