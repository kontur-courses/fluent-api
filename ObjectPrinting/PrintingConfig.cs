using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using ObjectPrinting.Solved;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public Settings settings;
        public Type configType;
        private Dictionary<object, Func<string>> customProps;

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

    public PrintingConfig()
        {
            configType = typeof(TOwner);
            settings = new Settings();
        }

        public TypeConfig<TOwner, TPropType> SelectType<TPropType>()
        {
            return new TypeConfig<TOwner, TPropType>(this, settings);
        }

        public MemberConfig<TOwner, TPropType> SelectProperty<TPropType>(Expression<Func<TOwner, TPropType>> member)
        {
            var memb = ((MemberExpression)member.Body).Member;
            return new MemberConfig<TOwner, TPropType>(this, memb, settings);
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 2);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            
            var valueType = obj.GetType();
            var line = Serialize(valueType, obj, 0);

            return line;
        }

        private string Serialize(object curType, object curValue, int level)
        {
            if (curValue == null) return null;

            var valueType = curValue.GetType();
            if (TypeInBlackList(valueType)) return null;
            if (curType is MemberInfo && MemberInBlackList((MemberInfo)curType)) return null;

            var line = string.Empty;

            if (curValue is IDictionary)
            {
                line = AddDictionary(curType, (IDictionary)curValue, level);
                return line;
            }
            else if (curValue is ICollection)
            {
                line = AddCollection(curType, (ICollection)curValue, level);
            }
            else if (curType is MemberInfo && settings.customMembs.ContainsKey((MemberInfo)curType))
            {
                curValue = settings.customMembs[(MemberInfo)curType](curValue);
                line = GetFinalStr(curType, curValue, level);
                return line;
            }
            else if (settings.customTypes.ContainsKey(curValue.GetType()))
            {
                curValue = settings.customTypes[curValue.GetType()](curValue);
                line = GetFinalStr(curType, curValue, level);
            }
            else if (finalTypes.Contains(valueType))
            {
                curValue = TrySetCulture(curValue);
                line = GetFinalStr(curType, curValue, level);
            }
            else
            {
                line = SerializeMembers(curType, curValue, level);
            }

            return line;
        }

        private object TrySetCulture(object formattableObj)
        {
            var objType = formattableObj.GetType();

            if (formattableObj is IFormattable && settings.customCultures.ContainsKey(objType))
            {
                var formatStr = ((IFormattable)formattableObj).ToString(null, settings.customCultures[objType]);
                return formatStr;
            }

            return formattableObj;
        }

        private string GetFinalStr(object objType, object objValue, int level)
        {
            var builder = new StringBuilder();
            builder.Append(new string('\t', level));
            var valueType = objValue.GetType();
            var line = "";

            if (objType is FieldInfo || objType is PropertyInfo)
            {
                var member = (MemberInfo)objType;
                line = member.Name + " = " + objValue + ";" + '\n';
            }
            else
            {
                line = objValue.ToString();
            }

            builder.Append(line);
            return builder.ToString();
        }

        private bool TypeInBlackList(Type typeToCheck)
        {
            return settings.typesToIgnore.Contains(typeToCheck);
        }

        private bool MemberInBlackList(MemberInfo memberToCheck)
        {
            return settings.membersToIgnor.Contains(memberToCheck);
        }

        private string SerializeMembers(object parent, object current, int level)
        {
            var valueType = current.GetType();
            var build = new StringBuilder();

            var members = valueType.GetFields()
                .Cast<MemberInfo>()
                .Concat(valueType.GetProperties())
                .ToArray();

            build.Append('\t', level);
            build.Append(((MemberInfo)parent).Name + ":\n");

            foreach (var member in members)
            {
                if (member.MemberType is MemberTypes.Field)
                {
                    var line = Serialize(member, ((FieldInfo)member).GetValue(current), level + 1);
                    build.Append(line);
                }
                else
                {
                    var line = Serialize(member, ((PropertyInfo)member).GetValue(current), level + 1);
                    build.Append(line);
                }
            }

            return build.ToString();
        }

        private string AddDictionary(object parent, IDictionary dict, int tabLevel)
        {
            var valueType = dict.GetType().GetGenericArguments();
            var isCollTypeClassMember = (parent is FieldInfo || parent is PropertyInfo);

            if (TypeInBlackList(valueType[0]) && TypeInBlackList(valueType[1])) return null;

            var builder = new StringBuilder();
            builder.Append('\t', tabLevel);
            builder.Append("(dict) ");

            if (isCollTypeClassMember) builder.Append(((MemberInfo)parent).Name + ": ");

            builder.Append("\n");

            foreach (DictionaryEntry entry in dict)
            {
                builder.Append('\t', tabLevel + 1);
                var keys = Serialize(dict, entry.Key, 0);
                builder.Append(keys + " = ");
                var values = Serialize(dict, entry.Value, 0);
                builder.Append(values + ";\n");
            }

            return builder.ToString();
        }

        private string AddCollection(object collType, ICollection collection, int tabLevel)
        {
            var collectionType = collection.GetType();
            var generics = collectionType.GetGenericArguments();
            var elementType = collectionType.GetElementType();
            var isCollTypeClassMember = (collType is FieldInfo || collType is PropertyInfo);

            if (generics.Length != 0) elementType = generics[0];
            if (TypeInBlackList(elementType)) return null;

            var builder = new StringBuilder();
            builder.Append('\t', tabLevel);
            builder.Append("(coll) ");

            if (isCollTypeClassMember)
            {
                builder.Append(((MemberInfo)collType).Name + ": \n");
                builder.Append('\t', tabLevel + 1);
            }
            else
            {
                builder.Append("\n");
            }

            builder.Append("[");
            var index = 0;

            foreach (var item in collection)
            {
                var collectionItem = Serialize(elementType, item, 0);

                if (index + 1 != collection.Count)
                {
                    builder.Append(collectionItem + ", ");
                    index++;
                }
                else
                {
                    builder.Append(collectionItem);
                }
            }

            if (isCollTypeClassMember) builder.Append("];\n");
            else builder.Append("]");
            
            return builder.ToString();
        }
    }
}