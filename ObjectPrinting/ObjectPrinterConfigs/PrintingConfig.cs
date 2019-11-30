using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();

        private readonly HashSet<string> excludingProperties = new HashSet<string>();

        private readonly Dictionary<Type, ISerializerConfig<TOwner>> typeSerializerConfigs 
            = new Dictionary<Type, ISerializerConfig<TOwner>>();

        private readonly Dictionary<string, ISerializerConfig<TOwner>> propertySerializerConfigs 
            = new Dictionary<string, ISerializerConfig<TOwner>>();

        HashSet<Type> IPrintingConfig<TOwner>.ExcludingTypes => excludingTypes;

        HashSet<string> IPrintingConfig<TOwner>.ExcludingProperties => excludingProperties;

        Dictionary<Type, ISerializerConfig<TOwner>> IPrintingConfig<TOwner>.TypeSerializerConfigs =>
            typeSerializerConfigs;

        Dictionary<string, ISerializerConfig<TOwner>> IPrintingConfig<TOwner>.PropertySerializerConfigs =>
            propertySerializerConfigs;

        string IPrintingConfig<TOwner>.Print(object obj, int nestingLevel) => Print(obj, nestingLevel);

        private string Print(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            return PrintingObjectFactory<TOwner>
                .MakePrintingObject(obj, this)
                .Print(nestingLevel);
        }

        public string PrintToString(TOwner obj)
        {
            return Print(obj, 0);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludingTypes.Add(typeof(T));
            return this;
        }
        
        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            excludingProperties.Add((func.Body as MemberExpression)?.Member.Name);
            return this;
        }

        public SerializerConfig<TOwner,T> Serialize<T>()
        {
            var config = new SerializerConfig<TOwner, T>(this);
            typeSerializerConfigs[typeof(T)] = config;
            return config;
        }

        public SerializerConfig<TOwner,T> Serialize<T>(Expression<Func<TOwner, T>> func)
        {
            var argName = (func.Body as MemberExpression)?.Member.Name;
            var config = new SerializerConfig<TOwner, T>(this);
            propertySerializerConfigs[argName ?? throw new ArgumentException()] = config;
            return config;
        }
    }
}