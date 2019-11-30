using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly PrintingConfigSettings settings;

        PrintingConfigSettings IPrintingConfig.Settings => settings;

        private readonly Type[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig(PrintingConfigSettings settings)
        {
            this.settings = settings;
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public ConcretePropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            CheckMemberSelector(memberSelector);
            var propertyInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            return new ConcretePropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            CheckMemberSelector(memberSelector);
            var propertyInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            return new PrintingConfig<TOwner>(settings.AddPropertyToIgnore(propertyInfo));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            return new PrintingConfig<TOwner>(settings.AddTypeToIgnore(typeof(TPropType)));
        }

        public PrintingConfig<TOwner> SetNestingLevel(int nestingLevel)
        {
            if (nestingLevel < 0)
                throw new ArgumentException($"{nameof(nestingLevel)} {nestingLevel} was negative");

            return new PrintingConfig<TOwner>(settings.SetNestingLevel(nestingLevel));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private void CheckMemberSelector<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector == null)
            {
                throw new ArgumentException($"{nameof(memberSelector)} was null");
            }

            if (!(memberSelector.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo))
            {
                throw new ArgumentException($"{nameof(memberSelector)} should be property get function");
            }
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (obj is ICollection collection)
                return PrintCollection(collection, nestingLevel);

            var type = obj.GetType();
            if (settings.WaysToSerializeTypes.ContainsKey(type))
                return settings.WaysToSerializeTypes[type](obj) + Environment.NewLine;

            if (IsFinalType(type) || nestingLevel >= settings.NestingLevel)
            {
                return (settings.TypesCultures.ContainsKey(type)
                           ? string.Format(settings.TypesCultures[type], "{0}", obj)
                           : obj) +
                       Environment.NewLine;
            }

            return PrintMembers(obj, nestingLevel);
        }

        private bool IsFinalType(Type type)
        {
            return finalTypes.Contains(type);
        }

        private string PrintMembers(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            var builder = new StringBuilder();
            builder.AppendLine(type.Name);

            AddMembersToStringBuilder(GetAllowedPropertiesForType(type), info =>
                {
                    var propertyValue = PrintProperty(obj, nestingLevel, info);
                    return $"{info.Name} = {propertyValue}";
                },
                nestingLevel, builder);

            AddMembersToStringBuilder(GetAllowedFieldsForType(type),
                info =>
                {
                    var fieldValue = PrintToString(info.GetValue(obj), nestingLevel + 1);
                    return $"{info.Name} = {fieldValue}";
                }, nestingLevel, builder);

            return builder.ToString();
        }

        private IEnumerable<FieldInfo> GetAllowedFieldsForType(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(f => !settings.TypesToIgnore.Contains(f.GetType()));
        }

        private IEnumerable<PropertyInfo> GetAllowedPropertiesForType(Type type)
        {
            return type.GetProperties()
                .Where(p => !settings.TypesToIgnore.Contains(p.PropertyType) &&
                            !settings.PropertiesToIgnore.Contains(p));
        }

        private string PrintProperty(object obj, int nestingLevel, PropertyInfo propertyInfo)
        {
            var propertyValue = settings.WaysToSerializeProperties.ContainsKey(propertyInfo)
                ? settings.WaysToSerializeProperties[propertyInfo](propertyInfo.GetValue(obj)) + Environment.NewLine
                : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);

            if (settings.MaxLengthsOfProperties.ContainsKey(propertyInfo))
            {
                var propertyValueWithoutNewLine = propertyValue.Substring(0, propertyValue.Length - 2);
                propertyValue = propertyValueWithoutNewLine.Truncate(settings.MaxLengthsOfProperties[propertyInfo]) +
                                Environment.NewLine;
            }

            return propertyValue;
        }

        private void AddMembersToStringBuilder<TMember>(IEnumerable<TMember> members,
            Func<TMember, string> print, int nestingLevel, StringBuilder builder)
        {
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var member in members)
            {
                builder.Append($"{indentation}{print(member)}");
            }
        }

        private string PrintCollection(ICollection collection, int nestingLevel)
        {
            var builder = new StringBuilder();
            builder.AppendLine(collection.GetType().Name);
            AddMembersToStringBuilder(collection.Cast<object>(),
                element => PrintToString(element, nestingLevel + 1),
                nestingLevel,
                builder);

            return builder.ToString();
        }
    }
}