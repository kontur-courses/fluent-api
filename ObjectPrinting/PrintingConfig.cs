using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions.Specialized;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public const string NullToString = "null";

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(long), typeof(Guid)
        };

        private readonly HashSet<object> serializedMembers = new HashSet<object>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedFieldsProperties = new HashSet<string>();

        private string pinnedPropertyName;

        private readonly Dictionary<Type, Delegate> specialSerializationsForTypes =
            new Dictionary<Type, Delegate>();


        private readonly Dictionary<string, Delegate> specialSerializationsForFieldsProperties =
            new Dictionary<string, Delegate>();


        private CultureInfo culture = CultureInfo.InvariantCulture;
        private int resultStartIndex;
        private int resultLength = int.MaxValue;



        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (serializedMembers.Contains(obj))
                return ReturnWhenCyclic(nestingLevel);

            var sb = new StringBuilder();
            if (obj == null)
                return NullToString + Environment.NewLine;

            var objType = obj.GetType();


            if (finalTypes.Contains(objType) && !excludedTypes.Contains(objType))
                return ReturnWhenFinal(obj);


            var identation = new string('\t', nestingLevel + 1);
            sb.AppendLine(objType.Name);
            var fieldsAndProperties = GetFieldsAndProperties(objType);

            foreach (var memberInfo in fieldsAndProperties)
            {
                PrintMemberInformation(memberInfo, obj, sb, nestingLevel, identation);
            }
            return (nestingLevel != 0) ? sb.ToString() : GetTrimString(sb);
        }

        private Delegate MakeSerializeDelegate(SerializationMemberInfo memberSerialization)
        {
            if (specialSerializationsForFieldsProperties.ContainsKey(memberSerialization.MemberName))
                return specialSerializationsForFieldsProperties[memberSerialization.MemberName];
            if (specialSerializationsForTypes.ContainsKey(memberSerialization.MemberType))
                return specialSerializationsForTypes[memberSerialization.MemberType];
            return null;
        }

        private void PrintMemberInformation(MemberInfo memberInfo, object obj, StringBuilder sb, int nestingLevel, string identation)
        {
            SerializationMemberInfo memberSerialization = default;
            if (memberInfo is PropertyInfo propertyInfo)
            {
                var indexParameters = propertyInfo.GetIndexParameters();
                if (indexParameters.Length != 0)
                {
                    PrintIndexes(obj, sb, identation, propertyInfo.Name);
                    return;
                }
                memberSerialization =
                    new SerializationMemberInfo(propertyInfo, obj);
            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                memberSerialization =
                    new SerializationMemberInfo(fieldInfo, obj);
            }


            if (memberSerialization == null || excludedTypes.Contains(memberSerialization.MemberType)
                                            || excludedFieldsProperties.Contains(memberSerialization.MemberName))
                return;

            var serializeDelegate = MakeSerializeDelegate(memberSerialization);
            serializedMembers.Add(obj);

            if (serializeDelegate != null)
                sb.Append(FormSerializeDelegateString(identation, nestingLevel, memberSerialization,
                    serializeDelegate));
            else
                sb.Append(FormSerializeString(identation, nestingLevel, memberSerialization));
        }

        private string FormSerializeDelegateString(string identation, int nestingLevel,
            SerializationMemberInfo memberSerialization, Delegate serializeDelegate)
        {
            return identation + memberSerialization.MemberName + " = " +
                   PrintToString(serializeDelegate.DynamicInvoke(memberSerialization.MemberValue),
                       nestingLevel + 1);
        }

        private string FormSerializeString(string identation, int nestingLevel,
            SerializationMemberInfo memberSerialization)
        {
            return identation + memberSerialization.MemberName + " = " +
                   PrintToString(memberSerialization.MemberValue,
                       nestingLevel + 1);
        }

        private static IEnumerable<MemberInfo> GetFieldsAndProperties(Type objType)
        {
            var fields =
                objType.GetFields(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>();
            var props =
                objType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>();
            return fields.Union(props);
        }

        private string GetTrimString(StringBuilder resultString)
        {
            return resultString.ToString()
                [resultStartIndex..Math.Min(resultLength, resultString.Length)];
        }

        private string ReturnWhenCyclic(int nestingLevel)
        {
            return ((nestingLevel != 0) ? "this (parentObj)"
                : GetTrimString(new StringBuilder("this (parentObj)"))) + "\r\n";
        }

        private string ReturnWhenFinal(object obj)
        {
            if (obj is IFormattable formattable)
                return formattable.ToString(null, culture) + Environment.NewLine;
            return obj + Environment.NewLine;
        }

        private void PrintIndexes(object obj, StringBuilder sb, string identation, string name)
        {
            if (!(obj is ICollection cast))
                sb.Append(name);
            else
            {
                var counter = 0;
                sb.Append(identation + name + " =\r\n");
                foreach (var parameter in cast)
                {
                    var value = parameter.ToString();
                    sb.Append(identation + "\t" + "Index " + counter + " = " + value + "\r\n");
                    counter++;
                }
            }
        }





        public PrintingConfig<TOwner> ExcludedType<TExType>()
        {
            excludedTypes.Add(typeof(TExType));
            return this;
        }

        public PrintingConfig<TOwner> SpecialSerializationType<TType>(Func<TType, string> specialSerializationForType)
        {
            specialSerializationsForTypes[typeof(TType)] = specialSerializationForType;
            return this;
        }

        public PrintingConfig<TOwner> SpecialSerializationField<TFieldType>(Func<TFieldType, string> serialization)
        {
            if (pinnedPropertyName != null)
            {
                specialSerializationsForFieldsProperties[pinnedPropertyName] = serialization;
                pinnedPropertyName = null;
            }
            else
            {
                foreach (var propertyName in specialSerializationsForFieldsProperties.Keys)
                    specialSerializationsForFieldsProperties[propertyName] = serialization;
            }
            return this;
        }

        public PrintingConfig<TOwner> ExcludedProperty(Expression<Func<TOwner, string>> propertyNameExpression)
        {

            excludedFieldsProperties.Add(((ConstantExpression)propertyNameExpression.Body).Value.ToString());
            return this;
        }

        public PrintingConfig<TOwner> SetCulture(Expression<Func<TOwner, CultureInfo>> inputCulture)
        {
            var cultureName = ((NewExpression)inputCulture.Body).Arguments.First().ToString();
            culture = new CultureInfo(cultureName[1..^1]);
            return this;
        }

        public PrintingConfig<TOwner> PinProperty(Expression<Func<TOwner, string>> propertyNameExpression)
        {
            pinnedPropertyName = ((ConstantExpression)propertyNameExpression.Body).Value.ToString();
            return this;
        }

        public PrintingConfig<TOwner> Trim<TStart,TLength>(Expression<Func<TOwner, Tuple<TStart, TLength>>> trimBorders)
        {
            var start = int.Parse(((NewExpression) trimBorders.Body).Arguments[0].ToString());
            var length = int.Parse(((NewExpression)trimBorders.Body).Arguments[1].ToString());
            
            if (start < 0 || length < 0)
                throw new ArgumentException();
            
            resultStartIndex = start;
            resultLength = length;
            return this;
        }
    }
}