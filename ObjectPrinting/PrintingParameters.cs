using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ObjectPrinting
{
    class PrintingParameters
    {
        public CultureInfo Culture { get; set; }
        public Func<object, string> Serializer { get; set; }
    }
}
