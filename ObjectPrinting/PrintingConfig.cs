using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private const int MaxNestingLevel = 10;
        
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
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel >= MaxNestingLevel)
                return "..." + Environment.NewLine;
            
            if (obj == null)
                return "null" + Environment.NewLine;

            if (TryPrintFinalType(obj, out var result))
                return result;
            
            

            var type = obj.GetType();
            return ObjectView(type, obj, nestingLevel);
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

        private string ObjectView(Type type, object obj, int nestingLevel)
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
        
        public PrintingConfig<TOwner> Excluding<TExcluding>()
        {
            excludedTypes.Add(typeof(TExcluding));
            return this;
        }

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

        public PropertyPrintingConfig<TOwner, TPrintType> Printing<TPrintType>()
        {
            return new PropertyPrintingConfig<TOwner, TPrintType>(this);
        }
        
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
        
        #endregion
    }
}