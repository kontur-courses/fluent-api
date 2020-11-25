using System;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting.Core
{
    public class MemberPrintingConfig<TOwner, TMemberType> : IMemberPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> _parentConfig;
        private readonly ElementInfo _elementInfo;
        PrintingConfig<TOwner> IMemberPrintingConfig<TOwner>.ParentConfig => _parentConfig;

        public MemberPrintingConfig(PrintingConfig<TOwner> parentConfig, ElementInfo elementInfo = null)
        {
            _parentConfig = parentConfig;
            _elementInfo = elementInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TMemberType, string> print)
        {
            if (_elementInfo != null)
                ((IPrintingConfig) _parentConfig).AlternativeSerializationByElementsInfo[_elementInfo] =
                    print;
            else
                ((IPrintingConfig) _parentConfig).AlternativeSerializationByTypes[typeof(TMemberType)]
                    = print;
            return _parentConfig;
        }
    }
}