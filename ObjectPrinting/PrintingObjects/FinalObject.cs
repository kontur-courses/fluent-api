using System;

namespace ObjectPrinting
{
    public class FinalObject<T> : PrintingObject<T>
    {
        public FinalObject(object obj, IPrintingConfig<T> config) : base(obj, config) { }
        
        public override string Print(int nestingLevel) => ObjectForPrint + Environment.NewLine;
    }
}