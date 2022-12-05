using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Configuration;

namespace ObjectPrinting
{
    public class ObjectPrinter<TOwner>
    {
        private readonly PrintingConfig<TOwner> _config;

        private readonly Type[] _finalTypes = new[]
        {
            typeof(string), typeof(StringBuilder)
        };

        public ObjectPrinter()
        {
            _config = new PrintingConfig<TOwner>();
        }

        public ObjectPrinter(PrintingConfig<TOwner> config)
        {
            _config = config;
        }


        public ObjectPrinter(Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> initPrinter)
        {
            _config = initPrinter(new PrintingConfig<TOwner>());
        }

        public string PrintToString(object obj)
        {
            var printedObjects = new HashSet<object>();
            return PrintToString(obj, printedObjects, 0);
        }

        public PrintingConfig<TOwner> Configurate()
        {
            return _config;
        }

        private string PrintToString(object obj, HashSet<object> printedObjects, int nestingLevel, bool returnOnNewLine = true)
        {
            if (obj == null)
                return returnOnNewLine ? "null" + Environment.NewLine : "null";

            var currentType = obj.GetType();

            if (_config.TypesForExcluding.Contains(currentType))
                return string.Empty;

            var sb = new StringBuilder();
            if (obj is IEnumerable enumerableObj && !_finalTypes.Contains(currentType))
            {
                EnumerableSerializing(sb, enumerableObj, printedObjects, nestingLevel);
                return sb.ToString();
            }

            if (currentType.IsValueType || _finalTypes.Contains(currentType) || currentType.IsSerializable)
                return returnOnNewLine ? obj + Environment.NewLine : obj.ToString();

            sb.AppendLine(currentType.Name);
            var identation = new string('\t', nestingLevel + 1);

            if (currentType.IsClass)
                if (!printedObjects.Add(obj))
                {
                    if (!_config.IgnoreCyclicReferences)
                        throw new StackOverflowException($"Member: {currentType.DeclaringType?.Name}.{currentType.Name} cyclic reference");

                    sb.Append($"{identation}cyclic reference{Environment.NewLine}");
                    return sb.ToString();
                }


            var members = GetMembersInfoByType(currentType);

            foreach (var memberInfo in members)
            {
                SerializeChildMember(obj, printedObjects, nestingLevel, memberInfo, sb, identation);
            }

            return sb.ToString();
        }

        private void SerializeChildMember(object obj, HashSet<object> printedObjects, int nestingLevel, MemberInfo memberInfo, StringBuilder sb, string identation)
        {
            var (memberValue, memberType) = GetMemberTypeAndValue(obj, memberInfo);

            sb.Append(identation + memberInfo.Name + " = ");

            if (CheckAlternativeSerializer(memberInfo, memberType, out var alternativeSerializer))
            {
                AlternativeMemberSerializing(sb, alternativeSerializer, memberValue);
            }
            else if (obj is IEnumerable collection)
            {
                EnumerableSerializing(sb, collection, printedObjects, nestingLevel);
            }
            else
            {
                DefaultObjectSerializing(printedObjects, nestingLevel, sb, memberValue);
            }
        }

        private void EnumerableSerializing(StringBuilder sb, IEnumerable enumerableObj, HashSet<object> printedObjects, int nestingLevel)
        {
            if (enumerableObj is IDictionary dict)
                sb.Append(PrintDictionary(dict, printedObjects, nestingLevel));
            else
            {
                sb.Append(PrintEnumerable(enumerableObj, printedObjects, nestingLevel));
            }
        }

        private static void AlternativeMemberSerializing(StringBuilder sb, Delegate alternativeSerializer, object memberValue)
        {
            var serialized = alternativeSerializer.DynamicInvoke(memberValue);
            sb.Append($"{serialized}" + Environment.NewLine);
        }

        private void DefaultObjectSerializing(HashSet<object> printedObjects, int nestingLevel, StringBuilder sb, object memberValue)
        {
            sb.Append(PrintToString(memberValue, printedObjects, nestingLevel + 1));
        }

        private bool CheckAlternativeSerializer(MemberInfo memberInfo, Type memberType, out Delegate alternativeSerializer)
        {
            return _config.TypesAlternativeSerializer.TryGetValue(memberType, out alternativeSerializer)
                || _config.MembersAlternativeSerializer.TryGetValue(memberInfo, out alternativeSerializer);
        }

        private static (object memberValue, Type memberType) GetMemberTypeAndValue(object obj, MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Field && memberInfo is FieldInfo fieldInfo)
            {
                var memberValue = fieldInfo.GetValue(obj);
                var memberType = fieldInfo.FieldType;
                return (memberValue, memberType);
            }
            else
            {
                var propertyInfo = memberInfo as PropertyInfo;

                var memberValue = propertyInfo!.GetValue(obj);
                var memberType = propertyInfo.PropertyType;
                return (memberValue, memberType);
            }
        }

        private List<MemberInfo> GetMembersInfoByType(Type currentType)
        {
            var properties = currentType.GetProperties().Select(property => (MemberInfo)property);
            var fields = currentType.GetFields().Select(field => (MemberInfo)field);
            var members = ExcludeMembers(properties.Union(fields));

            return members;
        }


        private List<MemberInfo> ExcludeMembers(IEnumerable<MemberInfo> membersToCheck)
        {
            var includeMembers = new List<MemberInfo>();

            foreach (var memberInfo in membersToCheck)
            {
                Type memberType;

                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Field when memberInfo is FieldInfo fieldInfo:
                        memberType = fieldInfo.FieldType;
                        break;
                    case MemberTypes.Property when memberInfo is PropertyInfo propertyInfo:
                        memberType = propertyInfo.PropertyType;
                        break;
                    default:
                        continue;
                }

                if (_config.TypesForExcluding.Contains(memberType))
                    continue;

                if (_config.MembersForExcluding.Contains(memberInfo))
                    continue;

                includeMembers.Add(memberInfo);
            }

            return includeMembers;
        }


        private StringBuilder PrintEnumerable(IEnumerable collection, HashSet<object> printedObjects, int nestingLevel)
        {
            var start = "(";
            var end = ")";

            if (collection is Array)
            {
                start = "[";
                end = "]";
            }

            var sb = new StringBuilder(start);
            var printedItems = (from object objItem in collection select PrintToString(objItem, printedObjects, nestingLevel, false)).ToList();
            sb.Append(string.Join(", ", printedItems));
            sb.Append($"{end}{Environment.NewLine}");

            return sb;
        }

        private string PrintDictionary(IDictionary dict, HashSet<object> printedObjects, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var nextIdentation = new string('\t', nestingLevel + 2);


            var sb = new StringBuilder();
            var index = 0;
            foreach (DictionaryEntry dictionaryEntry in dict)
            {
                sb.Append($"{identation}{{{Environment.NewLine}");
                sb.Append($"{nextIdentation}key: {PrintToString(dictionaryEntry.Key, printedObjects, nestingLevel + 3)}");
                sb.Append($"{nextIdentation}value: {PrintToString(dictionaryEntry.Value, printedObjects, nestingLevel + 3)}");
                sb.Append($"{identation}}}");
                if (++index < dict.Count)
                    sb.Append(",");
                sb.Append($"{Environment.NewLine}");
            }

            return $"{{{Environment.NewLine}{sb}}}{Environment.NewLine}";
        }
    }
}