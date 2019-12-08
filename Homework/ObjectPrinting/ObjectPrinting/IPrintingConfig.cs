using System;

namespace ObjectPrinting
{
    internal interface IPrintingConfig
    {
        void SetAlternateMemberSerialisator<TMemberType>(Func<TMemberType, string> alternateSerialisator);
        void SetCultureInfoApplierForNumberType<TNumber>(Func<TNumber, string> cultureApplier);
        void SetMaxValueLengthForStringMember(int maxValueLength);
    }
}