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
        private List<Type> _excludedTypes = new();
        private List<MemberInfo> _excludedProperties = new();
        private Dictionary<Type, Delegate> _typeConverters = new();
        private Dictionary<MemberInfo, Delegate> _propertyConverters = new();
        internal CultureInfo DoubleCultureInfo { get; set; } = CultureInfo.CurrentCulture;
        internal CultureInfo FloatCultureInfo { get; set; } = CultureInfo.CurrentCulture;
        internal int MaxStringLength { get; set; }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        internal void AddTypeConverter<TParam>(Type type, Func<TParam, string?> converter)
        {
            _typeConverters.Add(type, converter);
        }

        internal void AddPropertyConverter<TParam>(Func<TParam, string> converter, MemberInfo propertyInfo)
        {
            _propertyConverters.Add(propertyInfo, converter);
        }

        public PrintingConfig<TOwner> ExceptType<T>()
        {
            _excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> ExceptProperty(Expression<Func<TOwner, object>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException($"{nameof(propertyExpression)} cannot be null");

            _excludedProperties.Add(GetMemberInfo(propertyExpression));
            return this;
        }

        public ITypeSerializer<TParam, TOwner> ForType<TParam>()
        {
            return new TypeSerializerImpl<TParam, TOwner>(this);
        }

        public IPropertySerializer<TOwner, TProperty> ForProperty<TProperty>(
            Expression<Func<TOwner, TProperty>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException($"{nameof(propertyExpression)} cannot be null");

            return new PropertySerializerImpl<TOwner, TProperty>(this, GetMemberInfo(propertyExpression));
        }

        private static MemberInfo GetMemberInfo<TProperty>(Expression<Func<TOwner, TProperty>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExpression)
                return memberExpression.Member;
            
            if (propertyExpression.Body is UnaryExpression unaryExpression
                     && unaryExpression.Operand is MemberExpression unaryMemberExpression)
                return unaryMemberExpression.Member;
            
            throw new ArgumentException("Expression does not refer to a property or field.");
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
    }
}