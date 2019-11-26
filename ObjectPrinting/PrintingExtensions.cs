using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PrintingExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypeSerializationConfig<TOwner, double> config,
            CultureInfo info)
        {
            var cfg = (config as IPropertyPrintingConfig<TOwner>).Config;
            cfg.TypeSerializators.Add(typeof(double),
                TypeSerializer.CreateSerializer<double>(typeof(double), t => t.ToString(info)));
            return cfg;
        }
    }
}