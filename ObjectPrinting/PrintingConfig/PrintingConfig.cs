using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectPrinting.PrintingConfig
{
    public partial class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private CultureInfo DefaultCulture;

        public string PrintToString(TOwner obj)
        {
            foreach (var propertyInfo in obj.GetType().GetProperties())
                if (propertyInfo.PropertyType == typeof(string) && !StringCutFunctions.ContainsKey(propertyInfo.Name))
                    StringCutFunctions[propertyInfo.Name] = s => s;

            return PrintToString(obj, 0, new List<object>(), "");
        }


        private string PrintToString(object obj, int nestingLevel, List<object> parentObjects, string tail)
        {
            if (parentObjects.Contains(obj)) return "!Cyclic_Reference!" + Environment.NewLine;

            if (obj == null)
                return "null" + Environment.NewLine;

            if (AlternativeSerializationMethodConfigs.ContainsKey(tail)) return PrintIfHasAlternatedMethod(obj, tail);

            if (TypeDefaultConfigs.ContainsKey(obj.GetType())) return PrintIfHasAlternatedForTypeMethod(obj);

            if (obj is IFormattable && DefaultCulture != null)
                return (obj as IFormattable).ToString("", DefaultCulture) + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (obj is IEnumerable) return PrintIEnumerable(obj as IEnumerable, nestingLevel, parentObjects, tail);

            return PrintNested(obj, nestingLevel, parentObjects, tail);
        }

        private string PrintNested(object obj, int nestingLevel, List<object> parentObjects, string tail)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            if (tail != "") tail += '.';
            sb.AppendLine(type.Name);
            sb.Append(PrintNestedProperties(obj, nestingLevel, parentObjects, tail));
            sb.Append(PrintNestedFields(obj, nestingLevel, parentObjects, tail));
            return sb.ToString();
        }

        private string PrintNestedProperties(object obj, int nestingLevel, List<object> parentObjects, string tail)
        {
            StringBuilder sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();
            foreach (var propertyInfo in type.GetProperties())
            {
                var name = tail + propertyInfo.Name;

                if ((AlternativeSerializationMethodConfigs.ContainsKey(name) &&
                     AlternativeSerializationMethodConfigs[name].IsExcluded) ||
                    (TypeDefaultConfigs.ContainsKey(propertyInfo.PropertyType) &&
                     TypeDefaultConfigs[propertyInfo.PropertyType].IsExcluded)) continue;

                sb.Append(identation + propertyInfo.Name + " = ");
                var value = propertyInfo.GetValue(obj);
                if (propertyInfo.PropertyType == typeof(string)) value = GetStringCut(value as string, name);

                parentObjects.Add(obj);
                sb.Append(PrintToString(value,
                    nestingLevel + 1, parentObjects, name));
                parentObjects.Remove(parentObjects.Count - 1);
            }

            return sb.ToString();
        }

        private string PrintNestedFields(object obj, int nestingLevel, List<object> parentObjects, string tail)
        {
            StringBuilder sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();
            foreach (var fieldInfo in type.GetFields())
            {
                var name = tail + fieldInfo.Name;
                if ((AlternativeSerializationMethodConfigs.ContainsKey(name) &&
                     AlternativeSerializationMethodConfigs[name].IsExcluded) ||
                    (TypeDefaultConfigs.ContainsKey(fieldInfo.FieldType) &&
                     TypeDefaultConfigs[fieldInfo.FieldType].IsExcluded)) continue;
                
                sb.Append(identation + fieldInfo.Name + " = ");
                var value = fieldInfo.GetValue(obj);
                if (fieldInfo.FieldType == typeof(string)) value = GetStringCut(value as string, name);

                parentObjects.Add(obj);
                sb.Append(PrintToString(value,
                    nestingLevel + 1, parentObjects, name));
                parentObjects.Remove(parentObjects.Count - 1);
                
            }

            return sb.ToString();
        }

        private string PrintIfHasAlternatedForTypeMethod(object obj)
        {
            var sb = new StringBuilder();
            sb.Append(TypeDefaultConfigs[obj.GetType()]
                .Serialize(obj));
            sb.AppendLine();
            return sb.ToString();
        }

        private string PrintIfHasAlternatedMethod(object obj, string tail)
        {
            var sb = new StringBuilder();
            sb.Append(AlternativeSerializationMethodConfigs[tail]
                .Serialize(obj));
            sb.AppendLine();
            return sb.ToString();
        }

        private string PrintIEnumerable(IEnumerable obj, int nestingLevel, List<object> parentObjects,
            string tail)
        {
            IEnumerator enumerator = obj.GetEnumerator();
            if (!enumerator.MoveNext() ||
            TypeDefaultConfigs.ContainsKey(enumerator.Current.GetType()) &&
            TypeDefaultConfigs[enumerator.Current.GetType()].IsExcluded)
            {
                return "Empty" + Environment.NewLine;
            }
                
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            var newLineLength = Environment.NewLine.Length;
            sb.Append('[');
            sb.AppendLine();
            foreach (var var in obj)
            {
                var type = var.GetType();
                if (TypeDefaultConfigs.ContainsKey(type) && TypeDefaultConfigs[type].IsExcluded)
                    continue;

                sb.Append(identation);
                sb.Append(PrintToString(var,
                    nestingLevel + 1, parentObjects, tail + "."));

                sb.Remove(sb.Length - newLineLength, newLineLength);

                sb.Append(',');
                sb.AppendLine();
            }

            sb.Remove(sb.Length - newLineLength - 1, newLineLength + 1);
            sb.AppendLine();
            sb.Append(identation);
            sb.AppendLine("]");
            return sb.ToString();
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo cultureInfo)
        {
            DefaultCulture = cultureInfo;
            return this;
        }


        public IPropertyConfig<TOwner, T> ConfigForProperty<T>(Expression<Func<TOwner, T>> property)
        {
            CheckExpression(property);
            return new PropertyConfig<TOwner, T>(this, property);
        }

        public PrintingConfig<TOwner> SetSerializeMethodForType<T>(Func<T, string> method)
        {
            GetDefaultConfig<T>().SetNewSerializeMethod(method);
            return this;
        }

        private void CheckExpression<T>(Expression<Func<TOwner, T>> expr)
        {
            if (expr.Body.NodeType != ExpressionType.MemberAccess) throw new MissingMemberException();
        }

        public PrintingConfig<TOwner> ExcludeType<T>()
        {
            GetDefaultConfig<T>().IsExcluded = true;
            return this;
        }


        public PrintingConfig<TOwner> ExcludeProperty<T>(Expression<Func<TOwner, T>> propertyExpression)
        {
            GetConfig(propertyExpression).IsExcluded = true;
            return this;
        }


        private string GetFullName<T>(Expression<Func<TOwner, T>> propertyExpression)
        {
            var regex = new Regex("([^.]*)(.)(.*)");
            return regex.Match(propertyExpression.ToString()).Groups[3].ToString();
        }
    }
}