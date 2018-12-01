using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>: ISettings
    {
        private readonly Settings settings;
        Settings ISettings.Settings => settings;

        public PrintingConfig()
        {
            settings = new Settings();
        }

        public TypePrintingConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertySerializationConfig<TOwner, TPropType> Serialize<TPropType>(Expression<Func<TOwner, TPropType>> propertyFunc)
        {
            var memberExpression = (MemberExpression) propertyFunc.Body;
            var result = new PropertySerializationConfig<TOwner, TPropType>(this, memberExpression.Member.Name);

            return result;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            settings.AddTypeToExclude(typeof(TPropType));

            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (settings.GetExcludedTypes().Contains(propertyInfo.PropertyType) ||
                    settings.GetExcludedProperties().Contains(propertyInfo.Name))
                    continue;

                sb.Append(identation + propertyInfo.Name + " = " + 
                          PrepareDataToWrite(obj, propertyInfo, nestingLevel));
            }
            return sb.ToString();
        }

        private string PrepareDataToWrite(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            var rowValue = propertyInfo.GetValue(obj);
            var result = "";

            var type = propertyInfo.PropertyType;
            var name = propertyInfo.Name;

            if (settings.GetChangedTypesSerialization().ContainsKey(type))
                result = HandleTypeSerialization(type, rowValue);

            if (settings.GetPropertiesSerialization().ContainsKey(name))
                result = HandlePropertySerialization(name, result != ""? result : rowValue);

            if (settings.GetChangedCultureInfo().ContainsKey(type))
                result = HandleTypeCultureInfoChanging(type, result != "" ? result : rowValue);

            if (settings.GetStrPropertiesToTrim().ContainsKey(name))
                result = HandleStrPropertiesTrimming(name, result != "" ? result : rowValue);

            return result != "" ? result : PrintToString(rowValue, nestingLevel + 1);
        }

        private string HandleTypeSerialization(Type propertyType, object currentValue)
        {
            return settings.GetChangedTypesSerialization()[propertyType]
                       .DynamicInvoke(currentValue) + Environment.NewLine;
        }

        private string HandlePropertySerialization(string propertyName, object currentValue)
        {
            return settings.GetPropertiesSerialization()[propertyName]
                       .DynamicInvoke(currentValue) + Environment.NewLine;
        }

        private string HandleTypeCultureInfoChanging(Type propertyType, object currentValue)
        {
            return string.Format(settings.GetChangedCultureInfo()[propertyType], "{0}", currentValue)
                   + Environment.NewLine;
        }

        private string HandleStrPropertiesTrimming(string propertyName, object currentValue)
        {
            return settings.GetStrPropertiesToTrim()[propertyName]
                       .Invoke(propertyName) + Environment.NewLine;
        }
    }
}