using System;

namespace ObjectPrinting
{
    public class TypeConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly SerializerSettings settings;
        private readonly Type typeToConfig;

        public TypeConfig(PrintingConfig<TOwner> printingConfig, SerializerSettings serializerSettings)
        {
            parentConfig = printingConfig;
            settings = serializerSettings;
            typeToConfig = typeof(TPropType);
        }

        public PrintingConfig<TOwner> IgnoreType()
        {
            settings.TypesToIgnore.Add(typeToConfig);
            return parentConfig;
        }

        public PrintingConfig<TOwner> PrintAs(Func<TPropType, string> print)
        {
            var objFunc = new Func<object, string>(x => print((TPropType)x));
            settings.CustomTypes.Add(typeof(TPropType), objFunc);
            return parentConfig;
        }
    }
}
