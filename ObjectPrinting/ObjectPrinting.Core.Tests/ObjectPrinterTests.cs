using FluentAssertions;
using System.Globalization;
using ObjectPrinting.Core.Extension;

namespace ObjectPrinting.Core.Tests
{
    public class Tests
    {
        [Test]
        [MaxTime(1000)]
        public void PrintToString_CycleLinks_ShouldNotThrowException()
        {
            var person = GetTestPerson();

            var parent = new Person
            {
                Name = " лимов ѕетр —ергеевич",
                Id = Guid.NewGuid(),
                Age = 37,
                Child = person
            };

            person.Parent = parent;
            var printer = ObjectPrinter<Person>.Default;
            _ = printer.PrintToString(person);

            Assert.Pass();
        }

        [Test]
        public void Print_PrintDictionary_ShouldContainAll()
        {
            var person = GetTestPerson();
            var res = ObjectPrinter<Person>.Print(person);

            res.Should().ContainAll(person.Dates.Select(x => x.Key.ToString(CultureInfo.CurrentCulture)))
                        .And.ContainAll(person.Dates.Select(x => x.Value.ToString()));
        }

        [Test]
        public void Print_PrintCollection_ShouldContainAll()
        {
            var person = GetTestPerson();
            var res = ObjectPrinter<Person>.Configure(options => options
                    .WithSerializer<Guid>(guid => guid.ToString()))
                .PrintToString(person);

            res.Should().ContainAll(person.OtherPersons.Select(x => x.ToString()));
        }

        [Test]
        public void PrintToString_TrimEnd_ShouldNotThrowException()
        {
            var person = GetTestPerson();
            var printer = ObjectPrinter<Person>.Configure(options => options
                .ForProperty(x => x.Name, x => x.TrimEnd(10))
                .ForProperty(x => x.Biography, x => x.TrimEnd(10)));

            var res = printer.PrintToString(person);
            res.Should().Contain(person.Name[..10]).And.NotContain(person.Name)
                .And.Contain(person.Biography[..10]).And.NotContain(person.Biography);
        }

        [Test]
        public void PrintToString_TrimEndWithParam_ShouldNotThrowException()
        {
            var person = GetTestPerson();
            var printer = ObjectPrinter<Person>.Configure(options => options.TrimEnd(x => x.Name, 10));

            var res  = printer.PrintToString(person);
            res.Should().Contain(person.Name[..10])
                .And.NotContain(person.Name).And.Contain(person.Biography);
        }

        [Test]
        public void PrintToString_TrimEndByType_ShouldNotThrowException()
        {
            var person = GetTestPerson();
            var printer = ObjectPrinter<Person>.Configure(options => options
                .ForProperties<string>(x => x.TrimEnd(5)));

            var res = printer.PrintToString(person);

            res.Should().Contain(person.Biography[..5]).And.NotContain(person.Biography);
        }

        [Test]
        public void PrintToString_TrimEndForString_ShouldNotThrowException()
        {
            var person = GetTestPerson();
            var printer = ObjectPrinter<Person>.Configure(options => options.TrimEnd(10));
            var res  = printer.PrintToString(person);

            res.Should().Contain(person.Biography[..10]).And.NotContain(person.Biography);
        }

        [Test]
        public void PrintToString_UseCulture_ShouldNotThrowException()
        {
            var person = GetTestPerson();
            var printer = ObjectPrinter<Person>
                .Configure(options => options
                    .ForProperties<double>(x => x.WithCulture(CultureInfo.InvariantCulture)));

            var res = printer.PrintToString(person);

            res.Should().Contain(person.Height.ToString(null, CultureInfo.InvariantCulture));
        }

        [Test]
        public void PrintToString_UseCultureOther_ShouldNotThrowException()
        {
            var person = GetTestPerson();
            var printer = ObjectPrinter<Person>.Configure(options 
                => options.WithCulture<double>(CultureInfo.InvariantCulture));

            var res = printer.PrintToString(person);

            res.Should().Contain(person.Height.ToString(null, CultureInfo.InvariantCulture));
        }

        [Test]
        public void PrintToString_CustomSerializerByProp_ShouldNotThrowException()
        {
            var person = GetTestPerson();
            var printer = ObjectPrinter<Person>.Configure(options 
                => options.ForProperties<Guid>(x => x.WithSerializer(y => y.ToString())));

            var res = printer.PrintToString(person);

            res.Should().Contain(person.Id.ToString());
        }
        [Test]

        public void PrintToString_CustomSerializerByType_ShouldNotThrowException()
        {
            var person = GetTestPerson();
            var printer = ObjectPrinter<Person>.Configure(options 
                => options.WithSerializer<Guid>(y => y.ToString()));

            var res = printer.PrintToString(person);

            res.Should().Contain(person.Id.ToString());
        }

        [Test]
        public void PrintToString_ExcludeTypeByType_ShouldNotContain()
        {
            var person = GetTestPerson();
            var printer = ObjectPrinter<Person>.Configure(options => options.Exclude<Guid>());
            var res = printer.PrintToString(person);

            res.Should().NotContain(person.Id.ToString());
        }

        [Test]
        public void PrintToString_ExcludeTypeByProp_ShouldNotContain()
        {
            var person = GetTestPerson();
            var printer = ObjectPrinter<Person>.Configure(options 
                => options.ForProperties<Guid>(x => x.Exclude()));

            var res = printer.PrintToString(person);
            res.Should().NotContain(person.Id.ToString());
        }

        [Test]
        public void PrintToString_ExcludeProp_ShouldNotContain()
        {
            var person = GetTestPerson();
            var printer = ObjectPrinter<Person>.Configure(options 
                => options.ForProperty(x => x.Name, x => x.Exclude()));

            var res = printer.PrintToString(person);

            res.Should().NotContain(person.Name);
        }
        [Test]
        public void PrintToString_ExcludePropFunc_ShouldNotContain()
        {
            var person = GetTestPerson();
            var printer = ObjectPrinter<Person>.Configure(options 
                => options.Exclude(x => x.Name));

            var res = printer.PrintToString(person);

            res.Should().NotContain(person.Name);
        }
        public Person GetTestPerson()
        {
           return new Person
            {
                Name = "»ванов »ван »ванович",
                Age = 23,
                Height = 179.3,
                Id = Guid.NewGuid(),
                OtherPersons = new[]
                {
                    Guid.NewGuid(), 
                    Guid.Empty, 
                    Guid.NewGuid(), 
                    Guid.NewGuid()
                },
                Biography = @"—ъешь ещЄ этих м€гких французских булок, да выпей чаю. 
—ъешь ещЄ этих м€гких французских булок, да выпей чаю. —ъешь ещЄ этих м€гких французских булок, да выпей чаю.",
                Dates = new()
                {
                    [DateTime.Now] = "Today",
                    [DateTime.Now - TimeSpan.FromDays(15)] = "15 days ago"
                }
            };
        }
    }
}