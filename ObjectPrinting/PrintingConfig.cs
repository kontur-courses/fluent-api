using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : Config
    {
        private readonly Dictionary<object, int> printedObjects;

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(uint),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(char),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(long),
            typeof(ulong),
            typeof(decimal),
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
        };

        public PrintingConfig()
        {
            printedObjects = new Dictionary<object, int>();
        }

        public PrintingConfig(Config config) : base(config)
        {
            printedObjects = new Dictionary<object, int>();
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
            var memberInfo = GetMemberInfoFromExpression(memberSelector);
            return new MemberPrintingConfig<TOwner, TMemberType>(this, memberInfo);
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>()
        {
            AddExcludedType(typeof(TMemberType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            var memberInfo = GetMemberInfoFromExpression(memberSelector);
            AddExcludedMember(memberInfo);
            return this;
        }

        public PrintingConfig<TOwner> AddAltTypeSerialzier(Type type, Func<object, string> altSerializer)
        {
            AddTypeSerialzier(type, altSerializer);
            return this;
        }

        public PrintingConfig<TOwner> AddAltMemberSerializer(MemberInfo member, Func<object, string> altSerializer)
        {
            AddMemberSerializer(member, altSerializer);
            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (printedObjects.Keys.Any(printedObj =>
                nestingLevel > printedObjects[printedObj] && ReferenceEquals(printedObj, obj)))
            {
                return "Cyclic reference" + Environment.NewLine;
            }

            var type = obj.GetType();

            if (finalTypes.Contains(type))
            {
                return obj + Environment.NewLine;
            }

            printedObjects.Add(obj, nestingLevel);

            string serializedObj;

            switch (obj)
            {
                case IDictionary dictionary:
                    serializedObj = PrintDictionary(dictionary, nestingLevel + 1);
                    break;
                case IEnumerable enumerable:
                    serializedObj = PrintEnumerable(enumerable, nestingLevel + 1);
                    break;
                case Enum enumObj:
                    serializedObj = PrintEnum(enumObj);
                    break;
                default:
                    serializedObj = PrintComplexObject(obj, nestingLevel + 1);
                    break;
            }

            printedObjects.Remove(obj);

            return serializedObj;
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

        private string PrintDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);

            if (dictionary.Keys.Count == 0)
            {
                sb.Append("Empty dictionary" + Environment.NewLine);
                return sb.ToString();
            }

            sb.Append(Environment.NewLine);

            foreach (var key in dictionary.Keys)
            {
                var serializedKey = PrintToString(key, nestingLevel + 1);
                sb.Append(identation + "Key = " + serializedKey);
                var serializedValue = PrintToString(dictionary[key], nestingLevel + 1);
                sb.Append(identation + "Value = " + serializedValue);
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
                                  PrintToString(memberInfo.GetMemberValue(obj), nestingLevel + 1));
                    }
                    else
                    {
                        sb.Append(identation + memberInfo.Name + " = " +
                                  altSerializer.Invoke(memberInfo.GetMemberValue(obj)) + Environment.NewLine);
                    }
                }
            }

            return sb.ToString();
        }

        private string PrintEnum(Enum enumObj)
        {
            return enumObj + Environment.NewLine;
        }

        private MemberInfo GetMemberInfoFromExpression<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression expression))
                throw new InvalidCastException("Expression must be a MemberExpression");

            return expression.Member;
        }
    }
}