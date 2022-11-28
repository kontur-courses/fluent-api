using System;
using System.Collections.Generic;
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
                if (OverridedConfigs.ContainsKey(propertyInfo.Name))
                {
                    sb.Append(OverridedConfigs[propertyInfo.Name]
                        .GetStringResult(propertyInfo.GetValue(obj)));
                    sb.AppendLine();
                }
                else
                    sb.Append(PrintToString(propertyInfo.GetValue(obj),
                        nestingLevel + 1));

            }

            return sb.ToString();
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

        internal void ExcludeProperty<T>(Expression<Func<TOwner, T>> property)
        {
            if (property.Body.NodeType != ExpressionType.MemberAccess) throw new MissingMemberException();
            var info = (PropertyInfo) ((MemberExpression) property.Body).Member;
            if (!OverridedConfigs.ContainsKey(info.Name)) OverridedConfigs.Add(info.Name, new OverrideConfig());
            OverridedConfigs[info.Name].Exclude();
        }
        internal void OverrideSerializeMethodForProperty<T>(Expression<Func<TOwner, T>> property,
            Func<T, string> newMethod)
        {
            if (property.Body.NodeType != ExpressionType.MemberAccess) throw new MissingMemberException();
            var info = (PropertyInfo) ((MemberExpression) property.Body).Member;
            if (!OverridedConfigs.ContainsKey(info.Name)) OverridedConfigs.Add(info.Name, new OverrideConfig());
            OverridedConfigs[info.Name].SetNewSerializeMethod(newMethod);
        }
        
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