﻿using System;

namespace ObjectPrinting.Extensions
{
    public static class TypeConfigExtension
    {
        public static PrintingConfig<TOwner> Using<TOwner, TType>(this IInnerTypeConfig<TOwner, TType> config, 
            IFormatProvider formatter) where TType : IFormattable
        {
            return config
                .Using(type => type.ToString(null, formatter));;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this IInnerTypeConfig<TOwner, string> config, 
            int maxLength)
        {
            return config
                .Using(str => str[..Math.Min(str.Length, maxLength)]);
        }
    }
}