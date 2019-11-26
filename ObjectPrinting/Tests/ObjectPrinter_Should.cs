using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private PersonWithChild person;

        [SetUp]
        public void SetUp()
        {
            person = new PersonWithChild{Name = "Alex", Age = 19, Height = 30};
        }

        [Test]
        public void ObjectPrinter_Should_RemoveExcludingTypes()
        {
            var settings = new Func<PrintingConfig<PersonWithChild>, PrintingConfig<PersonWithChild>>(config => config.Excluding<int>());
            var result = person.PrintToString(settings);
            foreach (var propertyInfo in person.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType == typeof(int))
                {
                    result.Should().NotContain(propertyInfo.Name);
                }
            }
        }

        [Test]
        public void ObjectPrinter_Should_RemoveExcludingFields()
        {
            var settings = new Func<PrintingConfig<PersonWithChild>, PrintingConfig<PersonWithChild>>(config => config.Excluding(p => p.Age));
            var result = person.PrintToString(settings);
            result.Should().NotContain(nameof(PersonWithChild.Age));
        }

        [Test]
        public void ObjectPrinter_Should_SerializeTypesBySpecificRule()
        {
            var settings = new Func<PrintingConfig<PersonWithChild>, PrintingConfig<PersonWithChild>>(config => config.ChangePrintFor<int>().Using(number => (number + 10).ToString()));
            var result = person.PrintToString(settings);
            foreach (var propertyInfo in person.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType != typeof(int)) continue;
                result.Should().Contain(propertyInfo.Name + " = " + ((int)propertyInfo.GetValue(person) + 10));
                result.Should().NotContain(propertyInfo.Name + " = " + propertyInfo.GetValue(person));
            }
        }

        [Test]
        public void ObjectPrinter_Should_SerializeSpecificPropertyBySpecificRule()
        {
            var settings = new Func<PrintingConfig<PersonWithChild>, PrintingConfig<PersonWithChild>>(config => config.ChangePrintFor(p => p.Name).Using(property => property.ToUpper()));
            var result = person.PrintToString(settings);
            result.Should().Contain(nameof(PersonWithChild.Name) + " = " + person.Name.ToUpper());
            result.Should().NotContain(nameof(PersonWithChild.Name) + " = " + person.Name);
        }

        [Test]
        public void ObjectPrinter_Should_ThrowExceptionWhenFuncNotMemberExpression()
        {
            var settings = new Func<PrintingConfig<PersonWithChild>, PrintingConfig<PersonWithChild>>(config => config.ChangePrintFor(p => "").Using(property => property.ToUpper()));
            Action act = () => person.PrintToString(settings);
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void ObjectPrinter_Should_IgnoreDuplicateObject()
        {
            person.Child = person;
            var result = person.PrintToString();
            result.Should().NotContain(nameof(PersonWithChild.Child) + " = ");
        }

        [Test]
        public void ObjectPrinter_Should_ContainCollectionInfo()
        {
            var collection = new List<int> { 1, 2, 3, 4, 5 };
            for (var i = 0; i < 5; i++)
            {
                collection.Add(i);
            }
            var result = collection.PrintToString();
            for (var i = 0; i < 5; i++)
            {
                result.Should().Contain(i.ToString());
            }
        }

        [Test]
        public void ObjectPrinter_Should_ContainDictionaryInfo()
        {
            var collection = new Dictionary<int, PersonWithChild>();
            for (var i = 0; i < 5; i++)
            {
                collection[i] = new PersonWithChild{Name = "Test" + i, Age = i};
            }
            var result = collection.PrintToString();
            for (var i = 0; i < 5; i++)
            {
                result.Should().Contain(nameof(PersonWithChild.Name) + " = " + "Test" + i);
                result.Should().Contain(nameof(PersonWithChild.Age) + " = " + i);
            }
        }

        private class PersonWithChild
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public double Height { get; set; }
            public int Age { get; set; }
            public PersonWithChild Child { get; set; }
        }
    }
}
