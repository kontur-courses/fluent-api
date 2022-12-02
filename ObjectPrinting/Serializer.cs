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
        private int nesting;
        private HashSet<object> visitedObjects = new HashSet<object>();
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
            nesting = finalNestingLevel;
            SerializeObject(serializeObj, 0);
        }

        public void SerializeObject(object obj, int level)
        {
            if (level > nesting) return;
            
            //Как можно упростить это условие? 
            if (obj is IDictionary || obj is ICollection || obj is IFormattable || obj is string)
            {
                SerializeMember(null, obj, level);
                return;
            }

            var objType = obj.GetType();
            var members = objType.GetFields()
                .Cast<MemberInfo>()
                .Concat(objType.GetProperties())
                .ToArray();

            builder.Append('\t', level);
            builder.Append(objType.Name + ":");

            if (visitedObjects.Contains(obj))
            {
                builder.Append(" cycle;\r\n");
                return;
            }

            visitedObjects.Add(obj);
            builder.Append("\r\n");

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

            visitedObjects.Remove(obj);
        }

        private void SerializeMember(MemberInfo member, object value, int level = 1)
        {
            if (value == null) return;
            
            var valueType = value.GetType();

            if (member != null && (settings.MembersToIgnor.Contains(member) ||
                settings.TypesToIgnore.Contains(valueType))) return;
            
            if (member != null && settings.CustomMembs.ContainsKey(member))
            {
                builder.Append('\t', level);
                builder.Append(settings.CustomMembs[member](value) + "\r\n");
            }
            else if (value is IDictionary)
            {
                builder.Append('\t', level);
                builder.Append("(dict) ") ;
                if (member != null) builder.Append($"{member.Name}:");
                builder.Append("\r\n");
                SerializeCommonType(value, level + 1);
            }
            else if (value is ICollection)
            {
                builder.Append('\t', level);
                builder.Append("(enum) ");
                if (member != null) builder.Append($"{member.Name}:");
                builder.Append("\r\n");
                SerializeCommonType(value, level);
                builder.Append("\r\n");
            }
            else if (finalTypes.Contains(valueType))
            {
                builder.Append('\t', level);
                if (member != null) builder.Append($"{member.Name}: ");
                SerializeCommonType(value, level);
                builder.Append(";\r\n");
            }
            else
            {
                SerializeObject(value, level);
            }

        }
        private void SerializeCommonType(object finalObj, int level)
        {
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
                    finalObj = TryChangeCulture(finalObj);
                }

                if (settings.CustomTypes.ContainsKey(finalObjType)) 
                {
                    builder.Append(settings.CustomTypes[finalObjType](finalObj));
                    return;
                }

                builder.Append(finalObj.ToString());
            }
            else
            {
                SerializeObject(finalObj, level);
            };
        }

        private void AddDictionary(IDictionary dictionary, int level)
        {
            var valueType = dictionary.GetType().GetGenericArguments();

            if (valueType.Any(x => settings.TypesToIgnore.Contains(x))) return;

            foreach (DictionaryEntry entry in dictionary)
            {
                builder.Append('\t', level);
                SerializeCommonType(entry.Key, 0);
                builder.Append(" = ");
                SerializeCommonType(entry.Value, 0);
                builder.Append(";\r\n");
            }
        }

        private void AddCollection(ICollection collection, int level)
        {
            var collectionType = collection.GetType();
            var generics = collectionType.GetGenericArguments();

            if (generics.Any(x => settings.TypesToIgnore.Contains(x))) return;
            
            builder.Append('\t', level + 1);
            builder.Append("[");
            var index = 0;

            foreach (var item in collection)
            {
                SerializeCommonType(item, 0);
               
                if (index + 1 != collection.Count)
                {
                    builder.Append(", ");
                    index++;
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
