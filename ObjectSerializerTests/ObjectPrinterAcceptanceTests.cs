using System.Globalization;
using ApprovalTests;
using ApprovalTests.Reporters;
using ObjectSerializer;
using ObjectSerializerTests.ClassesToSerialize;

namespace ObjectSerializerTests;

[TestFixture]
public class Tests
{
    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanExclude_FieldsAndPropertiesOfType()
    {
        var person = new Person("Вася", 160, 18);

        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>();

        Approvals.Verify(printer.PrintToString(person));
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanConfigPrint_FieldsAndPropertiesOfType()
    {
        var person = new Person("Вася", 160, 18);

        var printer = ObjectPrinter.For<Person>()
            .For<Guid>().Using(p => $"Guid:{p.ToString()}");

        Approvals.Verify(printer.PrintToString(person));
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanApplyCulture_ForTypesHavingCulture()
    {
        var person = new Person("Вася", 160, 18);

        var printer = ObjectPrinter.For<Person>()
            .For<double>()
            .Using(new CultureInfo("ru-RU"));

        Approvals.Verify(printer.PrintToString(person));
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanConfigPrint_ForFieldOrProperty()
    {
        var person = new Person("Вася", 160, 18);

        var printer = ObjectPrinter.For<Person>()
            .For(p => p.Height)
                .Using(h => $"{h / 100}m {h % 100}cm");

        Approvals.Verify(printer.PrintToString(person));
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanTrim_StringFieldOrProperty()
    {
        var person = new Person(Guid.NewGuid(), "Ахмед Мухаммед Абдулрахман аль-Халиди", 180, 23);

        var printer = ObjectPrinter.For<Person>()
            .For(p => p.Name)
            .TrimToLength(14);

        Approvals.Verify(printer.PrintToString(person));
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanExclude_FieldOrProperty()
    {
        var person = new Person("Вася", 160, 18);

        var printer = ObjectPrinter.For<Person>()
            .Exclude(p => p.Height);

        Approvals.Verify(printer.PrintToString(person));
    }

    [UseReporter(typeof(DiffReporter))]
    [Test]
    public void ObjectPrinter_CanProcess_CyclicalLinks()
    {
        var waifu = new MarriedPerson("Мисато Кацураги", 162, 29);

        var skuf = new MarriedPerson("Вася", 160, 28);

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
        var person = new Person("Вася", 160, 18);

        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>()
            .For<int>()
                .Using(i => i.ToString("X"))
            .For<double>()
                .Using(CultureInfo.InvariantCulture)
            .For(p => p.Name)
                .TrimToLength(10)
            .Exclude(p => p.Age);

        Approvals.Verify(printer.PrintToString(person));
    }
}