namespace Homework.SerialisationContexts
{
    public interface ISerialisationIntermediateConfigurator<TOwner> : IPrinterConfigurator<TOwner>
    {
        public ISerialisationTargetConfigurator<TOwner> And { get; }
    }
}