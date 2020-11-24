using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private static readonly HashSet<Type> finalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();
        private readonly HashSet<string> excludingFields = new HashSet<string>();
        private readonly HashSet<object> objects = new HashSet<object>();

        internal readonly AlternativeSerializator serializator = new AlternativeSerializator();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludingFields.Add(memberSelector.GetFullNameProperty());
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel, string fullName = "", bool fromCollection = false)
        {
            var type = obj?.GetType();
            objects.Add(obj);

            var result = TryReturnFinalRecursion(obj, type, fullName, nestingLevel);
            if (result != null)
                return result + (!fromCollection ? Environment.NewLine : "");

            var identation = new string('\t', nestingLevel + 1);

            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (CanConsiderProperty())
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                        PrintToString(propertyInfo.GetValue(obj),
                        nestingLevel + 1, GetFullName()));
                }

                string GetFullName() => fullName == null ? null : fullName + '.' + propertyInfo.Name;

                bool CanConsiderProperty() => !IsExcluding() &&
                    (TypeIsFinal(propertyInfo.PropertyType) || IsReferenceCircle(propertyInfo.GetValue(obj)));

                bool IsExcluding() => excludingTypes.Contains(propertyInfo.PropertyType) ||
                    (fullName != null && excludingFields.Contains(GetFullName()));
            }
            return sb.ToString();

            bool IsReferenceCircle(object obj) => !objects.Any(o => ReferenceEquals(o, obj));
        }

        private bool TypeIsFinal(Type type) => type.IsPrimitive || finalTypes.Contains(type);

        private string TryReturnFinalRecursion(object obj, Type type, string fullName, int nestingLevel)
        {
            if (obj == null)
                return "null";

            if (serializator.TrySerializate(obj, type, fullName, out var result))
                return result;

            if (TypeIsFinal(type))
                return GetStringFinalType();

            if (obj is ICollection)
                return PrintToStringCollection(obj, nestingLevel);

            return null;

            string GetStringFinalType()
            {
                return obj.ToString();
            }
        }

        private string PrintToStringCollection(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            if (obj is IDictionary)
                return GetDictionaryResult();
            return GetCollectionResult();

            string GetDictionaryResult()
            {
                var dict = (IDictionary)obj;
                var keys = dict.Keys;
                var result = new StringBuilder(Environment.NewLine);
                foreach (var k in keys)
                {
                    result.Append(identation + "[Key] = [");
                    result.Append(PrintToString(k, nestingLevel + 1, null, true) + "]");
                    result.Append(", [Value] = [");
                    result.Append(PrintToString(dict[k], nestingLevel + 1, null, true) + "]");
                    result.Append(Environment.NewLine);
                }
                return result.ToString();
            }

            string GetCollectionResult()
            {
                var array = (ICollection)obj;
                var result = new StringBuilder(Environment.NewLine);
                foreach (var i in array)
                {
                    result.Append(identation + PrintToString(i, nestingLevel + 1, null, true));
                    result.Append(Environment.NewLine);
                }
                return result.ToString();
            }
        }
    }
}