using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();

        private readonly HashSet<string> excludingProperties = new HashSet<string>();

        private readonly Dictionary<Type, ISerializerConfig<TOwner>> typeSerializerConfigs 
            = new Dictionary<Type, ISerializerConfig<TOwner>>();

        private readonly Dictionary<string, ISerializerConfig<TOwner>> propertySerializerConfigs 
            = new Dictionary<string, ISerializerConfig<TOwner>>();

        private string Print(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (excludingTypes.Contains(obj.GetType()))
                return "";
            if (typeSerializerConfigs.ContainsKey(obj.GetType()))
                return typeSerializerConfigs[obj.GetType()].SerializeFunc(obj);
            
            if (ObjectPrinter.FinalTypes.Contains(obj.GetType()))
                return PrintFinalObj(obj);
            
            return obj is IEnumerable enumerable 
            ? PrintEnumerableObj(enumerable, nestingLevel) 
            : PrintNonFinalObj(obj, nestingLevel);
        }

        private string PrintFinalObj(object obj)
        {
            return obj + Environment.NewLine;
        }

        private string PrintEnumerableObj(IEnumerable obj, int nestingLevel)
        {
            if (nestingLevel == ObjectPrinter.MaxDepthSerialize) return "..." + Environment.NewLine;
            
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
            
            foreach (var element in obj)
                sb.Append(indentation + Print(element, nestingLevel + 1));

            return sb.ToString();
        }

        private string PrintNonFinalObj(object obj, int nestingLevel)
        {
            if (nestingLevel == ObjectPrinter.MaxDepthSerialize) return ObjectPrinter.MaxDepthSerializeString;
            
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (excludingTypes.Contains(propertyInfo.PropertyType)
                    || excludingProperties.Contains(propertyInfo.Name))
                {
                    continue;
                } 
                    
                sb.Append(indentation
                          + PrintProperty(propertyInfo, obj, nestingLevel));
            }

            return sb.ToString();
        }

        private string PrintProperty(PropertyInfo info, object obj, int nestingLevel)
        {
            if (propertySerializerConfigs.ContainsKey(info.Name))
                return Print(
                    propertySerializerConfigs[info.Name].SerializeFunc(info.GetValue(obj)), 
                    nestingLevel);
            return info.Name 
                   + " = " 
                   + Print(info.GetValue(obj), nestingLevel + 1);
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