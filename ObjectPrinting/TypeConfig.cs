using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using NUnit.Framework;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    public class Sample
    {
        public void Main()
        {
            var config = new Config<Person>()
                .Ignore<Guid>().InAllNestingLevels()
                .ChangeSerialisationFor<DateTime>().WithMethod(dt => $"{dt.Day}.{dt.Month} - {dt.Year}")
                .WithCultureAttributeForNumericTypes(CultureInfo.CurrentCulture)
                .Ignore(p => p.Age).InAllNestingLevels()
                .ChangeSerialisationFor(p => p.Name).WithMethod(n => n.Trim().ToUpper()).WithCharsLimit(10);
        }
    }

    public record SerialisationRule<T>(T Target, Func<object, string> Method, int CharsLimit = -1);

    public class OneTimeSetValue<TValue>
    {
        private TValue? value;
        public TValue Value 
        {
            get => value ?? throw new ArgumentNullException();
            set => this.value = value;
        }
        public bool Setted => value is not null;
    }
    
    public class PrintingRules
    {
        public HashSet<(Type, bool)> IgnoredTypes { get; } = new();
        public HashSet<(string, bool)> IgnoreProperties { get; } = new();
        public Dictionary<Type, SerialisationRule<Type>> SerialisationMethodByType { get; } = new();
        public Dictionary<string, SerialisationRule<string>> SerialisationMethodByProperty { get; } = new();
        public OneTimeSetValue<CultureAttribute> CultureAttribute { get; } = new();

    }
    
    public class StringSelialisationConfig<TOwner> : Config<TOwner>
    {
        public Config<TOwner> WithCharsLimit(int maxStringLenght)
        {
            var a = typeof(int)
            throw new NotImplementedException();
        }
    }
    
    public class SerialisationConfig<TOwner, TType>
    {
        public Config<TOwner> WithMethod(Func<TType, string> serialisationMethod)
        {
            throw new NotImplementedException();
        }
        
        public StringSelialisationConfig<TOwner> WithMethod(Func<string, string> serialisationMethod)
        {
            throw new NotImplementedException();
        }
    }

    public class Config<TOwner>
    {
        public SerialisationConfig<TOwner, T> ChangeSerialisationFor<T>(Expression<Func<TOwner, T>> extractor)
        {
            throw new NotImplementedException();
        }
        
        public TypeConfig<TOwner> Ignore<T>()
        {
            throw new NotImplementedException();
        }

        public TypeConfig<TOwner> Ignore(Expression<Func<TOwner, object>> extractor)
        {
            throw new NotImplementedException();
        }

        public SerialisationConfig<TOwner, T> ChangeSerialisationFor<T>()
        {
            throw new NotImplementedException();
        }
        
        public Config<TOwner> WithCultureAttributeForNumericTypes(CultureInfo currentCulture)
        {
            throw new NotImplementedException();
        }
    }

    public class TypeConfig<TOwner> : Config<TOwner>
    {
        public Config<TOwner> InAllNestingLevels()
        {
            throw new NotImplementedException();
        }
    }
}