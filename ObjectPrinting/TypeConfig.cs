using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class TypeConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> ParentConfig { get; private set; }
        public SerializerSettings settings;
        public Type typeToConfig;

        public TypeConfig(PrintingConfig<TOwner> printingConfig, SerializerSettings serializerSettings)
        {
            ParentConfig = printingConfig;
            this.settings = serializerSettings;
            typeToConfig = typeof(TPropType);
        }

        public PrintingConfig<TOwner> IgnoreType()
        {
            settings.TypesToIgnore.Add(typeToConfig);
            return ParentConfig;
        }

        public PrintingConfig<TOwner> PrintAs(Func<TPropType, string> print)
        {
            var objFunc = new Func<object, string>(x => print((TPropType)x));
            settings.CustomTypes.Add(typeof(TPropType), objFunc);
            return ParentConfig;
        }
    }
}
