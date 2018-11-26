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
        private readonly HashSet<Type> baseTypes = new HashSet<Type>() {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)};

        public PrintingConfig<TOwner> Excluding<TProp>()
        {
            if (typeof(TOwner) == typeof(TProp))
                throw new ArgumentException("Printing can not exclude printing type!");
            excludedTypes.Add(typeof(TProp));
            return this;
        }

        public PropertyPrintingConfig<TOwner, TProp> Printing<TProp>()
        {
            var typePrintingConfig = new PropertyPrintingConfig<TOwner, TProp>(this);
            typesPrintingConfigs[typeof(TProp)] = ExtractPrintingFunction(typePrintingConfig);
            return typePrintingConfig;
        }

        public PropertyPrintingConfig<TOwner, TProp> Printing<TProp>(Expression<Func<TOwner, TProp>> propertyGetter)
        {
            var printingConfig = new PropertyPrintingConfig<TOwner, TProp>(this);
            var propertyName = GetNameProperty(propertyGetter);
            namesPrintingConfigs[propertyName] = ExtractPrintingFunction(printingConfig);
            return printingConfig;
        }

        public PrintingConfig<TOwner> ExcludingProperty<TProp>(Expression<Func<TOwner, TProp>> propertyGetter)
        {
            var propertyName = GetNameProperty(propertyGetter);
            excludedNames.Add(propertyName);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            var printingResult = new PrintingResult();
            AddObjectPrinting(printingResult, obj, 0);
            return printingResult.GetPrinting();
        }

        private Func<object, string> ExtractPrintingFunction<TProp>(IPropertyPrintingConfig<TProp> printingConfig)
            => x => printingConfig.PrintingFunction((TProp)x);

        private string GetNameProperty<TProp>(Expression<Func<TOwner, TProp>> propertyGetter)
        {
            if (propertyGetter == null)
                throw new ArgumentException("Expression is null");

            var member = propertyGetter.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException("Expression refers to a method, not a property");
            var expressionBody = propertyGetter.Body.ToString();
            var expressionParameter = propertyGetter.Parameters[0].ToString();
            
            if (expressionParameter == expressionBody)
                throw new ArgumentException("Expression can not exclude itself");
            if (!expressionBody.StartsWith(expressionParameter + "."))
                throw new ArgumentException("Expression refers to not given config");

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException("Expression refers to a field, not a property");

            return expressionBody.Substring(expressionParameter.Length);
        }

        private void AddObjectPrinting(PrintingResult printingResult, object addedObject, int nestingLevel, string nameObject = "")
        {
            Func<object, string> printingFunction;
            if (nestingLevel >= 10)
                printingResult.AddPrinting("... recursion is too deep" + Environment.NewLine);
            else if (addedObject == null)
                printingResult.AddPrinting("null" + Environment.NewLine);
            else if (TryGetSpecialPrinting(addedObject, nameObject, out printingFunction))
                printingResult.AddPrinting(printingFunction(addedObject) + Environment.NewLine);
            else if (baseTypes.Contains(addedObject.GetType()))
                printingResult.AddPrinting(addedObject + Environment.NewLine);
            else if (!printingResult.AddObjectAndSetCurrent(addedObject, nestingLevel, nameObject))
                printingResult.AddPrinting("... this item was added already" + Environment.NewLine);
            else
            {
                AddCurrentObjectToPrintingResult(printingResult);
            }
        }

        private void AddCurrentObjectToPrintingResult(PrintingResult printingResult)
        {
            var currentObject = printingResult.CurrentObject;
            var type = currentObject.GetType();
            printingResult.AddPrinting(type.Name);
            if (currentObject is IEnumerable)
                AddCurrentCollectionToPrinting(printingResult);
            else
                AddCurrentObjectWithPropertiesToPrinting(printingResult);
        }

        private void AddCurrentCollectionToPrinting(PrintingResult printingResult)
        {
            var identation = printingResult.CurrentIdentation;
            var nestingLevel = printingResult.CurrentNestingLevel;
            var collection = printingResult.CurrentObject as IEnumerable;
            var objFullName = printingResult.CurrentFullName;

            printingResult.AddPrinting(" {" + Environment.NewLine);
            foreach (var obj in GetAddedObjects(collection))
            {
                printingResult.AddPrinting(identation);
                AddObjectPrinting(printingResult, obj, nestingLevel + 1, objFullName + ".");
            }
            printingResult.AddPrinting(identation + "}" + Environment.NewLine);
        }

        private void AddCurrentObjectWithPropertiesToPrinting(PrintingResult printingResult)
        {
            var identation = printingResult.CurrentIdentation;
            var nestingLevel = printingResult.CurrentNestingLevel;
            var obj = printingResult.CurrentObject;
            var objFullName = printingResult.CurrentFullName;
            var type = obj.GetType();

            printingResult.AddPrinting(Environment.NewLine);
            foreach (var propertyInfo in GetNonExcludedProperties(type))
            {
                var propertyName = objFullName + "." + propertyInfo.Name;
                if (IsExcludedName(propertyName))
                    continue;
                printingResult.AddPrinting(identation + propertyName + " = ");
                AddObjectPrinting(printingResult, propertyInfo.GetValue(obj), nestingLevel + 1, propertyName);
            }
        }

        private IEnumerable<PropertyInfo> GetNonExcludedProperties(Type type)
            => type.GetProperties().Where(x => IsAddedProperty(x.PropertyType));

        private IEnumerable GetAddedObjects(IEnumerable items)
            => items.Cast<object>().Where(x => IsAddedProperty(x.GetType()));

        private bool IsAddedProperty(Type propertyType)
            => !excludedTypes.Contains(propertyType);

        private bool IsExcludedName(string name)
            => excludedNames.Contains(name);

        private bool TryGetSpecialPrinting(object obj, string nameObject, out Func<object, string> printingFunction)
        {
            var type = obj.GetType();
            return (nameObject != "" && namesPrintingConfigs.TryGetValue(nameObject, out printingFunction) && printingFunction != null) ||
                   (typesPrintingConfigs.TryGetValue(type, out printingFunction) && printingFunction != null);
        }
    }
}