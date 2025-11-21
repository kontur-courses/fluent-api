using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting.Solved;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {

        private Person testPerson;

        [SetUp]
        public void Setup()
        {
            testPerson = new Person
            {
                Name = "Alexander",
                Age = 25,
                Height = 180.5,
                Id = Guid.NewGuid()
            };
        }

        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 160.1 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .ConfigureType<int>().Using(i => i + " лет")
                //3. Для числовых типов указать культуру
                .ConfigureType<double>().Using(new CultureInfo("ru-RU"))
                //4. Настроить сериализацию конкретного свойства
                .ConfigureProperty(p => p.Height).Using(i => i + " см")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .ConfigureProperty(p => p.Name).TrimTo(8)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Name);


            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();

            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Exclude(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);

        }

        [Test]
        public void PrintToString_DefaultConfiguration_ReturnsAllProperties()
        {
            var result = testPerson.PrintToString();

            result.Should().Contain("Name = Alexander")
                .And.Contain("Age = 25")
                .And.Contain("Height = 180.5")
                .And.Contain("Id = ");
        }

        [Test]
        public void Exclude_ByType_ExcludesPropertiesOfSpecifiedType()
        {
            var printer = ObjectPrinter.For<Person>().Exclude<Guid>();
            var result = printer.PrintToString(testPerson);

            result.Should().NotContain("Id")
                .And.Contain("Name = Alexander")
                .And.Contain("Age = 25")
                .And.Contain("Height = 180.5");
        }

        [Test]
        public void Exclude_ByProperty_ExcludesSpecificProperty()
        {
            var printer = ObjectPrinter.For<Person>().Exclude(p => p.Name);
            var result = printer.PrintToString(testPerson);

            result.Should().NotContain("Name = Alexander")
                .And.Contain("Age = 25")
                .And.Contain("Height = 180.5");
        }

        [Test]
        public void ConfigureType_UsingCustomSerialization_AppliesCustomFormat()
        {
            var printer = ObjectPrinter.For<Person>()
                .ConfigureType<int>().Using(i => i + " years");

            var result = printer.PrintToString(testPerson);

            result.Should().Contain("Age = 25 years");
        }


        [Test]
        public void ConfigureProperty_UsingCustomSerialization_AppliesToSpecificProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .ConfigureProperty(p => p.Height).Using(h => h + " cm");

            var result = printer.PrintToString(testPerson);

            result.Should().Contain("Height = 180.5 cm");
        }

        [Test]
        public void ConfigureProperty_TrimTo_TrimsStringProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .ConfigureProperty(p => p.Name).TrimTo(4);

            var result = printer.PrintToString(testPerson);

            result.Should().Contain("Name = Alex");
        }

        [Test]
        public void ConfigureProperty_TrimTo_WithLongString_TrimsCorrectly()
        {
            var person = new Person { Name = "VeryLongName", Age = 20, Height = 170 };
            var printer = ObjectPrinter.For<Person>()
                .ConfigureProperty(p => p.Name).TrimTo(5);

            var result = printer.PrintToString(person);

            result.Should().Contain("Name = VeryL");
        }

        [Test]
        public void PrintToString_WithConfiguration_AppliesConfiguration()
        {
            var result = testPerson.PrintToString(s => s.Exclude(p => p.Age));

            result.Should().NotContain("Age = 25")
                .And.Contain("Name = Alexander")
                .And.Contain("Height = 180.5");
        }

        [Test]
        public void MultipleConfigurations_WorkTogetherCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .ConfigureType<int>().Using(i => i + " лет")
                .ConfigureProperty(p => p.Height).Using(h => h + " см")
                .ConfigureProperty(p => p.Name).TrimTo(3)
                .Exclude(p => p.Age);

            var result = printer.PrintToString(testPerson);

            result.Should().NotContain("Id")
                .And.NotContain("Age")
                .And.Contain("Name = Ale")
                .And.Contain("Height = 180.5 см");
        }

        [Test]
        public void EmptyObject_ReturnsCorrectFormat()
        {
            var emptyPerson = new Person();
            var result = emptyPerson.PrintToString();

            result.Should().Contain("Name = null")
                .And.Contain("Age = 0")
                .And.Contain("Height = 0");
        }

        [Test]
        public void NullObject_ReturnsNullString()
        {
            Person nullPerson = null;
            var result = nullPerson.PrintToString();
            result.Should().Be("null" + Environment.NewLine);
        }

        [Test]
        public void ConfigureType_ForStringType_WorksCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .ConfigureType<string>().Using(s => s?.ToUpper() ?? "NULL");

            var result = printer.PrintToString(testPerson);

            result.Should().Contain("Name = ALEXANDER");
        }

        [Test]
        public void Demo_WithAllFeatures_ProducesExpectedOutput()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 160.1 };

            var printer = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .ConfigureType<int>().Using(i => i + " лет")
                .ConfigureType<double>().Using(new CultureInfo("ru-RU"))
                .ConfigureProperty(p => p.Height).Using(i => i + " см") 
                .ConfigureProperty(p => p.Name).TrimTo(3)
                .Exclude(p => p.Name);

            var result = printer.PrintToString(person);

            result.Should().NotContain("Id")
                .And.NotContain("Name = Ale")     
                .And.Contain("Age = 19 лет")      
                .And.Contain("Height = 160.1 см")  
                .And.NotContain("160,1");          
        }
        
        [Test]
        public void ConfigureProperty_TrimTo_WithNullString_HandlesGracefully()
        {
            var person = new Person { Name = null, Age = 25 };
            var printer = ObjectPrinter.For<Person>()
                .ConfigureProperty(p => p.Name).TrimTo(5);

            var result = printer.PrintToString(person);

            result.Should().Contain("Name = null"); 
        }
        
        [Test]
        public void Exclude_MultipleTypes_ExcludesAllSpecifiedTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .Exclude<string>()
                .Exclude<int>();

            var result = printer.PrintToString(testPerson);

            result.Should().NotContain("Id")
                .And.NotContain("Name")
                .And.NotContain("Age")
                .And.Contain("Height = 180.5");
        }
        
        [Test]
        public void ConfigureType_And_ConfigureProperty_ForSameProperty_PropertyWins()
        {
            var printer = ObjectPrinter.For<Person>()
                .ConfigureType<double>().Using(d => d + " TYPE")
                .ConfigureProperty(p => p.Height).Using(h => h + " PROPERTY");

            var result = printer.PrintToString(testPerson);

            result.Should().Contain("Height = 180.5 PROPERTY") 
                .And.NotContain("TYPE");
        }
        
        [Test]
        public void Exclude_AllProperties_ReturnsOnlyTypeName()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude(p => p.Id)
                .Exclude(p => p.Name)
                .Exclude(p => p.Height)
                .Exclude(p => p.Age);

            var result = printer.PrintToString(testPerson);

            result.Trim().Should().Be("Person"); 
        }

        [Test]
        public void ConfigureType_WithCultureInfo_AppliesCultureFormatting()
        {
            // Arrange
            var person = new Person { Name = "Test", Age = 25, Height = 160.5 };
            var culture = new CultureInfo("ru-RU"); 

            var printer = ObjectPrinter.For<Person>()
                .ConfigureType<double>().Using(culture);

            // Act
            var result = printer.PrintToString(person);

            // Assert
            result.Should().Contain("Height = 160,5"); 
        }

        [Test]
        public void ConfigureType_WithDifferentCultures_FormatsNumbersCorrectly()
        {
            // Arrange
            var person = new Person { Name = "Test", Age = 25, Height = 160.5 };
            var usCulture = new CultureInfo("en-US");
            var frCulture = new CultureInfo("fr-FR");

            var printerUS = ObjectPrinter.For<Person>()
                .ConfigureType<double>().Using(usCulture);
    
            var printerFR = ObjectPrinter.For<Person>()
                .ConfigureType<double>().Using(frCulture);

            // Act
            var resultUS = printerUS.PrintToString(person);
            var resultFR = printerFR.PrintToString(person);

            // Assert
            resultUS.Should().Contain("Height = 160.5"); 
            resultFR.Should().Contain("Height = 160,5"); 
        }
        

    }
    

}
