using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        private readonly Dictionary<MemberInfo, IHasSerializationFunc> membersSerializesInfos =
            new Dictionary<MemberInfo, IHasSerializationFunc>();

        private readonly Dictionary<Type, IHasSerializationFunc> typeSerializesInfos =
            new Dictionary<Type, IHasSerializationFunc>();

        private Func<object, string> handleMaxRecursion;

        private int maxRecursion = 60;

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> WithMaxRecursion(int maxRecursion)
        {
            if (maxRecursion < 1)
                throw new ArgumentException();
            this.maxRecursion = maxRecursion;

            return this;
        }

        public PrintingConfig<TOwner> OnMaxRecursion(Func<object,string> func)
        {
            handleMaxRecursion = func;

            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> exclude)
        {
            if (exclude == null)
                throw new ArgumentNullException();

            var memberInfo = GetMemberInfo(exclude);
            excludedMembers.Add(memberInfo);

            return this;
        }

        private static MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> expression)
        {
            var memberExpression = expression.Body is UnaryExpression unaryExpression
                ? (MemberExpression)unaryExpression.Operand
                : (MemberExpression)expression.Body;

            return memberExpression.Member;
        }


        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            var serializer = new Serializer(
                excludedMembers,
                excludedTypes,
                membersSerializesInfos,
                typeSerializesInfos,
                maxRecursion,
                handleMaxRecursion);

            return serializer.Serialize(obj, nestingLevel);
        }

        public IUsing<TOwner, T> Printing<T>()
        {
            var config = new SerializationConfig<TOwner, T>(this);
            typeSerializesInfos[typeof(T)] = config;

            return config;
        }

        public IUsing<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> property)
        {
            if (property == null)
                throw new ArgumentNullException();

            var memberInfo = GetMemberInfo(property);

            var config = new SerializationConfig<TOwner, T>(this);
            membersSerializesInfos[memberInfo] = config;

            return config;
        }
    }
}