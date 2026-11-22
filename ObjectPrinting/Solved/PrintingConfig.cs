using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        internal HashSet<Type> ExcludedTypes { get; } = new();
        internal HashSet<string> ExcludedMembers { get; } = new();

        internal Dictionary<Type, Func<object, string>> TypeSerializers { get; } = new();
        internal Dictionary<string, Func<object, string>> MemberSerializers { get; } = new();

        internal Dictionary<Type, CultureInfo> TypeCultures { get; } = new();
        internal Dictionary<string, CultureInfo> MemberCultures { get; } = new();

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            ExcludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            ExcludedMembers.Add(GetMemberName(memberSelector));
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
            => new PropertyPrintingConfig<TOwner, TPropType>(this, null);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
            => new PropertyPrintingConfig<TOwner, TPropType>(
                this, GetMemberName(memberSelector));

        public string PrintToString(TOwner obj, int maxDepth = int.MaxValue)
            => new ObjectSerializer<TOwner>(this, maxDepth).Serialize(obj);

        private static string GetMemberName<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            throw new ArgumentException("Selector must be a member expression.");
        }
    }
}