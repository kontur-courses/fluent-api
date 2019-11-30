namespace ObjectPrinting
{
    public class CustomSerializeObject<T> : PrintingObject<T>
    {
        public CustomSerializeObject(object obj, PrintingConfig<T> config) : base(obj, config)
        {
        }

        public override string Print(int nestingLevel)
        {
            return (PrintingConfig as IPrintingConfig<T>)
                .TypeSerializerConfigs[ObjectForPrint.GetType()]
                .SerializeFunc(ObjectForPrint);
        }
    }
}