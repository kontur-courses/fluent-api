using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    public static class StringConfigExtensions
    {
        public static PrintingConfig<TOwner> CropToLength<TOwner, T>(this PropertyConfig<TOwner, T> aboba, int length)
        {
            aboba.Func = (x) => ((string)x).Substring(0, length);
            return (PrintingConfig<TOwner>)aboba.Father;
        }
    }
}
