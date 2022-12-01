using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertySetting<T>
    {
        public bool IsExcept { private set; get; }
        public Func<object, string> OutputMethod { private set; get; }
        private readonly PrintingConfig<T> config;
        public CultureInfo Culture { get; set; }
        public PropertySetting(PrintingConfig<T> config)
        {
            IsExcept = false;
            OutputMethod = null;
            this.config = config;
            Culture = CultureInfo.CurrentCulture;
        }

        public PrintingConfig<T> Except()
        {
            IsExcept = true;
            return config;
        }

        public PrintingConfig<T> ChangeOutput(Func<object, string> func)
        {
            OutputMethod = func;
            return config;
        }

        public PrintingConfig<T> ChangeCulture(CultureInfo culture)
        {
            Culture = culture;
            return config;
        }
    }
}