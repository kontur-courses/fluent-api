using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.HomeWork
{
    public class PrintingConfig<TOwner>
    {
        private const string NullToString = "null";
        private const string ParentObj = "this (parentObj)";
        private const string MemberExpressionMessage = "Need member expression here(which giving access to the field)";
        private const string NewExpressionMessage = "Need new Expression here(creating a new object)";
        private const string Index = "Index ";
        private const BindingFlags BindFlags = BindingFlags.Public | BindingFlags.Instance;


        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(long), typeof(Guid)
        };

        private readonly HashSet<object> serializedMembers = new HashSet<object>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludedFieldsProperties = new HashSet<MemberInfo>();
        private readonly Dictionary<Type, Delegate> specialSerializationsForTypes =
            new Dictionary<Type, Delegate>();
        private readonly Dictionary<MemberInfo, Delegate> specialSerializationsForFieldsProperties =
            new Dictionary<MemberInfo, Delegate>();
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


            if ((finalTypes.Contains(objType) || objType.IsPrimitive) && !excludedTypes.Contains(objType))
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

        private Delegate MakeSerializeDelegate(MemberInfo memberSerialization)
        {
            if (specialSerializationsForFieldsProperties.ContainsKey(memberSerialization))
                return specialSerializationsForFieldsProperties[memberSerialization];

            if (memberSerialization is FieldInfo fieldSerialization &&
                specialSerializationsForTypes.ContainsKey(fieldSerialization.FieldType))
            {
                return specialSerializationsForTypes[fieldSerialization.FieldType];
            }

            if (memberSerialization is PropertyInfo propertySerialization &&
                specialSerializationsForTypes.ContainsKey(propertySerialization.PropertyType))
            {
                return specialSerializationsForTypes[propertySerialization.PropertyType];
            }

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

            if (memberSerialization == null || excludedTypes.Contains(memberSerialization.MemberType) || excludedFieldsProperties.Contains(memberInfo))
                return;

            var serializeDelegate = MakeSerializeDelegate(memberInfo);
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
            var fieldsAndProperties = objType.GetFields(BindFlags).Cast<MemberInfo>()
                .Union(objType.GetProperties(BindFlags));
            return fieldsAndProperties;
        }

        private string GetTrimString(StringBuilder resultString)
        {
            return resultString.ToString()
                [resultStartIndex..Math.Min(resultLength, resultString.Length)];
        }

        private string ReturnWhenCyclic(int nestingLevel)
        {
            return (nestingLevel != 0 ? ParentObj
                : GetTrimString(new StringBuilder(ParentObj))) + "\r\n";
        }

        private string ReturnWhenFinal(object obj)
        {
            if (obj is IFormattable formatObj)
                return formatObj.ToString(null, culture) + Environment.NewLine;
            return obj + Environment.NewLine;
        }

        private void PrintIndexes(object obj, StringBuilder sb, string identation, string name)
        {
            if (!(obj is ICollection collection))
                sb.Append(name);
            else
            {
                var counter = 0;
                sb.Append(identation + name + " =\r\n");
                foreach (var parameter in collection)
                {
                    var value = parameter.ToString();
                    sb.Append(identation + "\t" + Index + counter + " = " + value + "\r\n");
                    counter++;
                }
            }
        }

        public PrintingConfig<TOwner> ExcludedType<TExType>()
        {
            excludedTypes.Add(typeof(TExType));
            return this;
        }

        public PrintingConfig<TOwner> ExcludedProperty<TTarget>(Expression<Func<TOwner, TTarget>> propertyNameExpression)
        {
            if (propertyNameExpression.Body is MemberExpression propertyMember)
            {
                var member = propertyMember.Member;
                if (GetFieldsAndProperties(typeof(TOwner)).Contains(member))
                    excludedFieldsProperties.Add(member);
                return this;
            }
            throw new InvalidExpressionException(MemberExpressionMessage);
        }

        public PrintingConfig<TOwner> SpecialSerializationType<TType>(Func<TType, string> specialSerializationForType)
        {
            specialSerializationsForTypes[typeof(TType)] = specialSerializationForType;
            return this;
        }

        /*
        public PrintingConfig<TOwner> PinProperty(Expression<Func<TOwner, object>> propertyNameExpression)
        {
            MemberInfo prop = null;
            if (propertyNameExpression.Body is UnaryExpression unExpression)
            {
                if (!(unExpression.Operand is MemberExpression memb))
                    throw new InvalidExpressionException("Need member expression(which giving access to the field)");
                prop = memb.Member;
            }




            if (!((GetFieldsAndProperties(typeof(TOwner))).Contains(prop)))
            {
                pinnedPropertyName = null;
            }
            else
            {
                pinnedPropertyName = prop;
            }

            return this;
        }
        */

        public PrintingConfig<TOwner> SpecialSerializationField<TFieldType>(Expression<Func<TOwner, TFieldType>> memberAccess, Func<TFieldType, string> serialization)
        {

            if (memberAccess.Body is MemberExpression memberExpression)
            {
                specialSerializationsForFieldsProperties[memberExpression.Member] = serialization;
                return this;
            }
            throw new InvalidExpressionException(MemberExpressionMessage);
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo cultureInfo)
        {
            /*
            if (!(inputCulture.Body is NewExpression))
                throw new InvalidExpressionException(NewExpressionMessage);
            */
            //var cultureName = ((NewExpression)inputCulture.Body).Arguments.First().ToString();
            //culture = new CultureInfo(cultureName[1..^1]);
            culture = cultureInfo;
            return this;
        }

        ///*******************************************************************

        public PrintingConfig<TOwner> Trim<TStart, TLength>(Expression<Func<TOwner, Tuple<TStart, TLength>>> trimBorders)
        {
            if (!(trimBorders.Body is NewExpression))
                throw new InvalidExpressionException(NewExpressionMessage);

            var start = int.Parse(((NewExpression)trimBorders.Body).Arguments[0].ToString());
            var length = int.Parse(((NewExpression)trimBorders.Body).Arguments[1].ToString());

            if (start < 0 || length < 0)
                throw new ArgumentException();

            resultStartIndex = start;
            resultLength = length;
            return this;
        }
    }
}