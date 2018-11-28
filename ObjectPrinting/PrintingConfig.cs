using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.TypesSerializers;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        internal readonly IDictionary<Type, Delegate> TypesSerializers = new Dictionary<Type, Delegate>();
        internal readonly IDictionary<Type, CultureInfo> CustomCultures = new Dictionary<Type, CultureInfo>();
        internal readonly IDictionary<MemberInfo, Delegate> MembersSerializers =
            new Dictionary<MemberInfo, Delegate>();
        internal readonly IDictionary<MemberInfo, int> StringsTrimValues = new Dictionary<MemberInfo, int>();
        private readonly HashSet<MemberInfo> excludedSpecificMembers = new HashSet<MemberInfo>();
        private int maxElementsCountForEnumerables = Constants.MaxElementCountForCollection;

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        public PrintingConfig<TOwner> SetMaxElementsCountForEnumerables(int count)
        {
            maxElementsCountForEnumerables = count;

            return this;
        }

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new MemberPrintingConfig<TOwner, TPropType>(this);
        }

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> selector)
        {
            return new MemberPrintingConfig<TOwner, TPropType>(
                this,
                selector
                    .Body
                    .CastToMemberExpression()
                    .Member);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> selector)
        {
            excludedSpecificMembers.Add(
                selector
                    .Body
                    .CastToMemberExpression()
                    .Member);

            return this;
        }

        public string PrintToString(TOwner obj)
        {
            TypeSerializer typeSerializer = new NullSerializer();
            var membersSerializer = new MembersSerializer(
                excludedTypes.ToImmutableHashSet(),
                (IReadOnlyDictionary<Type, Delegate>)TypesSerializers,
                (IReadOnlyDictionary<Type, CultureInfo>)CustomCultures,
                (IReadOnlyDictionary<MemberInfo, Delegate>)MembersSerializers,
                (IReadOnlyDictionary<MemberInfo, int>)StringsTrimValues,
                excludedSpecificMembers.ToImmutableHashSet(),
                typeSerializer);

            typeSerializer.SetSuccessor(new FinalTypesSerializer()
                .SetSuccessor(new EnumerableSerializer(maxElementsCountForEnumerables, typeSerializer)
                    .SetSuccessor(membersSerializer)
                ));

            return PrintToString(typeSerializer, obj, 0, ImmutableHashSet<object>.Empty);
        }

        private string PrintToString(
            TypeSerializer typeSerializer,
            object obj,
            int nestingLevel,
            ImmutableHashSet<object> excludedValues)
        {
            return typeSerializer.Serialize(obj, nestingLevel, excludedValues);
        }
    }
}