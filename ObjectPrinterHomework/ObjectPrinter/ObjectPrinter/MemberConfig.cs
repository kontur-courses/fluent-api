using System;
using System.Globalization;
using System.Reflection;
using ObjectPrinter.Interfaces;

namespace ObjectPrinter.ObjectPrinter
{
    
    public class MemberConfig<TOwner, TPropType> : IMemberConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> _printingConfig;
        private readonly MemberInfo _memberInfo;

        public MemberConfig(MemberInfo memberInfo, PrintingConfig<TOwner> printingConfig)
        {
            _printingConfig = printingConfig;
            _memberInfo = memberInfo;
        }

        public MemberConfig(PrintingConfig<TOwner> printingConfig)
        {
            _printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (_memberInfo != null)
                _printingConfig.CustomMemberSerializer.Add(_memberInfo, obj => print.Invoke((TPropType)obj!));
            else
                _printingConfig.CustomTypeSerializer.Add(typeof(TPropType), obj => print.Invoke((TPropType)obj!));
            return _printingConfig;
        }

        public PrintingConfig<TOwner> UsingTrim(int maxLen)
        {
            _printingConfig.TrimForMembers.Add(_memberInfo, maxLen);
            return _printingConfig;
        }
        
        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            _printingConfig.CultureForTypes.Add(typeof(TPropType), culture);
            return _printingConfig;
        }

        PrintingConfig<TOwner> IMemberConfig<TOwner>.ParentConfig => _printingConfig;
    }
    
}