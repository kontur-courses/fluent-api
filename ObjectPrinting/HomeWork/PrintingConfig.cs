using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using static ObjectPrinting.HomeWork.SerializationDelegateMaker;
using static ObjectPrinting.HomeWork.TrimMaker;

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

        internal readonly HashSet<object> serializedMembers = new HashSet<object>();
        internal readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        internal readonly HashSet<MemberInfo> excludedFieldsProperties = new HashSet<MemberInfo>();
        internal readonly Dictionary<Type, Delegate> specialSerializationsForTypes =
            new Dictionary<Type, Delegate>();
        internal readonly Dictionary<MemberInfo, Delegate> specialSerializationsForFieldsProperties =
            new Dictionary<MemberInfo, Delegate>();

        internal readonly Dictionary<Type, Borders> trimTypes =
            new Dictionary<Type, Borders>();
        internal readonly Dictionary<MemberInfo, Borders> trimMembers = 
            new Dictionary<MemberInfo, Borders>();

        private CultureInfo culture = CultureInfo.InvariantCulture;


        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (serializedMembers.Contains(obj))
                return ReturnWhenCyclic(nestingLevel);

            if (obj == null)
                return NullToString + Environment.NewLine;

            var sb = new StringBuilder();
            var objType = obj.GetType();


            if ((finalTypes.Contains(objType) || objType.IsPrimitive) && !excludedTypes.Contains(objType))
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

        /*
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
        */
        /*
        private void PrintMemberInformation(MemberInfo memberInfo, object obj, StringBuilder sb, int nestingLevel, string identation)
        {
            SerializationMemberInfo memberSerialization = default;
            if (memberInfo is PropertyInfo propertyInfo)
            {
                var indexParameters = propertyInfo.GetIndexParameters();
                if (indexParameters.Length != 0)
                {
                    PrintIndexes(obj, sb, identation, Items, nestingLevel);
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

            var serializeDelegate = MakeSerializeDelegate(memberInfo, 
                specialSerializationsForTypes, specialSerializationsForFieldsProperties);
            serializedMembers.Add(obj);

            string specialSerialize;

            if (serializeDelegate != null)
                specialSerialize = (FormSerializeDelegateString(identation, nestingLevel, memberSerialization,
                    serializeDelegate));
            else
                specialSerialize = (FormSerializeString(identation, nestingLevel, memberSerialization));


            if (trimMembers.TryGetValue(memberInfo, out var memberBorders))
                specialSerialize = MakeTrim(identation,memberBorders, memberSerialization);
            if (trimTypes.TryGetValue(memberSerialization.MemberType, out var typeBorders))
                specialSerialize = MakeTrim(identation, typeBorders, memberSerialization);


            sb.Append(specialSerialize);
        }
        */

        /*
        private static string MakeTrim(string identation, Borders typeBorders, SerializationMemberInfo serializationMemberInfo)
        {
            return identation + serializationMemberInfo.MemberName + " = " + 
                   serializationMemberInfo.MemberValue.ToString().Substring(typeBorders.Start, typeBorders.Length) + "\r\n";
        }
        */

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



        public PrintingConfig<TOwner> TrimType<TTrimType>(Borders borders)
        {
            trimTypes[typeof(TTrimType)] = borders;
            return this;
        }

        public PrintingConfig<TOwner> TrimProperty<TTarget>(Expression<Func<TOwner, TTarget>> propertyExpression,
           Borders borders)
        {
            if (propertyExpression.Body is MemberExpression memberExpression)
            {

                trimMembers[memberExpression.Member] = borders;
                return this;
            }
            throw new InvalidExpressionException(MemberExpressionMessage);
        }

        public PrintingConfig<TOwner> SpecialSerializationType<TType>(Func<TType, string> specialSerializationForType)
        {
            specialSerializationsForTypes[typeof(TType)] = specialSerializationForType;
            return this;
        }

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
            culture = cultureInfo;
            return this;
        }
    }
}