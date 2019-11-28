using NUnit.Framework;
using System;
using System.Globalization;
using System.Collections.Generic;
using ObjectPrintingTests.TestClasses;
using ObjectPrinting;
using FluentAssertions;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterShould
    {
        private Person person;
        private Sphere sphere;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            person = new Person { Id = Guid.Empty, Name = "Peter", Height = 204.4, Age = 52 };
            sphere = new Sphere { Radius = 3, Material = new Material { Name = "Diffuse" } };
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
        public void FormatWithChangedFormattedPropertyPrinting_Correctly()
        {
            var expectedFormat =
                $"Person" + Environment.NewLine +
                $"\tId : Guid" + Environment.NewLine +
                $"\tName : {person.Name}" + Environment.NewLine +
                $"\tHeight : {person.Height}" + Environment.NewLine +
                $"\tAge : {person.Age}" + Environment.NewLine;

            var resultFormat = person.ConfigureFormat()
                .ChangeFormattedPropertyPrinting(
                    (args) => 
                    $"{ new string('\t', args.nestingLevel + 1) }{args.propertyName} : " +
                    $"{args.formattedProperty}")
                .Print();

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

        [Test]
        public void PrintNestedProperty_Correctly()
        {
            var expectedFormat =
                $"Sphere"                               + Environment.NewLine +
                $"\tRadius = {sphere.Radius}"           + Environment.NewLine +
                $"\tMaterial = Material"                + Environment.NewLine +
                $"\t\tName = {sphere.Material.Name}"    + Environment.NewLine;

            var resultFormat = sphere.Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void SetFormatForNestedProperty_Correctly()
        {
            Func<string, string> materialFormatter = (n) => $"Semi-{n}";
            var expectedFormat =
                $"Sphere"                                               + Environment.NewLine +
                $"\tRadius = {sphere.Radius}"                           + Environment.NewLine +
                $"\tMaterial = Material"                                + Environment.NewLine +
                $"\t\tName = {materialFormatter(sphere.Material.Name)}" + Environment.NewLine;

            var resultFormat = sphere.ConfigureFormat()
                .ForProperty(s => s.Material.Name)
                    .SetFormat(n => materialFormatter(n))
                .Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void UseLastDefinedFormat_ForNestedProperty()
        {
            Func<string, string> firstMaterialFormatter = (n) => $"Semi-{n}";
            Func<string, string> secondMaterialFormatter = (n) => $"Non-{n}";
            var expectedFormat =
                $"Sphere"                                                       + Environment.NewLine +
                $"\tRadius = {sphere.Radius}"                                   + Environment.NewLine +
                $"\tMaterial = Material"                                        + Environment.NewLine +
                $"\t\tName = {secondMaterialFormatter(sphere.Material.Name)}"   + Environment.NewLine;

            var resultFormat = sphere.ConfigureFormat()
                .ForProperty(s => s.Material.Name)
                    .SetFormat(n => firstMaterialFormatter(n))
                .ForProperty(s => s.Material.Name)
                    .SetFormat(n => secondMaterialFormatter(n))
                .Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void UseHighestPropertyFormatter_IfDefined()
        {
            Func<string, string> firstMaterialFormatter = (n) => $"Semi-{n}";
            Func<string, string> secondMaterialFormatter = (n) => $"Non-{n}";
            Func<Material, string> materialFormatter = material =>
                        material.ConfigureFormat()
                        .ForProperty(m => m.Name)
                            .SetFormat(n => firstMaterialFormatter(n))
                        .Print();
            var expectedFormat =
                $"Sphere"                                               + Environment.NewLine +
                $"\tRadius = {sphere.Radius}"                           + Environment.NewLine +
                $"\tMaterial = {materialFormatter(sphere.Material)}"    + Environment.NewLine;

            var resultFormat = sphere.ConfigureFormat()
                .ForProperty(s => s.Material)
                    .SetFormat(materialFormatter)
                .ForProperty(s => s.Material.Name)
                    .SetFormat(n => secondMaterialFormatter(n))
                .Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void FormatArray_Properly()
        {
            var numbers = new int[] { 1, 2, 3, 4 };

            var expectedFormat =
                $"{numbers[0]}" + Environment.NewLine +
                $"{numbers[1]}" + Environment.NewLine +
                $"{numbers[2]}" + Environment.NewLine +
                $"{numbers[3]}" + Environment.NewLine;

            var resultFormat = numbers.Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void FormatArrayProperty_Properly()
        {
            var numbers = new Numbers { Values = new int[] { 1, 2, 3, 4 } };
            var expectedFormat = 
                $"Numbers"                          + Environment.NewLine +
                $"\tValues = {numbers.Values[0]}"   + Environment.NewLine +
                $"{numbers.Values[1]}"              + Environment.NewLine + 
                $"{numbers.Values[2]}"              + Environment.NewLine +
                $"{numbers.Values[3]}"              + Environment.NewLine;

            var resultFormat = numbers.Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void SetElementFormat_ForPropertyArray()
        {
            var numbers = new Numbers { Values = new int[] { 1, 2, 3, 4 } };
            var expectedFormat =
                $"Numbers"                          + Environment.NewLine +
                $"\tValues = {-numbers.Values[0]} " +
                $"{-numbers.Values[1]} "            +
                $"{-numbers.Values[2]} "            +
                $"{-numbers.Values[3]} "            + Environment.NewLine;

            var resultFormat = numbers.ConfigureFormat()
                .ForProperty(n => n.Values)
                    .SetElementFormat(val => $"{-1*val} ")
                .Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void FormatDictionary_Properly()
        {
            var entries = new Dictionary<string, int>();
            entries.Add("Andrew", 19);
            entries.Add("Andrew Jr", 13);
            var expectedFormat = 
                "KeyValuePair`2"    + Environment.NewLine +
                "\tKey = Andrew"    + Environment.NewLine +
                "\tValue = 19"      + Environment.NewLine +
                "KeyValuePair`2"    + Environment.NewLine +
                "\tKey = Andrew Jr" + Environment.NewLine +
                "\tValue = 13"      + Environment.NewLine;

            var resultFormat = entries.Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }

        [Test]
        public void FormatDictionaryProperty_Properly()
        {
            var roster = new Roster { Entries = new Dictionary<string, int>() };
            roster.Entries.Add("Andrew", 19);
            roster.Entries.Add("Andrew Jr", 13);
            var expectedFormat = 
                $"Roster\r\n" + 
                $"\tEntries = KeyValuePair`2"   + Environment.NewLine +
                $"\t\tKey = Andrew"             + Environment.NewLine +
                $"\t\tValue = 19"               + Environment.NewLine +
                "KeyValuePair`2"                + Environment.NewLine +
                "\t\tKey = Andrew Jr"           + Environment.NewLine +
                "\t\tValue = 13"                + Environment.NewLine;

            var resultFormat = roster.Print();

            resultFormat.Should().BeEquivalentTo(expectedFormat);
        }
    }
}