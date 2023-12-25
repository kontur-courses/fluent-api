using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
        internal readonly Dictionary<Type, Func<object, string>> TypeSerializers = new();
        internal readonly Dictionary<MemberInfo, Func<object, string>> MemberSerializers = new();
        private int MaxNestingLevel { get; }
        public PrintingConfig(int maxNestingLevel = 10)
        {
            MaxNestingLevel = maxNestingLevel;
        }

        public TypePrintingConfig<TOwner, TType> Printing<TType>()
        {
            return new TypePrintingConfig<TOwner, TType>(this);
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            var memberInfo = ((MemberExpression) memberSelector.Body).Member;
            return new MemberPrintingConfig<TOwner, TMemberType>(this, memberInfo);
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            var memberInfo = ((MemberExpression) memberSelector.Body).Member;
            excludedMembers.Add(memberInfo);
            return Excluding<TMemberType>();
        }

        public PrintingConfig<TOwner> Excluding<TType>()
        {
            excludedTypes.Add(typeof(TType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (TypeSerializers.TryGetValue(obj.GetType(), out var serializator))
                return serializator(obj) + Environment.NewLine;
            if (finalTypes.Contains(type) || nestingLevel == MaxNestingLevel)
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var member in type.GetFieldsAndProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => !excludedMembers.Contains(x)))
            {
                if (member is PropertyInfo propertyInfo && !excludedTypes.Contains(propertyInfo.PropertyType))
                    sb.Append(identation + propertyInfo.Name + " = " +
                              (MemberSerializers.TryGetValue(member, out var memberSerializator) 
                                    ? memberSerializator(propertyInfo.GetValue(obj)) + Environment.NewLine
                                    : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1)));
                if (member is FieldInfo fieldInfo && !excludedTypes.Contains(fieldInfo.FieldType))
                {
                    sb.Append(identation + fieldInfo.Name + " = " +
                              (MemberSerializers.TryGetValue(member, out var memberSerializator) 
                                  ? memberSerializator(fieldInfo.GetValue(obj)) + Environment.NewLine
                                  : PrintToString(fieldInfo.GetValue(obj), nestingLevel + 1)));
                }
            }

            return sb.ToString();
        }
    }
}