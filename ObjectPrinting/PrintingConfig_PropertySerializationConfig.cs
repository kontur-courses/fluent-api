using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public partial class PrintingConfig<TOwner>
    {
        private Dictionary<Type, PropertySerializationConfig> TypeDefaultConfigs =
            new Dictionary<Type, PropertySerializationConfig>();

        private Dictionary<string, PropertySerializationConfig> AlternativeSerializationMethodConfigs =
            new Dictionary<string, PropertySerializationConfig>();

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