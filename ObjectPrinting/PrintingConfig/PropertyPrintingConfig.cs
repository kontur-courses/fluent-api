using System;
using System.Collections.Generic;
using ObjectPrinting.Solved;

namespace ObjectPrinting.PrintingConfig
{
    public class PropertyPrintingConfig<TOwner, TPropType>(
        PrintingConfig<TOwner> printingConfig,
        Dictionary<Type, Func<object, string>> typeSerializers)
        : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly Dictionary<Type, Func<object, string>> typeSerializers = typeSerializers;

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            Func<object, string> func = obj => print((TPropType)obj);

            printingConfig.DataConfig.TypeSerializers.Add(typeof(TPropType), func);
            return printingConfig;
        }
        

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig =>
            printingConfig; // не ту ты понял, если мы напишем так, то этот метод будет доступен только через интерфейс. он как бы публичынй, но по сути, его не будут видеть, если специально не за кастить то интерфейса.
    }
}