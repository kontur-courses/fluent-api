using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    public static class StringConfigExtensions
    {
        public static PrintingConfig<TOwner> CropToLength<TOwner, T>(this PropertyConfig<TOwner, T> config, int length)
        {
            config.Func = (x) => ((string)x).Substring(0, length);
            return (PrintingConfig<TOwner>)config.Father;
        }
    }
}
