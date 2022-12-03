using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Configuration
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> _typesForExcluding;
        private readonly HashSet<MemberInfo> _membersForExcluding;
        private readonly Dictionary<Type, Delegate> _typesAlternativeSerializer;
        private readonly Dictionary<MemberInfo, Delegate> _membersAlternativeSerializer;
        private bool _ignoreCyclicReferences;


        public HashSet<Type> TypesForExcluding => _typesForExcluding;
        public HashSet<MemberInfo> MembersForExcluding => _membersForExcluding;
        public Dictionary<Type, Delegate> TypesAlternativeSerializer => _typesAlternativeSerializer;
        public Dictionary<MemberInfo, Delegate> MembersAlternativeSerializer => _membersAlternativeSerializer;
        public bool IgnoreCyclicReferences => _ignoreCyclicReferences;


        public PrintingConfig()
        {
            _typesForExcluding = new HashSet<Type>();
            _membersForExcluding = new HashSet<MemberInfo>();
            _typesAlternativeSerializer = new Dictionary<Type, Delegate>();
            _membersAlternativeSerializer = new Dictionary<MemberInfo, Delegate>();
        }


        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, null);
        }


        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, ((MemberExpression)memberSelector.Body).Member);
        }

        public PrintingConfig<TOwner> AddAlternativeTypeSerializer<TMember>(Delegate alternativeSerializer)
        {
            _typesAlternativeSerializer[typeof(TMember)] = alternativeSerializer;
            return this;
        }

        public PrintingConfig<TOwner> AddAlternativeTypeSerializer(MemberInfo memberInfo, Delegate alternativeSerializer)
        {
            _membersAlternativeSerializer[memberInfo] = alternativeSerializer;
            return this;
        }


        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            _membersForExcluding.Add(((MemberExpression)memberSelector.Body).Member);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _typesForExcluding.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> IgnoringCyclicReferences(bool ignore = true)
        {
            _ignoreCyclicReferences = ignore;
            return this;
        }

        public ObjectPrinter<TOwner> Build()
        {
            return new ObjectPrinter<TOwner>(this);
        }
    }
}