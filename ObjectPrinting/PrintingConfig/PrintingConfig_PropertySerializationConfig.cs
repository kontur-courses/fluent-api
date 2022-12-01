using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ObjectPrinting.PrintingConfig
{
    public partial class PrintingConfig<TOwner>
    {
        private Dictionary<Type, PropertySerializationConfig> TypeDefaultConfigs =
            new Dictionary<Type, PropertySerializationConfig>();

        private Dictionary<string, PropertySerializationConfig> AlternativeSerializationMethodConfigs =
            new Dictionary<string, PropertySerializationConfig>();

        private class PropertySerializationConfig
        {
            public bool IsExcluded { get; set; }
            private Func<object, string> AlternativeSerializationMethod;


            public void SetNewSerializeMethod<T>(Func<T, string> newMethod)
            {
                AlternativeSerializationMethod = (object obj) => newMethod((T) obj);
            }

            public string Serialize<T>(T value) => AlternativeSerializationMethod(value);
        }

        private PropertySerializationConfig GetDefaultConfig<T>()
        {
            var type = typeof(T);
            if (!TypeDefaultConfigs.ContainsKey(type)) TypeDefaultConfigs.Add(type, new PropertySerializationConfig());
            return TypeDefaultConfigs[type];
        }

        private PropertySerializationConfig GetConfig<T>(Expression<Func<TOwner, T>> propertyExpression)
        {
            CheckExpression(propertyExpression);
            var name = GetFullName(propertyExpression);
            if (!AlternativeSerializationMethodConfigs.ContainsKey(name))
                AlternativeSerializationMethodConfigs.Add(name, new PropertySerializationConfig());
            return AlternativeSerializationMethodConfigs[name];
        }
        
        internal void SetSerializeMethodForProperty<T>(Expression<Func<TOwner, T>> propertyExpression,
            Func<T, string> newMethod)
        {
            CheckExpression(propertyExpression);
            GetConfig(propertyExpression).SetNewSerializeMethod(newMethod);
        }
    }
}