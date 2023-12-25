using System;

namespace ObjectPrinting
{
    public class FieldPrintingConfig<TOwner, T> : PrintingConfig<TOwner>
    {
        public FieldPrintingConfig(PrintingConfig<TOwner> printingConfig) : base(printingConfig) { }
        
        public FieldPrintingConfig<TOwner, T> SpecificSerialization(Func<T, string> serializer)
        {
            SetSerializer(FieldInfo, obj => serializer((T)obj));
            
            return this;
        }

        public FieldPrintingConfig<TOwner, T> Exclude()
        {
            ExcludedFields.Add(FieldInfo);
            
            return this;
        }
    }
}