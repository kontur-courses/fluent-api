using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Printer
{
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<Type, IPrinter> typeCustomSerializers = new();
        private readonly Dictionary<MemberInfo, IPrinter> memberCustomSerializers = new();

        private readonly List<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();

        private readonly HashSet<object> serializedObjects = new();

        private readonly IReadOnlyList<Type> finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(Enum)
        };

        public string PrintToString(TOwner obj, int recursionLimit = 50)
        {
            return PrintToString(obj, 0, recursionLimit);
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner, TType> For<TType>()
        {
            var newConfig = new PrintingConfig<TOwner, TType>(this);
            typeCustomSerializers[typeof(TType)] = newConfig;
            return newConfig;
        }

        public PrintingConfig<TOwner, TMember> ForMember<TMember>(Expression<Func<TOwner, TMember>> memberSelector)
        {
            var config = new PrintingConfig<TOwner, TMember>(this);
            var memberName = memberSelector.Body.ToString().Split('.').Last();
            var members = typeof(TOwner).GetMember(memberName)
                .Where(member => member.MemberType is MemberTypes.Field or MemberTypes.Property);

            if (!members.Any())
            {
                throw new ArgumentException($"Field or property with name {memberName} is not found");
            }

            foreach (var memberInfo in members)
            {
                memberCustomSerializers[memberInfo] = config;
            }

            return config;
        }

        public PrintingConfig<TOwner> For<TType>(Func<TType, string> serializer)
        {
            var config = new PrintingConfig<TOwner, TType>(this);
            config.SetCustomSerializing(serializer);
            typeCustomSerializers[typeof(TType)] = config;

            return this;
        }

        public PrintingConfig<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            var memberName = memberSelector.Body.ToString().Split('.').Last();
            var members = typeof(TOwner).GetMember(memberName)
                .Where(member => member.MemberType is MemberTypes.Field or MemberTypes.Property);

            if (!members.Any())
            {
                throw new ArgumentException($"Field or property with name {memberName} is not found");
            }

            excludedMembers.UnionWith(members);

            return this;
        }


        private string PrintToString(object obj, int nestingLevel, int recursionLimit = 50)
        {
            if (obj is null)
            {
                return "null";
            }

            if (nestingLevel >= recursionLimit)
            {
                return string.Empty;
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();

            if (typeCustomSerializers.ContainsKey(type))
            {
                sb.Append(typeCustomSerializers[type].PrintObject(obj));
                return sb.ToString();
            }

            if (finalTypes.Contains(obj.GetType()))
            {
                return obj.ToString();
            }

            sb.Append(type.Name + Environment.NewLine);
            if (!serializedObjects.Add(obj))
            {
                sb.Append($"({obj.GetHashCode()})");
                return sb.Replace("\r", "").Replace("\n", "").ToString();
            }

            foreach (var memberInfo
                in type.GetMembers().Where(member => member.MemberType is MemberTypes.Field or MemberTypes.Property))
            {
                if (TryAppendMember(memberInfo, obj, nestingLevel, out var result))
                {
                    sb.Append(identation + result + Environment.NewLine);
                }
            }

            return sb.ToString();
        }

        private bool TryAppendMember(MemberInfo memberInfo, object obj, int nestingLevel, out string stringMember)
        {
            if (excludedMembers.Contains(memberInfo))
            {
                stringMember = string.Empty;

                return false;
            }

            var name = memberInfo.Name;
            object value;
            Type memberType;

            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    value = fieldInfo.GetValue(obj);
                    memberType = fieldInfo.FieldType;
                    break;

                case PropertyInfo propertyInfo:
                    value = propertyInfo.GetValue(obj);
                    memberType = propertyInfo.PropertyType;
                    break;

                default:
                    throw new ArgumentException();
            }

            if (!excludedTypes.Contains(memberType))
            {
                if (!memberCustomSerializers.ContainsKey(memberInfo))
                {
                    stringMember = name + " = " + PrintToString(value, nestingLevel + 1);
                }
                else
                {
                    stringMember = name + " = " + memberCustomSerializers[memberInfo].PrintObject(value);
                }

                return true;
            }

            stringMember = string.Empty;
            return false;
        }
    }

    public class PrintingConfig<TOwner, TType> : IPrinter
    {
        private readonly PrintingConfig<TOwner> parent;

        public PrintingConfig(PrintingConfig<TOwner> parent)
        {
            this.parent = parent;
        }

        private Func<TType, string> Serializer { get; set; }

        public PrintingConfig<TOwner, TType> SetCustomSerializing(Func<TType, string> serializer)
        {
            Serializer = serializer;
            return this;
        }

        public PrintingConfig<TOwner> Apply() => parent;

        public string PrintObject(object obj) => Serializer((TType)obj);
    }
}