using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    public interface ITypeConfig
    {
        public Type GetType()
        {
            return null;
        }
        
        public bool Print { get; }
        public Func<Type, string> SerializeMethod { get; }
    }
    
    public class TypeConfig<T> : ITypeConfig
    {
        public Type GetType()
        {
            return typeof(T);
        }
        
        public bool Print { get; }
        public Func<T, string> SerializeMethod { get; }
    }
    
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Func<PropertyInfo, bool>> excludingMemberExtractors = new HashSet<Func<PropertyInfo, bool>>();
        private readonly Dictionary<string, ITypeConfig> configs;

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }
        
        public PrintingConfig<TOwner> Exclude<T>()
        {
            return Exclude(x => x.PropertyType == typeof(T));
        }
        
        public PrintingConfig<TOwner> Exclude(Expression<Func<TOwner, object>> excludingMemberExtractor)
        {
            // Console.WriteLine(excludingMemberExtractor.ToString().Split('.').Last().ToString());
            return Exclude(x => x.Name == excludingMemberExtractor.ToString().Split('.').Last().Split(',').First());
        }
        
        public PrintingConfig<TOwner> Exclude(Func<PropertyInfo, bool> excludingMemberExtractor)
        {
            excludingMemberExtractors.Add(excludingMemberExtractor);
            return this;
        }
        
        // public PrintingConfig<TOwner> Include(Func<PropertyInfo, bool> includingMemberExtractor)

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
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

            var validProps = Enumerable.Empty<PropertyInfo>();
            foreach (var excludingMemberExtractor in excludingMemberExtractors)
            {
                validProps = type.GetProperties().Where(p => !excludingMemberExtractor(p));
            }

            foreach (var propertyInfo in validProps)
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        public PrintingConfig<TOwner> WithSerialize<T>(Func<T, string> func)
        {
            configs.TryAdd(typeof(T), new TypeConfig<T>() as ITypeConfig);
        }

        public PrintingConfig<TOwner> SetCultureAttributeFor<T>(CultureInfo currentCulture)
        {
            throw new NotImplementedException();
        }
    }
}