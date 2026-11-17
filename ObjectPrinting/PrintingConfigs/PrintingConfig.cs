using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.PrintingConfigs
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new ();
        private readonly HashSet<(Type, string)> excludedMembers = new ();
        private readonly Dictionary<Type, Delegate> typeSerializers = new ();
        private readonly Dictionary<(Type, string), Delegate> memberSerializers = new ();

        private static readonly Type[] finalTypes =
        {
            typeof(int), typeof(long), typeof(double), typeof(float), typeof(decimal),
            typeof(string), typeof(bool), typeof(Guid),
            typeof(DateTime), typeof(TimeSpan)
        };
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetMember(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedMembers.Add(Key(GetMember(memberSelector)));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }
        
        internal void SetTypeSerializer(Type type, Delegate serializer)
        {
            typeSerializers[type] = serializer;
        }

        internal void SetMemberSerializer(MemberInfo member, Delegate serializer)
        {
            memberSerializers[Key(member)] = serializer;
        }

        private static MemberInfo GetMember<TPropType>(Expression<Func<TOwner, TPropType>> selector)
        {
            if (selector.Body is MemberExpression memberExpression)
                return memberExpression.Member;
            throw new ArgumentException("Expression must select a property or field", nameof(selector));
        }

        private static (Type, string) Key(MemberInfo member) => (member.DeclaringType, member.Name);
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (typeSerializers.TryGetValue(type, out var typeSerializer))
                return (string)typeSerializer.DynamicInvoke(obj) + Environment.NewLine;

            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            foreach (var property in type.GetProperties())
            {
                if (excludedTypes.Contains(property.PropertyType) ||
                    excludedMembers.Contains(Key(property)))
                    continue;

                sb.Append(indentation + property.Name + " = " +
                          PrintMember(property, obj, nestingLevel));
            }

            return sb.ToString();
        }

        private string PrintMember(PropertyInfo property, object owner, int nestingLevel)
        {
            var value = property.GetValue(owner);
            
            if (value != null && memberSerializers.TryGetValue(Key(property), out var serializer))
                return (string)serializer.DynamicInvoke(value) + Environment.NewLine;

            return PrintToString(value, nestingLevel + 1);
        }
    }
}