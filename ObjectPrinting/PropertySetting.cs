using System;
using System.Globalization;
using Microsoft.VisualBasic.FileIO;
using NUnit.Framework;

namespace ObjectPrinting
{
    public class PropertySetting<T>
    {
        protected internal bool IsExcept { get; }
        protected internal Func<object, string> OutputMethod { private set; get; }
        protected PrintingConfig<T> Config { get; private set; }
        protected internal CultureInfo Culture { get; private set; }

        public PropertySetting(PrintingConfig<T> config, bool isExcept = false)
        {
            IsExcept = isExcept;
            OutputMethod = null;
            Config = config;
            Culture = CultureInfo.CurrentCulture;
        }

        public PrintingConfig<T> ChangeField(Func<object, string> func)
        {
            OutputMethod = func;
            return Config;
        }

        public PrintingConfig<T> ChangeCulture(CultureInfo culture)
        {
            Culture = culture;
            return Config;
        }
    }
}