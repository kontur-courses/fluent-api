using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        private readonly Dictionary<MemberInfo, Delegate> ownSerializationsForMembers =
            new Dictionary<MemberInfo, Delegate>();

        private readonly Dictionary<Type, Delegate> ownSerializationsForTypes = new Dictionary<Type, Delegate>();
        private object selectedMember;

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedMembers.Add(GetMemberInfo(memberSelector));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            selectedMember = typeof(TPropType);
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            selectedMember = GetMemberInfo(memberSelector);
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public void AddOwnSerializationForSelectedMember<TPropType>(Func<TPropType, string> print)
        {
            if (selectedMember is Type)
                ownSerializationsForTypes[(Type) selectedMember] = print;
            else if (selectedMember is MemberInfo)
                ownSerializationsForMembers[(MemberInfo) selectedMember] = print;

            selectedMember = null;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var expressionBody = memberSelector.Body.ToString();
            var pattern = new Regex("[\\w]+.[\\w]+");
            if (!pattern.IsMatch(expressionBody))
                throw new Exception("Expression is not in a correct format");
            var propertyOrFieldName = expressionBody.Substring(expressionBody.IndexOf('.') + 1);
            var property = typeof(TOwner).GetProperty(propertyOrFieldName);
            var field = typeof(TOwner).GetField(propertyOrFieldName);
            if (property != null)
                return property;
            if (field != null)
                return field;
            throw new Exception("There is no property or field with this name");
        }

        private string GetOwnSerializationForType(Type type, MemberInfo memberInfo, object obj, string indentation)
        {
            var serializationFunc = ownSerializationsForTypes[type];
            string ownSerialization;
            if (memberInfo is PropertyInfo)
                ownSerialization = (string) serializationFunc.DynamicInvoke(((PropertyInfo) memberInfo).GetValue(obj));
            else
                ownSerialization = (string) serializationFunc.DynamicInvoke(((FieldInfo) memberInfo).GetValue(obj));
            return indentation + memberInfo.Name + " = " +
                   ownSerialization + Environment.NewLine;
        }

        private string GetOwnSerializationForMember(MemberInfo memberInfo, object obj, string indentation)
        {
            var serializationFunc = ownSerializationsForMembers[memberInfo];
            string ownSerialization;
            if (memberInfo is PropertyInfo)
                ownSerialization = (string) serializationFunc.DynamicInvoke(((PropertyInfo) memberInfo).GetValue(obj));
            else
                ownSerialization = (string) serializationFunc.DynamicInvoke(((FieldInfo) memberInfo).GetValue(obj));
            return indentation + memberInfo.Name + " = " +
                   ownSerialization + Environment.NewLine;
        }

        private string GetSerializationForMember(MemberInfo memberInfo, object obj, int nestingLevel)
        {
            var type = memberInfo is PropertyInfo
                ? ((PropertyInfo) memberInfo).PropertyType
                : ((FieldInfo) memberInfo).FieldType;

            var indentation = new string('\t', nestingLevel + 1);
            if (excludedMembers.Contains(memberInfo))
                return null;
            if (excludedTypes.Contains(type))
                return null;
            if (ownSerializationsForMembers.ContainsKey(memberInfo))
                return GetOwnSerializationForMember(memberInfo, obj, indentation);
            if (ownSerializationsForTypes.ContainsKey(type))
                return GetOwnSerializationForType(type, memberInfo, obj, indentation);

            var value = memberInfo is PropertyInfo
                ? ((PropertyInfo) memberInfo).GetValue(obj)
                : ((FieldInfo) memberInfo).GetValue(obj);
            return indentation + memberInfo.Name + " = " +
                   PrintToString(value,
                       nestingLevel + 1);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel > 5)
                return "..." + Environment.NewLine;
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

            var type = obj.GetType();

            if (finalTypes.Contains(type)) return obj + Environment.NewLine;

            var serialization = new StringBuilder();
            serialization.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertySerialization = GetSerializationForMember(propertyInfo, obj, nestingLevel);
                if (propertySerialization == null)
                    continue;
                serialization.Append(propertySerialization);
            }

            foreach (var fieldInfo in type.GetFields().Where(field => !field.IsStatic))
            {
                var fieldSerialization = GetSerializationForMember(fieldInfo, obj, nestingLevel);
                if (fieldSerialization == null)
                    continue;
                serialization.Append(fieldSerialization);
            }

            return serialization.ToString();
        }
    }
}