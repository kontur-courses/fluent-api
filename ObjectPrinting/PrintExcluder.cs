using System;
using System.Collections.Immutable;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintExcluder
    {
        private ImmutableHashSet<Type> TypesForExclude { get; }
        private ImmutableHashSet<PropertyInfo> PropertiesForExclude { get; }
        private ImmutableHashSet<FieldInfo> FieldsForExclude { get; }

        public PrintExcluder()
        {
            TypesForExclude = ImmutableHashSet<Type>.Empty;
            PropertiesForExclude = ImmutableHashSet<PropertyInfo>.Empty;
            FieldsForExclude = ImmutableHashSet<FieldInfo>.Empty;
        }

        private PrintExcluder(
            ImmutableHashSet<FieldInfo> fieldsForExclude,
            ImmutableHashSet<PropertyInfo> propertiesForExclude,
            ImmutableHashSet<Type> typesForExclude)
        {
            FieldsForExclude = fieldsForExclude;
            PropertiesForExclude = propertiesForExclude;
            TypesForExclude = typesForExclude;
        }

        public bool DidExclude(PropertyInfo property) => PropertiesForExclude.Contains(property) ||
                                                         TypesForExclude.Contains(property.PropertyType);

        public bool DidExclude(FieldInfo field) => FieldsForExclude.Contains(field) ||
                                                   TypesForExclude.Contains(field.FieldType);

        public PrintExcluder Exclude(Type type) =>
            new PrintExcluder(FieldsForExclude, PropertiesForExclude, TypesForExclude.Add(type));

        public PrintExcluder Exclude(FieldInfo field) =>
            new PrintExcluder(FieldsForExclude.Add(field), PropertiesForExclude, TypesForExclude);

        public PrintExcluder Exclude(PropertyInfo property) =>
            new PrintExcluder(FieldsForExclude, PropertiesForExclude.Add(property), TypesForExclude);
    }
}