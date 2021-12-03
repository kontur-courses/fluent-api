using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Homework
{
    public class ObjectPrinter<TOwner>
    {
        private const int MaxNestingLevel = 10;

        private readonly HashSet<object> printedObjects = new();
        private readonly PrintingRules rules;
        private readonly MyStringBuilder printingObject = new();
        private readonly HashSet<Type> finalTypes = new()
        {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(char),
            typeof(DateTime), typeof(TimeSpan)
        };

        internal ObjectPrinter(PrintingRules rules)
        {
            this.rules = rules;
        }
        
        public string PrintToString(object? obj)
        {
            printingObject.Clear();
            var nullCaseResult = $"null {Environment.NewLine}";
            PrintToString(obj, obj?.GetType().Name + ":" ?? "", 0);
            return !printingObject.IsEmpty() 
                ? printingObject.ToString() 
                : nullCaseResult;
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

        private bool TryPrintKeyValuePair(object pair, int nestingLevel)
        {
            if (TryPrintFinalObject(pair, "", nestingLevel)) return true;

            var valueType = pair.GetType();
            if (valueType.IsGenericType)
            {
                var baseType = valueType.GetGenericTypeDefinition();
                if (baseType == typeof(KeyValuePair<,>))
                {
                    var key = valueType.GetProperty("Key")?.GetValue(pair, null);
                    var value = valueType.GetProperty("Value")?.GetValue(pair, null);
                    return !ShouldIgnore(key!.GetType(), "", nestingLevel) 
                           && !ShouldIgnore(value!.GetType(), "", nestingLevel);
                }
            }
            
            return false;
        }

        private void TryPrintEnumerableElement(int nestingLevel, bool isDict, bool isList, object value, int index)
        {
            if (isDict)
            {
                if (!TryPrintKeyValuePair(value, nestingLevel)) return ;
                var printed = printingObject.Last()!;
                if (printed.StartsWith("\t = ")) printingObject.ReplaceLast("\t" + printed[3..]);
            }
            else
            {
                PrintToString(value, $"[{index++}]", nestingLevel + 1);
                if (!isList) 
                    printingObject.ReplaceLast(printingObject.Last()!.Replace($"[{index - 1}] = ", ""));
            }
        }

        private void TryPrintIEnumerable(object? obj, string name, int nestingLevel)
        {
            if (obj is null) return ;
            var indexer = 0;
            var indentation = new string('\t', nestingLevel);
            var printedIEnumerableObject = new StringBuilder();
            var interfaces = obj.GetType().GetInterfaces();
            printedIEnumerableObject.AppendLine(indentation + name + " = {");
            printingObject.AppendLine(indentation + name + " = {");
            

            printedObjects.Add(obj);
            foreach (var collectionValue in (IEnumerable)obj) 
            {
                if (collectionValue is null || CheckForCycleReference(collectionValue)) continue;
                TryPrintEnumerableElement(
                    nestingLevel,
                    interfaces.Contains(typeof(IDictionary)),
                    interfaces.Contains(typeof(IList)),
                    collectionValue, indexer++);
            }
            
            printingObject.AppendLine(indentation + "}");
        }

        private bool TryPrintFinalObject(object value, string name, int nestingLevel)
        {
            var rName = name +  " = ";
            var indentation = new string('\t', nestingLevel);
            var type = value.GetType();
            
            if (ShouldIgnore(type, name, nestingLevel)) return true;
            if (TryGetPrintRuleFor(type, name, out var serialisationRule))
            {
                printingObject.Append(indentation + rName + serialisationRule.ToString(value) + Environment.NewLine);
                return true;
            }
            if (finalTypes.Contains(type))
            {
                var toAppend = indentation + rName + (type.GetInterfaces().Contains(typeof(IConvertible))
                    ? ((IConvertible) value).ToString(rules.GetCultureForType(type)) : value) + Environment.NewLine;
                printingObject.Append(toAppend);
                return true;
            }

            return false;
        }

        private void  TryPrintPropertyInfo(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            if (MaxNestingLevel == nestingLevel) return ;
            var value = propertyInfo.GetValue(obj);
            
            if (value is null)
            {
                printingObject.Append($"{new string('\t', nestingLevel)}{propertyInfo.Name} = null{Environment.NewLine}");
                return;
            }
            if (TryPrintFinalObject(value, propertyInfo.Name, nestingLevel)) return ;
            var interfaces = propertyInfo.PropertyType.GetInterfaces();
            if (interfaces.Contains(typeof(IEnumerable)))
                TryPrintIEnumerable(value, propertyInfo.Name, nestingLevel);
            else TryPrintNestingObject(value, propertyInfo.Name, nestingLevel, true);
        }

        private void TryPrintNestingObject(object value, string name, int nestingLevel, bool isNestedObject = false)
        {
            var indentation = new string('\t', nestingLevel);
            
            printedObjects.Add(value);
            var nestedProps = value.GetType().GetProperties();
            if (!nestedProps.Any())
            {
                printingObject.Append(indentation + name + " = " + Environment.NewLine);
                return;
            }
            printingObject.AppendLine(indentation + name + (isNestedObject ? " = {" : ""));
            foreach (var nestedPropertyInfo in nestedProps)
            {
                if (CheckForCycleReference(nestedPropertyInfo.GetValue(value))) continue;
                TryPrintPropertyInfo(value, nestedPropertyInfo, nestingLevel + 1);
            }

            printingObject.AppendLine(isNestedObject ? indentation + "}" : "");
        }

        private void PrintToString(object? obj, string name, int nestingLevel)
        {
            if (obj is null) return;
            if (TryPrintFinalObject(obj, name, nestingLevel)) return;
            TryPrintNestingObject(obj, name, nestingLevel);
        }
    }
}