using System;


namespace ObjectPrinting
{
    public class PropertySerializationConfig<TOwner, TPropType>: IPropertySerializationConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;

        public PropertySerializationConfig(PrintingConfig<TOwner> printingConfig, string propertyName)
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<Type, string> newSerializationFunc)
        {
            ((ISettings)printingConfig).Settings.SetPropertySerialization(propertyName, newSerializationFunc);

            return printingConfig;
        }

        public PrintingConfig<TOwner> Exclude()
        {
            ((ISettings)printingConfig).Settings.AddPropertyToExclude(propertyName);

            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertySerializationConfig<TOwner>.PrintingConfig => printingConfig;
        string IPropertySerializationConfig<TOwner>.PropertyName => propertyName;
    }
}
