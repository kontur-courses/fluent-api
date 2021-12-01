using System;

namespace ObjectPrinting.PrintingConfig
{
    public class TypePrintingConfig<TOwner, TType> : INestingPrintingConfig<TOwner, TType>
    {
        private readonly PrintingConfig<TOwner> parent;
        private readonly SerializationSettings settings;

        public TypePrintingConfig(PrintingConfig<TOwner> parent, SerializationSettings settings)
        {
            this.parent = parent;
            this.settings = settings;
        }

        public PrintingConfig<TOwner> Use(Func<TType, string> transformer)
        {
            if (transformer == null) throw new ArgumentNullException(nameof(transformer));
            settings.SetTransformer(transformer);
            return parent;
        }
    }
}