using ObjectPrinting.Solved;
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
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedProperties = new();
        private readonly Dictionary<Type, Delegate> typeSerializers = new();
        private readonly Dictionary<string, Delegate> propertySerializers = new();
        private readonly Dictionary<Type, CultureInfo> typeCultures = new();
        private readonly Dictionary<string, int> propertyTrim = new();

        // методы пошли кучно:

        // 1. Исключить из сериализации свойства определенного типа
        public PrintingConfig<TOwner> ExcludePropertiesWithType<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        // 2.  Указать альтернативный способ сериализации для определенного типа
        public PrintingConfiguration<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var memberInfo = GetPropertyName(propertySelector);
            return new PrintingConfiguration<TOwner, TPropType>(this, memberInfo);
        }

        public PrintingConfiguration<TOwner, TPropType> Printing<TPropType>()
        {
            return new PrintingConfiguration<TOwner, TPropType>(this, null);
        }

        // 6. Исключить из сериализации конкретного свойства
        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var propertyName = GetPropertyName(propertySelector);
            excludedProperties.Add(propertyName);
            return this;
        }

        //

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }


        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
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
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        private MemberInfo GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            if (propertySelector.Body is MemberExpression memberExpression)
                return memberExpression.Member;

            if (propertySelector.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
                return operand.Member;

            throw new ArgumentException("Invalid property selector expression");
        }

        internal void AddPropertySerializer<TPropType>(string propertyName, Func<TPropType, string> serializer) =>
            propertySerializers[propertyName] = serializer;

        internal void AddTypeSerializer<TPropType>(Func<TPropType, string> serializer) =>
            typeSerializers[typeof(TPropType)] = serializer;

        internal void AddStringPropertyTrim(string propertyName, int maxLength) =>
            propertyTrim[propertyName] = maxLength;

        internal void AddNumericCulture<TNumericCulture>(CultureInfo culture) =>
            typeCultures[typeof(TNumericCulture)] = culture;
    }
}