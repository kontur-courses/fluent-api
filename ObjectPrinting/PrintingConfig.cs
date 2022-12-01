using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

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

            return PrintToString(obj, 0, new List<object>());
        }
        
        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        
        private string PrintToString(object obj, int nestingLevel, List<object> parentObjects)
        {
            if (parentObjects.Contains(obj)) throw new CyclicReferenceException();
            if (obj == null)
                return "null" + Environment.NewLine;

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
                        .CalculateStringResult(value));
                    sb.AppendLine();
                }
                else
                {
                    parentObjects.Add(obj);
                    sb.Append(PrintToString(value,
                        nestingLevel + 1, parentObjects));
                    parentObjects.Remove(parentObjects.Count - 1);
                }
            }

            return sb.ToString();
        }
        
        //TODO
        public PrintingConfig<TOwner> SetCulture(CultureInfo cultureInfo)
        {
            foreach (var info in typeof(TOwner).GetProperties())
            {
                if (info.PropertyType.GetInterface("IFormattable")!=null)
                {
                    if (!OverridedConfigs.ContainsKey(info.Name)) OverridedConfigs.Add(info.Name, new PropertySerializationConfig());
                    OverridedConfigs[info.Name].SetNewSerializeMethod<IFormattable>((f) => f.ToString("", cultureInfo));
                }
            }
            
            return this;
        }

        public IPropertyConfig<TOwner, T> ConfigForProperty<T>(Expression<Func<TOwner, T>> property)
        {
            return new PropertyConfig<TOwner, T>(this, property);
        }

        //TODO
        public PrintingConfig<TOwner> SetSerializeMethodForType<T>(Func<T, string> method)
        {
            foreach (var info in typeof(TOwner).GetProperties().Where(pi=>pi.PropertyType == typeof(T)))
            {
                if (!OverridedConfigs.ContainsKey(info.Name)) OverridedConfigs.Add(info.Name, new PropertySerializationConfig());
                OverridedConfigs[info.Name].SetNewSerializeMethod(method);
            }

            return this;
        }
        
        
        //TODO
        //change behaviour
        public PrintingConfig<TOwner> ExcludeType<T>()
        {
            foreach (var info in typeof(TOwner).GetProperties().Where(pi=>pi.PropertyType == typeof(T)))
            {
                if (!OverridedConfigs.ContainsKey(info.Name)) OverridedConfigs.Add(info.Name, new PropertySerializationConfig());
                OverridedConfigs[info.Name].Excluded = true;
            }

            return this;
        }

        public PrintingConfig<TOwner> ExcludeProperty<T>(Expression<Func<TOwner, T>> propertyExpression)
        {
            GetConfig(propertyExpression).Excluded = true;
            return this;
        }
        internal void SetSerializeMethodForProperty<T>(Expression<Func<TOwner, T>> propertyExpression,
            Func<T, string> newMethod)
        {
            GetConfig(propertyExpression).SetNewSerializeMethod(newMethod);
        }

        internal void SetStringCut(Expression<Func<TOwner, string>> propertyExpression, int maxLength)
        {
            var name = GetFullName(propertyExpression);
            StringCutFunctions[name] = (s) =>
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

        private PropertySerializationConfig GetConfig<T>(Expression<Func<TOwner, T>> propertyExpression)
        {
            var name = GetFullName(propertyExpression);
            if (!OverridedConfigs.ContainsKey(name)) OverridedConfigs.Add(name, new PropertySerializationConfig());
            return OverridedConfigs[name];
        }

        private string GetFullName<T>(Expression<Func<TOwner, T>> propertyExpression)
        {
            Regex regex = new Regex("([^.]*)(.*)");
            return regex.Match(propertyExpression.ToString()).Groups[2].ToString();
        }
        private Dictionary<string, Func<string, string>> StringCutFunctions =
            new Dictionary<string, Func<string, string>>();
        private Dictionary<string, PropertySerializationConfig> OverridedConfigs = new Dictionary<string, PropertySerializationConfig>();
        private class PropertySerializationConfig
        {
            public bool Excluded { get; set; } = false;
            private Func<object, string> OverrideFunc;


            public void SetNewSerializeMethod<T>(Func<T, string> newMethod)
            {
                OverrideFunc = (object obj) => newMethod((T) obj);
            }

            public string CalculateStringResult<T>(T value) => OverrideFunc(value);
        }
    }
}