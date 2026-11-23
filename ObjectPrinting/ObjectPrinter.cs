using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }

        internal static string PrintToString<T>(PrintingConfig<T> config, T? obj)
        {
            return PrintToString(config, obj, 0);
        }

        private static string PrintToString<T>(PrintingConfig<T> config, object? obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            config.printedObjects.TryGetValue(obj, out var printedCount);
            config.printedObjects[obj] = ++printedCount;
            if (printedCount > config.recursionLimit)
            {
                return "<already been>" + Environment.NewLine;
            }

            var objType = obj.GetType();

            if (config.typesPrinting.TryGetValue(objType, out var typeFunc))
            {
                return typeFunc(obj) + Environment.NewLine;
            }

            if (PrintingConfig<T>.PrimitiveTypes.Contains(objType))
            {
                if (config.typesCultureInfo.TryGetValue(objType, out var typeCulture) && obj is IFormattable formattable)
                {
                    return formattable.ToString(null, typeCulture) + Environment.NewLine;
                }
                return obj + Environment.NewLine;
            }

            if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                dynamic kvp = obj;
                return PrintKeyValuePairToString(config, kvp.Key, kvp.Value, nestingLevel);
            }

            if (obj is IEnumerable enumerable)
            {
                return PrintEnumerableToString(config, enumerable, nestingLevel);
            }

            var identation = new string('\t', nestingLevel + 1);
            var closure = new string('\t', nestingLevel) + "}" + Environment.NewLine;
            var sb = new StringBuilder(objType.Name + " {" + Environment.NewLine);
            foreach (var memberInfo in objType.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                var (memberType, value) = memberInfo switch
                {
                    PropertyInfo p => (p.PropertyType, p.GetValue(obj)),
                    FieldInfo f => (f.FieldType, f.GetValue(obj)),
                    _ => (null, null)
                };
                if (memberType is null || value is null) continue;

                if (config.excludedFields.Contains((memberInfo.DeclaringType!, memberInfo.Name)) || config.excludedTypes.Contains(memberType))
                    continue;

                sb.Append(identation + memberInfo.Name + " = "
                    + PrintObjectFieldToString(config, value, nestingLevel + 1, memberInfo.DeclaringType!, memberInfo.Name));
            }

            sb.Append(closure);
            return sb.ToString();
        }

        private static string PrintObjectFieldToString<T>(PrintingConfig<T> config, object? obj, int nestingLevel, Type fieldDeclaringType, string fieldName)
        {
            if (config.fieldsPrinting.TryGetValue((fieldDeclaringType, fieldName), out var fieldFunc))
            {
                return fieldFunc(obj) + Environment.NewLine;
            }
            var objType = obj.GetType();
            if (obj is string strObj && config.stringFieldsTrim.TryGetValue((fieldDeclaringType, fieldName), out int maxLen))
            {
                return (strObj.Length > maxLen ? strObj.Substring(0, maxLen) : strObj) + Environment.NewLine;
            }
            return PrintToString(config, obj, nestingLevel);
        }

        private static string PrintEnumerableToString<T>(PrintingConfig<T> config, IEnumerable enumerable, int nestingLevel)
        {
            var sb = new StringBuilder("{" + Environment.NewLine);
            var identation = new string('\t', nestingLevel + 1);
            var closure = new string('\t', nestingLevel) + "}" + Environment.NewLine;
            sb.Append(identation + "Count = " + enumerable.Cast<object>().Count() + Environment.NewLine);
            var index = 0;
            foreach (var item in enumerable)
            {
                sb.Append(identation + $"[{index}] = ");
                sb.Append(PrintToString(config, item, nestingLevel + 1));
                index++;

            }
            sb.Append(closure);
            return sb.ToString();
        }

        internal static string PrintKeyValuePairToString<T>(PrintingConfig<T> config, object key, object value, int nestingLevel)
        {
            var sb = new StringBuilder("{" + Environment.NewLine);
            var identation = new string('\t', nestingLevel + 1);
            var closure = new string('\t', nestingLevel) + "}" + Environment.NewLine;
            sb.Append(identation + "Key = " + PrintToString(config, key, nestingLevel + 1));
            sb.Append(identation + "Value = " + PrintToString(config, value, nestingLevel + 1));
            sb.Append(closure);
            return sb.ToString();
        }
    }
}