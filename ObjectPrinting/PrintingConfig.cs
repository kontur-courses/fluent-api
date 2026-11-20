using ObjectPrinting.Methods;
using ObjectPrinting.Printing;
using ObjectPrinting.Printing.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {

        internal HashSet<Type> finalTypes = new()
        {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(bool),
                typeof(long), typeof(decimal), typeof(byte),
                typeof(sbyte), typeof(short)
        };

        internal PrintingRules Rules { get; } = new();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetMember(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            Rules.ExcludedMembers.Add(MemberKey.From(GetMember(memberSelector)));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            Rules.ExcludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
            => new Printer<TOwner>(this).PrintToString(obj, 0, 
                new HashSet<object>(ReferenceEqualityComparer.Instance));

        internal MemberInfo GetMember<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
            => ((MemberExpression)memberSelector.Body).Member;



    }
}