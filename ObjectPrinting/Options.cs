using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ObjectPrinting
{
    public class Options
    {
        public int MaxStringLength = -1;
        public CultureInfo CultureInfo = CultureInfo.InvariantCulture;
        public int MaxRecursionDepth = 10;
    }
}
