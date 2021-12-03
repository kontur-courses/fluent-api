using System;
using System.Collections;
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
        private readonly HashSet<object> serializedObjects = new HashSet<object>();
        private readonly HashSet<Type> exceptTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> exceptMembers = new HashSet<MemberInfo>();
        private readonly Dictionary<Type, Delegate> alternativeSerializers = new Dictionary<Type, Delegate>();
        private readonly Dictionary<MemberInfo, Delegate> memberSerializers = new Dictionary<MemberInfo, Delegate>();

        private readonly Dictionary<MemberInfo, (int start, int end)> serializationsBounds =
            new Dictionary<MemberInfo, (int, int)>();

        public PrintingConfig()
        {
            finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan),
                typeof(Guid)
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

        public void AddSerializationBounds(MemberInfo member, int start, int end)
        {
            var bounds = (start, end);
            if (memberSerializers.ContainsKey(member))
            {
                serializationsBounds[member] = bounds;
            }
            else
            {
                serializationsBounds.Add(member, bounds);
            }
        }

        public TypeConfig<TOwner, TProperty> Serialize<TProperty>()
        {
            return new TypeConfig<TOwner, TProperty>(this);
        }

        public MemberConfig<TOwner, TMember> Serialize<TMember>(Expression<Func<TOwner, TMember>> selector)
        {
            return new MemberConfig<TOwner, TMember>(this, (selector.Body as MemberExpression)?.Member);
        }

        public string PrintToString(TOwner obj)
        {
            return Print(obj, 0);
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

        private string Print(object obj, int nestingLevel)
        {
            if (TrySerializeObject(obj, nestingLevel, out var result))
            {
                return result;
            }

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var members = type.GetProperties().Concat<MemberInfo>(type.GetFields());
            foreach (var memberInfo in members.Where(m =>
                !exceptTypes.Contains(GetTypeOfMember(m)) && !exceptMembers.Contains(m)))
            {
                sb.Append(indentation + memberInfo.Name + " = " +
                          GetMemberSerialization(obj, memberInfo, nestingLevel));
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

        private string GetMemberSerialization(object obj, MemberInfo memberInfo, int nestingLevel)
        {
            string result;
            if (memberSerializers.ContainsKey(memberInfo))
            {
                result = memberSerializers[memberInfo].DynamicInvoke(memberInfo)?.ToString();
            }
            else if (alternativeSerializers.ContainsKey(GetTypeOfMember(memberInfo)))
            {
                result = alternativeSerializers[GetTypeOfMember(memberInfo)]
                    .DynamicInvoke(GetValueOfMember(memberInfo, obj))?.ToString();
            }
            else if (finalTypes.Contains(GetTypeOfMember(memberInfo)))
            {
                result = GetValueOfMember(memberInfo, obj).ToString();
            }
            else
            {
                result = Print(GetValueOfMember(memberInfo, obj), nestingLevel + 1);
            }

            if (serializationsBounds.ContainsKey(memberInfo))
            {
                var (start, end) = serializationsBounds[memberInfo];
                if (start < 0 || end >= result.Length || end < start)
                {
                    throw new ArgumentException();
                }

                result = result.Substring(start, end - start + 1);
            }

            return result + Environment.NewLine;
        }

        private bool TrySerializeObject(object obj, int nestingLevel, out string result)
        {
            result = null;
            if (obj == null)
            {
                result = "null";
                return true;
            }

            if (obj is IEnumerable enumerable)
            {
                result = GetCollectionSerialization(enumerable, nestingLevel);
                return true;
            }

            if (finalTypes.Contains(obj.GetType()))
            {
                result = obj.ToString();
                return true;
            }

            if (serializedObjects.Contains(obj))
            {
                result = "object with cyclic link";
                return true;
            }

            serializedObjects.Add(obj);
            return false;
        }

        private string GetCollectionSerialization(IEnumerable collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            if (collection is IDictionary dictionary)
            {
                return "Keys : " + GetCollectionSerialization(dictionary.Keys, nestingLevel) + " Values : " +
                       GetCollectionSerialization(dictionary.Values, nestingLevel);
            }

            sb.Append("[ ");
            foreach (var e in collection)
            {
                sb.Append(Print(e, nestingLevel) + " ");
            }

            sb.Append("]");
            return sb.ToString();
        }
    }
}