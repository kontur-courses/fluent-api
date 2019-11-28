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
            var propertyInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            return new ConcretePropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
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
                throw new ArgumentOutOfRangeException($"{nameof(nestingLevel)} {nestingLevel} was negative");

            return new PrintingConfig<TOwner>(settings.SetNestingLevel(nestingLevel));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            CheckNestingLevel(nestingLevel);
            if (obj == null)
                return "null" + Environment.NewLine;

            if (obj is ICollection collection)
                return PrintCollection(collection, nestingLevel);

            var type = obj.GetType();
            if (settings.WaysToSerializeTypes.ContainsKey(type))
                return settings.WaysToSerializeTypes[type](obj) + Environment.NewLine;

            if (IsFinalType(type))
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
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            return finalTypes.Contains(type);
        }

        private string PrintMembers(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();
            builder.AppendLine(type.Name);

            AddMembersToStringBuilder(GetAllowedPropertiesForType(type), info => PrintProperty(obj, nestingLevel, info),
                indentation, builder);

            AddMembersToStringBuilder(GetAllowedFieldForType(type),
                info => PrintToString(info.GetValue(obj), nestingLevel + 1), indentation, builder);

            return builder.ToString();
        }

        private IEnumerable<FieldInfo> GetAllowedFieldForType(Type type)
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
                ? settings.WaysToSerializeProperties[propertyInfo](propertyInfo.GetValue(obj))
                : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);

            if (settings.MaxLengthsOfProperties.ContainsKey(propertyInfo))
                propertyValue = propertyValue.Truncate(settings.MaxLengthsOfProperties[propertyInfo]);

            return propertyValue;
        }

        private void AddMembersToStringBuilder<TMember>(IEnumerable<TMember> members,
            Func<TMember, string> print, string indentation, StringBuilder builder)
        where TMember : MemberInfo
        {
            foreach (var member in members)
            {
                var memberValue = print(member);
                builder.Append($"{indentation}{member.Name} = {memberValue}");
            }
        }

        private string PrintCollection(ICollection collection, int nestingLevel)
        {
            var builder = new StringBuilder();
            builder.Append("[");
            foreach (var element in collection)
            {
                builder.Append(PrintToString(element, nestingLevel));
            }

            builder.Append("]");
            return builder.ToString();
        }

        private void CheckNestingLevel(int nestingLevel)
        {
            if (nestingLevel > settings.NestingLevel)
            {
                throw new InvalidOperationException(
                    $@"{nameof(nestingLevel)} {nestingLevel} was more than configured {nameof(nestingLevel)} {settings.NestingLevel}
                          possible due to cyclic reference, you can configure {nameof(nestingLevel)} by using {nameof(SetNestingLevel)}");
            }
        }
    }
}