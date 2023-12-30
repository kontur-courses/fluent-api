using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;


namespace ObjectPrintingTests
{
    public class ObjectPrinting_Should
    {
        private Person person;
        
        [SetUp]
        public void CreateDefaultPerson() =>
            person = new Person { Name = "Alex", Age = 19, Surname = "Vasilyev", Weight = 70.3 };

        [Test]
        public void ExcludeByType()
        {
            const string notExcepted = nameof(Person.Name) + " = ";
            var result = ObjectPrinter.For<Person>()
                .ExcludeProperty<string>()
                .PrintToString(person);

            result.Should().NotContain(notExcepted);
        }

        [Test]
        public void ExcludeProperty()
        {
            var notExcepted = nameof(Person.Id) + " = " + person.Id + Environment.NewLine;
            var result = ObjectPrinter.For<Person>()
                .ExcludeProperty(x => x.Id)
                .PrintToString(person);

            result.Should().NotContain(notExcepted);
        }

        [Test]
        public void ProvideCustomSerialization_ForType()
        {
            var heightExcepted = nameof(Person.Height) + " = " + person.Height.ToString("f0") + Environment.NewLine;
            var weightExcepted = nameof(Person.Weight) + " = " + person.Weight.ToString("f0") + Environment.NewLine;
            var result = ObjectPrinter.For<Person>()
                .ChangeSerializationFor<double>()
                .To(x => x.ToString("f0"))
                .PrintToString(person);

            result.Should().Contain(heightExcepted).And.Contain(weightExcepted);
        }

        [Test]
        public void ProvideCustomSerialization_ForProperty()
        {
            var excepted = nameof(Person.Weight) + " = " + person.Weight.ToString("f0") + Environment.NewLine;
            var result = ObjectPrinter.For<Person>()
                .ChangeSerializationFor(t => t.Weight)
                .To(x => x.ToString("f0"))
                .PrintToString(person);

            result.Should().Contain(excepted);
        }

        [Test]
        public void ProvideCulture_ForNumberTypes()
        {
            var excepted = nameof(Person.Weight) + " = " +
                           person.Weight.ToString(CultureInfo.CurrentCulture) + Environment.NewLine;
            var result = ObjectPrinter.For<Person>()
                .ChangeSerializationFor<double>()
                .To(CultureInfo.CurrentCulture)
                .PrintToString(person);

            result.Should().Contain(excepted);
        }

        [Test]
        public void ProvideTrimForStrings()
        {
            const int length = 5;
            var excepted = nameof(Person.Surname) + " = " + person.Surname[..length] + Environment.NewLine;
            var result = ObjectPrinter.For<Person>()
                .ChangeSerializationFor<string>()
                .ToTrimmedLength(length)
                .PrintToString(person);

            result.Should().Contain(excepted);
        }


        [Test]
        public void Work_WhenReferenceCycles()
        {
            const string excepted = "Cycle, object was already serialized";
            person.Parents = [person];
            person.Friends = [person];
            var result = ObjectPrinter.For<Person>().PrintToString(person);

            result.Should().Contain(excepted);
        }

        [Test]
        public void Print_WhenArray()
        {
            var excepted = "Person" + Environment.NewLine + 
                           "\tParents = Person[] {" + Environment.NewLine +
                           "\t\tPerson" + Environment.NewLine + 
                           "\t\t\tFriends = null" + Environment.NewLine +
                           "\t\t\tParents = null" + Environment.NewLine +
                           "\t\t\tSomeDictionary = null" + Environment.NewLine +
                           "\t\t\tChild = null" + Environment.NewLine +
                           "\t\tPerson" + Environment.NewLine +
                           "\t\t\tFriends = null" + Environment.NewLine +
                           "\t\t\tParents = null" + Environment.NewLine +
                           "\t\t\tSomeDictionary = null" + Environment.NewLine +
                           "\t\t\tChild = null" + Environment.NewLine +
                           "\t}" + Environment.NewLine;
            person.Parents = [new Person(), new Person()];
            var result = ObjectPrinter.For<Person>()
                .ExcludeProperty(t => t.SomeDictionary)
                .ExcludeProperty(t => t.Friends)
                .ExcludeProperty<Guid>()
                .ExcludeProperty<double>()
                .ExcludeProperty<bool>()
                .ExcludeProperty<string>()
                .ExcludeProperty<int>()
                .ExcludeProperty(x => x.Child)
                .PrintToString(person);

            result.Should().Be(excepted);
        }

        [Test]
        public void Print_WhenList()
        {
            string excepted = "Person" + Environment.NewLine +
                                    "\tFriends = List`1 {" + Environment.NewLine +
                                    "\t\tPerson" + Environment.NewLine +
                                    "\t\t\tFriends = null" + Environment.NewLine +
                                    "\t\t\tParents = null" + Environment.NewLine +
                                    "\t\t\tSomeDictionary = null" + Environment.NewLine +
                                    "\t\t\tChild = null" + Environment.NewLine +
                                    "\t\tPerson" + Environment.NewLine +
                                    "\t\t\tFriends = null" + Environment.NewLine +
                                    "\t\t\tParents = null" + Environment.NewLine +
                                    "\t\t\tSomeDictionary = null" + Environment.NewLine +
                                    "\t\t\tChild = null" + Environment.NewLine +
                                    "\t}" + Environment.NewLine;
                person.Friends = [new Person(), new Person()];

            var result = ObjectPrinter.For<Person>()
                .ExcludeProperty(t => t.SomeDictionary)
                .ExcludeProperty(t => t.Parents)
                .ExcludeProperty<Guid>()
                .ExcludeProperty<double>()
                .ExcludeProperty<bool>()
                .ExcludeProperty<string>()
                .ExcludeProperty<int>()
                .ExcludeProperty(x => x.Child)
                .PrintToString(person);
            
            result.Should().Be(excepted);
        }

        [Test]
        public void Print_WhenDictionaries()
        {
            string expected = "Person" + Environment.NewLine +
                                    "\tSomeDictionary = Dictionary`2 {" + Environment.NewLine +
                                    "\t\t1 = aboba" + Environment.NewLine +
                                    "\t\t2 = biba" + Environment.NewLine +
                                    "\t}" + Environment.NewLine;
            person.SomeDictionary = new Dictionary<int, string>
            {
                { 1, "aboba" },
                { 2, "biba" }
            };
            
            var result = ObjectPrinter.For<Person>()
                .ExcludeProperty(t => t.Parents)
                .ExcludeProperty(t => t.Friends)
                .ExcludeProperty<Guid>()
                .ExcludeProperty<double>()
                .ExcludeProperty<bool>()
                .ExcludeProperty<string>()
                .ExcludeProperty<int>()
                .ExcludeProperty(x => x.Child)
                .PrintToString(person);
            
            result.Should().Be(expected);
        }
        
        [Test]
        public void SerializationForChildDoesNotAffectParent()
        {
            person.Child = new Person();
            var result = ObjectPrinter.For<Person>()
                .ChangeSerializationFor(e => e.Child.Age)
                .To(s => "ВОЗРАСТ")
                .PrintToString(person);
            Console.WriteLine(result);
            result.Should().Contain("ВОЗРАСТ", LessThan.Twice());
        }
        
        [Test]
        public void ExcludingForChildDoesNotAffectParent()
        {
            person.Child = new Person();
            var result = ObjectPrinter.For<Person>()
                .ExcludeProperty(x => x.Child.Age)
                .PrintToString(person);
            Console.WriteLine(result);
            result.Should().Contain("Age", Exactly.Once());
        }
    }
}
