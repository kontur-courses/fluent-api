using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> customExcludedTypes = new List<Type>();
        private readonly List<MemberInfo> customExcludedProperties = new List<MemberInfo>();
        private readonly Dictionary<Type, Delegate> customSerializedByType = new Dictionary<Type, Delegate>();

        private readonly Dictionary<MemberInfo, Delegate> customSerializedByMemberInfo =
            new Dictionary<MemberInfo, Delegate>();

        private readonly HashSet<object> customOpenObjects = new HashSet<object>();

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PropertyPrintingConfig<TOwner, TMemberType> Printing<TMemberType>()
        {
            return new PropertyPrintingConfig<TOwner, TMemberType>(this, null);
        }

        public PropertyPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TMemberType>(this, GetMemberInfo(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (memberSelector != null)
            {
                customExcludedProperties.Add(GetMemberInfo(memberSelector));
            }
            else
            {
                if (!typeof(TMemberType).IsPublic)
                {
                    throw new ArgumentException($"Exclusion of non-public type {typeof(TMemberType)} is not allowed.");
                }

                customExcludedTypes.Add(typeof(TMemberType));
            }

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>()
        {
            customExcludedTypes.Add(typeof(TMemberType));
            return this;
        }

        private static MemberInfo GetMemberInfo<TOwner, TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
            {
                if (!IsPropertyOrFieldMember(memberExpression.Member))
                    throw new ArgumentException($"The expression '{memberSelector}' refers to an unsupported member. Only properties and fields are supported.");

                return memberExpression.Member;
            }

            throw new ArgumentException($"The expression '{memberSelector}' must refer to a property or field.");
        }

        private static bool IsPrintableType(Type type)
        {
            return type == typeof(string) || type.IsPrimitive || typeof(IFormattable).IsAssignableFrom(type);
        }

        private string PrintToString(object obj, int indentationLevel)
        {
            var newLine = Environment.NewLine;

            if (obj == null)
                return "null" + newLine;

            var objectType = obj.GetType();

            if (IsPrintableType(objectType))
                return obj + newLine;

            var builder = new StringBuilder();
            if (customOpenObjects.Any(x => ReferenceEquals(x, obj)))
            {
                builder.AppendLine("(Cycle)" + objectType.FullName);
            }
            else
            {
                customOpenObjects.Add(obj);
                builder.AppendLine(objectType.Name);

                if (IsDictionary(objectType) || IsArrayOrList(objectType))
                {
                    builder.Append(SerializeEnumerable(obj, indentationLevel));
                }
                else
                {
                    builder.Append(PrintPropertiesAndFields(obj, indentationLevel, objectType));
                }

                customOpenObjects.Remove(obj);
            }

            return builder.ToString();
        }

        private static Type GetMemberType(MemberInfo member)
        {
            if (!IsPropertyOrFieldMember(member))
                throw new ArgumentException($"Member '{member}' refers to a method, which is not supported. Please specify a property or field.");
            return (member as FieldInfo)?.FieldType ?? (member as PropertyInfo)?.PropertyType;
        }

        private static object GetMemberValue(MemberInfo member, object obj)
        {
            if (!IsPropertyOrFieldMember(member))
                throw new ArgumentException($"Member '{member}' refers to a method, which is not supported. Please specify a property or field.");

            return (member as FieldInfo)?.GetValue(obj)
                ?? (member as PropertyInfo)?.GetValue(obj);
        }
        private string PrintPropertiesAndFields(object obj, int indentationLevel, Type type)
        {
            var indentation = new string('\t', indentationLevel + 1);
            var builder = new StringBuilder();

            foreach (var memberInfo in type.GetMembers()
                .Where(prop => IsPropertyOrFieldMember(prop) && !IsExcluded(prop)))
            {
                if (TrySerializeMember(obj, memberInfo, GetMemberType(memberInfo), out var serializedValue))
                {
                    builder.AppendLine($"{indentation}{memberInfo.Name} = {serializedValue}");
                    continue;
                }

                if (IsDictionary(GetMemberType(memberInfo)))
                {
                    builder.AppendLine($"{indentation}{memberInfo.Name} = ");
                    builder.Append(SerializeDictionary(GetMemberValue(memberInfo, obj), indentationLevel + 1));
                }
                else if (IsArrayOrList(GetMemberType(memberInfo)))
                {
                    builder.AppendLine($"{indentation}{memberInfo.Name} = ");
                    builder.Append(SerializeEnumerable(GetMemberValue(memberInfo, obj), indentationLevel + 1));
                }
                else
                {
                    builder.Append(
                        $"{indentation}{memberInfo.Name} = " +
                        $"{PrintToString(GetMemberValue(memberInfo, obj), indentationLevel + 1)}"
                    );
                }
            }

            return builder.ToString();
        }

        private static bool IsPropertyOrFieldMember(MemberInfo member)
        {
            return member.MemberType is MemberTypes.Field or MemberTypes.Property;
        }

        private bool TrySerializeMember(object obj, MemberInfo memberInfo, Type propertyType, out string serializedValue)
        {
            serializedValue = null;
            Delegate valueToUse = GetDelegateForMember(memberInfo, propertyType);

            return valueToUse != null && TrySerializeValue(valueToUse, GetMemberValue(memberInfo, obj), out serializedValue);
        }

        private Delegate GetDelegateForMember(MemberInfo memberInfo, Type propertyType)
        {
            if (customSerializedByMemberInfo.TryGetValue(memberInfo, out var propertyValue))
                return propertyValue;

            if (customSerializedByType.TryGetValue(propertyType, out var typeValue))
                return typeValue;

            return null;
        }

        private static bool TrySerializeValue(Delegate serializer, object value, out string serializedValue)
        {
            try
            {
                serializedValue = serializer.DynamicInvoke(value)?.ToString();
                return true;
            }
            catch
            {
                serializedValue = null;
                return false;
            }
        }

        private static bool IsArrayOrList(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type) && !IsPrintableType(type);
        }

        private static bool IsDictionary(Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type);
        }

        private string SerializeEnumerable(object obj, int indentationLevel)
        {
            var enumerable = obj as IEnumerable;
            var builder = new StringBuilder();
            var indentation = new string('\t', indentationLevel + 1);

            if (enumerable == null) 
                return $"{indentation}null {Environment.NewLine}";

            foreach (var value in enumerable)
            {
                builder.Append(indentation);
                builder.Append(PrintToString(value, indentationLevel + 1));
            }

            return builder.ToString();
        }

        private string SerializeDictionary(object obj, int indentationLevel)
        {
            var dictionary = obj as IDictionary;
            var builder = new StringBuilder();
            var indentation = new string('\t', indentationLevel + 1);

            if (dictionary == null)
                return $"{indentation}null{Environment.NewLine}";

            foreach (DictionaryEntry keyVal in dictionary)
            {
                builder.Append(indentation);
                var key = keyVal.Key;
                var value = keyVal.Value;
                builder.Append($"{PrintToString(key, indentationLevel)} : {PrintToString(value, indentationLevel + 1)}");
            }

            return builder.ToString();
        }

        private bool IsExcluded(MemberInfo memberInfo)
        {
            return customExcludedProperties.Contains(memberInfo) || customExcludedTypes.Contains(GetMemberType(memberInfo));
        }

        public void AddSerializeMember<TMemberType>(Func<TMemberType, string> print, MemberInfo memberInfo)
        {
            customSerializedByMemberInfo.Add(memberInfo, print);
        }

        public void AddSerializeByType<TMemberType>(Type type, Func<TMemberType, string> print)
        {
            customSerializedByType.Add(type, print);
        }
    }
}