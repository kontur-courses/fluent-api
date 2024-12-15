using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TParent, TProperty>: PrintingConfig<TParent>
    {
        private readonly PrintingConfig<TParent> parentConfig;

        public PropertyPrintingConfig(PrintingConfig<TParent> parent) : base(parent)
        {
            parentConfig = parent;
        }

        public PrintingConfig<TParent> ToParentObjectConfig() => parentConfig;
    }
}
