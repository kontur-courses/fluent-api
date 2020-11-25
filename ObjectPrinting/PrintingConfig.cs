using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> _excludedTypes = new HashSet<Type>();

        private readonly HashSet<MemberInfo> _excludedMembers = new HashSet<MemberInfo>();

        private readonly Dictionary<Type, Delegate> _specSerializationTypes = new Dictionary<Type, Delegate>();

        private readonly Dictionary<MemberInfo, Delegate> _specSerializationMembers = new Dictionary<MemberInfo, Delegate>();

        HashSet<Type> IPrintingConfig<TOwner>.ExcludedTypes => _excludedTypes;

        HashSet<MemberInfo> IPrintingConfig<TOwner>.ExcludedMembers => _excludedMembers;

        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.SpecialSerializationTypes => _specSerializationTypes;

        Dictionary<MemberInfo, Delegate> IPrintingConfig<TOwner>.SpecialSerializationMembers =>
            _specSerializationMembers;

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new MemberPrintingConfig<TOwner, TPropType>(this);
        }

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;
            return new MemberPrintingConfig<TOwner, TPropType>(this, member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;
            _excludedMembers.Add(member);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            var s = new Serializer<TOwner>(this);
            return s.Serialize(obj, 0);
        }
    }
}