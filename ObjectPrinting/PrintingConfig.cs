using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes;
        private readonly HashSet<Type> exceptTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> exceptMembers = new HashSet<MemberInfo>();
        private readonly Dictionary<Type, Delegate> alternativeSerializers = new Dictionary<Type, Delegate>();
        private readonly Dictionary<MemberInfo, Delegate> memberSerializers = new Dictionary<MemberInfo, Delegate>();

        public PrintingConfig()
        {
            finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
        }

        public void AddAlternativeTypeSerializer(Type type, Delegate serializer)
        {
            if (alternativeSerializers.ContainsKey(type))
            {
                alternativeSerializers[type] = serializer;
            }
            else
            {
                alternativeSerializers.Add(type, serializer);
            }
        }

        public void AddAlternativeMemberSerializer(MemberInfo member, Delegate serializer)
        {
            if (memberSerializers.ContainsKey(member))
            {
                memberSerializers[member] = serializer;
            }
            else
            {
                memberSerializers.Add(member, serializer);
            }
        }

        public TypeConfig<TOwner, TProperty> Serialize<TProperty>()
        {
            return new TypeConfig<TOwner, TProperty>(this);
        }

        public PropertyConfig<TOwner> Serialize<TMember>(Expression<Func<TOwner, TMember>> selector)
        {
            return new PropertyConfig<TOwner>(this, (selector.Body as MemberExpression)?.Member);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> ExceptType<TExcept>()
        {
            exceptTypes.Add(typeof(TExcept));
            return this;
        }

        public PrintingConfig<TOwner> ExceptMember<TMember>(Expression<Func<TOwner, TMember>> selector)
        {
            exceptMembers.Add((selector.Body as MemberExpression)?.Member);
            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var members = type.GetProperties().Concat<MemberInfo>(type.GetFields());
            foreach (var memberInfo in members
                .Where(m => !exceptTypes.Contains(GetTypeOfMember(m)) && !exceptMembers.Contains(m)))
            {
                sb.Append(indentation + memberInfo.Name + " = " + GetSerialization(obj, memberInfo, nestingLevel));
            }

            return sb.ToString();
        }

        private static Type GetTypeOfMember(MemberInfo memberInfo)
        {
            return memberInfo switch
            {
                FieldInfo fieldInfo => fieldInfo.FieldType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                _ => throw new ArgumentException()
            };
        }

        private static object GetValueOfMember(MemberInfo memberInfo, object obj)
        {
            return memberInfo switch
            {
                FieldInfo fieldInfo => fieldInfo.GetValue(obj),
                PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
                _ => throw new ArgumentException()
            };
        }

        private string GetSerialization(object obj, MemberInfo propertyInfo, int nestingLevel)
        {
            if (obj == null)
            {
                return "null" + Environment.NewLine;
            }
            
            
            if (memberSerializers.ContainsKey(propertyInfo))
            {
                return memberSerializers[propertyInfo].DynamicInvoke(propertyInfo) + Environment.NewLine;
            }

            if (alternativeSerializers.ContainsKey(GetTypeOfMember(propertyInfo)))
            {
                return alternativeSerializers[GetTypeOfMember(propertyInfo)].DynamicInvoke(GetValueOfMember(propertyInfo, obj)) +
                       Environment.NewLine;
            }
            
            var type = GetTypeOfMember(propertyInfo);
            if (finalTypes.Contains(type))
            {
                return GetValueOfMember(propertyInfo, obj) + Environment.NewLine;
            }

            return PrintToString(GetValueOfMember(propertyInfo, obj), nestingLevel + 1);
        }
    }
}