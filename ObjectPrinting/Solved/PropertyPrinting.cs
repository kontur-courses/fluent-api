using ObjectPrinting.Solved.Interfaces;
using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public class PropertyPrinting<TOwner, TPropType> : IPropertyPrinting<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> parent;
        private readonly string memberName;

        public PropertyPrinting(PrintingConfig<TOwner> parent, string memberName)
        {
            this.parent = parent;
            this.memberName = memberName;
        }

        public PrintingConfig<TOwner> ParentConfig => parent;

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            parent.MemberSerializers[memberName] = o => serializer((TPropType)o);
            return parent;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            if (!typeof(IFormattable).IsAssignableFrom(typeof(TPropType)))
                throw new InvalidOperationException(
                    $"Type {typeof(TPropType).Name} does not implement IFormattable.");
            
            parent.MemberCultures[memberName] = culture;
            return parent;
        }
    }
}