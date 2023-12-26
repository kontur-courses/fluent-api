namespace ObjectPrinting
{
    public class StringSetting<T> : PropertySetting<T>
    {
        protected internal int MaxLength { get; private set; }

        public StringSetting(PrintingConfig<T> config, bool isExcept = false) : base(config, isExcept)
        {
            MaxLength = -1;
        }

        public PrintingConfig<T> SetMaxLength(int length)
        {
            MaxLength = length;
            return Config;
        }
    }
}