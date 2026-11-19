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

        internal HashSet<MemberKey> _excludingMember = new();
        internal HashSet<Type> _excludingType = new();

        internal Dictionary<MemberKey, Func<object, string>> _formaterMember = new();
        internal Dictionary<Type, Func<object, string>> _formaterType = new();


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
            _excludingMember.Add(MemberKey.From(GetMember(memberSelector)));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _excludingType.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
            => new Printer<TOwner>(this).PrintToString(obj, 0, 
                new HashSet<object>(ReferenceEqualityComparer.Instance));

        internal MemberInfo GetMember<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
            => ((MemberExpression)memberSelector.Body).Member;

        internal static IEnumerable<MemberInfo> GetMembers(Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            var properties = type.GetProperties(flags)
                .Where(p => p.GetIndexParameters().Length == 0);

            return properties.Cast<MemberInfo>().Concat(type.GetFields(flags));
        }

        internal static Type GetMemberType(MemberInfo member)
            => member is PropertyInfo property 
            ? property.PropertyType 
            : ((FieldInfo)member).FieldType;

        internal static object? GetValue(MemberInfo member, object obj)
            => member is PropertyInfo property 
            ? property.GetValue(obj) 
            : ((FieldInfo)member).GetValue(obj);

        internal bool IsMemberExcluded(MemberInfo member)
        {
            return _excludingMember.Contains(MemberKey.From(member))
                || _excludingType.Contains(GetMemberType(member));
        }

    }
}