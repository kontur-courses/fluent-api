using NUnit.Framework;
using System;
using System.Globalization;
using System.Reflection;
using ObjectPrintingTests.TestClasses;
using ObjectPrinting;
using FluentAssertions;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterShould
    {
        private Person person;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            person = new Person { Id = Guid.Empty, Name = "Peter", Height = 204.4, Age = 52 };
        }

        [Test]
        public void FormatNull_Correctly()
        {
            Person nullPerson = null;
            var expectedFormat = "null" + Environment.NewLine;

            var resultFormat = nullPerson.Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void FormatWithoutConfiguration_Correctly()
        {
            var expectedFormat = 
                $"Person"                       + Environment.NewLine +
                $"\tId = Guid"                  + Environment.NewLine +
                $"\tName = {person.Name}"       + Environment.NewLine +
                $"\tHeight = {person.Height}"   + Environment.NewLine +
                $"\tAge = {person.Age}"         + Environment.NewLine;

            var resultFormat = person.Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void ExcludeType_Correctly()
        {
            var expectedFormat = 
                $"Person"                       + Environment.NewLine +
                $"\tName = {person.Name}"       + Environment.NewLine +
                $"\tHeight = {person.Height}"   + Environment.NewLine +
                $"\tAge = {person.Age}"         + Environment.NewLine;

            var resultFormat = person.ConfigureFormat()
                .Excluding<Guid>()
                .Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void ExcludeProperty_Correctly()
        {
            var expectedFormat = 
                $"Person"                       + Environment.NewLine +
                $"\tName = {person.Name}"       + Environment.NewLine +
                $"\tHeight = {person.Height}"   + Environment.NewLine +
                $"\tAge = {person.Age}"         + Environment.NewLine;

            var resultFormat = person.ConfigureFormat()
                .Excluding(p => p.Id)
                .Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void SetNumberFormat_Correctly()
        {
            var expectedFormat = 
                $"Person"                       + Environment.NewLine +
                $"\tId = Guid"                  + Environment.NewLine +
                $"\tName = {person.Name}"       + Environment.NewLine +
                $"\tHeight = {person.Height}"   + Environment.NewLine +
                $"\tAge = {String.Format(CultureInfo.GetCultureInfo("eu-ES"), "{0}", person.Age)}"         
                    + Environment.NewLine;

            var resultFormat = person.ConfigureFormat()
                .ForProperty<double>()
                    .SetFormat(CultureInfo.GetCultureInfo("eu-ES"))
                .Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void SetTypeFormat_Correctly()
        {
            Func<string, string> changeStringFormat = (s) => $"{s} the Great";
            var expectedFormat =
                $"Person"                                       + Environment.NewLine +
                $"\tId = Guid"                                  + Environment.NewLine +
                $"\tName = {changeStringFormat(person.Name)}"   + Environment.NewLine +
                $"\tHeight = {person.Height}"                   + Environment.NewLine +
                $"\tAge = {person.Age}"                         + Environment.NewLine;

            var resultFormat = person.ConfigureFormat()
                .ForProperty<string>()
                    .SetFormat(changeStringFormat)
                .Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void SetPropertyFormat_Correctly()
        {
            Func<string, string> changeNameFormat = (s) => $"{s} the Great";
            var expectedFormat = 
                $"Person"                                   + Environment.NewLine +
                $"\tId = Guid"                              + Environment.NewLine +
                $"\tName = {changeNameFormat(person.Name)}" + Environment.NewLine +
                $"\tHeight = {person.Height}"               + Environment.NewLine +
                $"\tAge = {person.Age}"                     + Environment.NewLine;

            var resultFormat = person.ConfigureFormat()
                .ForProperty(p => p.Name)
                    .SetFormat(changeNameFormat)
                .Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void CutStringProperties_Correctly()
        {
            var expectedFormat =
                $"Person"                                   + Environment.NewLine +
                $"\tId = Guid"                              + Environment.NewLine +
                $"\tName = {person.Name.Substring(0, 2)}"   + Environment.NewLine +
                $"\tHeight = {person.Height}"               + Environment.NewLine +
                $"\tAge = {person.Age}"                     + Environment.NewLine;

            var resultFormat = person.ConfigureFormat()
                .ForProperty(p => p.Name)
                    .Cut(2)
                .Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void NotAllowChooseProperty_MoreThanOnce()
        {
            var formatConfig = person.ConfigureFormat().ForProperty<int>();

            formatConfig.GetType().GetMethod("ForProperty").Should().BeNull();
        }

        [Test]
        public void NotAllowChooseCut_ForNonStringProperties()
        {
            var formatConfig = person.ConfigureFormat().ForProperty(p => p.Age);

            formatConfig.GetType().GetMethod("Cut").Should().BeNull();
        }
    }
}