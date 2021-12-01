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
        private readonly HashSet<Type> finalTypes;
        private readonly HashSet<object> printedObjects;
        private readonly Config config;

        public PrintingConfig()
        {
            config = new Config();
            printedObjects = new HashSet<object>();
            finalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(long)
            };
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>()
        {
            return new MemberPrintingConfig<TOwner, TMemberType>(this);
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression expression))
                throw new InvalidCastException("Expression must be a MemberExpression");

            var memberInfo = expression.Member;
            return new MemberPrintingConfig<TOwner, TMemberType>(this, memberInfo);
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>()
        {
            config.ExcludedTypes.Add(typeof(TMemberType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression expression))
                throw new InvalidCastException("Expression must be a MemberExpression");

            var memberInfo = expression.Member;
            config.ExcludedMembers.Add(memberInfo);
            return this;
        }

        public PrintingConfig<TOwner> AddAltTypeSerialzier(Type type, Func<object, string> customPrint)
        {
            config.AltTypeSerializers[type] = customPrint;
            return this;
        }

        public PrintingConfig<TOwner> AddAltMemberSerializer(MemberInfo member, Func<object, string> customPrint)
        {
            config.AltMemberSerializers[member] = customPrint;
            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (printedObjects.Contains(obj))
            {
                return "This object has already been printed" + Environment.NewLine;
            }

            printedObjects.Add(obj);

            string serializedObj;

            var type = obj.GetType();

            if (finalTypes.Contains(type))
            {
                serializedObj = obj + Environment.NewLine;
            }
            else if (obj is IEnumerable enumerable)
            {
                serializedObj = PrintEnumerable(enumerable, nestingLevel + 1);
            }
            else
            {
                serializedObj = PrintComplexObject(obj, nestingLevel + 1);
            }

            printedObjects.Remove(obj);

            return serializedObj;
        }

        private static Type GetMemberType(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    return propertyInfo.PropertyType;
                case FieldInfo fieldInfo:
                    return fieldInfo.FieldType;
                default:
                    throw new ArgumentException("Member is not a property or a field");
            }
        }

        private static object GetMemberValue(MemberInfo memberInfo, object obj)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    return propertyInfo.GetValue(obj);
                case FieldInfo fieldInfo:
                    return fieldInfo.GetValue(obj);
                default:
                    throw new ArgumentException("Member is not a property or a field");
            }
        }

        private bool IsExcluded(MemberInfo member)
        {
            return config.ExcludedTypes.Contains(GetMemberType(member)) || config.ExcludedMembers.Contains(member);
        }

        private string PrintEnumerable(IEnumerable enumerable, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);

            var objects = enumerable.Cast<object>().ToList();
            if (objects.Count == 0)
            {
                sb.Append("Empty collection" + Environment.NewLine);
                return sb.ToString();
            }

            sb.Append(Environment.NewLine);

            foreach (var obj in objects)
            {
                sb.Append(identation + PrintToString(obj, nestingLevel + 1));
            }

            return sb.ToString();
        }

        private string PrintComplexObject(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            var identation = new string('\t', nestingLevel + 1);

            var objFieldsAndProperties = type.GetMembers().Where(info => info is PropertyInfo || info is FieldInfo);
            foreach (var memberInfo in objFieldsAndProperties)
            {
                if (!IsExcluded(memberInfo))
                {
                    var altSerializer = TryGetAltSerializer(memberInfo);

                    if (altSerializer == null)
                    {
                        sb.Append(identation + memberInfo.Name + " = " +
                                  PrintToString(GetMemberValue(memberInfo, obj), nestingLevel + 1));
                    }
                    else
                    {
                        sb.Append(identation + memberInfo.Name + " = " +
                                  altSerializer.Invoke(GetMemberValue(memberInfo, obj)) + Environment.NewLine);
                    }
                }
            }

            return sb.ToString();
        }

        private Func<object, string> TryGetAltSerializer(MemberInfo memberInfo)
        {
            if (config.AltMemberSerializers.TryGetValue(memberInfo, out var memberSerializer))
                return memberSerializer;

            return config.AltTypeSerializers.TryGetValue(GetMemberType(memberInfo), out var typeSerializer)
                ? typeSerializer
                : null;
        }
    }
}