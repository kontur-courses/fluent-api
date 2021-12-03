namespace Homework.IgnoreContexts
{
    public interface IIgnoreTypeConfigurator<TOwner> : IPrinterConfigurator<TOwner>
    {
        public IIgnoreConfigurator<TOwner> And { get; }
        public IIgnoreTypeIntermediateConfigurator<TOwner> InAllNestingLevels();
    }
}