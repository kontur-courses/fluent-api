using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class BodyPrintingConfig<TOwner>
    {
        public BodyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            PrintingConfig = printingConfig;
            ExcludingTypes = new List<Type>();
            AlternativePrinting = new Dictionary<Type, Func<object, string>>();
            AlternativePrintingProp = new Dictionary<string, Func<object, string>>();
            ExcludingProp = new List<string>();
        }
        public string PropertyKey { get; set; }
        public PrintingConfig<TOwner> PrintingConfig { get; }
        public List<Type> ExcludingTypes { get; }
        public Dictionary<Type, Func<object, string>> AlternativePrinting { get; }
        public Dictionary<string, Func<object, string>> AlternativePrintingProp { get; }
        public List<string> ExcludingProp { get; }
    }
}
