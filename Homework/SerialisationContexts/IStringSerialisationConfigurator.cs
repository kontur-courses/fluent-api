namespace Homework.SerialisationContexts
{
    public interface IStringSerialisationConfigurator<TOwner> : ISerialisationIntermediateConfigurator<TOwner>
    {
        public ISerialisationIntermediateConfigurator<TOwner> WithCharsLimit(int maxStringLenght);
    }
}