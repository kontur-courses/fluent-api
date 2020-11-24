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
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly Dictionary<MemberInfo, Delegate> ownSerializationsForMembers =
            new Dictionary<MemberInfo, Delegate>();

        private readonly Dictionary<Type, Delegate> ownSerializationsForTypes = new Dictionary<Type, Delegate>();
        private readonly List<SerializedObject> serializedObjectsWithNestingLevel = new List<SerializedObject>();

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
            var selectedMember = typeof(TPropType);
            return new PropertyPrintingConfig<TOwner, TPropType>(this, selectedMember);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var selectedMember = GetMemberInfo(memberSelector);
            return new PropertyPrintingConfig<TOwner, TPropType>(this, selectedMember);
        }

        public void AddOwnSerializationForSelectedMember<TPropType>(Func<TPropType, string> print,
            object selectedMember)
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
            MemberExpression memberExpression;
            try
            {
                memberExpression = (MemberExpression) memberSelector.Body;
            }
            catch
            {
                throw new Exception("Expression is not in a correct format");
            }

            if (memberExpression.Expression == null)
                throw new Exception("Member doesn't exist in a class");
            var member = memberExpression.Member;
            return member;
        }

        private string GetOwnSerializationForType(Type type, MemberInfo memberInfo, object obj, string indentation)
        {
            if (!ownSerializationsForTypes.ContainsKey(type))
                return null;
            var serializationFunc = ownSerializationsForTypes[type];
            var valueOfMember = GetValueOfMember(memberInfo, obj);
            var ownSerialization = (string) serializationFunc.DynamicInvoke(valueOfMember);
            return indentation + memberInfo.Name + " = " +
                   ownSerialization + Environment.NewLine;
        }

        private object GetValueOfMember(MemberInfo memberInfo, object obj)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo) memberInfo).GetValue(obj);
                case MemberTypes.Property:
                    return ((PropertyInfo) memberInfo).GetValue(obj);
            }

            throw new Exception("This member is not a property or a field");
        }

        private Type GetTypeOfMember(MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo) memberInfo).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo) memberInfo).PropertyType;
            }

            throw new Exception("This member is not a property or a field");
        }

        private string GetOwnSerializationForMember(MemberInfo memberInfo, object obj, string indentation)
        {
            if (!ownSerializationsForMembers.ContainsKey(memberInfo))
                return null;
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
            var type = GetTypeOfMember(memberInfo);
            var value = GetValueOfMember(memberInfo, obj);
            var indentation = new string('\t', nestingLevel + 1);
            if (excludedMembers.Contains(memberInfo))
                return null;
            if (excludedTypes.Contains(type))
                return null;
            if (ownSerializationsForMembers.ContainsKey(memberInfo) && value != null)
                return GetOwnSerializationForMember(memberInfo, obj, indentation);
            if (ownSerializationsForTypes.ContainsKey(type) && value != null)
                return GetOwnSerializationForType(type, memberInfo, obj, indentation);

            return indentation + memberInfo.Name + " = " +
                   PrintToString(value,
                       nestingLevel + 1);
        }

        private string GetSerializationForDictionary(IDictionary obj, int nestingLevel)
        {
            var serialization = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var key in obj.Keys)
            {
                serialization.Append(PrintToString(key, nestingLevel).TrimEnd() + ":" + Environment.NewLine);
                serialization.Append(indentation + PrintToString(obj[key], nestingLevel + 1));
            }

            return serialization.ToString();
        }

        private string GetSerializationForIEnumerable(IEnumerable obj, int nestingLevel)
        {
            var serialization = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);
            var index = 0;
            foreach (var element in obj)
            {
                serialization.Append(PrintToString(index, nestingLevel).TrimEnd() + ":" + Environment.NewLine);
                serialization.Append(indentation + PrintToString(element, nestingLevel + 1));
                index++;
            }

            return serialization.ToString();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (serializedObjectsWithNestingLevel.Any(serializedObject =>
                serializedObject.Object.Equals(obj) && serializedObject.NestingLevel < nestingLevel))
                return "Cyclical reference";
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (finalTypes.Contains(type)) return obj + Environment.NewLine;

            serializedObjectsWithNestingLevel.Add(new SerializedObject(obj, nestingLevel));

            if (obj is IEnumerable)
            {
                if (obj is IDictionary)
                    return GetSerializationForDictionary((IDictionary) obj, nestingLevel);
                return GetSerializationForIEnumerable((IEnumerable) obj, nestingLevel);
            }

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