using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using ObjectPrinting.Solved;
using ObjectPrinting.Solved.Tests;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int length)
        {
            propConfig.strTrimLength = length;
            return propConfig.ParentConfig;
        }
    }
}
