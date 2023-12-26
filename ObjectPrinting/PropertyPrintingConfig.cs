using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TParent, TOwner>
    {
        public PrintingConfig<TParent> Parent => parent;

        private readonly PrintingConfig<TParent> parent;
        private readonly Configuration configuration;
        private readonly Predicate selector;

        public PropertyPrintingConfig
            (PrintingConfig<TParent> parent,
            Configuration configuration,
            Predicate selector)
        {
            this.parent = parent;
            this.configuration = configuration;
            this.selector = selector;
        }

        public PrintingConfig<TParent> Using(Func<TOwner, string> toString)
        {
            Func<object, string> serializer = o => toString((TOwner)o); 

            configuration.Serializers.Add((selector, serializer));

            return parent;
        }
    }
}
