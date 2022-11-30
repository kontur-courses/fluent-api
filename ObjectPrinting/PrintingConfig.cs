using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private int maxNestingLevel = 10;
        
        #region Fields declaration

        private readonly List<PropertyInfo> excludedProperties;
        private readonly List<Type> excludedTypes;

        private readonly Dictionary<PropertyInfo, Func<object, string>> anotherPropertiesSerializer;
        private readonly Dictionary<Type, Func<object, string>> anotherTypesSerializer;
        
        private readonly Dictionary<PropertyInfo, int> maxPropertiesLengths;
        private readonly Dictionary<Type, int> maxTypesLengths;
        
        private readonly Dictionary<Type, CultureInfo> typesCultureInfos;
        
        #endregion

        public PrintingConfig()
        {
            excludedProperties = new List<PropertyInfo>();
            excludedTypes = new List<Type>();
            anotherPropertiesSerializer = new Dictionary<PropertyInfo, Func<object, string>>();
            anotherTypesSerializer = new Dictionary<Type, Func<object, string>>();
            maxPropertiesLengths = new Dictionary<PropertyInfo, int>();
            maxTypesLengths = new Dictionary<Type, int>();
            typesCultureInfos = new Dictionary<Type, CultureInfo>();
        }

        #region Printing methods
        
        /// <summary>
        /// Метод, котторый позволяет сериализовать объект
        /// </summary>
        /// <param name="obj">Объект для сериализации</param>
        /// <returns>Строковое представление объекта</returns>
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel >= maxNestingLevel)
                return "..." + Environment.NewLine;
            
            if (obj == null)
                return "null" + Environment.NewLine;

            if (TryPrintFinalType(obj, out var finalTypeResult))
                return finalTypeResult;

            if (TryPrintIList(obj, nestingLevel, out var arrayResult))
                return arrayResult;

            if (TryPrintIDictionary(obj, nestingLevel, out var dictResult))
                return dictResult;

            var type = obj.GetType();
            return GetObjectView(type, obj, nestingLevel);
        }

        private bool TryPrintIDictionary(object obj, int nestingLevel, out string result)
        {
            if (!(obj is IDictionary dict))
            {
                result = null;
                return false;
            }

            const string separator = " : ";
            var serialisedItems = new List<string>();
            var identation = new string('\t', nestingLevel + 1);
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (DictionaryEntry dictionaryEntry in dict)
            {
                var key = GetStringValueWithIdentationAndNewLine(dictionaryEntry.Key, nestingLevel);
                var value = GetStringValueWithIdentationAndNewLine(dictionaryEntry.Value, nestingLevel + 1);
                serialisedItems.Add(key + separator + value);
            }

            result = "{" + Environment.NewLine +
                     identation + 
                     string.Join(", "+Environment.NewLine + identation, serialisedItems) + Environment.NewLine +
                     new string('\t', nestingLevel) + "}" + Environment.NewLine;
            return true;

        }

        private bool TryPrintIList(object obj, int nestingLevel, out string result)
        {
            if (!(obj is IList list))
            {
                result = null;
                return false;
            }

            var serializedItems = (
                from object item in list
                select GetStringValueWithIdentationAndNewLine(item, nestingLevel))
                .ToList();

            var view = string.Join(", ", serializedItems);
            var end = view.IndexOf(Environment.NewLine, StringComparison.Ordinal) != -1
                ? Environment.NewLine + new string('\t', nestingLevel) + "]"
                : " ]";
            result = "[ " + view + end + Environment.NewLine;
            return true;
        }

        private string GetStringValueWithIdentationAndNewLine(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            if (TryPrintFinalType(obj, out var itemResult))
            {
                return itemResult.Substring(0, itemResult.Length - 2);
            }
            
            itemResult = PrintToString(obj, nestingLevel + 1);
            return Environment.NewLine + identation + itemResult.Substring(0, itemResult.Length-2);
        }

        private bool TryPrintFinalType(object obj, out string result)
        {
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            
            result = finalTypes.Contains(obj.GetType())
                ? obj + Environment.NewLine
                : null;
            return result != null;
        }

        private string GetObjectView(Type type, object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var currentCulture = CultureInfo.CurrentCulture;
                var changeCulture = typesCultureInfos.TryGetValue(propertyInfo.PropertyType, out var newCulture);
                if (excludedTypes.Contains(propertyInfo.PropertyType)) continue;
                if (excludedProperties.Contains(propertyInfo)) continue;
                if (changeCulture) CultureInfo.CurrentCulture = newCulture;

                sb.Append(identation + propertyInfo.Name + " = " +
                          GetView(propertyInfo, obj, nestingLevel));
                
                if (changeCulture) CultureInfo.CurrentCulture = currentCulture;
            }
            return sb.ToString();
        }

        private string GetView(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            if (anotherTypesSerializer.TryGetValue(propertyInfo.PropertyType, out var typesSerializer))
                return typesSerializer(propertyInfo.GetValue(obj)) + Environment.NewLine;

            if (anotherPropertiesSerializer.TryGetValue(propertyInfo, out var propSerializer))
                return propSerializer(propertyInfo.GetValue(obj)) + Environment.NewLine;

            if (maxTypesLengths.TryGetValue(propertyInfo.PropertyType, out var maxTypeLength))
                return Trim((string)propertyInfo.GetValue(obj), maxTypeLength) + Environment.NewLine;

            if (maxPropertiesLengths.TryGetValue(propertyInfo, out var maxPropLength))
                return Trim((string)propertyInfo.GetValue(obj), maxPropLength) + Environment.NewLine;

            return PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
        }

        private string Trim(string text, int maxLength)
        {
            return text.Length <= maxLength
                ? text
                : text.Substring(0, maxLength) + "...";
        }

        #endregion

        #region Serializer rules changer
        
        // В тестах IDE будет предлагать эти методы. Но если это использовать как библиотеку, то они будут скрыты?
        internal void AddSerializer<T>(PropertyInfo property, Func<T, string> serializer)
        {
            if (typeof(T) != property.PropertyType)
                throw new ArgumentException($"typeof(property) ({property.PropertyType}) и" +
                                            $" передаваемый тип ({typeof(T)}) сериализатора должны совпадать");
            anotherPropertiesSerializer[property] = o => serializer((T)o);
        }
        
        internal void AddSerializer<T>(Type type, Func<T, string> serializer)
        {
            if (typeof(T) != type)
                throw new ArgumentException("type и передаваемый тип сериализатора должны совпадать");
            anotherTypesSerializer[type] = o => serializer((T)o);
        }
        
        #endregion

        #region Max length changer

        internal void SetMaxLength(PropertyInfo property, int maxLength)
        {
            maxPropertiesLengths[property] = maxLength;
        }
        
        internal void SetMaxLength(Type type, int maxLength)
        {
            maxTypesLengths[type] = maxLength;
        }

        #endregion

        #region Culture info changer

        internal void AddCultureInfo(Type type, CultureInfo cultureInfo)
        {
            typesCultureInfos[type] = cultureInfo;
        }

        #endregion

        #region Сonfiguration methods
        
        /// <summary>
        /// Позволяет исключить из сериализации все поля заданного типа
        /// </summary>
        /// <typeparam name="TExcluding">Тип поля, который нужно исключить из сериализации</typeparam>
        /// <returns>Исходный экзкмпляр сериализатора, с обновлёнными параметрами</returns>
        public PrintingConfig<TOwner> Excluding<TExcluding>()
        {
            excludedTypes.Add(typeof(TExcluding));
            return this;
        }
        
        /// <summary>
        /// Позволяет исключить из сериализации конкретное свойство объекта
        /// </summary>
        /// <param name="memberSelector">Выбор свойства, которое необходимо исключить (o => o.Property)</param>
        /// <returns>Исходный экзкмпляр сериализатора, с обновлёнными параметрами</returns>
        /// <exception cref="ArgumentException">В случае передачи не свойства или несуществующего свойства</exception>
        public PrintingConfig<TOwner> Excluding<TExcluding>(Expression<Func<TOwner, TExcluding>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression member))
                throw new ArgumentException("Function must be a member expression.");
            var excludedMember = typeof(TOwner).GetMember(member.Member.Name).FirstOrDefault();
            if (!(excludedMember is PropertyInfo excludedProperty))
                throw new ArgumentException($"Member should be {typeof(TOwner)}'s property");
            excludedProperties.Add(excludedProperty);
            return this;
        }
        
        /// <summary>
        /// Позволяет задать собственное правило сериализации для выбранного типа
        /// </summary>
        /// <typeparam name="TPrintType">Тип, для которого нужно задать правило</typeparam>
        /// <returns>Объект настроек, в котором указывается новое правило</returns>
        public PropertyPrintingConfig<TOwner, TPrintType> Printing<TPrintType>()
        {
            return new PropertyPrintingConfig<TOwner, TPrintType>(this);
        }
        
        /// <summary>
        /// Позволяет задать собственное правило сериализации для конкретного свойства обхекта
        /// </summary>
        /// <param name="memberSelector">Выбор свойства, которое необходимо исключить (o => o.Property)</param>
        /// <returns>Объект настроек, в котором указывается новое правило</returns>
        /// <exception cref="ArgumentException">В случае передачи не свойства или несуществующего свойства</exception>
        public PropertyPrintingConfig<TOwner, TPrintType> Printing<TPrintType>(
            Expression<Func<TOwner, TPrintType>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression member))
                throw new ArgumentException("Function must be a member expression.");
            var printingMember = typeof(TOwner).GetMember(member.Member.Name).FirstOrDefault();
            if (!(printingMember is PropertyInfo printingProperty))
                throw new ArgumentException($"Member should be {typeof(TOwner)}'s property");
            return new PropertyPrintingConfig<TOwner, TPrintType>(this, printingProperty);
        }
        
        /// <summary>
        /// Позволяет задать максимальную глубину погружения при сериализации. (По умолчанию 10)
        /// </summary>
        /// <param name="nestingLevel">Максимальная глубина рекурсивного поиска при сериализации</param>
        /// <returns>Исходный экзкмпляр сериализатора, с обновлёнными параметрами</returns>
        public PrintingConfig<TOwner> WithMaxNestingLevel(int nestingLevel)
        {
            maxNestingLevel = nestingLevel;
            return this;
        }
        #endregion
    }
}