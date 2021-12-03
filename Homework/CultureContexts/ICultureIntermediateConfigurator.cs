namespace Homework.CultureContexts
{
    public interface ICultureIntermediateConfigurator<T> : IPrinterConfigurator<T>
    {
        public ICultureConfigurator<T> And { get; }
    }
}