using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting.Solved.Interfaces
{
    public interface ITypePrinting<TOwner, TPropType> : IPrinting<TOwner>
    {
        PrintingConfig<TOwner> Using(Func<TPropType, string> serializer);
        PrintingConfig<TOwner> Using(CultureInfo culture);
    }
}
