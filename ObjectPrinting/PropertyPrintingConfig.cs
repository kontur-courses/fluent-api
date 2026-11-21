using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProp>
    {
        private PrintingConfig<TOwner> _config;
        private MemberInfo _member;

        public PropertyPrintingConfig(PrintingConfig<TOwner> config, MemberInfo member)
        {
            _config = config;
            _member = member;
        }

        public PrintingConfig<TOwner> Using(Func<TProp, string> serializer)
        {
            _config.MemberSerializers[(_member.DeclaringType, _member.Name)] = o => serializer((TProp)o);
            return _config;
        }

        public string PrintToString(TOwner obj)
        {
            // Нужно для возможности терминального вызова
            return _config.PrintToString(obj);
        }

        public PrintingConfig<TOwner> Exclude()
        {
            _config.ExcludedMembers.Add((_member.DeclaringType, _member.Name));
            return _config;
        }

        public static implicit operator PrintingConfig<TOwner>(PropertyPrintingConfig<TOwner, TProp> cfg)
        {
            return cfg._config;
        }
    }
}
