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
        private StringBuilder builder;

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
            builder = new StringBuilder();
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

            var members = obj.GetType().GetFields()
                .Cast<MemberInfo>()
                .Concat(obj.GetType().GetProperties())
                .ToArray();

            foreach (var member in members)
            {
                if (member.MemberType is MemberTypes.Field)
                {
                    LoadDataToBuilder(member,  ((FieldInfo)member).GetValue(obj), 0);
                }
                else
                {
                    LoadDataToBuilder(member,  ((PropertyInfo)member).GetValue(obj), 0);
                }
            }

            return builder.ToString();
        }

        private void LoadDataToBuilder(MemberInfo membInfo, object value, int level)
        {
            if (value == null) return;

            var valueType = value.GetType();
            var line = string.Empty;

            if (settings.typesToIgnore.Contains(valueType) ||
                settings.membersToIgnor.Contains(membInfo))
            {
                return;
            }

            if (settings.customMembs.ContainsKey(membInfo))
            {
                line = settings.customMembs[membInfo](value);
                builder.Append(line);
                return;
            }

            if (settings.customTypes.Keys.Contains(valueType))
            {
                line = settings.customTypes[valueType](value);
                builder.Append(line);
                return;
            }

            if (value is IDictionary)
            {
                AddDictionary(membInfo, (IDictionary)value, level);
                return;
            }

            if (value is ICollection)
            {
                AddCollection(membInfo, (ICollection)value, level);
                return;
            }

            if (finalTypes.Contains(valueType))
            {
                builder.Append(new string('\t', level));

                if (membInfo.MemberType is MemberTypes.Field)
                {
                    line = "Member: " + membInfo.Name + " type: "  +((FieldInfo)membInfo).FieldType.Name + " Value: " + value.ToString() + '\n';
                }
                else
                {
                    line = "Member: " + membInfo.Name + " type: "  +((PropertyInfo)membInfo).PropertyType.Name + " Value: " + value.ToString() + '\n';
                }

                builder.Append(line);
                return;
            }

            var members = valueType.GetFields()
                .Cast<MemberInfo>()
                .Concat(valueType.GetProperties())
                .ToArray();

            foreach (var member in members)
            {
                if (member.MemberType is MemberTypes.Field)
                {
                    LoadDataToBuilder(member, ((FieldInfo)member).GetValue(value), level);
                }
                else
                {
                    LoadDataToBuilder(member, ((PropertyInfo)member).GetValue(value), level);
                }
            }
        }

        private void AddDictionary(MemberInfo memInfo, IDictionary obj, int tabLevel)
        {
            foreach (DictionaryEntry entry in obj)
            {
                LoadDataToBuilder(memInfo, entry.Key, tabLevel + 1);
                LoadDataToBuilder(memInfo, entry.Value, tabLevel + 1);
            }
        }

        private void AddCollection(MemberInfo memInfo, ICollection obj, int tabLevel)
        {
            foreach (var item in obj)
            {
                LoadDataToBuilder(memInfo, item, tabLevel + 1);
            }
        }
    }
}