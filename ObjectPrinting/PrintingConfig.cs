using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedNames = new HashSet<string>();
        private readonly Dictionary<Type, Func<object, string>> typesPrintingConfigs = new Dictionary<Type, Func<object, string>>();
        private readonly Dictionary<string, Func<object, string>> namesPrintingConfigs = new Dictionary<string, Func<object, string>>();
        private Func<object, string> stringPrintingConfig;
        private readonly Dictionary<string, Func<object, string>> namesStringPrintingConfigs = new Dictionary<string, Func<object, string>>();


        private readonly HashSet<Type> baseTypes = new HashSet<Type>()
        {
            typeof(int), typeof(double), typeof(float), typeof(long), typeof(short), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(char), typeof(long), typeof(byte),
            typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte)
        };

        public PrintingConfig<TOwner> Excluding<TProp>()
        {
            excludedTypes.Add(typeof(TProp));
            return this;
        }

        public PropertyPrintingConfig<TOwner, TProp> Printing<TProp>()
        {
            var typePrintingConfig = new PropertyPrintingConfig<TOwner, TProp>(this);
            if (typeof(TProp) == typeof(string))
                AddCommonStringPrintingConfig((IPropertyPrintingConfig<string>)typePrintingConfig);
            else
                typesPrintingConfigs[typeof(TProp)] = ExtractPrintingFunction(typePrintingConfig);
            return typePrintingConfig;
        }


        public PropertyPrintingConfig<TOwner, TProp> Printing<TProp>(Expression<Func<TOwner, TProp>> propertyOrMethodGetter)
        {
            var printingConfig = new PropertyPrintingConfig<TOwner, TProp>(this);
            var fullPropertyOrFieldName = GetFullPropertyOrFieldName(propertyOrMethodGetter);
            if (typeof(TProp) == typeof(string))
                AddStringPrintingConfigWithNames((IPropertyPrintingConfig<string>)printingConfig, fullPropertyOrFieldName);
            else
                namesPrintingConfigs[fullPropertyOrFieldName] = ExtractPrintingFunction(printingConfig);
            return printingConfig;
        }

        public PrintingConfig<TOwner> ExcludingPropertyOrField<TProp>(Expression<Func<TOwner, TProp>> propertyOrMethodGetter)
        {
            var propertyOrFieldName = GetFullPropertyOrFieldName(propertyOrMethodGetter);
            excludedNames.Add(propertyOrFieldName);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            if (!IsAddedType(obj.GetType()))
                return string.Empty;
            var printingResult = new PrintingInformation();
            AddObjectPrinting(printingResult, obj, 0);
            return printingResult.GetPrinting();
        }

        private void AddCommonStringPrintingConfig(IPropertyPrintingConfig<string> printingConfig)
        {
            var printingFunction = ExtractPrintingFunction(printingConfig);
            stringPrintingConfig = stringPrintingConfig == null
                ? printingFunction
                : ConcatFunctions(stringPrintingConfig, printingFunction);
            foreach (var name in namesStringPrintingConfigs.Keys.ToArray())
                namesStringPrintingConfigs[name] = ConcatFunctions(namesStringPrintingConfigs[name], printingFunction);
        }

        private void AddStringPrintingConfigWithNames(IPropertyPrintingConfig<string> printingConfig, string name)
        {
            var printingFunction = ExtractPrintingFunction(printingConfig);
            Func<object, string> previousFunction;
            if (namesStringPrintingConfigs.TryGetValue(name, out previousFunction))
                namesStringPrintingConfigs[name] = ConcatFunctions(previousFunction, printingFunction);
            else
                namesStringPrintingConfigs[name] = stringPrintingConfig == null
                    ? printingFunction
                    : ConcatFunctions(stringPrintingConfig, printingFunction);
        }

        private Func<object, string> ConcatFunctions(Func<object, string> firstFunction,
            Func<object, string> secondFunction)
            => (x) => secondFunction(firstFunction(x));

        private Func<object, string> ExtractPrintingFunction<TProp>(IPropertyPrintingConfig<TProp> printingConfig)
            => x => printingConfig.PrintingFunction((TProp)x);

        private string GetFullPropertyOrFieldName<TProp>(Expression<Func<TOwner, TProp>> propertyOrFieldGetter)
        {
            if (propertyOrFieldGetter == null)
                throw new ArgumentException("Expression is null");

            var member = propertyOrFieldGetter.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException("Expression refers to a method, not a property");
            var expressionBody = propertyOrFieldGetter.Body.ToString();
            var expressionParameter = propertyOrFieldGetter.Parameters[0].ToString();

            if (expressionParameter == expressionBody)
                throw new ArgumentException("Expression can not exclude itself");
            if (!expressionBody.StartsWith($"{expressionParameter}."))
                throw new ArgumentException("Expression refers to not given config");

            if (member.Member is PropertyInfo || member.Member is FieldInfo)
                return expressionBody.Substring(expressionParameter.Length);
            throw new ArgumentException("Expression should refers to a field or property!");
        }

        private void AddObjectPrinting(PrintingInformation printingInformation, object addedObject, int nestingLevel, string nameObject = "")
        {
            Func<object, string> printingFunction;
            if (nestingLevel >= 10)
                printingInformation.AddPrintingNewLine("... recursion is too deep");
            else if (addedObject == null)
                printingInformation.AddPrintingNewLine("null");
            else if (TryGetSpecialPrinting(addedObject, nameObject, out printingFunction))
                printingInformation.AddPrintingNewLine(printingFunction(addedObject));
            else if (baseTypes.Contains(addedObject.GetType()))
                printingInformation.AddPrintingNewLine(addedObject.ToString());
            else if (!printingInformation.AddObjectAndSetCurrent(addedObject, nestingLevel, nameObject))
                printingInformation.AddPrintingNewLine("... this item was added already");
            else
                AddCurrentObjectToPrintingResult(printingInformation);
        }

        private void AddCurrentObjectToPrintingResult(PrintingInformation printingInformation)
        {
            var currentObject = printingInformation.CurrentObject;
            var type = currentObject.GetType();
            printingInformation.AddPrinting(type.Name);
            if (currentObject is IEnumerable)
                AddCurrentCollectionToPrinting(printingInformation);
            else
                AddCurrentObjectWithPropertiesToPrinting(printingInformation);
        }

        private void AddCurrentCollectionToPrinting(PrintingInformation printingInformation)
        {
            var indentation = printingInformation.CurrentIndentation;
            var nestingLevel = printingInformation.CurrentNestingLevel;
            var collection = printingInformation.CurrentObject as IEnumerable;
            var objFullName = printingInformation.CurrentFullName;

            printingInformation.AddPrintingNewLine(" {");
            foreach (var obj in GetAddedObjects(collection))
            {
                printingInformation.AddPrinting(indentation);
                AddObjectPrinting(printingInformation, obj, nestingLevel + 1, $"{objFullName}.");
            }
            printingInformation.AddPrintingNewLine($"{indentation}}}");
        }

        private void AddCurrentObjectWithPropertiesToPrinting(PrintingInformation printingInformation)
        {
            var indentation = printingInformation.CurrentIndentation;
            var nestingLevel = printingInformation.CurrentNestingLevel;
            var obj = printingInformation.CurrentObject;
            var objFullName = printingInformation.CurrentFullName;
            var type = obj.GetType();

            printingInformation.AddPrintingNewLine("");

            foreach (var propertyInfo in GetNonExcludedProperties(type))
            {
                var propertyName = $"{objFullName}.{propertyInfo.Name}";
                if (IsExcludedName(propertyName))
                    continue;
                printingInformation.AddPrinting($"{indentation}{propertyName} = ");
                AddObjectPrinting(printingInformation, propertyInfo.GetValue(obj), nestingLevel + 1, propertyName);
            }

            foreach (var fieldInfo in GetNonExcludedFields(type))
            {
                var fieldName = $"{objFullName}.{fieldInfo.Name}";
                if (IsExcludedName(fieldName))
                    continue;
                printingInformation.AddPrinting($"{indentation}{fieldName} = ");
                AddObjectPrinting(printingInformation, fieldInfo.GetValue(obj), nestingLevel + 1, fieldName);
            }
        }

        private IEnumerable<PropertyInfo> GetNonExcludedProperties(Type type)
            => type.GetProperties().Where(x => IsAddedType(x.PropertyType));

        private IEnumerable<FieldInfo> GetNonExcludedFields(Type type)
            => type.GetFields().Where(x => IsAddedType(x.FieldType));

        private IEnumerable GetAddedObjects(IEnumerable items)
            => items.Cast<object>().Where(x => IsAddedType(x.GetType()));

        private bool IsAddedType(Type propertyType)
            => !excludedTypes.Contains(propertyType);

        private bool IsExcludedName(string name)
            => excludedNames.Contains(name);

        private bool TryGetSpecialPrinting(object obj, string nameObject, out Func<object, string> printingFunction)
        {
            var type = obj.GetType();
            return type == typeof(string)
                ? TryGetSpecialStringPrinting(nameObject, out printingFunction)
                : TryGetSpecialNotStringPrinting(type, nameObject, out printingFunction);
        }

        private bool TryGetSpecialStringPrinting(string nameObject,
            out Func<object, string> printingFunction)
        {
            if (nameObject != "" && namesStringPrintingConfigs.TryGetValue(nameObject, out printingFunction) &&
                printingFunction != null)
                return true;
            printingFunction = stringPrintingConfig;
            return printingFunction != null;
        }

        private bool TryGetSpecialNotStringPrinting(Type type, string nameObject,
            out Func<object, string> printingFunction)
        {
            return (nameObject != "" && namesPrintingConfigs.TryGetValue(nameObject, out printingFunction) && printingFunction != null) ||
                   (typesPrintingConfigs.TryGetValue(type, out printingFunction) && printingFunction != null);
        }
    }
}