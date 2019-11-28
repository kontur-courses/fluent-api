using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfigSettings
    {
        public ImmutableHashSet<Type> TypesToIgnore { get; }
        public ImmutableDictionary<Type, Func<object, string>> WaysToSerializeTypes { get; }
        public ImmutableDictionary<Type, CultureInfo> TypesCultures { get; }
        public ImmutableDictionary<PropertyInfo, Func<object, string>> WaysToSerializeProperties { get; }
        public ImmutableDictionary<PropertyInfo, int> MaxLengthsOfProperties { get; }
        public ImmutableHashSet<PropertyInfo> PropertiesToIgnore { get; }
        public int NestingLevel { get; }

        public PrintingConfigSettings()
        {
            TypesToIgnore = ImmutableHashSet<Type>.Empty;
            WaysToSerializeTypes = ImmutableDictionary<Type, Func<object, string>>.Empty;
            TypesCultures = ImmutableDictionary<Type, CultureInfo>.Empty;
            WaysToSerializeProperties = ImmutableDictionary<PropertyInfo, Func<object, string>>.Empty;
            MaxLengthsOfProperties = ImmutableDictionary<PropertyInfo, int>.Empty;
            PropertiesToIgnore = ImmutableHashSet<PropertyInfo>.Empty;
            NestingLevel = 10;
        }

        public PrintingConfigSettings(ImmutableHashSet<Type> typesToIgnore,
            ImmutableDictionary<Type, Func<object, string>> waysToSerializeTypes,
            ImmutableDictionary<Type, CultureInfo> typesCultures,
            ImmutableDictionary<PropertyInfo, Func<object, string>> waysToSerializeProperties,
            ImmutableDictionary<PropertyInfo, int> maxLengthsOfProperties,
            ImmutableHashSet<PropertyInfo> propertiesToIgnore,
            int nestingLevel)
        {
            TypesToIgnore = typesToIgnore;
            WaysToSerializeTypes = waysToSerializeTypes;
            TypesCultures = typesCultures;
            WaysToSerializeProperties = waysToSerializeProperties;
            MaxLengthsOfProperties = maxLengthsOfProperties;
            PropertiesToIgnore = propertiesToIgnore;
            NestingLevel = nestingLevel;
        }

        public PrintingConfigSettings AddTypeToIgnore(Type type)
        {
            return new PrintingConfigSettings(TypesToIgnore.Add(type), WaysToSerializeTypes, TypesCultures,
                WaysToSerializeProperties, MaxLengthsOfProperties, PropertiesToIgnore, NestingLevel);
        }

        public PrintingConfigSettings AddWayToSerializeType(Type type, Func<object, string> print)
        {
            return new PrintingConfigSettings(TypesToIgnore,
                WaysToSerializeTypes.Add(type, print), TypesCultures,
                WaysToSerializeProperties,
                MaxLengthsOfProperties, PropertiesToIgnore, NestingLevel);
        }

        public PrintingConfigSettings AddTypeCulture(Type type, CultureInfo cultureInfo)
        {
            return new PrintingConfigSettings(TypesToIgnore, WaysToSerializeTypes, TypesCultures.Add(type, cultureInfo),
                WaysToSerializeProperties, MaxLengthsOfProperties, PropertiesToIgnore, NestingLevel);
        }

        public PrintingConfigSettings AddWayToSerializeProperty(PropertyInfo propertyInfo, Func<object, string> print)
        {
            return new PrintingConfigSettings(TypesToIgnore, WaysToSerializeTypes, TypesCultures,
                WaysToSerializeProperties.Add(propertyInfo, print), MaxLengthsOfProperties, PropertiesToIgnore,
                NestingLevel);
        }

        public PrintingConfigSettings AddMaxLengthOfProperty(PropertyInfo propertyInfo, int maxLength)
        {
            return new PrintingConfigSettings(TypesToIgnore, WaysToSerializeTypes, TypesCultures,
                WaysToSerializeProperties, MaxLengthsOfProperties.Add(propertyInfo, maxLength), PropertiesToIgnore,
                NestingLevel);
        }

        public PrintingConfigSettings AddPropertyToIgnore(PropertyInfo propertyInfo)
        {
            return new PrintingConfigSettings(TypesToIgnore, WaysToSerializeTypes, TypesCultures,
                WaysToSerializeProperties, MaxLengthsOfProperties, PropertiesToIgnore.Add(propertyInfo), NestingLevel);
        }

        public PrintingConfigSettings SetNestingLevel(int nestingLevel)
        {
            return new PrintingConfigSettings(TypesToIgnore, WaysToSerializeTypes, TypesCultures,
                WaysToSerializeProperties, MaxLengthsOfProperties, PropertiesToIgnore, nestingLevel);
        }
    }
}