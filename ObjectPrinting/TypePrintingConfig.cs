using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwnerLocal, TPropLocal>(PrintingConfig<TOwnerLocal> parent)
    {
        public PrintingConfig<TOwnerLocal> Using(Func<TPropLocal, string> serializer)
        {
            parent.SetTypeSerializer(serializer);
            return parent;
        }

        public PrintingConfig<TOwnerLocal> Using(CultureInfo culture)
        {
            parent.SetTypeCulture<TPropLocal>(culture);
            return parent;
        }
    }
}
