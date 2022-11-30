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
            
            var builder = new StringBuilder();

            var valueType = obj.GetType();
            var line = Serialize(valueType, obj, 0);
            builder.Append(line);

            return builder.ToString();
        }

        private string Serialize(object parent, object current, int level)
        {
            if (current == null) return null;

            var build = new StringBuilder();
            var valueType = current.GetType();
            var line = string.Empty;

            if (current is IDictionary)
            {
                line = AddDictionary(parent, (IDictionary)current, level);
                return line;
            }

            if (current is ICollection)
            {
                line = AddCollection(parent, (ICollection)current, level);
                return line;
            }

            if (finalTypes.Contains(valueType))
            {
                var builder = new StringBuilder();
                builder.Append(new string('\t', level));

                if(parent is MemberInfo)
                {
                    var member = (MemberInfo)parent;
                    line = member.Name + " = " + current.ToString() + ";" + '\n';
                }
                else
                {
                    line = current.ToString();
                }

                builder.Append(line);
                return builder.ToString();
            }

            var members = valueType.GetFields()
                .Cast<MemberInfo>()
                .Concat(valueType.GetProperties())
                .ToArray();

            build.Append('\t', level);
            build.Append(valueType.Name + ":\n");
            build.Append('\t', level);

            foreach (var member in members)
            {
                if (member.MemberType is MemberTypes.Field)
                {
                    line = Serialize(member, ((FieldInfo)member).GetValue(current), level + 1);
                    build.Append(line);
                }
                else
                {
                    line = Serialize(member, ((PropertyInfo)member).GetValue(current), level + 1);
                    build.Append(line);
                }
            }

            return build.ToString();
        }

        private string AddDictionary(object parent, IDictionary dict, int tabLevel)
        {
            var builder = new StringBuilder();
            builder.Append('\t', tabLevel);

            if (parent is MemberInfo)
            {
                builder.Append("(Dictionary) " + ((MemberInfo)parent).Name + ": \n");
            }

            foreach (DictionaryEntry entry in dict)
            {
                builder.Append('\t', tabLevel + 1);
                builder.Append("[");
                var keys = Serialize(dict, entry.Key, 0);
                builder.Append(keys + "] = [");
                var values = Serialize(dict, entry.Value, 0);
                builder.Append(values + "];\n");
            }

            return builder.ToString();
        }

        private string AddCollection(object parent, ICollection collection, int tabLevel)
        {
            var builder = new StringBuilder();
            builder.Append('\t', tabLevel);

            if (parent is MemberInfo)
            {
                builder.Append("(Collection) " + ((MemberInfo)parent).Name + ": \n");
            }

            builder.Append('\t', tabLevel + 1);
            builder.Append("[");
            var index = 0;

            foreach (var item in collection)
            {
                var collectionItem = Serialize(collection, item, 0);

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

            builder.Append("];\n");
            return builder.ToString();
        }
    }
}