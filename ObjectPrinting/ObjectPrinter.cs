using ObjectPrinting.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static IPrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }
}