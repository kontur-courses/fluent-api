using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            propConfig.Using(property => property.Substring(0,Math.Min(maxLen, property.Length)).ToString());
            return propConfig.ParentConfig;
        }

    }
}