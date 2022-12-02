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
        private StringBuilder _serializedResult = new StringBuilder();
        private int _nestingLevel;
        private string _identation;
        private int _maxLength;
        private ObjectSerializer<TOwner> _serializer;

        private readonly HashSet<Type> _finalTypes = new HashSet<Type>()
        {
            typeof(bool), typeof(sbyte),  typeof(byte),  typeof(short),  typeof(ushort),
            typeof(int),  typeof(uint),  typeof(long), typeof(ulong), typeof(float),
            typeof(double),  typeof(decimal),  typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };
        
        private List<Type> _excludingTypes = new List<Type>();
        private List<string> _excludingProperties = new List<string>();

        private Dictionary<PropertyInfo, Func<PropertyInfo, string>> _serializeFunctions = new Dictionary<PropertyInfo, Func<PropertyInfo, string>>();
        private Dictionary<Type, Func<PropertyInfo, string>> _serializeTypeFunctions = new Dictionary<Type, Func<PropertyInfo, string>>();
        private Dictionary<Type, CultureInfo> _cultureInfos = new Dictionary<Type, CultureInfo>();

        public HashSet<Type> FinalTypes
        {
            get => _finalTypes;
        }

        public int MaxLineLength
        {
            get => _maxLength;
        }

        public Dictionary<PropertyInfo, Func<PropertyInfo, string>> SerializeFunctions
        {
            get => _serializeFunctions;
        }

        public Dictionary<Type, Func<PropertyInfo, string>> SerializeTypeFunctions
        {
            get => _serializeTypeFunctions;
        }

        public Dictionary<Type, CultureInfo> CultureInfos
        {
            get => _cultureInfos;
        }

        public List<Type> ExcludedTypes
        {
            get => _excludingTypes;
        }

        public List<string> ExcludedProperties
        {
            get => _excludingProperties;
        }

        public PrintingConfig()
        {
            _serializer = new ObjectSerializer<TOwner>(this);
        }

        public string PrintToString(TOwner obj) => PrintToString(obj, -1).ToString();
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
                result.Append(PrintToString(collection, -1));
                _serializer.Clear();
            }

            return result;
        }

        private StringBuilder PrintToString(object obj, int nestingLevel)
        {
            _serializer.Serialize("", obj, nestingLevel);
            return _serializer.Result;
        }
        
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
    }
}