using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        internal GlobalConfig Config = new GlobalConfig();

        public PrintingConfig()
        {
            Config.FinalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
        }

        public PrintingConfig<TOwner> Excluding<TExcluded>()
        {
            TypeExtensions.GetAllMembersOfType<TOwner, TExcluded>(Config.FinalTypes)
                .ForEach(m => Config.ExcludedMembers.Add(m));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TExcluded>
            (Expression<Func<TOwner, TExcluded>> selector)
        {
            var member = TryGetMemberInfo(selector);
            Config.ExcludedMembers.Add(member);
            return this;
        }

        public TypePrintingConfig<TOwner, TPrinted> Printing<TPrinted>()
        {
            return new TypePrintingConfig<TOwner, TPrinted>(this);
        }

        public MemberPrintingConfig<TOwner, TPrinted> Printing<TPrinted>
            (Expression<Func<TOwner, TPrinted>> selector)
        {
            var member = TryGetMemberInfo(selector);
            return new MemberPrintingConfig<TOwner, TPrinted>(this, member);
        }

        public PrintingConfig<TOwner> WithDefaultCutToLength(int length)
        {
            Config.DefaultCutLength = length;
            return this;
        }

        public PrintingConfig<TOwner> WithDefaultCulture(CultureInfo culture)
        {
            Config.DefaultCulture =  culture;
            return this;
        }

        public string PrintToString(TOwner obj) 
            => new Printer(Config).PrintToString(obj);

        private MemberInfo TryGetMemberInfo<TMember>(Expression<Func<TOwner, TMember>> selector)
        {
            var selectorBody = selector.Body as MemberExpression;
            if (selectorBody == null)
                throw new InvalidCastException("Selector should be a MemberExpression");
            var member = selectorBody.Member;
            if(!member.IsSerializedMemberType())
                throw new InvalidCastException("Selector should select Field or Property");
            return selectorBody.Member;
        }
    }
}