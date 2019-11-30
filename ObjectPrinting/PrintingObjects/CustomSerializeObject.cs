namespace ObjectPrinting
{
    public class CustomSerializeObject<T> : PrintingObject<T>
    {
        public CustomSerializeObject(object obj, IPrintingConfig<T> config) : base(obj, config) { }

        public override string Print(int nestingLevel)
        {
            return PrintingConfig
                .TypeSerializerConfigs[ObjectForPrint.GetType()]
                .SerializeFunc(ObjectForPrint);
        }
    }
}