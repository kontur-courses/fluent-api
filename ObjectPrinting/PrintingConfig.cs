using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<MemberInfo> excludedProperties = new HashSet<MemberInfo>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        private readonly HashSet<FieldInfo> excludedFields = new HashSet<FieldInfo>();

        private readonly Dictionary<object, int> complexObjectLinks = new Dictionary<object, int>();
        private int maxRecursion = 2;

        private readonly HashSet<Type> finalTypes = new HashSet<Type>() 
        {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly Dictionary<Type, Func<object, string>> typeSerializesInfos =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<MemberInfo, Func<object, string>> membersSerializesInfos =
            new Dictionary<MemberInfo, Func<object, string>>();


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

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> exclude)
        {
            var memberInfo = GetMemberInfo(exclude);

            excludedProperties.Add(memberInfo);

            return this;
        }

        public PrintingConfig<TOwner> SetCulture<TPropType>(CultureInfo culture)
            where TPropType : IFormattable
        {
            return SerializeWith<TPropType>(
                p => p.ToString(null, culture));
        }

        public PrintingConfig<TOwner> SerializeWith<TPropType>(Func<TPropType, string> serialize)
        {
            Func<object, string> func = p => serialize((TPropType)p);

            if (!typeSerializesInfos.ContainsKey(typeof(TPropType)))
                typeSerializesInfos[typeof(TPropType)] = func;

            return this;
        }

        public PrintingConfig<TOwner> SerializeWith<TPropType>(
            Expression<Func<TOwner, TPropType>> property,
            Func<TPropType, string> serialize)
        {
            var memberInfo = GetMemberInfo(property);
            Func<object, string> func = p => serialize((TPropType)p);

            membersSerializesInfos[memberInfo] = func;

            return this;
        }

        private static MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> expression)
        {
            var memberExpression = expression.Body is UnaryExpression unaryExpression
                ? (MemberExpression)unaryExpression.Operand
                : (MemberExpression)expression.Body;

            return memberExpression.Member;
        }

        public PrintingConfig<TOwner> Trim(Expression<Func<TOwner, string>> property, int length)
        {
            Func<string, string> func = value => value.Length <= length
                ? value
                : value[..length];

            return SerializeWith(property, func);
        }

        public PrintingConfig<TOwner> Trim(int length)
        {
            Func<string, string> func = value => value.Length <= length
                ? value
                : value[..length];

            return SerializeWith(func);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return $"null{Environment.NewLine}";

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (MaxRecursionHasBeenReached(obj))
                return $"Maximum recursion has been reached{Environment.NewLine}";

            var indentation = string.Intern(new string('\t', nestingLevel + 1));
            
            var type = obj.GetType();
            var sb = new StringBuilder().AppendLine(type.Name);


            HandleMembers(type, sb, indentation, obj, nestingLevel);
        
           
                
                
            return sb.ToString();
        }

        private void HandleMembers(Type type, StringBuilder sb, string indentation, object obj, int nestingLevel)
        {
            foreach (var propertyInfo in type.GetProperties())
            {
                DataMember data = new DataMember(propertyInfo);

                HandleMember(sb, data, obj, indentation, nestingLevel);
            }

            foreach (var fieldInfo in type.GetFields())
            {
                DataMember data = new DataMember(fieldInfo);

                HandleMember(sb, data, obj, indentation, nestingLevel);
            }
        }

        private void HandleMember(StringBuilder sb, DataMember member, object obj, string indentation, int nestingLevel)
        {
            if (excludedProperties.Any(memberInfo => memberInfo.Name == member.Name) ||
                excludedTypes.Contains(member.Type))
                return;

            if (membersSerializesInfos.TryGetValue(member.MemberInfo, out var serializeMember))
            {
                sb.Append(GetSerializedString(obj, member, indentation, serializeMember));
                return;
            }

            if (typeSerializesInfos.TryGetValue(member.Type, out var serializeType))
            {
                sb.Append(GetSerializedString(obj, member, indentation, serializeType));
                return;
            }

            sb.Append(
                GetSerializedString(
                    obj, 
                    member, 
                    indentation, 
                    (value) => PrintToString(
                        value,
                        nestingLevel + 1),
                    false));
        }


        private bool MaxRecursionHasBeenReached(object obj)
        {
            complexObjectLinks.TryAdd(obj, 0);
            complexObjectLinks[obj]++;

            return complexObjectLinks[obj] == maxRecursion;
        }

        private string GetSerializedString(
            object obj,
            DataMember memberInfo,
            string indentation,
            Func<object, string> serializeMember,
            bool needNewLine = true)
        {
            var serializedString = serializeMember(memberInfo.GetValue(obj));
            var stringEnd = needNewLine ? Environment.NewLine : string.Empty;

            return $"{indentation}{memberInfo.Name} = {serializedString}{stringEnd}";
        }
    }
}