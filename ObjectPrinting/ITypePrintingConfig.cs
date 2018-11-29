using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting
{
    interface ITypePrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
        string TypeName { get; }
    }
}