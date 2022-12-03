using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Configuration
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        public HashSet<Type> TypesForExcluding { get; private set; }
        public HashSet<MemberInfo> MembersForExcluding { get; private set; }
        public Dictionary<Type, Delegate> TypesAlternativeSerializer { get; private set; }
        public Dictionary<MemberInfo, Delegate> MembersAlternativeSerializer { get; private set; }
        public bool IgnoreCyclicReferences { get; private set; }


        public PrintingConfig()
        {
            TypesForExcluding = new HashSet<Type>();
            MembersForExcluding = new HashSet<MemberInfo>();
            TypesAlternativeSerializer = new Dictionary<Type, Delegate>();
            MembersAlternativeSerializer = new Dictionary<MemberInfo, Delegate>();
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
            TypesAlternativeSerializer[typeof(TMember)] = alternativeSerializer;
            return this;
        }

        public PrintingConfig<TOwner> AddAlternativeTypeSerializer(MemberInfo memberInfo, Delegate alternativeSerializer)
        {
            MembersAlternativeSerializer[memberInfo] = alternativeSerializer;
            return this;
        }


        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            MembersForExcluding.Add(((MemberExpression)memberSelector.Body).Member);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            TypesForExcluding.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> IgnoringCyclicReferences(bool ignore = true)
        {
            IgnoreCyclicReferences = ignore;
            return this;
        }

        public ObjectPrinter<TOwner> Build()
        {
            return new ObjectPrinter<TOwner>(this);
        }
    }
}