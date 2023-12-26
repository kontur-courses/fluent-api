using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertySetting<T>
    {
        protected internal bool IsExcept { get; }
        protected internal Func<object, string> OutputMethod { get; private set; }
        protected PrintingConfig<T> Config { get; private set; }
        protected internal CultureInfo Culture { get; private set; }

        public PropertySetting(PrintingConfig<T> config, bool isExcept = false)
        {
            IsExcept = isExcept;
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