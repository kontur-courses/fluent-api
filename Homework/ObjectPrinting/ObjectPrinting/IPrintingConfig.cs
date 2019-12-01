using System;

namespace ObjectPrinting
{
    internal interface IPrintingConfig
    {
        void SetAlternatePropertySerialisator<TPropType>(Func<TPropType, string> alternateSerialisator);
        void SetCultureInfoApplierForNumberType<TNumber>(Func<TNumber, string> cultureApplier);
        void SetMaxValueLengthForStringProperty(int maxValueLength);
    }
}