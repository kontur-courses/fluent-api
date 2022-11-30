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
        public string PrintToString(TOwner obj)
        {
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType == typeof(string) && !StringCutFunctions.ContainsKey(propertyInfo.Name))
                    StringCutFunctions[propertyInfo.Name] = (s) => s;
            }

            return PrintToString(obj, 0);
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
                string name = propertyInfo.Name;
                if (OverridedConfigs.ContainsKey(name) && OverridedConfigs[name].Excluded) continue;
                
                sb.Append(identation + propertyInfo.Name + " = ");
                var value = propertyInfo.GetValue(obj);
                if (propertyInfo.PropertyType == typeof(string)) value = StringCutFunctions[name](value as string);
                if (OverridedConfigs.ContainsKey(name))
                {
                    sb.Append(OverridedConfigs[name]
                        .GetStringResult(value));
                    sb.AppendLine();
                }
                else
                    sb.Append(PrintToString(value,
                        nestingLevel + 1));

            }

            return sb.ToString();
        }
        
        public PrintingConfig<TOwner> SetCulture(CultureInfo cultureInfo)
        {
            foreach (var info in typeof(TOwner).GetProperties())
            {
                if (info.PropertyType.GetInterface("IFormattable")!=null)
                {
                    if (!OverridedConfigs.ContainsKey(info.Name)) OverridedConfigs.Add(info.Name, new OverrideConfig());
                    OverridedConfigs[info.Name].SetNewSerializeMethod<IFormattable>((f) => f.ToString("", cultureInfo));
                }
            }

            return this;
        }

        public IPropertyConfig<TOwner, T> ConfigForProperty<T>(Expression<Func<TOwner, T>> property)
        {
            return new PropertyConfig<TOwner, T>(this, property);
        }

        public IPropertyConfig<TOwner,T> AlternateForType<T>(Func<TOwner, T> property)
        {
            throw new NotImplementedException();
        }
        
        public IPropertyConfig<TOwner, T> ExcludeType<T>()
        {
            throw new NotImplementedException();
        }

        internal void ExcludeProperty<T>(Expression<Func<TOwner, T>> propertyExpression)
        {
            var info = ExtractProperty(propertyExpression);
            if (!OverridedConfigs.ContainsKey(info.Name)) OverridedConfigs.Add(info.Name, new OverrideConfig());
            OverridedConfigs[info.Name].Exclude();
        }
        internal void OverrideSerializeMethodForProperty<T>(Expression<Func<TOwner, T>> propertyExpression,
            Func<T, string> newMethod)
        {

            var info = ExtractProperty(propertyExpression);
            if (!OverridedConfigs.ContainsKey(info.Name)) OverridedConfigs.Add(info.Name, new OverrideConfig());
            OverridedConfigs[info.Name].SetNewSerializeMethod(newMethod);
        }

        internal void SetStringCut(Expression<Func<TOwner, string>> propertyExpression, int maxLength)
        {
            var info = ExtractProperty(propertyExpression);
            StringCutFunctions[info.Name] = (s) =>
            {
                if (s.Length > maxLength) return s.Substring(0, maxLength);
                else return s;
            };
        }

        private PropertyInfo ExtractProperty<T>(Expression<Func<TOwner, T>> property)
        {
            if (property.Body.NodeType != ExpressionType.MemberAccess) throw new MissingMemberException();
            return (PropertyInfo) ((MemberExpression) property.Body).Member;
        }

        private Dictionary<string, Func<string, string>> StringCutFunctions =
            new Dictionary<string, Func<string, string>>();
        private Dictionary<string, OverrideConfig> OverridedConfigs = new Dictionary<string, OverrideConfig>();
        private class OverrideConfig
        {
            public bool Excluded { get; private set; } = false;
            private Func<object, string> OverrideFunc;

            public OverrideConfig()
            {
                
            }

            public void Exclude()
            {
                Excluded = true;
            }

            public void SetNewSerializeMethod<T>(Func<T, string> newMethod)
            {
                OverrideFunc = (object obj) => newMethod((T) obj);
            }

            public string GetStringResult<T>(T value) => OverrideFunc(value);
        }
    }
}