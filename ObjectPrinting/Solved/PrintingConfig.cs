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
        private readonly Excluder excluder = new Excluder();
        private readonly HashSet<object> objects = new HashSet<object>();
        internal readonly AlternativeSerializator serializator = new AlternativeSerializator();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector = null)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector = null)
        {
            excluder.Exclude(memberSelector);
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

                bool CanConsiderProperty() => !excluder.IsExclude(propertyInfo.PropertyType, GetFullName()) &&
                    (FinalTypes.IsFinalType(propertyInfo.PropertyType) || !IsReferenceCircle(propertyInfo.GetValue(obj)));
            }
            return sb.ToString();

            bool IsReferenceCircle(object obj) => objects.Any(o => ReferenceEquals(o, obj));
        }

        private string TryReturnFinalRecursion(object obj, Type type, string fullName, int nestingLevel)
        {
            if (obj == null)
                return "null";

            if (serializator.TrySerializate(obj, type, fullName, out var result))
                return result;

            if (FinalTypes.IsFinalType(type))
                return GetStringFinalType();

            if (obj is ICollection collection)
                return PrintToStringCollection(collection, nestingLevel);

            return null;

            string GetStringFinalType()
            {
                return obj.ToString();
            }
        }

        private string PrintToStringCollection(ICollection collection, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            if (collection is IDictionary dict)
                return GetDictionaryResult(dict);
            return GetCollectionResult(collection);

            string GetDictionaryResult(IDictionary dict)
            {
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

            string GetCollectionResult(ICollection collection)
            {
                var result = new StringBuilder(Environment.NewLine);
                foreach (var i in collection)
                {
                    result.Append(identation + PrintToString(i, nestingLevel + 1, null, true));
                    result.Append(Environment.NewLine);
                }
                return result.ToString();
            }
        }
    }
}