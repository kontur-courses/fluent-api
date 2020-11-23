using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting
{
    public class AlternativePrinter
    {
        private Dictionary<Type, Func<object, string>> PrintsForTypes { get; }
        private Dictionary<PropertyInfo, Func<object, string>> PrintsForProps { get; }
        private Dictionary<FieldInfo, Func<object, string>> PrintsForFields { get; }

        public AlternativePrinter()
        {
            PrintsForTypes = new Dictionary<Type, Func<object, string>>();
            PrintsForFields = new Dictionary<FieldInfo, Func<object, string>>();
            PrintsForProps = new Dictionary<PropertyInfo, Func<object, string>>();
        }

        private AlternativePrinter(
            Dictionary<Type, Func<object, string>> printsForTypes,
            Dictionary<PropertyInfo, Func<object, string>> printsForProps,
            Dictionary<FieldInfo, Func<object, string>> printsForFields
        )
        {
            PrintsForTypes = printsForTypes;
            PrintsForProps = printsForProps;
            PrintsForFields = printsForFields;
        }

        public bool HasAlternativePrintFor(object value) =>
            value != default && PrintsForTypes.ContainsKey(value.GetType());

        public bool HasAlternativePrintFor(PropertyInfo property) =>
            property != default && PrintsForProps.ContainsKey(property);

        public bool HasAlternativePrintFor(FieldInfo field) =>
            field != default && PrintsForFields.ContainsKey(field);

        public AlternativePrinter AddOrUpdate(Type type, Func<object, string> print) =>
            new AlternativePrinter
            (
                AddOrUpdateDictionary(PrintsForTypes, type, print),
                PrintsForProps,
                PrintsForFields
            );

        public AlternativePrinter AddOrUpdate(PropertyInfo property, Func<object, string> print) =>
            new AlternativePrinter
            (
                PrintsForTypes,
                AddOrUpdateDictionary(PrintsForProps, property, print),
                PrintsForFields
            );

        public AlternativePrinter AddOrUpdate(FieldInfo field, Func<object, string> print) =>
            new AlternativePrinter
            (
                PrintsForTypes,
                PrintsForProps,
                AddOrUpdateDictionary(PrintsForFields, field, print)
            );

        private static Dictionary<TKey, Func<object, string>> AddOrUpdateDictionary<TKey>(
            IDictionary<TKey, Func<object, string>> dict, TKey key,
            Func<object, string> print)
        {
            var newDict = dict.ToDictionary(pair => pair.Key, pair => pair.Value);
            newDict[key] = dict.TryGetValue(key, out var prevState)
                ? obj => print(prevState(obj))
                : print;
            return newDict;
        }

        public string SerializeValue(object obj) => PrintsForTypes[obj.GetType()](obj);
        public string SerializeProperty(PropertyInfo property, object obj) => PrintsForProps[property](obj);
        public string SerializeField(FieldInfo field, object obj) => PrintsForFields[field](obj);
    }
}