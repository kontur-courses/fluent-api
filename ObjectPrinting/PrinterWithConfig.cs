using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrinterWithConfig<TOwner>
    {
        private const int MaxNestingLevel = 10;

        private readonly HashSet<object> printedObjects = new();
        private readonly PrintingRules rules;
        private readonly HashSet<Type> finalTypes = new()
        {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(char),
            typeof(DateTime), typeof(TimeSpan)
        };

        internal PrinterWithConfig(PrintingRules rules)
        {
            this.rules = rules;
        }
        
        public string PrintToString(object? obj)
        {
            var nullCaseResult = $"null {Environment.NewLine}";
            return PrintToString(obj, obj?.GetType().Name + ":" ?? "", 0) ?? nullCaseResult;
        }

        private bool ShouldIgnore(Type type, string name, int nestingLevel)
        {
            return rules.ShouldIgnoreInAllNestingLevel(type)
                   || nestingLevel == 1 && rules.ShouldIgnore(type)
                   || nestingLevel == 1 && rules.ShouldIgnore(name);
        }

        private bool TryGetPrintRuleFor(Type type, string name, [NotNullWhen(true)] out SerialisationRule? serialisationRule)
        {
            return rules.TryGetSerialisationMethodForProperty(name, out serialisationRule) 
                   || rules.TryGetSerialisationMethodForType(type, out serialisationRule);
        }

        private bool CheckForCycleReference(object? obj)
        {
            return printedObjects.Any(x => ReferenceEquals(x, obj));
        }

        private bool TryPrintKeyValuePair(object pair, int nestingLevel, int index, out string? result)
        {
            result = null;
            if (TryPrintFinalObject(pair, "", nestingLevel, out result)) return true;

            var valueType = pair.GetType();
            if (valueType.IsGenericType)
            {
                var baseType = valueType.GetGenericTypeDefinition();
                if (baseType == typeof(KeyValuePair<,>))
                {
                    var key = valueType.GetProperty("Key")?.GetValue(pair, null);
                    var value = valueType.GetProperty("Value")?.GetValue(pair, null);
                    if (ShouldIgnore(key!.GetType(), "", nestingLevel) || ShouldIgnore(value!.GetType(), "", nestingLevel)) 
                        return false;
                    result = PrintToString(key, $"Key[{index}]:", nestingLevel) +
                             "\t" + PrintToString(value, $"Value[{index}]", nestingLevel); 
                    
                    return true;
                }
            }
            
            return false;
        }

        private string? TryPrintIEnumerable(object? obj, string name, int nestingLevel)
        {
            if (obj is null) return null;
            var indexer = 0;
            var indentation = new string('\t', nestingLevel);
            var printedIEnumerableObject = new StringBuilder();
            printedIEnumerableObject.AppendLine(indentation + name + " = {");

            printedObjects.Add(obj);
            foreach (var collectionValue in (IEnumerable)obj) 
            {
                if (collectionValue is null || CheckForCycleReference(collectionValue)) continue;
                if (obj.GetType().GetInterfaces().Contains(typeof(IDictionary)))
                {
                    if (!TryPrintKeyValuePair(collectionValue, nestingLevel, indexer++, out var printed) || printed is null) return null;
                    if (printed.StartsWith("\t = ")) 
                        printed = "\t" + printed[3..];
                    printedIEnumerableObject.Append(indentation + printed);
                }
                else
                {
                    var printed = PrintToString(collectionValue, $"[{indexer++}]", nestingLevel + 1);
                    if (printed is null) return null;
                    printedIEnumerableObject.Append(obj.GetType().GetInterfaces().Contains(typeof(IList))
                        ? printed 
                        : printed?.Replace($"[{indexer - 1}] = ", ""));
                }
            }
            return printedIEnumerableObject.AppendLine(indentation + "}").ToString();
        }

        private bool TryPrintFinalObject(object value, string name, int nestingLevel, out string? result)
        {
            result = null;
            var rName = name +  " = ";
            var indentation = new string('\t', nestingLevel);
            var type = value.GetType();
            
            if (ShouldIgnore(type, name, nestingLevel)) return true;
            if (TryGetPrintRuleFor(type, name, out var serialisationRule))
            {
                result = indentation + rName + serialisationRule.ToString(value) + Environment.NewLine;
                return true;
            }
            if (finalTypes.Contains(type))
            {
                result = indentation + rName + (type.GetInterfaces().Contains(typeof(IConvertible))
                    ? ((IConvertible) value).ToString(rules.GetCultureForType(type)) : value) + Environment.NewLine;
                return true;
            }

            return false;
        }

        private string? TryPrintPropertyInfo(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            if (MaxNestingLevel == nestingLevel) return null;
            var value = propertyInfo.GetValue(obj);
            
            if (value is null) return $"{new string('\t', nestingLevel)}{propertyInfo.Name} = null{Environment.NewLine}";
            if (TryPrintFinalObject(value, propertyInfo.Name, nestingLevel, out var f)) return f;
            var interfaces = propertyInfo.PropertyType.GetInterfaces();
            return interfaces.Contains(typeof(IEnumerable)) 
                ? TryPrintIEnumerable(value, propertyInfo.Name, nestingLevel)
                : TryPrintNestingObject(value, propertyInfo.Name, nestingLevel, true);
        }

        private string TryPrintNestingObject(object value, string name, int nestingLevel, bool isNestedObject = false)
        {
            var indentation = new string('\t', nestingLevel);
            var printedObject = new StringBuilder();
            printedObject.AppendLine(indentation + name + (isNestedObject ? " = {" : ""));
            
            printedObjects.Add(value);
            var nestedProps = value.GetType().GetProperties();
            if (!nestedProps.Any()) return indentation + name + " = " + Environment.NewLine;
            foreach (var nestedPropertyInfo in nestedProps)
            {
                if (CheckForCycleReference(nestedPropertyInfo.GetValue(value))) continue;
                printedObject.Append(TryPrintPropertyInfo(value, nestedPropertyInfo, nestingLevel + 1));
            }
            return printedObject.AppendLine(isNestedObject ? indentation + "}" : "").ToString();
        }

        private string? PrintToString(object? obj, string name, int nestingLevel)
        {
            if (obj is null) return null;
            if (TryPrintFinalObject(obj, name, nestingLevel, out var printedObject)) return printedObject;
            return TryPrintNestingObject(obj, name, nestingLevel);
        }
    }
}