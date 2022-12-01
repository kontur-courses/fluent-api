using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectPrinting
{
    public partial class PrintingConfig<TOwner>
    {
        private Dictionary<string, Func<string, string>> StringCutFunctions =
            new Dictionary<string, Func<string, string>>();


        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private CultureInfo DefaultCulture = null;

        public string PrintToString(TOwner obj)
        {
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType == typeof(string) && !StringCutFunctions.ContainsKey(propertyInfo.Name))
                    StringCutFunctions[propertyInfo.Name] = (s) => s;
            }

            return PrintToString(obj, 0, new List<object>(), "");
        }


        private string PrintToString(object obj, int nestingLevel, List<object> parentObjects, string tail)
        {
            if (parentObjects.Contains(obj))
            {
                return "!Cyclic_Reference!" + Environment.NewLine;
            }

            if (obj == null)
                return "null" + Environment.NewLine;

            if (AlternativeSerializationMethodConfigs.ContainsKey(tail))
            {
                return PrintIfHasAlternatedMethod(obj, tail);
            }

            if (TypeDefaultConfigs.ContainsKey(obj.GetType()))
            {
                return PrintIfHasAlternatedForTypeMethod(obj);
            }

            if (obj is IFormattable && DefaultCulture != null)
            {
                return (obj as IFormattable).ToString("", DefaultCulture);
            }

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (obj is IEnumerable)
            {
                return PrintIEnumerable(obj as IEnumerable, nestingLevel, parentObjects, tail);
            }

            return PrintNested(obj, nestingLevel, parentObjects, tail);
        }

        private string PrintNested(object obj, int nestingLevel, List<object> parentObjects, string tail)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            if (tail != "") tail += '.';
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                string name = tail + propertyInfo.Name;

                if (AlternativeSerializationMethodConfigs.ContainsKey(name) &&
                    AlternativeSerializationMethodConfigs[name].Excluded ||
                    TypeDefaultConfigs.ContainsKey(propertyInfo.PropertyType) &&
                    TypeDefaultConfigs[propertyInfo.PropertyType].Excluded) continue;

                sb.Append(identation + propertyInfo.Name + " = ");
                var value = propertyInfo.GetValue(obj);
                if (propertyInfo.PropertyType == typeof(string)) value = GetStringCut(value as string, name);

                {
                    parentObjects.Add(obj);
                    sb.Append(PrintToString(value,
                        nestingLevel + 1, parentObjects, name));
                    parentObjects.Remove(parentObjects.Count - 1);
                }
            }

            return sb.ToString();
        }

        private string PrintIfHasAlternatedForTypeMethod(object obj)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(TypeDefaultConfigs[obj.GetType()]
                .CalculateStringResult(obj));
            sb.AppendLine();
            return sb.ToString();
        }

        private string PrintIfHasAlternatedMethod(object obj, string tail)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(AlternativeSerializationMethodConfigs[tail]
                .CalculateStringResult(obj));
            sb.AppendLine();
            return sb.ToString();
        }

        private string PrintIEnumerable(IEnumerable obj, int nestingLevel, List<object> parentObjects,
            string tail)
        {
            StringBuilder sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            sb.Append('[');
            sb.AppendLine();
            foreach (var var in (obj as IEnumerable))
            {
                sb.Append(identation);
                sb.Append(PrintToString(var,
                    nestingLevel + 1, parentObjects, tail + "."));
                sb.Remove(sb.Length - 2, 2);
                sb.Append(',');
                sb.AppendLine();
            }

            sb.Append(identation);
            sb.AppendLine("]");
            return sb.ToString();
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo cultureInfo)
        {
            foreach (var info in typeof(TOwner).GetProperties())
            {
                if (info.PropertyType.GetInterface("IFormattable") != null)
                {
                    if (!AlternativeSerializationMethodConfigs.ContainsKey(info.Name))
                        AlternativeSerializationMethodConfigs.Add(info.Name, new PropertySerializationConfig());
                    AlternativeSerializationMethodConfigs[info.Name]
                        .SetNewSerializeMethod<IFormattable>((f) => f.ToString("", cultureInfo));
                }
            }

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
            GetDefaultConfig<T>().Excluded = true;
            return this;
        }


        public PrintingConfig<TOwner> ExcludeProperty<T>(Expression<Func<TOwner, T>> propertyExpression)
        {
            GetConfig(propertyExpression).Excluded = true;
            return this;
        }

        private string GetStringCut(string s, string propertyName)
        {
            if (!StringCutFunctions.ContainsKey(propertyName)) return s;
            else return StringCutFunctions[propertyName](s);
        }

        internal void SetStringCut(Expression<Func<TOwner, string>> propertyExpression, int maxLength)
        {
            CheckExpression(propertyExpression);
            var name = GetFullName(propertyExpression);
            StringCutFunctions[name] = (s) =>
            {
                if (s.Length > maxLength) return s.Substring(0, maxLength);
                else return s;
            };
        }

        private string GetFullName<T>(Expression<Func<TOwner, T>> propertyExpression)
        {
            Regex regex = new Regex("([^.]*)(.)(.*)");
            return regex.Match(propertyExpression.ToString()).Groups[3].ToString();
        }
    }
}