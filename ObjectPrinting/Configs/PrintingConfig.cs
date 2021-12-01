using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Configs
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

        public PrintingConfig<TOwner> Excluding<TType>()
        {
            GetAllMembersOfType<TType>()
                .ForEach(m => Config.ExcludedMembers.Add(m));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TMember>
            (Expression<Func<TOwner, TMember>> selector)
        {
            var member = TryGetMemberInfo(selector);
            Config.ExcludedMembers.Add(member);
            return this;
        }

        public TypePrintingConfig<TOwner, TType> Printing<TType>()
        {
            return new TypePrintingConfig<TOwner, TType>(this, GetAllMembersOfType<TType>());
        }

        public MemberPrintingConfig<TOwner, TMember> Printing<TMember>
            (Expression<Func<TOwner, TMember>> selector)
        {
            var member = TryGetMemberInfo(selector);
            return new MemberPrintingConfig<TOwner, TMember>(this, member);
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

        private List<MemberInfo> GetAllMembersOfType<TType>()
        {
            var result = new List<MemberInfo>();
            foreach (var m in typeof(TOwner).GetSerializedMembers())
                AddMembers<TType>(m, result);
            return result;
        }

        private void AddMembers<TType>
            (MemberInfo member, List<MemberInfo> members)
        {
            var type = member.GetTypeOfPropertyOrField();
            if (members.Contains(member) || type == typeof(TOwner))
                return;
            if (type == typeof(TType))
                members.Add(member);
            if (((IList) Config.FinalTypes).Contains(type))
                return;
            foreach (var m in type.GetSerializedMembers())
                AddMembers<TType>(m, members);
        }
    }
}