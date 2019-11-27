using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using ObjectPrinting.Configs.ConfigInterfaces;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Configs
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<PropertyInfo> excludedProperties;
        private readonly Dictionary<Type, LinkedList<Func<object, string>>> typesSerializations;
        private readonly Dictionary<PropertyInfo, LinkedList<Func<object, string>>> propertiesSerializations;
        private readonly object initialObject;
        private readonly HashSet<Type> finalTypes = new HashSet<Type> 
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
            
        };

        private int nestingLevelMax = 10;
        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddExcludedType(Type type)
        {
            var newExcludingTypes = new HashSet<Type>(excludedTypes) {type};
            return new PrintingConfig<TOwner>(
                newExcludingTypes, 
                excludedProperties, 
                typesSerializations, 
                propertiesSerializations,
                nestingLevelMax,
                initialObject);
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddTypeSerialization(
            Type type, 
            Func<object, string> serialization)
        {
            var newSerializations = CopySerializations(typesSerializations);
            if(!newSerializations.ContainsKey(type))
                newSerializations[type] = new LinkedList<Func<object, string>>();
            newSerializations[type].AddLast(obj => serialization(obj));
            return new PrintingConfig<TOwner>(
                excludedTypes,
                excludedProperties,
                newSerializations,
                propertiesSerializations,
                nestingLevelMax,
                initialObject);
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddPropertySerialization(
            PropertyInfo propertyInfo, 
            Func<object, string> serialization)
        {
            var newSerializations = CopySerializations(propertiesSerializations);
            if(!newSerializations.ContainsKey(propertyInfo))
                newSerializations[propertyInfo] = new LinkedList<Func<object, string>>();
            newSerializations[propertyInfo].AddLast(obj => serialization(obj));
            return new PrintingConfig<TOwner>(
                excludedTypes,
                excludedProperties,
                typesSerializations,
                newSerializations,
                nestingLevelMax,
                initialObject);
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddExcludedProperty(PropertyInfo propertyInfo)
        {
            var newExcludingProperties = new HashSet<PropertyInfo>(excludedProperties) {propertyInfo};
            return new PrintingConfig<TOwner>(
                excludedTypes, 
                newExcludingProperties, 
                typesSerializations, 
                propertiesSerializations,
                nestingLevelMax,
                initialObject);
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.SetNestingLevel(int levelMax)
        {
            if(levelMax < 0) throw new ArgumentException();
            nestingLevelMax = levelMax;
            return new PrintingConfig<TOwner>(
                excludedTypes, 
                excludedProperties, 
                typesSerializations, 
                propertiesSerializations,
                levelMax,
                initialObject);
        }

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            typesSerializations = new Dictionary<Type, LinkedList<Func<object, string>>>();
            propertiesSerializations = new Dictionary<PropertyInfo, LinkedList<Func<object, string>>>();
        }

        public PrintingConfig(TOwner initialObject) : this()
        {
            this.initialObject = initialObject;
        }

        private PrintingConfig(
            HashSet<Type> excludedTypes,
            HashSet<PropertyInfo> excludedProperties,
            Dictionary<Type, LinkedList<Func<object, string>>> typesSerializations,
            Dictionary<PropertyInfo, LinkedList<Func<object, string>>> propertiesSerializations,
            int nestingLevelMax,
            object initialObject)
        {
            this.excludedTypes = new HashSet<Type>(excludedTypes);
            this.excludedProperties = new HashSet<PropertyInfo>(excludedProperties);
            this.typesSerializations = CopySerializations(typesSerializations);
            this.propertiesSerializations = propertiesSerializations;
            this.nestingLevelMax = nestingLevelMax;
            this.initialObject = initialObject;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new HashSet<object>(new ReferenceComparer()));
        }
        
        public string PrintToString()
        {
            if(initialObject == null) throw new InvalidOperationException("No object is set - use PrintToString(obj)");
            return PrintToString(initialObject, 0, new HashSet<object>(new ReferenceComparer()));
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> serializedObjects)
        {
            if (nestingLevel == nestingLevelMax)
                return string.Empty;
            if (obj == null)
                return "null" + Environment.NewLine;
            if (serializedObjects.Any(o => o == obj) && nestingLevel > 0)
                return obj.GetType().Name + "..." + Environment.NewLine;
            if (excludedTypes.Contains(obj.GetType()))
                return string.Empty;
            if (typesSerializations.ContainsKey(obj.GetType()) &&
                typesSerializations[obj.GetType()].Count != 0)
                return ApplySerializationChain(obj, typesSerializations[obj.GetType()]);
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            serializedObjects.Add(obj);
            
            if (obj is IEnumerable)
                return PrintEnumerableToString(obj, nestingLevel, CopySerializedObjects(serializedObjects));

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties())
            {
                if(excludedTypes.Contains(propertyInfo.PropertyType)) continue;
                if(excludedProperties.Contains(propertyInfo)) continue;
                if (propertiesSerializations.ContainsKey(propertyInfo) &&
                    propertiesSerializations[propertyInfo].Count != 0)
                {
                    var serialized = ApplySerializationChain(propertyInfo.GetValue(obj), 
                        propertiesSerializations[propertyInfo]);
                    sb.Append(identation + propertyInfo.Name + " = " + serialized);
                    continue;
                }
                var serializedProperty = PrintToString(
                    propertyInfo.GetValue(obj), 
                    nestingLevel + 1,
                    CopySerializedObjects(serializedObjects));
                if(serializedProperty != "")
                    sb.Append(identation + propertyInfo.Name + " = " + serializedProperty);
            }
            return sb.ToString();
        }

        private string PrintEnumerableToString(object obj, int nestingLevel, HashSet<object> serializedObjects)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var identation = new string('\t', nestingLevel + 1);
            
            if (obj is IDictionary asDict)
                return PrintDictionaryToString(nestingLevel, asDict, CopySerializedObjects(serializedObjects));

            foreach (var item in (IEnumerable) obj)
            {
                var serialized = PrintToString(
                    item, 
                    nestingLevel + 1,
                    CopySerializedObjects(serializedObjects));
                if(serialized != "")
                    sb.Append(identation + serialized);
            }
            return sb.ToString();
        }

        private string PrintDictionaryToString(int nestingLevel, IDictionary asDict, HashSet<object> serializedObjects)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            var type = asDict.GetType();
            sb.AppendLine(type.Name);
            foreach (DictionaryEntry entry in asDict)
            {
                var keyPrinted = entry.Key.ToString();
                var valuePrinted = PrintToString(
                    entry.Value, 
                    nestingLevel + 1,
                    CopySerializedObjects(serializedObjects));
                sb.Append(identation + keyPrinted + ": " + valuePrinted);
            }
            return sb.ToString();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            var casted = (IPrintingConfig<TOwner>) this;
            return casted.AddExcludedType(typeof(T));
        }
        
        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> propertyDelegate)
        {
            var propertyInfo = ((MemberExpression) propertyDelegate.Body).Member as PropertyInfo;
            if(propertyInfo == null) throw new ArgumentException();
            var casted = (IPrintingConfig<TOwner>) this;
            return casted.AddExcludedProperty(propertyInfo);
        }

        public PropertySerializationConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new PropertySerializationConfig<TOwner, TPropType>(this);
        }
        
        public PropertySerializationConfig<TOwner, TPropType> Serialize<TPropType>(Expression<Func<TOwner, TPropType>> propertyDelegate)
        {
            var propertyInfo = ((MemberExpression) propertyDelegate.Body).Member as PropertyInfo;
            if(propertyInfo == null) throw new ArgumentException();
            return new PropertySerializationConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> NestingLevel(int levelMax)
        {
            if(levelMax < 0) throw new ArgumentException("Nesting level is less than zero");
            var casted = (IPrintingConfig<TOwner>) this;
            return casted.SetNestingLevel(levelMax);
        }
        
        private HashSet<object> CopySerializedObjects(HashSet<object> serializedObjects) => 
            new HashSet<object>(serializedObjects, new ReferenceComparer());

        private Dictionary<T, LinkedList<Func<object, string>>> CopySerializations<T>(
            Dictionary<T, LinkedList<Func<object, string>>> serializations)
        {
            var newDict = new Dictionary<T, LinkedList<Func<object, string>>>();
            foreach (var pair in serializations)
                newDict[pair.Key] = new LinkedList<Func<object, string>>(serializations[pair.Key]);
            return newDict;
        }

        private string ApplySerializationChain(object obj, LinkedList<Func<object, string>> serializationChain)
        {
            var result = serializationChain.First()(obj);
            foreach (var serialization in serializationChain.Skip(1))
                result = serialization(result);
            return result + Environment.NewLine;
        }
    }
}