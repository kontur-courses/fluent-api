using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.PrintingConfig
{
    public class PropertyPrintingConfig<TOwner, TField> : PrintingConfig<TOwner>
    {
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parent, PropertyInfo propertyInfo) : base(parent)
        {
            this.propertyInfo = propertyInfo;
        }
        
        internal PropertyPrintingConfig(int maxLength, PropertyPrintingConfig<TOwner, TField> parent) 
            : base(parent.propertyInfo, maxLength, parent)
        { }

        public PropertyPrintingConfig<TOwner, TField> As(Func<TField, string> serializer)
        {
            PropertySerializers[propertyInfo] = serializer;
            return this;
        }
    }
}