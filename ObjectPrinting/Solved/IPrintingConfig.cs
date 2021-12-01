using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ObjectPrinting.Solved
{
    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Func<object, string>> AlternativePrinting { get; }
        Dictionary<Type, CultureInfo> SpecialCulture { get; }
    }
}
