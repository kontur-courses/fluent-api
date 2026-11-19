using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly ObjectSerializer serializer;

        internal PrintingConfig(PrintingSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            Settings = settings;
            serializer = new ObjectSerializer(Settings);
        }

        internal PrintingSettings Settings { get; }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            Settings.ExcludeType(typeof(T));

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TValue>(
            Expression<Func<TOwner, TValue>> memberSelector)
        {
            var member = MemberSelector.GetMember(memberSelector);

            Settings.ExcludeMember(member);

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

            return new PropertyPrintingConfig<TOwner, TValue>(this, member);
        }

        public string PrintToString(TOwner obj)
        {
            return serializer.PrintToString(obj);
        }
    }
}