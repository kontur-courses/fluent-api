using System;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting.Core
{
    public class MemberPrintingConfig<TOwner, TMemberType> : IMemberPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> _parentConfig;
        private readonly string _memberName;
        PrintingConfig<TOwner> IMemberPrintingConfig<TOwner>.ParentConfig => _parentConfig;

        public MemberPrintingConfig(PrintingConfig<TOwner> parentConfig, string memberName = null)
        {
            _parentConfig = parentConfig;
            _memberName = memberName;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> print)
        {
            throw new NotImplementedException();
        }
    }
}