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
        internal readonly string Items = "Items";
        private const BindingFlags BindFlags = BindingFlags.Public | BindingFlags.Instance;


        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(long), typeof(Guid)
        };

        internal readonly HashSet<object> SerializedMembers = new HashSet<object>();
        internal readonly HashSet<Type> ExcludedTypes = new HashSet<Type>();
        internal readonly HashSet<MemberInfo> ExcludedFieldsProperties = new HashSet<MemberInfo>();
        internal readonly Dictionary<Type, Delegate> SpecialSerializationsForTypes =
            new Dictionary<Type, Delegate>();
        internal readonly Dictionary<MemberInfo, Delegate> SpecialSerializationsForFieldsProperties =
            new Dictionary<MemberInfo, Delegate>();

        internal readonly Dictionary<Type, Borders> TrimTypes =
            new Dictionary<Type, Borders>();
        internal readonly Dictionary<MemberInfo, Borders> TrimMembers = 
            new Dictionary<MemberInfo, Borders>();

        private CultureInfo culture = CultureInfo.InvariantCulture;


        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (SerializedMembers.Contains(obj))
                return ReturnWhenCyclic(nestingLevel);

            if (obj == null)
                return NullToString + Environment.NewLine;

            var sb = new StringBuilder();
            var objType = obj.GetType();


            if ((finalTypes.Contains(objType) || objType.IsPrimitive) && !ExcludedTypes.Contains(objType))
                return ReturnWhenFinal(obj);


            var identation = new string('\t', nestingLevel + 1);
            sb.AppendLine(objType.Name);
            var fieldsAndProperties = GetFieldsAndProperties(objType);

            foreach (var memberInfo in fieldsAndProperties)
            {
                MemberInformationPrinter.PrintMemberInformation(this, memberInfo, obj, sb, nestingLevel, identation);
            }
            return sb.ToString();
        }

        internal string FormSerializeDelegateString(string identation, int nestingLevel,
            SerializationMemberInfo memberSerialization, Delegate serializeDelegate)
        {
            return identation + memberSerialization.MemberName + " = " +
                   PrintToString(serializeDelegate.DynamicInvoke(memberSerialization.MemberValue),
                       nestingLevel + 1);
        }

        internal string FormSerializeString(string identation, int nestingLevel,
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

        internal void PrintIndexes(object obj, StringBuilder sb, string identation, string name, int nestingLevel)
        {
            if (!(obj is ICollection collection))
                sb.Append(name);
            else
            {
                sb.Append(identation + name + " =\r\n");
                foreach (var parameter in collection)
                {
                    sb.Append(identation + '\t' + PrintToString(parameter, nestingLevel + 2));
                }
            }
        }


        private string ReturnWhenCyclic(int nestingLevel)
        {
            return (nestingLevel != 0 ? ParentObj
                : (new StringBuilder(ParentObj)).ToString()) + "\r\n";
        }

        private string ReturnWhenFinal(object obj)
        {
            if (obj is IFormattable formatObj)
                return formatObj.ToString(null, culture) + Environment.NewLine;
            return obj + Environment.NewLine;
        }

        public PrintingConfig<TOwner> ExcludedType<TExType>()
        {
            ExcludedTypes.Add(typeof(TExType));
            return this;
        }

        public PrintingConfig<TOwner> ExcludedProperty<TTarget>(Expression<Func<TOwner, TTarget>> propertyNameExpression)
        {
            if (propertyNameExpression.Body is MemberExpression propertyMember)
            {
                var member = propertyMember.Member;
                if (GetFieldsAndProperties(typeof(TOwner)).Contains(member))
                    ExcludedFieldsProperties.Add(member);
                return this;
            }
            throw new InvalidExpressionException(MemberExpressionMessage);
        }



        public PrintingConfig<TOwner> TrimType<TTrimType>(Borders borders)
        {
            TrimTypes[typeof(TTrimType)] = borders;
            return this;
        }

        public PrintingConfig<TOwner> TrimProperty<TTarget>(Expression<Func<TOwner, TTarget>> propertyExpression,
           Borders borders)
        {
            if (propertyExpression.Body is MemberExpression memberExpression)
            {

                TrimMembers[memberExpression.Member] = borders;
                return this;
            }
            throw new InvalidExpressionException(MemberExpressionMessage);
        }

        public PrintingConfig<TOwner> SpecialSerializationType<TType>(Func<TType, string> specialSerializationForType)
        {
            SpecialSerializationsForTypes[typeof(TType)] = specialSerializationForType;
            return this;
        }

        public PrintingConfig<TOwner> SpecialSerializationField<TFieldType>(Expression<Func<TOwner, TFieldType>> memberAccess, Func<TFieldType, string> serialization)
        {

            if (memberAccess.Body is MemberExpression memberExpression)
            {
                SpecialSerializationsForFieldsProperties[memberExpression.Member] = serialization;
                return this;
            }
            throw new InvalidExpressionException(MemberExpressionMessage);
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo cultureInfo)
        {
            culture = cultureInfo;
            return this;
        }
    }
}