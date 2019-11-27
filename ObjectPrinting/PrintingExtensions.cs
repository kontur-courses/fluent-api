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
            var configuration = (config as IPropertyPrintingConfig<TOwner>).Config;
            configuration.TypeSerializators.Add(typeof(double),
                TypeSerializer.CreateSerializer<double>(typeof(double), t => t.ToString(info)));
            return configuration;
        }

        public static string PrintWithConfig<TOwner>(this TOwner target, PrintingConfig<TOwner> printer)
        {
            return printer.PrintObject(target);
        }
    }
}