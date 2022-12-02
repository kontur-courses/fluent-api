using FluentAssertions.Equivalency;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace ObjectPrinting
{
    public class Serializer
    {
        private SerializerSettings settings;
        private object serializeObj;
        private int nesting;
        private Type[] finalTypes = new[]
            {
                typeof(int),
                typeof(double),
                typeof(float),
                typeof(string),
                typeof(Guid),
                typeof(DateTime),
                typeof(TimeSpan)
        };

        private StringBuilder builder = new StringBuilder();

        public Serializer(SerializerSettings settings, object serializeObj, int finalNestingLevel)
        {
            this.settings = settings;
            this.serializeObj = serializeObj;
            nesting = finalNestingLevel;
        }

        private void SerializeObject(object obj, int level)
        {
            if (level > nesting) return;

            var objType = obj.GetType();
            var members = objType.GetFields()
                .Cast<MemberInfo>()
                .Concat(objType.GetProperties())
                .ToArray();

            builder.Append(objType.Name + ":\n");

            foreach (var member in members)
            {
                if (member.MemberType is MemberTypes.Field)
                {
                    var fieldMember = (FieldInfo)member;
                    SerializeMember(fieldMember, (fieldMember).GetValue(obj), level + 1);
                }
                else
                {
                    var propertyMember = (PropertyInfo)member;
                    SerializeMember(propertyMember, (propertyMember).GetValue(obj), level + 1);
                }
            }
        }

        private void SerializeMember(MemberInfo member, object value, int level = 1)
        {
            if (level > nesting) return;
            var valueType = value.GetType();

            if (value == null || 
                settings.MembersToIgnor.Contains(member) ||
                settings.CustomTypes.ContainsKey(valueType)) return;

            if (value is IDictionary)
            {
                builder.Append('\t', level);
                builder.Append($"(dict) {member.Name}:\n") ;
                SerializeCommonType(value, level + 1);
            }
            else if (value is ICollection)
            {
                builder.Append('\t', level);
                builder.Append($"(enum) {member.Name}:\n");
                SerializeCommonType(value, level);
                builder.Append(";\n");
            }
            else if (finalTypes.Contains(valueType))
            {
                SerializeCommonType(value, level);
            }
            else
            {
                SerializeObject(value, level + 1);
            }

        }
        private void SerializeCommonType(object finalObj, int level)
        {
            if (level > nesting) return;
            var finalObjType = finalObj.GetType();
            
            if (finalObj is IDictionary)
            {
                AddDictionary((IDictionary) finalObj, level);
            }
            else if (finalObj is ICollection)
            {
                AddCollection((ICollection)finalObj, level);
            }
            else if (finalTypes.Contains(finalObjType))
            {
                if (finalObj is IFormattable)
                {
                    finalObj = TryChangeCulture(finalObjType);
                }

                if (settings.CustomTypes.ContainsKey(finalObjType)) 
                {
                    builder.Append(settings.CustomTypes[finalObjType](finalObj));
                    return;
                }

                builder.Append(finalObj.ToString());
            }
            else throw new ArgumentException();
        }

        private void AddDictionary(IDictionary dictionary, int level)
        {
            var valueType = dictionary.GetType().GetGenericArguments();

            if (valueType.Any(x => settings.TypesToIgnore.Contains(x))) return;

            foreach (DictionaryEntry entry in dictionary)
            {
                builder.Append('\t', level + 1);
                SerializeCommonType(entry.Key, 0);
                builder.Append(" = ");
                SerializeCommonType(entry.Value, 0);
                builder.Append(";\n");
            }
        }

        private void AddCollection(ICollection collection, int tabLevel)
        {
            var collectionType = collection.GetType();
            var generics = collectionType.GetGenericArguments();

            if (generics.Any(x => settings.TypesToIgnore.Contains(x))) return;

            builder.Append("[");
            var index = 0;

            foreach (var item in collection)
            {
                SerializeCommonType(item, 0);
               
                if (index + 1 != collection.Count)
                {
                    builder.Append(item + ", ");
                    index++;
                }
                else
                {
                    builder.Append(item);
                }
            }

            builder.Append("]");
        }

        private object TryChangeCulture(object formattableObj)
        {
            var objType = formattableObj.GetType();

            if (formattableObj is IFormattable && settings.CustomCultures.ContainsKey(objType))
            {
                var formatStr = ((IFormattable)formattableObj).ToString(null, settings.CustomCultures[objType]);
                return formatStr;
            }

            return formattableObj;
        }

        public override string ToString()
        {
            return builder.ToString();
        }

    }
}
