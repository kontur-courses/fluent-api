using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Reflection;
namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();

        private readonly Dictionary<Type, Delegate> typeSerializers = new Dictionary<Type, Delegate>();
        private readonly Dictionary<Type, CultureInfo> typeCultures = new Dictionary<Type, CultureInfo>();

        private readonly Dictionary<MemberInfo, Delegate> memberSerializers = new Dictionary<MemberInfo, Delegate>();
        private readonly Dictionary<MemberInfo, int> memberTrimLengths = new Dictionary<MemberInfo, int>();
        private readonly HashSet<Type> finalTypes =
        [
            typeof(int), typeof(double), typeof(float), typeof(long), typeof(short), typeof(string),
            typeof(byte), typeof(decimal), typeof(bool), typeof(DateTime), typeof(TimeSpan)
        ];
        internal IReadOnlyCollection<Type> ExcludedTypes => excludedTypes;
        internal IReadOnlyCollection<MemberInfo> ExcludedMembers => excludedMembers;
        internal IReadOnlyDictionary<Type, Delegate> TypeSerializers => typeSerializers;
        internal IReadOnlyDictionary<Type, CultureInfo> TypeCultures => typeCultures;
        internal IReadOnlyDictionary<MemberInfo, Delegate> MemberSerializers => memberSerializers;
        internal IReadOnlyDictionary<MemberInfo, int> MemberTrimLengths => memberTrimLengths;

        internal IReadOnlyCollection<Type> FinalTypes => finalTypes;
        public PrintingConfig<TOwner> Excluding<TProp>()
        {
            excludedTypes.Add(typeof(TProp));
            return this;
        }
        public PrintingConfig<TOwner> Excluding<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
        {
            var member = GetMemberInfo(memberSelector);
            excludedMembers.Add(member);
            return this;
        }
        internal void SetTypeSerializer<TProp>(Func<TProp, string> serializer)
        {
            typeSerializers[typeof(TProp)] = serializer;
        }

        internal void SetTypeCulture<TProp>(CultureInfo culture)
        {
            typeCultures[typeof(TProp)] = culture;
        }

        internal void SetMemberSerializer<TProp>(MemberInfo member, Func<TProp, string> serializer)
        {
            memberSerializers[member] = serializer;
        }

        internal void SetMemberTrimLength(MemberInfo member, int length)
        {
            memberTrimLengths[member] = length;
        }
        public TypePrintingConfig<TOwner, TProp> Printing<TProp>()
        {
            return new TypePrintingConfig<TOwner, TProp>(this);
        }

        public MemberPrintingConfig<TOwner, TProp> Printing<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
        {
            var member = GetMemberInfo(memberSelector);
            return new MemberPrintingConfig<TOwner, TProp>(this, member);
        }

        public MemberPrintingConfigForString<TOwner> Printing(Expression<Func<TOwner, string>> memberSelector)
        {
            var member = GetMemberInfo(memberSelector);
            return new MemberPrintingConfigForString<TOwner>(this, member);
        }
        public string PrintToString(TOwner obj)
        {
            return new Serializer<TOwner>(this).Serialize(obj);
        }
        private static MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member;
            }
            throw new ArgumentException("Expression is not a member access", nameof(memberSelector));
        }
    }
}