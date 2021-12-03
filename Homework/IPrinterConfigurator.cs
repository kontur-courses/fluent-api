using Homework.CultureContexts;
using Homework.IgnoreContexts;
using Homework.SerialisationContexts;

namespace Homework
{
    public interface IPrinterConfigurator<TOwner>
    {
        public ICultureConfigurator<TOwner> SetCulture();
        public IIgnoreConfigurator<TOwner> Ignore();
        public ISerialisationTargetConfigurator<TOwner> SetAlternativeSerialisation();
        public ObjectPrinter<TOwner> Configure();
    }
}