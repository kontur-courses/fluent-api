using System;

namespace ObjectPrinting
{
    public record SerialisationRule
    {
        private readonly OneTimeSetValue<int> charsLimit;

        private Func<object?, string> Method { get; }

        public Type Type { get; set; }
        public int CharsLimit
        {
            get => charsLimit.Value;
            set => charsLimit.Value = value;
        }

        public SerialisationRule(Type type, Func<object?, string> method)
        {
            Type = type;
            Method = method;
            charsLimit = new OneTimeSetValue<int>(-1);
        }

        public string ToString(object? obj)
        {
            var result = Method(obj);
            return CharsLimit != -1 && result.Length > CharsLimit ? result[..CharsLimit] : result;
        }
    }
}