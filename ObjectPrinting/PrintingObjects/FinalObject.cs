using System;

namespace ObjectPrinting
{
    public class FinalObject<T> : PrintingObject<T>
    {
        public override string Print(int nestingLevel)
        {
            return ObjectForPrint + Environment.NewLine;
        }

        public FinalObject(object obj, PrintingConfig<T> config) : base(obj, config)
        {
        }
    }
}