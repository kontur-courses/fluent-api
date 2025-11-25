using System;
using System.Collections;
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
        public delegate string SerializerDelegate(object? obj, int identationLevel, int depthLevel, List<string> pathSegments);
        private readonly IReadOnlySet<Type> excludedTypes;
        private readonly IReadOnlyDictionary<Type, SerializerDelegate> typeSerializers;
        private readonly IReadOnlyDictionary<Type, CultureInfo> customCultureInfos;
        private readonly IReadOnlyDictionary<string, SerializerDelegate> propertySerializers;
        private readonly IReadOnlyDictionary<string, int> maxStringLength;
        private readonly IReadOnlySet<string> excludedProperties;
        private const int MaxDeepnessLevel = 3;
        private const char Tabchar = '\t';
        private const char ResetCaretChar = '\r';
        private const char NewLineChar = '\n';
        private static readonly string EnvironmentNewLine = Environment.NewLine;
        private const string EqualityWithSpaces = " = ";
        private readonly string deepnessExceededString = $"Deepness exceeded!{EnvironmentNewLine}";

        private static HashSet<Type> _finalTypes =
        [
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
            typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(double), typeof(string), typeof(bool),
            typeof(decimal), typeof(char), typeof(string), typeof(Guid),
            typeof(DateTime), typeof(TimeSpan)
        ];
        
        public PrintingConfig() : this(
            new HashSet<Type>(),
            new Dictionary<Type, SerializerDelegate>(),
            new Dictionary<Type, CultureInfo>(),
            new Dictionary<string, SerializerDelegate>(),
            new Dictionary<string, int>(),
            new HashSet<string>())
        {}

        private PrintingConfig(
            IReadOnlySet<Type> excludedTypes,
            IReadOnlyDictionary<Type, SerializerDelegate> typeSerializers,
            IReadOnlyDictionary<Type, CultureInfo> customCultureInfos,
            IReadOnlyDictionary<string, SerializerDelegate> propertySerializers,
            IReadOnlyDictionary<string, int> maxStringLength,
            IReadOnlySet<string> excludedProperties)
        {
            this.excludedTypes = excludedTypes;
            this.typeSerializers = typeSerializers;
            this.customCultureInfos = customCultureInfos;
            this.propertySerializers = propertySerializers;
            this.maxStringLength = maxStringLength;
            this.excludedProperties = excludedProperties;
        }

        public PrintingConfig<TOwner> ExcludePropertyOfType<T>()
        {
            if (excludedTypes.Contains(typeof(T)))
                return this;
            var tempSet = new HashSet<Type>(excludedTypes) { typeof(T) };
            return new PrintingConfig<TOwner>(tempSet,
                typeSerializers,
                customCultureInfos,
                propertySerializers,
                maxStringLength,
                excludedProperties);
        }

        public PrintingConfig<TOwner> WithTypeSerializationStyle<T>(SerializerDelegate serializer)
        {
            var tempDict = new Dictionary<Type, SerializerDelegate>(typeSerializers)
            {
                [typeof(T)] = serializer
            };
            return new PrintingConfig<TOwner>(excludedTypes,
                tempDict,
                customCultureInfos,
                propertySerializers,
                maxStringLength,
                excludedProperties);
        }

        public PrintingConfig<TOwner> SetTypeCulture<T>(CultureInfo cultureInfo)
            where T : IFormattable
        {
            var tempDict = new Dictionary<Type, CultureInfo>(customCultureInfos)
            {
                [typeof(T)] = cultureInfo
            };
            return new PrintingConfig<TOwner>(excludedTypes,
                typeSerializers,
                tempDict,
                propertySerializers,
                maxStringLength,
                excludedProperties);
        }

        public PrintingConfig<TOwner> WithPropertySerialization(PropertyPath propertyName, SerializerDelegate serializer)
        {
            var tempDict = new Dictionary<string, SerializerDelegate>(propertySerializers)
            {
                [propertyName.ToString()] = serializer
            };
            return new PrintingConfig<TOwner>(excludedTypes,
                typeSerializers,
                customCultureInfos,
                tempDict,
                maxStringLength,
                excludedProperties);
        }

        public PrintingConfig<TOwner> TrimStringToLength(Expression<Func<TOwner, string>> propertyExpr, int length)
        {
            if (propertyExpr.Body is not MemberExpression member)
                throw new ArgumentException("Expression must select a property");
            var tempDict = new Dictionary<string, int>(maxStringLength)
            {
                [PropertyPath.ConvertFromSelector(propertyExpr).ToString()] = length
            };
            return new PrintingConfig<TOwner>(excludedTypes,
                typeSerializers,
                customCultureInfos,
                propertySerializers,
                tempDict,
                excludedProperties);
        }

        public PrintingConfig<TOwner> Exclude(PropertyPath propertyName)
        {
            if (excludedProperties.Contains(propertyName.ToString()))
                return this;
            var tempSet = new HashSet<string>(excludedProperties) { propertyName.ToString() };
            return new PrintingConfig<TOwner>(excludedTypes,
                typeSerializers,
                customCultureInfos,
                propertySerializers,
                maxStringLength,
                tempSet);
        }

        public PropertyPrintingConfig<TOwner, TPropType> GetPropertyByName<TPropType>(
            Expression<Func<TOwner, TPropType>> targetProperty)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, targetProperty);
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object? obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj is null)
                return "null" + EnvironmentNewLine;

            var type = obj.GetType();
            var path = new List<string>{type.Name};
            return GetSerializerForType(type).Invoke(obj, nestingLevel, 0, path);
        }
        
        private string Serialize(object? obj, int nestingLevel, int deepnessLevel, List<string> pathSegments)
        {
            if (obj is null)
                return "null" + EnvironmentNewLine;
            var type = obj.GetType();
            if (TrySerializeAsBaseFields(obj, type, out var serialized) && serialized is not null)
                return serialized;

            var sb = new StringBuilder();
            sb.Append(SerializeComplexField(type, deepnessLevel, obj, nestingLevel, pathSegments));
            return sb.ToString();
        }

        private bool TrySerializeAsBaseFields(object obj, Type type, out string? result)
        {
            result = null;
            if (customCultureInfos.TryGetValue(type, out var info))
            {
                result = (obj as IFormattable)?.ToString(null, info) + EnvironmentNewLine;
                return true;
            }
            if (_finalTypes.Contains(type))
            {
                result = obj + EnvironmentNewLine;
                return true;
            }
            return false;
        }

        private string SerializeComplexField(Type type, int deepnessLevel, object obj, int nestingLevel, List<string> pathSegments)
        {
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            if (obj is IEnumerable enumerable && type != typeof(string))
                return SerializeCollection(enumerable, nestingLevel, deepnessLevel, pathSegments);

            var fullFieldName = new PropertyPath(pathSegments);
            var objectInfo = type.GetMembers(BindingFlags.Instance
                                             | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.MemberType is MemberTypes.Field or MemberTypes.Property)
                .Where(x => !IsBackingField(x) && !IsShadowingProperty(x, type))
                .Where(x => !excludedTypes.Contains(ToMemberType(x)));
            foreach (var propertyInfo in objectInfo)
            {
                pathSegments.Add(propertyInfo.Name);
                fullFieldName = new PropertyPath(pathSegments);
                if (excludedProperties.Contains(fullFieldName.ToString()))
                {
                    pathSegments.RemoveAt(pathSegments.Count - 1);
                    continue;
                }
                if (IsDeepnessOverflow(deepnessLevel))
                    return deepnessExceededString;
                sb.Append(SerializePropertyOrField(propertyInfo, obj, nestingLevel, deepnessLevel, pathSegments));
                pathSegments.RemoveAt(pathSegments.Count - 1);
            }
            return sb.ToString();
        }

        private Type ToMemberType(MemberInfo member)
        {
            if (member is PropertyInfo propertyInfo)
                return propertyInfo.PropertyType;
            if (member is FieldInfo fieldInfo)
                return fieldInfo.FieldType;
            throw new ArgumentException($"{member.GetType().Name} is not a property or field");
        }

        private object? GetMemberValue(MemberInfo member, object? obj)
        {
            if (member is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(obj);
            if (member is FieldInfo fieldInfo)
                return fieldInfo.GetValue(obj);
            throw new ArgumentException($"{member.GetType().Name} is not a property or field");
        }

        private bool IsBackingField(MemberInfo member)
        {
            return member.Name.Contains("k__BackingField");
        }

        private bool IsShadowingProperty(MemberInfo member, Type parent)
        {
            if (member is not FieldInfo field)
                return false;
            var memberName = member.Name.ToLower().TrimStart('_');
            var shadowedField = parent.GetProperty(memberName,  BindingFlags.Instance |
                                                                BindingFlags.Public |
                                                                BindingFlags.NonPublic |
                                                                BindingFlags.IgnoreCase);
            return shadowedField is not null;
        }

        private string SerializeCollection(IEnumerable enumerable, int nestingLevel, int deepnessLevel,
            List<string> pathSegments)
        {
            var sb = new StringBuilder();
            sb.AppendLine(enumerable.GetType().Name);
            sb.Append(new string(Tabchar, nestingLevel + 1));
            foreach (var obj in enumerable)
            {
                var objType = obj.GetType();
                pathSegments.Add(objType.Name);
                var isKeyValuePair = objType.IsGenericType
                                     && objType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
                if (isKeyValuePair)
                    sb.Append(SerializeDictionaryElement(obj, deepnessLevel + 1, pathSegments));
                else
                    sb.Append(GetSerializerForType(obj.GetType())
                        .Invoke(obj, nestingLevel + 1, deepnessLevel + 1, pathSegments));
                pathSegments.RemoveAt(pathSegments.Count - 1);
                sb.Append(new string(Tabchar, nestingLevel + 1));
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        private string SerializeDictionaryElement(object obj, int deepnessLevel, List<string> pathSegments)
        {
            if (IsDeepnessOverflow(deepnessLevel))
                return deepnessExceededString;
            var type = obj.GetType();
            var key = type.GetProperty("Key")!.GetValue(obj);
            var value = type.GetProperty("Value")!.GetValue(obj);
            var sb = new StringBuilder();
            sb.Append(GetSerializerForType(key!.GetType()).Invoke(key, 0,
                deepnessLevel + 1, pathSegments).TrimEnd(ResetCaretChar, NewLineChar, Tabchar));
            sb.Append(EqualityWithSpaces);
            sb.Append(GetSerializerForType(value!.GetType()).Invoke(value, 0,
                deepnessLevel + 1, pathSegments).TrimEnd(ResetCaretChar, NewLineChar, Tabchar));
            sb.Append(EnvironmentNewLine);
            return sb.ToString();
        }

        private bool IsDeepnessOverflow(int deepnessLevel)
        {
            return deepnessLevel >= MaxDeepnessLevel;
        }

        private string SerializePropertyOrField(MemberInfo memberInfo, object obj, int nestingLevel,
            int deepnessLevel, List<string> path)
        {
            var sb = new StringBuilder();
            var fullPropertyName = new PropertyPath(path);
            if (IsPropertyNeedsUniqueSerialization(fullPropertyName, out var serializer))
            {
                var newData = BuildSerializedField(memberInfo, serializer, obj, nestingLevel, deepnessLevel, path)
                    .TrimEnd(ResetCaretChar, NewLineChar, Tabchar);
                sb.Append(newData + EnvironmentNewLine);
                return sb.ToString();
            }
            if (IsPropertyNeedsTrim(fullPropertyName, out var trimLen))
            {
                var identation = new string(Tabchar, nestingLevel + 1);
                var serializedInfo = BuildSerializedField(memberInfo, null, obj, nestingLevel, deepnessLevel, path);
                var dataPrefixLength = identation.Length + memberInfo.Name.Length + EqualityWithSpaces.Length;
                serializedInfo = serializedInfo.Substring(0, dataPrefixLength + trimLen)
                    .TrimEnd(ResetCaretChar, NewLineChar, Tabchar);
                
                sb.Append(serializedInfo + EnvironmentNewLine);
                return sb.ToString();
            }
            sb.Append(BuildSerializedField(memberInfo, null, obj, nestingLevel, deepnessLevel, path));
            return sb.ToString();
        }

        private string BuildSerializedField(MemberInfo memberInfo, SerializerDelegate? serializer,
            object? obj, int nestingLevel, int deepnessLevel, List<string> path)
        {
            var memberValue = GetMemberValue(memberInfo, obj);
            if (memberValue is null)
                return string.Empty;
            var identation = new string(Tabchar, nestingLevel + 1);
            var sb = new StringBuilder();
            sb.Append(identation + memberInfo.Name + EqualityWithSpaces);
            if (serializer is null)
                serializer = GetSerializerForType(memberValue.GetType());
            sb.Append(serializer.Invoke(GetMemberValue(memberInfo, obj), nestingLevel + 1,
                deepnessLevel + 1, path));
            return sb.ToString();
        }

        private SerializerDelegate GetSerializerForType(Type type)
        {
            if (typeSerializers.TryGetValue(type, out var forType))
                return forType;
            return Serialize;
        }
        
        private bool IsPropertyNeedsUniqueSerialization(PropertyPath propertyName,
            out SerializerDelegate? serializer)
        {
            serializer = null;
            if (propertySerializers.TryGetValue(propertyName.ToString(), out var propertySerializer))
            {
                serializer = propertySerializer;
                return true;
            }
            return false;
        }

        private bool IsPropertyNeedsTrim(PropertyPath fullPropertyName, out int trimLength)
        {
            trimLength = 0;
            if (maxStringLength.TryGetValue(fullPropertyName.ToString(), out var value))
            {
                trimLength = value;
                return true;
            }
            return false;
        }
    }
}