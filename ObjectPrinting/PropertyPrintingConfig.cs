﻿using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<Type, Func<object, string>> serializationMap;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Dictionary<Type, Func<object, string>> serializationMap)
        {
            this.printingConfig = printingConfig;
            this.serializationMap = serializationMap;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> format)
        {
            serializationMap[typeof(TPropType)] = objectToPrint => format((TPropType) objectToPrint);
            return printingConfig;
        }
    }
}