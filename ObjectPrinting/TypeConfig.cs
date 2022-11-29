using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class TypeConfig<TOwner, TPropType> : IConfig<TOwner, TPropType>
    {
        public readonly PrintingConfig<TOwner> printingConfig;
        public PrintingConfig<TOwner> ParentConfig => printingConfig;
        public Settings serializerSettings;
        public Type typeToConfig;

        public TypeConfig(PrintingConfig<TOwner> printingConfig, Settings serializerSettings)
        {
            this.printingConfig = printingConfig;
            this.serializerSettings = serializerSettings;
        }

        public PrintingConfig<TOwner> IgnoreType()
        {
            serializerSettings.typesToIgnore.Add(typeToConfig);
            return ParentConfig;
        }

        public PrintingConfig<TOwner> PrintAs(Func<TPropType, string> print)
        {
            var objFunc = new Func<object, string>(x => print((TPropType)x));
            serializerSettings.customTypes.Add(typeof(TPropType), objFunc);
            return printingConfig;
        }
    }
}
