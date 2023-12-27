using System;
using System.Globalization;

namespace ObjectPrinting.Configs
{
    public interface ITypePrintingConfig<TOwner>
    {
        public PrintingConfig<TOwner> PrintingConfig { get; }

        public CultureInfo CultureInfo { get;}

        public Func<object, string> Serializer { get; }
    }
}