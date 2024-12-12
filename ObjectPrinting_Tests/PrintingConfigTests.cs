using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting_Tests.ExampleClasses;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting_Tests;

[TestFixture]
[SetCulture("")]
public class PrintingConfigTests
{
    private static readonly ClassWithFinalTypeProperties _classWithFinalTypeProperties = new()
    {
        Boolean = false,
        Int32 = 123456,
        Int64 = 123456,
        Single = 123.456f,
        Double = 123.456,
        Guid = new Guid("61c3ee87-2434-4464-ba40-0226fd5fcef9"),
        DateTime = new DateTime(638695533372380454),
        TimeSpan = new TimeSpan(638695533372380454),
        String = "abc",
    };

    private static readonly ClassWithMultipleProperties _classWithMultipleProperties = new()
    {
        Number1 = 123456,
        Number2 = 456789,
        String1 = "abcdef",
        String2 = "ghijkl",
    };

    private static readonly LinkedClass _linkedClass = new()
    {
        Number = 1,
        Other = new LinkedClass
        {
            Number = 2,
            Other = new LinkedClass
            {
                Number = 3,
            },
        },
    };

    private readonly VerifySettings _verifySettings;

    public PrintingConfigTests()
    {
        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("PrintingConfigTestsResults");
    }

    [Test]
    public void PrintToString_CanPrintNull()
    {
        var obj = default(string);
        var expected = $"null{Environment.NewLine}";

        var actual = obj.PrintToString();

        actual.Should().Be(expected);
    }

    [Test]
    public void PrintToString_PrintsSameStringForOneObject_WhenCalledFromConfigsOfDifferentTypes()
    {
        var obj = _classWithFinalTypeProperties;

        var printedByObjectPrintingConfig = ObjectPrinter.For<object>().PrintToString(obj);
        var printedByClassPrintingConfig = ObjectPrinter.For<ClassWithFinalTypeProperties>().PrintToString(obj);

        printedByObjectPrintingConfig.Should().Be(printedByClassPrintingConfig);
    }

    [Test]
    public Task PrintToString_PrintsFinalTypesValues()
    {
        var actual = _classWithFinalTypeProperties.PrintToString();

        return Verify(actual, _verifySettings);
    }

    [Test]
    public Task PrintToString_PrintsNestedObjects()
    {
        var actual = _linkedClass.PrintToString();

        return Verify(actual, _verifySettings);
    }

    [Test]
    public void PrintToString_ThrowsException_IfObjectHasCyclicReference()
    {
        var obj = new LinkedClass();
        obj.Other = obj;

        var print = () => obj.PrintToString();

        print.Should().Throw<InvalidOperationException>()
            .WithMessage("Unable to print object with cyclic reference.");
    }

    [Test]
    public Task PrintToString_ChangesFinalTypeValues_IfUsingCultureInfo()
    {
        var cultureInfo1 = new CultureInfo("ru-RU");
        var cultureInfo2 = new CultureInfo("en-US");
        
        var actual = ObjectPrinter
            .For<ClassWithFinalTypeProperties>()
            .UsingCulture(cultureInfo1)
            .Printing<double>().Using(cultureInfo2)
            .Printing<DateTime>().Using(cultureInfo2)
            .PrintToString(_classWithFinalTypeProperties);

        return Verify(actual, _verifySettings);
    }

    [Test]
    public Task PrintToString_ChangesValue_IfPrintIsOverriden()
    {
        var actual = ObjectPrinter
            .For<ClassWithMultipleProperties>()
            .Printing<int>(o => o.Number1).Using(i => i.ToString()[..3])
            .Printing<string>().Using(str => $"\"{str}\"")
            .PrintToString(_classWithMultipleProperties);

        return Verify(actual, _verifySettings);
    }

    [Test]
    public Task PrintToString_DoesNotPrintValue_IfExcluded()
    {
        var actual = ObjectPrinter
            .For<ClassWithMultipleProperties>()
            .Excluding<int>(o => o.Number2)
            .Excluding<string>()
            .PrintToString(_classWithMultipleProperties);

        return Verify(actual, _verifySettings);
    }

    [Test]
    public void PrintToString_PrintsNothing_IfExcludedOwner()
    {
        var actual = ObjectPrinter
            .For<ClassWithMultipleProperties>()
            .Excluding<ClassWithMultipleProperties>()
            .PrintToString(_classWithMultipleProperties);

        actual.Should().Be("");
    }

    [Test]
    public Task PrintToString_DoesNotPrintDeepPropertiesValues_IfMaxNestingLevelIsSet()
    {
        var actual = ObjectPrinter
            .For<LinkedClass>()
            .UsingMaxNestingLevel(2)
            .PrintToString(_linkedClass);

        return Verify(actual, _verifySettings);
    }

    [Test]
    public Task PrintToString_CanLimitStringLength()
    {
        var actual = ObjectPrinter
            .For<ClassWithMultipleProperties>()
            .Printing<string?>(o => o.String1)!.TrimmedToLength(3)
            .PrintToString(_classWithMultipleProperties);

        return Verify(actual, _verifySettings);
    }

    [Test]
    public Task PrintToString_PrintsArrayElements()
    {
        var obj = new[] { "abc", "def", "ghi" };

        var actual = obj.PrintToString();

        return Verify(actual, _verifySettings);
    }

    [Test]
    public Task PrintToString_PrintsListElements()
    {
        var obj = new List<string> { "abc", "def", "ghi" };

        var actual = obj.PrintToString();

        return Verify(actual, _verifySettings);
    }

    [Test]
    public Task PrintToString_PrintsIDictionaryElements()
    {
        var obj = new Dictionary<string, int> { { "abc", 0 }, { "def", 1 }, { "ghi", 2 } };

        var actual = obj.PrintToString();

        return Verify(actual, _verifySettings);
    }

    [Test]
    public void Printing_ThrowsException_IfExpressionIsNotPropertyPath()
    {
        Expression<Func<ClassWithMultipleProperties, int>> propertySelector = o => _linkedClass.Number;

        var printing = () => ObjectPrinter
            .For<ClassWithMultipleProperties>()
            .Printing(propertySelector);

        printing.Should().Throw<ArgumentException>()
            .WithMessage($"Expression '{propertySelector}' is not a property path.");
    }

    [Test]
    public void Printing_ThrowsException_IfExpressionIsNull()
    {
        var printing = () => ObjectPrinter
            .For<ClassWithMultipleProperties>()
            .Printing(default(Expression<Func<ClassWithMultipleProperties, string>>)!);

        printing.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Excluding_ThrowsException_IfExpressionIsNotPropertyPath()
    {
        Expression<Func<ClassWithMultipleProperties, int>> propertySelector = o => _linkedClass.Number;

        var excluding = () => ObjectPrinter
            .For<ClassWithMultipleProperties>()
            .Excluding(propertySelector);

        excluding.Should().Throw<ArgumentException>()
            .WithMessage($"Expression '{propertySelector}' is not a property path.");
    }

    [Test]
    public void Excluding_ThrowsException_IfExpressionIsNull()
    {
        var excluding = () => ObjectPrinter
            .For<ClassWithMultipleProperties>()
            .Excluding(default(Expression<Func<ClassWithMultipleProperties, string>>)!);

        excluding.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void UsingCulture_ThrowsException_IfCultureInfoIsNull()
    {
        var usingCulture = () => ObjectPrinter
            .For<ClassWithMultipleProperties>()
            .UsingCulture(default!);

        usingCulture.Should().Throw<ArgumentNullException>();
    }

    [TestCase(-1)]
    [TestCase(0)]
    public void UsingMaxNestingLevel_ThrowsException_IfValueIsNegativeOrZero(int maxNestingLevel)
    {
        var usingMaxNestingLevel = () => ObjectPrinter
            .For<ClassWithMultipleProperties>()
            .UsingMaxNestingLevel(maxNestingLevel);

        usingMaxNestingLevel.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Using_ThrowsException_IfFuncIsNull()
    {
        var usingPrint = () => ObjectPrinter
            .For<ClassWithMultipleProperties>()
            .Printing<int>().Using(default(Func<int, string>)!);

        usingPrint.Should().Throw<ArgumentNullException>();
    }
}
