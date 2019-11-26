using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Delegate> TypeToFormatter { get; }
        Dictionary<PropertyInfo, Delegate> PropertyToFormatter { get; }
        Dictionary<PropertyInfo, int> PropertyToLength { get; }
        Dictionary<Type, CultureInfo> DigitTypeToCulture { get; }
    }
}
