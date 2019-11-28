using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly HashSet<Type> excludingTypes;
        private readonly HashSet<MemberInfo> excludingProperty;
        private readonly Dictionary<Type, Delegate> typeSerialisation;
        private readonly Dictionary<MemberInfo, Delegate> propertySerialisation;

        Dictionary<Type, Delegate> IPrintingConfig.typeSerialisation => typeSerialisation;

        Dictionary<MemberInfo, Delegate> IPrintingConfig.propertySerialisation => propertySerialisation;

        private static readonly Type[] FinalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig()
        {
            excludingTypes = new HashSet<Type>();
            excludingProperty = new HashSet<MemberInfo>();
            typeSerialisation = new Dictionary<Type, Delegate>();
            propertySerialisation = new Dictionary<MemberInfo, Delegate>();
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (obj is IEnumerable enumerable)
            {
                return PrintCollection(enumerable, nestingLevel);
            }

            return PrintClass(obj, nestingLevel);
        }

        private string PrintCollection(IEnumerable enumerable, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = enumerable.GetType();
            sb.AppendLine(type.Name);
            foreach (var el in enumerable)
            {
                sb.Append(identation + PrintToString(el, nestingLevel + 1));
            }

            return sb.ToString();
        }

        private string PrintClass(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var approvedProp = type.GetProperties().Where(prop =>
                !excludingTypes.Contains(prop.PropertyType) && !excludingProperty.Contains(prop));
            var approvedField = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => !excludingTypes.Contains(field.FieldType) && !excludingProperty.Contains(field));

            foreach (var propertyInfo in approvedProp)
            {
                sb.Append(
                    $"{identation}{propertyInfo.Name} = {PrintToString(PrintProperty(propertyInfo, obj), nestingLevel + 1)}");
            }

            foreach (var fieldInfo in approvedField)
            {
                sb.Append(
                    $"{identation}{fieldInfo.Name} = {PrintToString(PrintField(fieldInfo, obj), nestingLevel + 1)}");
            }

            return sb.ToString();
        }

        private object PrintProperty(PropertyInfo propertyInfo, object obj)
        {
            var value = propertyInfo.GetValue(obj);
            if (propertySerialisation.ContainsKey(propertyInfo))
            {
                return propertySerialisation[propertyInfo].DynamicInvoke(value);
            }

            if (typeSerialisation.ContainsKey(propertyInfo.PropertyType))
            {
                return typeSerialisation[propertyInfo.PropertyType].DynamicInvoke(value);
            }

            return value;
        }

        private object PrintField(FieldInfo fieldInfo, object obj)
        {
            var value = fieldInfo.GetValue(obj);

            if (propertySerialisation.ContainsKey(fieldInfo))
            {
                return propertySerialisation[fieldInfo].DynamicInvoke(value);
            }

            if (typeSerialisation.ContainsKey(fieldInfo.FieldType))
            {
                return typeSerialisation[fieldInfo.FieldType].DynamicInvoke(value);
            }

            return value;
        }

        public PropertySerializingConfig<TOwner, T> AlternativeFor<T>()
        {
            return new PropertySerializingConfig<TOwner, T>(this);
        }

        public PropertySerializingConfig<TOwner, T> AlternativeFor<T>(Expression<Func<TOwner, T>> func)
        {
            if (func.Body is MemberExpression memberExpression && IsPropertyOrField(memberExpression.Member))
            {
                return new PropertySerializingConfig<TOwner, T>(this, memberExpression.Member);
            }

            throw new ArgumentException();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludingTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> func)
        {
            if (func.Body is MemberExpression memberExpression && IsPropertyOrField(memberExpression.Member))
            {
                excludingProperty.Add(memberExpression.Member);
            }
            else
            {
                throw new ArgumentException();
            }

            return this;
        }

        private static bool IsPropertyOrField(MemberInfo memberInfo)
        {
            return memberInfo.MemberType == MemberTypes.Property || memberInfo.MemberType == MemberTypes.Field;
        }
    }
}