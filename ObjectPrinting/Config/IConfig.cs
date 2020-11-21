using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    public interface IConfig<TOwner, TPropType>
    {
        IPrintingConfig<TOwner> ParentConfig { get; }
        public PrintingConfig<TOwner> Using(Func<TPropType, string> print);
    }
}
