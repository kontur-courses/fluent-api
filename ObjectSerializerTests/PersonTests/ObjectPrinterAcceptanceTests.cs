using System.Globalization;
using ApprovalTests;
using ApprovalTests.Reporters;
using ObjectSerializer;
using ObjectSerializerTests.ClassesToSerialize;
using ObjectSerializerTests.ClassesToSerialize.Persons;

namespace ObjectSerializerTests.PersonTests;

[TestFixture]
public class Tests
{
    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanExclude_FieldsAndPropertiesOfType()
    {
        var guid = new Guid("07406af6-61a9-434f-aefe-f99a10cdadfd");
        var person = new Person(guid, "Вася", 160, 18);

        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>();

        Approvals.Verify(printer.PrintToString(person));
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanConfigPrint_FieldsAndPropertiesOfType()
    {
        var guid = new Guid("07406af6-61a9-434f-aefe-f99a10cdadfd");
        var person = new Person(guid, "Вася", 160, 18);

        var printer = ObjectPrinter.For<Person>()
            .Print<Guid>().Using(p => $"Guid:{p.ToString()}");

        Approvals.Verify(printer.PrintToString(person));
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanApplyCulture_ForTypesHavingCulture()
    {
        var guid = new Guid("07406af6-61a9-434f-aefe-f99a10cdadfd");
        var person = new Person(guid, "Вася", 160.28, 18);

        var printer = ObjectPrinter.For<Person>()
            .Print<double>()
            .Using(new CultureInfo("ru-RU"));

        Approvals.Verify(printer.PrintToString(person));
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanConfigPrint_ForFieldOrProperty()
    {
        var guid = new Guid("07406af6-61a9-434f-aefe-f99a10cdadfd");
        var person = new Person(guid, "Вася", 160, 18);

        var printer = ObjectPrinter.For<Person>()
            .Print(p => p.Height)
                .Using(h => $"{h / 100} m");

        Approvals.Verify(printer.PrintToString(person));
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanTrim_StringFieldOrProperty()
    {
        var guid = new Guid("07406af6-61a9-434f-aefe-f99a10cdadfd");
        var person = new Person(guid, "Ахмед Мухаммед Абдулрахман аль-Халиди", 180, 23);

        var printer = ObjectPrinter.For<Person>()
            .Print(p => p.Name)
            .TrimToLength(14);

        Approvals.Verify(printer.PrintToString(person));
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanExclude_FieldOrProperty()
    {
        var guid = new Guid("07406af6-61a9-434f-aefe-f99a10cdadfd");
        var person = new Person(guid, "Вася", 160, 18);

        var printer = ObjectPrinter.For<Person>()
            .Exclude(p => p.Height);

        Approvals.Verify(printer.PrintToString(person));
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanProcess_CyclicalLinks()
    {
        var guid1 = new Guid("08406af6-61a9-434f-aefe-f99a10cdadfd");
        var guid2 = new Guid("07406af6-61a9-434f-aefe-f99a10cdadfd");

        var waifu = new MarriedPerson(guid1, "Мисато Кацураги", 162, 29);
        var skuf = new MarriedPerson(guid2, "Вася", 160, 18);

        waifu.SetPartner(skuf);
        skuf.SetPartner(waifu);

        var printer = ObjectPrinter.For<Person>()
            .Exclude(p => p.Height);

        try
        {
            Approvals.Verify(printer.PrintToString(skuf));
        }
        //Может быть и поймаю...
        catch (StackOverflowException ex)
        {
            Assert.Fail("Ошибка StackOverflowException возникла при обработке циклических ссылок.");
        }
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanApply_AllConfigurationMethods()
    {
        var guid = new Guid("07406af6-61a9-434f-aefe-f99a10cdadfd");
        var person = new Person(guid, "Вася", 160, 18);

        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>()
            .Print<int>()
                .Using(i => i.ToString("X"))
            .Print<double>()
                .Using(CultureInfo.InvariantCulture)
            .Print(p => p.Name)
                .Trim(1, 3)
            .Exclude(p => p.Age);

        Approvals.Verify(printer.PrintToString(person));
    }
}