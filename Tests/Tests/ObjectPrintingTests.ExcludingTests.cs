using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting.PrintingConfigs;
using ObjectPrinting.PrintingConfigs.Extensions;
using Tests.TestEntities;

namespace Tests.Tests;

public partial class ObjectPrintingTests
{
    [TestFixture]
    public class ExcludingTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alex", Age = 19, Height = 175.123 };
        }


        [Test]
        public void PrintToString_ExcludedType_ShouldNotSerializeMembersOfThatType()
        {
            var printer = new PrintingConfig<Person>()
                .Excluding<double>()
                .Create();

            var result = printer.PrintToString(person);
            
            var expected = $"""
                            Person
                            {'\t'}Id = 00000000-0000-0000-0000-000000000000
                            {'\t'}Name = Alex
                            {'\t'}Age = 19

                            """; 
            
            result.Should().Be(expected);
        }
        

        [Test]
        public void PrintToString_ExcludedProperty_ShouldNotSerializeProperty()
        {
            var printer = new PrintingConfig<Person>()
                .Excluding(p => p.Age)
                .Create();

            var result = printer.PrintToString(person);
            
            var expected = $"""
                            Person
                            {'\t'}Id = 00000000-0000-0000-0000-000000000000
                            {'\t'}Name = Alex
                            {'\t'}Height = 175.123

                            """; 
            
            result.Should().Be(expected);
        }

        [Test]
        public void PrintToString_ExcludedField_ShouldNotSerializeField()
        {
            var obj = new TestClass { Property = "Prop", field = "Field" };

            var result = obj.Print(c => c.Excluding(o => o.field));

            var expected = $"""
                            TestClass
                            {'\t'}Property = Prop
                            
                            """; 
            
            result.Should().Be(expected);
        }

        [Test]
        public void PrintToString_ExcludedMultipleMembers_ShouldNotSerializeMultipleMembers()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Id)
                .Excluding(p => p.Age)
                .Create();
            
            var result = printer.PrintToString(person);
            var expected = $"""
                            Person
                            {'\t'}Name = Alex
                            {'\t'}Height = 175.123

                            """; 
            
            result.Should().Be(expected);
        }

        [Test]
        public void Excluding_WhenNotMemberProvided_ShouldThrowArgumentException()
        {
            var printer = ObjectPrinter.For<Person>();

            var act = () => printer.Excluding(p => p.GetHashCode());

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void PrintToString_AllPropertiesExcluded_ShouldShowTypeName()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .Excluding<int>()
                .Excluding(p => p.Id)
                .Excluding(p => p.Height)
                .Create();

            var result = printer.PrintToString(person);

            result.Should().Be("Person" + Environment.NewLine);
        }

        [Test]
        public void PrintToString_Excluding_ShouldDominateOnPrinting()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Id)
                .Printing(p => p.Id).Using(p => "id")
                .Create();
            var result = printer.PrintToString(person);
            
            var expected = $"""
                            Person
                            {'\t'}Name = Alex
                            {'\t'}Height = 175.123
                            {'\t'}Age = 19

                            """; 
            
            result.Should().Be(expected);
            
        }
    }
}   