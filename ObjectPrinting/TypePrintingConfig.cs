﻿using System;


namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        PrintingConfig<TOwner> ITypePrintingConfig<TOwner>.PrintingConfig => printingConfig;

        internal TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<Type, string> serializationMethod)
        {
            printingConfig.Settings.SetSerializationForType(typeof(TPropType), serializationMethod);

            return printingConfig;
        }
        public PrintingConfig<TOwner> Exclude()
        {
            return new PrintingConfig<TOwner>();
        }
    }
}
