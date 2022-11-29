using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    public static class TypeConfigExtensions
    {
        public static PrintingConfig<TOwner> SetCulture<TOwner, T>(this TypeConfig<TOwner, T> prop, IFormatProvider culture) where T : IFormattable
        {
            prop.serializerSettings.customCultures.Add(typeof(T), culture);
            return prop.ParentConfig;
        }
    }
}
