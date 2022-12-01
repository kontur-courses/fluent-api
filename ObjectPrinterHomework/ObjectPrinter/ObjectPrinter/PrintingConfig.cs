using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinter.ObjectPrinter
{
    public class PrintingConfig<TOwner>
    {
        public readonly List<Type> ExcludingTypes = new();
        public readonly List<MemberInfo> ExcludingMembers = new();
        public readonly Dictionary<Type, CultureInfo> CultureForTypes = new();
        public readonly Dictionary<MemberInfo, Func<object?, string>> CustomMemberSerializer = new();
        public readonly Dictionary<Type, Func<object?, string>> CustomTypeSerializer = new();
        public readonly Dictionary<MemberInfo, int> TrimForMembers = new();
        
        
        public MemberConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new(this);
        }

        public MemberConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = ((MemberExpression) memberSelector.Body).Member;
            return new MemberConfig<TOwner, TPropType>(memberInfo, this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            ExcludingMembers.Add(((MemberExpression) memberSelector.Body).Member);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            ExcludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string? PrintToString(TOwner obj)
        {
            return new Printer<TOwner>(this).PrintToString(obj, 0);
        }
    }
}