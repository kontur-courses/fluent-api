using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public class CycleReferencePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;

        public CycleReferencePrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Skip()
        {
            return new PrintingConfig<TOwner>(parentConfig, true);
        }

        public PrintingConfig<TOwner> Throw()
        {
            return new PrintingConfig<TOwner>(parentConfig, shouldCyclesThrow: true);
        }

        public PrintingConfig<TOwner> ShowMessage(string message)
        {
            return new PrintingConfig<TOwner>(parentConfig, 
                shouldShowMessage: true, 
                message: message);
        }
    }
}
