using System;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting.Core
{
    public class MemberPrintingConfig<TOwner, TMemberType> : IMemberPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> _parentConfig;
        private readonly string _propertyName;
        PrintingConfig<TOwner> IMemberPrintingConfig<TOwner>.ParentConfig => _parentConfig;

        public MemberPrintingConfig(PrintingConfig<TOwner> parentConfig, string propertyName = null)
        {
            _parentConfig = parentConfig;
            _propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> print)
        {
            throw new NotImplementedException();
        }
    }
}