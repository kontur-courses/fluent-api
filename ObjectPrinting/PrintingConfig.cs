using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Xml.Schema;
using FluentAssertions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private StringBuilder _serializedResult = new StringBuilder();
        private int _nestingLevel;
        private string _identation;
        private int _maxLength;

        private readonly Type[] _finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        
        private List<Type> _excludingTypes = new List<Type>();
        private List<string> _excludingProperties = new List<string>();

        private Dictionary<PropertyInfo, Func<PropertyInfo, string>> _serializeFunctions = new Dictionary<PropertyInfo, Func<PropertyInfo, string>>();
        private Dictionary<Type, Func<PropertyInfo, string>> _serializeTypeFunctions = new Dictionary<Type, Func<PropertyInfo, string>>();
        private Dictionary<Type, CultureInfo> _cultureInfos = new Dictionary<Type, CultureInfo>();

        public string PrintToString(TOwner obj) => PrintToString(obj, 0).ToString();

        public string PrintToString(TOwner[] objs) => PrintCollections(objs).ToString();
        public string PrintToString(List<TOwner> objs) => PrintCollections(objs).ToString();
        public string PrintToString(Dictionary<string, TOwner> objs)
        {
            var listOfPersons = objs.Values.Select(v => v).ToList();
            return PrintCollections(listOfPersons).ToString();
        }

        private StringBuilder PrintCollections(IEnumerable<TOwner> collections)
        {
            var result = new StringBuilder();
            foreach (var collection in collections)
            {
                result.Append(PrintToString(collection, 0));
                _serializedResult.Clear();
            }

            return result;
        }

        private StringBuilder PrintToString(object obj, int nestingLevel)
        {
            Serialize(obj, nestingLevel);
            Console.WriteLine($"Result {_serializedResult}");
            return _serializedResult;
        }

        private string Serialize(object obj, int nestingLevel) 
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            
            var identation = new string('\t', nestingLevel + 1);
            var objectType = obj.GetType();

            if (_finalTypes.Contains(objectType))
                return obj + Environment.NewLine;
            
            _serializedResult.Append(objectType.Name + Environment.NewLine);
            foreach (var propertyInfo in objectType.GetProperties())
            {
                if (IsExcluded(propertyInfo))
                    continue;
                
                var serializedProperty = identation + propertyInfo.Name + " = ";

                var anotherSerialization = HaveAnotherSerialization(propertyInfo);

                if (anotherSerialization != "")
                    serializedProperty += anotherSerialization + Environment.NewLine;
                else
                    serializedProperty += Serialize(propertyInfo.GetValue(obj), nestingLevel + 1);

                if (_maxLength != 0 && IsTooLongLine(serializedProperty))
                {
                    AppendWithLineBreak(serializedProperty);
                    continue;
                }
                    
                _serializedResult.Append(serializedProperty);
            }

            return "";
        }
        
        private bool IsExcluded(PropertyInfo propertyInfo)
        {
            return _excludingProperties.Contains(propertyInfo.Name) 
                   || _excludingTypes.Contains(propertyInfo.PropertyType);
        }

        private string HaveAnotherSerialization(PropertyInfo propertyInfo)
        {
            if (_serializeFunctions.ContainsKey(propertyInfo))
                return _serializeFunctions[propertyInfo](propertyInfo);
            if (_serializeTypeFunctions.ContainsKey(propertyInfo.PropertyType))
                return _serializeTypeFunctions[propertyInfo.PropertyType](propertyInfo);
            return "";
        }

        private void AppendWithLineBreak(string serializedProperty)
        {
            var idx = 0;
            foreach (var symbol in serializedProperty)
                _serializedResult.Append(idx == _maxLength ? '\n' : symbol);
        }

        private bool IsTooLongLine(string line) => line.Length >= _maxLength;
        
        public PrintingConfig<TOwner> Exclude<TOwnerProperty>()
        {
            _excludingTypes.Add(typeof(TOwnerProperty));
            
            return this;
        }
        
        public PrintingConfig<TOwner> Exclude<TOwnerProperty>(Expression<Func<TOwner, TOwnerProperty>> propertySelector)
        {
            _excludingProperties.Add(propertySelector.Body.ToString().Split(".")[1]);
            
            return this;
        }

        public PropertyPrinter<TOwner, TOwnerProperty> CustomSerialize<TOwnerProperty>(Expression<Func<TOwner, TOwnerProperty>> propertySelector)
        {
            return new PropertyPrinter<TOwner, TOwnerProperty>(this, GetPropertyInfo<TOwnerProperty>(propertySelector));
        }
        
        public PropertyPrinter<TOwner, TOwnerProperty> CustomSerialize<TOwnerProperty>()
        {
            return new PropertyPrinter<TOwner, TOwnerProperty>(this, null);
        }

        public void AddSerilizationOptions<T>(PropertyInfo propertyInfo, Func<PropertyInfo, string> printOptions)
        {
            _serializeFunctions.Add(propertyInfo, printOptions);
        }
        
        public void AddSerilizationOptions<T>(Func<PropertyInfo, string> printOptions)
        {
            _serializeTypeFunctions.Add(typeof(T), printOptions);
        }
        
        private PropertyInfo GetPropertyInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;

            return member as PropertyInfo;
        }
        
        public PrintingConfig<TOwner> MaxLength(int maxLength)
        {
            _maxLength = maxLength;
            
            return this;
        }
        
        public PrintingConfig<TOwner> WithCulture<TOwnerProperty>(CultureInfo cultureInfo)
        {
            _cultureInfos.Add(typeof(TOwnerProperty), cultureInfo);
            
            return this;
        }
    }
}