namespace Homework.IgnoreContexts
{
    public interface IIgnoreTypeIntermediateConfigurator<TOwnerInner> : IPrinterConfigurator<TOwnerInner>
    {
        public IIgnoreConfigurator<TOwnerInner> And { get; }
    }
}