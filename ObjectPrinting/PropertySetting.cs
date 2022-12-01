using System;

namespace ObjectPrinting
{
    public class PropertySetting<T>
    {
        public bool IsExcept { private set; get; }
        public Func<object, string> OutputMethod { private set; get; }
        private readonly PrintingConfig<T> config;

        public PropertySetting(PrintingConfig<T> config)
        {
            IsExcept = false;
            OutputMethod = null;
            this.config = config;
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
    }
}