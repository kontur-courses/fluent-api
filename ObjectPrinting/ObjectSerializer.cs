using System;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class ObjectSerializer<TOwner>
    {
        private StringBuilder _serializedResult = new StringBuilder();
        private PrintingConfig<TOwner> _printingConfig;

        public StringBuilder Result
        {
            get => _serializedResult;
        }
        
        public ObjectSerializer(PrintingConfig<TOwner> printingConfig)
        {
            _printingConfig = printingConfig;
        }
        
        public string Serialize(string name, object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            
            var identation = new string('\t', nestingLevel + 1);
            var objectType = obj.GetType();

            if (_printingConfig.FinalTypes.Contains(objectType))
                return obj + Environment.NewLine;

            if (objectType.IsClass)
            {
                var finalName = name == "" ? objectType.Name : name + " = " + objectType.Name;
                if (_printingConfig.MaxLineLength != 0 && IsTooLongLine(finalName))
                    AppendWithLineBreak(identation + finalName + Environment.NewLine);
                else
                    _serializedResult.Append(identation + finalName + Environment.NewLine);
                identation = new string('\t', nestingLevel + 2);
            }
            
            foreach (var propertyInfo in objectType.GetProperties())
            {
                if (IsExcluded(propertyInfo))
                    continue;
                
                var serializedProperty = identation + propertyInfo.Name + " = ";
                var anotherSerialization = HaveAnotherSerialization(propertyInfo);

                if (anotherSerialization != "")
                    serializedProperty += anotherSerialization + Environment.NewLine;
                else
                {
                    var serializedNonFinalType = Serialize(propertyInfo.Name, propertyInfo.GetValue(obj), nestingLevel + 1);
                    serializedProperty =
                        serializedNonFinalType != "" ? serializedProperty + serializedNonFinalType : "";
                }

                if (_printingConfig.MaxLineLength != 0 && IsTooLongLine(serializedProperty))
                {
                    AppendWithLineBreak(serializedProperty);
                    continue;
                }
                    
                _serializedResult.Append(serializedProperty);
            }

            return "";
        }

        public void Clear()
        {
            _serializedResult.Clear();
            Result.Clear();
        }
            
        private string HaveAnotherSerialization(PropertyInfo propertyInfo)
        {
            if (_printingConfig.SerializeFunctions.ContainsKey(propertyInfo))
                return _printingConfig.SerializeFunctions[propertyInfo](propertyInfo);
            if (_printingConfig.SerializeTypeFunctions.ContainsKey(propertyInfo.PropertyType))
                return _printingConfig.SerializeTypeFunctions[propertyInfo.PropertyType](propertyInfo);
            return "";
        }
        
        private void AppendWithLineBreak(string serializedProperty)
        {
           _serializedResult.Append(
               serializedProperty.Substring(0, _printingConfig.MaxLineLength) + Environment.NewLine);
        }
        
        public bool IsExcluded(PropertyInfo propertyInfo)
        {
            return _printingConfig.ExcludedProperties.Contains(propertyInfo.Name) 
                   || _printingConfig.ExcludedTypes.Contains(propertyInfo.PropertyType);
        }

        public bool IsTooLongLine(string line) => line.Length >= _printingConfig.MaxLineLength;
    }
}