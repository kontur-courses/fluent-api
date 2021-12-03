using System;
using System.Text;

namespace ObjectPrinting
{
    public class CycleConfig<TOwner>
    {
        private bool shouldThrow = false;
        private readonly PrintingConfig<TOwner> parentConfig;
        private string cycleText = "...";
        
        public CycleConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        internal StringBuilder AppendCycleText(StringBuilder builder)
        {
            if (shouldThrow)
                throw new Exception("Cycle found");
            builder.Append(cycleText);
            return builder;
        }

        public PrintingConfig<TOwner> Throw()
        {
            this.shouldThrow = true;
            return parentConfig;
        }
        
        public PrintingConfig<TOwner> NotThrow()
        {
            this.shouldThrow = false;
            return parentConfig;
        }

        public PrintingConfig<TOwner> AddText(string onCycleText)
        {
            this.cycleText = onCycleText;
            return parentConfig;
        }
    }
}