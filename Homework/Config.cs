using Homework.CultureContexts;
using Homework.IgnoreContexts;
using Homework.SerialisationContexts;

namespace Homework
{
    public class Config<TOwner>
    {
        private protected readonly PrintingRules Rules; 
        
        internal Config(PrintingRules rules)
        {
            Rules = rules;
        }

        public static Config<TOwner> CreateConfig() => new(new PrintingRules());
        public CultureConfig<TOwner> SetCulture() => new(Rules);
        public IgnoreConfig<TOwner> Ignore() => new(Rules);
        public SerialisationTargetConfig<TOwner> SetAlternativeSerialisation() => new(Rules);

        public PrinterWithConfig<TOwner> Configure()
        {
            return new PrinterWithConfig<TOwner>(Rules);
        }
    }
}