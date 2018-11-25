using System;
using System.Globalization;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        void AddSpecialTypeSerializing(Type type, Delegate format);
        void AddTypeCulture(Type type, CultureInfo culture);
        void AddSpecialPropertySerializing(string propName, Delegate format);
    }
}