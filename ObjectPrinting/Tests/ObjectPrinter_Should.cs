using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person{Name = "Alex", Age = 19, Height = 30};
        }

        [Test]
        public void ObjectPrinter_Should_RemoveExcludingTypes()
        {
            var settings = new Func<PrintingConfig<Person>, PrintingConfig<Person>>(config => config.Excluding<int>());
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
            var settings = new Func<PrintingConfig<Person>, PrintingConfig<Person>>(config => config.Excluding(p => p.Age));
            var result = person.PrintToString(settings);
            result.Should().NotContain(nameof(Person.Age));
        }

        [Test]
        public void ObjectPrinter_Should_SerializeTypesBySpecificRule()
        {
            var settings = new Func<PrintingConfig<Person>, PrintingConfig<Person>>(config => config.ChangePrintFor<int>().Using(number => (number + 10).ToString()));
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
            var settings = new Func<PrintingConfig<Person>, PrintingConfig<Person>>(config => config.ChangePrintFor(p => p.Name).Using(property => property.ToUpper()));
            var result = person.PrintToString(settings);
            result.Should().Contain(nameof(Person.Name) + " = " + person.Name.ToUpper());
            result.Should().NotContain(nameof(Person.Name) + " = " + person.Name);
        }

        [Test]
        public void ObjectPrinter_Should_ThrowExceptionWhenFuncNotMemberExpression()
        {
            var settings = new Func<PrintingConfig<Person>, PrintingConfig<Person>>(config => config.ChangePrintFor(p => "").Using(property => property.ToUpper()));
            Action act = () => person.PrintToString(settings);
            act.Should().Throw<ArgumentException>();
        }
    }
}
