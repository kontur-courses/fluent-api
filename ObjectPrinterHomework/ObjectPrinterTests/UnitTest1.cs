using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinter.ObjectPrinter;

namespace ObjectPrinterTests
{
    public class Tests
    {
        private PrintingConfig<Person>? _printerConfig;
        private Person? _mom;
        private Person? _dad;
        private Person? _examplePerson;
            
        public static Guid GenerateSeededGuid(int seed)
        {
            var r = new Random(seed);
            var guid = new byte[16];
            r.NextBytes(guid);

            return new Guid(guid);
        }
        
        [SetUp]
        public void Setup()
        {
            _printerConfig = Printer.For<Person>();
            _mom = new Person(null, null, GenerateSeededGuid(1), "Anna", "Karenina", 175.0, 42);
            _dad = new Person(null, null, GenerateSeededGuid(2), "Bob", "Rockwood", 183.47, 53);
            _examplePerson = new Person(_mom, _dad, GenerateSeededGuid(3), "Bill", "Gates", 160.5, 15);

        }
        
        [TestCase("ru", ",")]
        [TestCase("en", ".")]
        public void PrintToString_ShouldGiveCorrectFloatingPointDelimiter_WhenSetCulture(string culture, string delimiter)
        {
            var printerConfig = Printer.For<double>();
            
            printerConfig = printerConfig.Printing<double>().Using(new CultureInfo(culture));
            var result = printerConfig.PrintToString(1.1d);
            
            result.Should().Contain(delimiter);
        }
        
        [Test]
        public void PrintToString_CanParseTypeWithCustomSerializer()
        {
            _printerConfig = _printerConfig.Printing<string>().Using(s => string.Join("", s.Select(_ => '*')));
            var result = _printerConfig.PrintToString(_examplePerson);
            
            result.Should().ContainAll(
                "\tId = a905569d-db07-3ae3-63a0-322750a4a3bd",
                "\tHeight = 160,5",
                "\tAge = 15",
                "\tmother = Person",
                "\t\tId = 8286d046-9740-a3e4-95cf-ff46699c73c4",
                "\t\tmother = null",
                "\tfather = Person",
                "\tName = ****",
                "\tSurname = *****",
                "\tSurname = ********");
        }
        
        [Test]
        public void PrintToString_CanParseMemberWithCustomSerializer()
        {
            _printerConfig = _printerConfig.Printing(p => p.Surname).Using(s => string.Join("", s.Select(_ => '*')));
            var result = _printerConfig.PrintToString(_examplePerson);
            
            result.Should().ContainAll(
                "\tId = a905569d-db07-3ae3-63a0-322750a4a3bd",
                "\tHeight = 160,5",
                "\tAge = 15",
                "\tmother = Person",
                "\t\tId = 8286d046-9740-a3e4-95cf-ff46699c73c4",
                "\t\tmother = null",
                "\tfather = Person",
                "\tName = Bill",
                "\tSurname = *****",
                "\tSurname = ********");
        }
        
        [Test]
        public void PrintToString_ShouldExcludeMember_WhenExcluding()
        {
            _printerConfig = _printerConfig.Excluding(p => p.Surname);
            var result = _printerConfig.PrintToString(_examplePerson);

            result.Should().NotContainAll("Surname = ");
            result.Should().ContainAll(
                "\tId = a905569d-db07-3ae3-63a0-322750a4a3bd",
                "\tHeight = 160,5",
                "\tAge = 15",
                "\tmother = Person",
                "\t\tId = 8286d046-9740-a3e4-95cf-ff46699c73c4",
                "\t\tmother = null",
                "\tfather = Person",
                "\tName = Bill");
        }

        [Test]
        public void PrintToString_ShouldExcludeType_WhenExcluding()
        {
            _printerConfig = _printerConfig.Excluding<string>();
            var result = _printerConfig.PrintToString(_examplePerson);

            result.Should().NotContainAll("Name = ", "Surname = ");
            result.Should().ContainAll(
                "\tId = a905569d-db07-3ae3-63a0-322750a4a3bd",
                "\tHeight = 160,5",
                "\tAge = 15",
                "\tmother = Person",
                "\t\tId = 8286d046-9740-a3e4-95cf-ff46699c73c4",
                "\t\tmother = null",
                "\tfather = Person");
        }

        [Test]
        public void PrintToString_ShouldParseFieldsAndProperties()
        {
            _printerConfig.PrintToString(_examplePerson).Should().ContainAll(
                "\tId = a905569d-db07-3ae3-63a0-322750a4a3bd",
                "\tHeight = 160,5",
                "\tAge = 15",
                "\tmother = Person",
                "\t\tId = 8286d046-9740-a3e4-95cf-ff46699c73c4",
                "\t\tmother = null",
                "\tfather = Person",
                "\tName = Bill",
                "\tSurname = Gates");
        }
        
        [Test]
        public void PrintToString_ShouldTrimParsedString()
        {
            _printerConfig = _printerConfig.Printing(p => p.Id).UsingTrim(6);
            var result = _printerConfig.PrintToString(_examplePerson);

            result.Should().NotContainAll("a905569d-db07-3ae3-63a0-322750a4a3bd", "8286d046-9740-a3e4-95cf-ff46699c73c4");
            result.Should().ContainAll(
                "\tId = a90556",
                "\tHeight = 160,5",
                "\tAge = 15",
                "\tmother = Person",
                "\t\tId = 8286d0",
                "\t\tmother = null",
                "\tfather = Person",
                "\tName = Bill",
                "\tSurname = Gates");
        }
    }
}