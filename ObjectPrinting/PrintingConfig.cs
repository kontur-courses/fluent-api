using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly PrintingSettings settings;
        private readonly ObjectSerializer serializer;

        public PrintingConfig()
            : this(new PrintingSettings())
        {
        }

        internal PrintingConfig(PrintingSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            this.settings = settings;
            serializer = new ObjectSerializer(settings);
        }

        internal PrintingSettings Settings => settings;

        public PrintingConfig<TOwner> Excluding<T>()
        {
            settings.ExcludeType(typeof(T));

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TValue>(
            Expression<Func<TOwner, TValue>> memberSelector)
        {
            var member = MemberSelector.GetMember(memberSelector);

            settings.ExcludeMember(member);

            return this;
        }

        public TypePrintingConfig<TOwner, TValue> Printing<TValue>()
        {
            return new TypePrintingConfig<TOwner, TValue>(this);
        }

        public PropertyPrintingConfig<TOwner, TValue> Printing<TValue>(
            Expression<Func<TOwner, TValue>> memberSelector)
        {
            var member = MemberSelector.GetMember(memberSelector);

            return new PropertyPrintingConfig<TOwner, TValue>(
                this,
                member);
        }

        public string PrintToString(TOwner obj)
        {
            return serializer.PrintToString(obj);
        }
    }
}