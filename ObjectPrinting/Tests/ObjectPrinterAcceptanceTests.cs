using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private PrintingConfig<Person> printer;

        private Person person;
        
        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            var parent = new Person
            {
                FirstName = "Donald", LastName = "Trump", Age = 74, Height = 1.9, Weight = 90,
                Cousins = new List<Person> {new Person {FirstName = "Cousin"}}
            };
            person = new Person
                {FirstName = "Alex", LastName = "Terminator", Age = 19, Height = 1.79, Weight = 75.5, Parent = parent};
        }
        
        [Test]
        public void Demo()
        {
            printer
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.FirstName).TrimmedToLength(3)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Weight);

            var s1 = printer.PrintToString(person);
            
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();
            
            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void Exclude_Guid_FromSerialization()
        {
            var actual = printer.Excluding<Guid>().PrintToString(person);

            actual.Should().NotContain("Id");
        }
        
        [Test]
        public void Exclude_String_FromSerialization()
        {
            var actual = printer.Excluding<string>().PrintToString(person);

            actual.Should().NotContain("Name");
        }

        [Test]
        public void Change_Serialization_ForInteger()
        {
            var actual = printer.Printing<int>().Using(i => i.ToString("X")).PrintToString(person);

            actual.Should().Contain(person.Age.ToString("X"));
        }
        
        [Test]
        public void Change_Serialization_ForGuid()
        {
            var actual = printer.Printing<Guid>().Using(i => i + "!").PrintToString(person);

            actual.Should().Contain(person.Id + "!");
        }
        
        [Test]
        public void Change_Culture_ForDouble()
        {
            var actual = printer.Printing<double>().Using(CultureInfo.InvariantCulture).PrintToString(person);

            actual.Should().Contain(person.Height.ToString(CultureInfo.InvariantCulture));
        }
        
        [Test]
        public void Change_Culture_ForInt()
        {
            var actual = printer.Printing<int>().Using(CultureInfo.InvariantCulture).PrintToString(person);

            actual.Should().Contain(person.Age.ToString(CultureInfo.InvariantCulture));
        }
        
        [Test]
        public void Change_Serialization_ForProperty()
        {
            var actual = printer.Printing(p => p.Height).Using(h => h + "!").PrintToString(person);

            actual.Should().Contain(person.Height + "!");
            actual.Should().NotContain(person.Weight + "!");
        }

        [Test]
        public void Make_Alex_Flex()
        {
            var actual = printer.Printing(p => p.FirstName).Using(n => n.Replace("A", "F")).PrintToString(person);

            actual.Should().Contain(person.FirstName.Replace("A", "F"));
        }

        [Test]
        public void Trim_AllStringProperties_WithGreaterMaxLength()
        {
            var length = Math.Max(person.FirstName.Length, person.LastName.Length) + 1;
            var actual = printer.Printing<string>().TrimmedToLength(length).PrintToString(person);

            actual.Should().Contain(person.FirstName);
            actual.Should().Contain(person.LastName);
        }
        
        [Test]
        public void Trim_AllStringProperties_WithSmallerMaxLength()
        {
            var length = Math.Min(person.FirstName.Length, person.LastName.Length) - 1;
            var actual = printer.Printing<string>().TrimmedToLength(length).PrintToString(person);

            actual.Should().Contain(person.FirstName.Substring(0, length));
            actual.Should().Contain(person.LastName.Substring(0, length));
        }
        
        [Test]
        public void Trim_ChosenProperties_WithSmallerMaxLength()
        {
            var length = person.FirstName.Length - 1;
            var actual = printer.Printing(p => p.FirstName).TrimmedToLength(length).PrintToString(person);

            actual.Should().Contain(person.FirstName.Substring(0, length));
            actual.Should().Contain(person.LastName);
        }

        [Test]
        public void Exclude_ChosenProperty()
        {
            var actual = printer.Excluding(p => p.Weight).PrintToString(person);
            
            actual.Should().NotContain(nameof(person.Weight));
            actual.Should().Contain(nameof(person.Height));
        }

        [Test]
        public void Use_DefaultSerialization_WithExtension()
        {
            var actual = person.PrintToString();
            var expected = printer.PrintToString(person);

            actual.Should().Be(expected);
        }
        
        [Test]
        public void Use_ConfigSerialization_WithExtension()
        {
            var actual = person.PrintToString(config => config.Excluding(p => p.Age));

            actual.Should().NotContain(person.Age.ToString());
        }

        [Test]
        public void Find_CyclicReference_WithParent()
        {
            person.Parent.Parent = person;

            var actual = "";
            Assert.DoesNotThrow(() => actual = printer.PrintToString(person));
            actual.Should().Be("Cyclic reference found");
        }
        
        [Test]
        public void Find_CyclicReference_WithGrandparent()
        {
            var secondParent = new Person{Parent = person};
            var grandParent = new Person{Parent = secondParent};
            person.Parent = grandParent;

            var actual = "";
            Assert.DoesNotThrow(() => actual = printer.PrintToString(person));
            actual.Should().Be("Cyclic reference found");
        }

        [Test]
        public void Serialize_Array_OfIntegers()
        {
            var array = new []{3, 4, 5, 2, 1, 0};
            var actual = ObjectPrinter.For<int[]>().PrintToString(array);

            actual.Should().ContainAll(array.Select(n => n.ToString()));
            Console.WriteLine(actual);
        }
        
        [Test]
        public void Serialize_Array_OfPersons()
        {
            var array = new []{person, person.Parent};
            var actual = ObjectPrinter.For<Person[]>().PrintToString(array);

            actual.Should().ContainAll(array.SelectMany(p => printer.PrintToString(person).Split()));
            Console.WriteLine(actual);
        }

        [Test]
        public void Find_CyclicReference_InArray_OfPersons()
        {
            var secondParent = new Person{Parent = person};
            var grandParent = new Person{Parent = secondParent};
            person.Parent = grandParent;
            var array = new []{person, person.Parent, grandParent, secondParent};

            var actual = "";
            Assert.DoesNotThrow(() => actual = ObjectPrinter.For<Person[]>().PrintToString(array));
            actual.Should().Be("Cyclic reference found");
        }
        
        [Test]
        public void Serialize_List_OfPersons()
        {
            var list = new List<Person> {person, person.Parent};
            var actual = ObjectPrinter.For<List<Person>>().PrintToString(list);

            actual.Should().ContainAll(list.SelectMany(p => printer.PrintToString(person).Split()));
            Console.WriteLine(actual);
        }
        
        [Test]
        public void Serialize_Dictionary_IntString()
        {
            var dictionary = new Dictionary<int, string>{[3] = "abc", [5] = "cde", [person.Age] = person.LastName};
            var actual = ObjectPrinter.For<Dictionary<int, string>>().PrintToString(dictionary);

            actual.Should().ContainAll(dictionary.SelectMany(p => new[]{p.Key.ToString(), p.Value}));
            Console.WriteLine(actual);
        }
    }
}