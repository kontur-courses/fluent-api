using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertySetting<T>
    {
        protected internal bool IsExcept { get; }
        protected internal Func<object, string> OutputMethod { protected set; get; }
        protected PrintingConfig<T> Config { get; private set; }
        protected internal CultureInfo Culture { get; protected set; }
        public PropertySetting(PrintingConfig<T> config, bool isExcept = false)
        {
            IsExcept = isExcept;
            OutputMethod = null;
            this.Config = config;
            Culture = CultureInfo.CurrentCulture;
        }

        public PrintingConfig<T> ChangeOutput(Func<object, string> func)
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