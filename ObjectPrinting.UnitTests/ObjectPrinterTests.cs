using System.Globalization;
using FluentAssertions;

namespace ObjectPrinting.UnitTests;

[TestFixture]
public class ObjectPrinterTests
{
    [Test(Description = "minimal requirements p 1.1")]
    public void For_Exclude_FieldOfType()
    {
        var input = new For_Exclude_FieldOfType_Type();
        var printingConfig = ObjectPrinter.For<For_Exclude_FieldOfType_Type>()
            .ForType<TestingVoid>()
            .Exclude();

        var actual = printingConfig.PrintToString(input);

        actual.Should().Be($@"{nameof(For_Exclude_FieldOfType_Type)}
	{nameof(For_Exclude_FieldOfType_Type.Test)} = {TestingConstants.TestStringValue}
");
    }


    [Test(Description = "minimal requirements p 1.2")]
    public void For_Exclude_PropertyOfType()
    {
        var input = new For_Exclude_PropertyOfType_Type();
        var printingConfig = ObjectPrinter.For<For_Exclude_PropertyOfType_Type>()
            .ForType<TestingVoid>()
            .Exclude();

        var actual = printingConfig.PrintToString(input);

        actual.Should().Be($@"{nameof(For_Exclude_PropertyOfType_Type)}
	{nameof(For_Exclude_PropertyOfType_Type.Test)} = {TestingConstants.TestStringValue}
");
    }


    [Test(Description = "minimal requirements p 2.1")]
    public void For_Serialize_FieldOfType()
    {
        var input = new For_Serialize_FieldOfType_Type();
        var printingConfig = ObjectPrinter.For<For_Serialize_FieldOfType_Type>()
            .ForType<string>()
            .Serialize(x => x.Length.ToString());

        var actual = printingConfig.PrintToString(input);

        actual.Should().Be($@"{nameof(For_Serialize_FieldOfType_Type)}
	{nameof(For_Serialize_FieldOfType_Type.Test)} = {TestingConstants.TestStringValue.Length.ToString()}
");
    }


    [Test(Description = "minimal requirements p 2.2")]
    public void For_Serialize_PropertyOfType()
    {
        var input = new For_Serialize_PropertyOfType_Type();
        var printingConfig = ObjectPrinter.For<For_Serialize_PropertyOfType_Type>()
            .ForType<string>()
            .Serialize(x => x.Length.ToString());

        var actual = printingConfig.PrintToString(input);

        actual.Should().Be($@"{nameof(For_Serialize_PropertyOfType_Type)}
	{nameof(For_Serialize_PropertyOfType_Type.Test)} = {TestingConstants.TestStringValue.Length.ToString()}
");
    }

    [Test(Description = "minimal requirements p 3")]
    public void For_Serialize_FieldOrPropertyThatImplementsIFormattable()
    {
        var tempCultureInfo = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-ru");
        var input = new For_Serialize_FieldOrPropertyThatImplementsIFormattable_Type();
        var printingConfig = ObjectPrinter.For<For_Serialize_FieldOrPropertyThatImplementsIFormattable_Type>()
            .SpecifyCulture(CultureInfo.GetCultureInfo("en-us"));
        var expectedTestDoubleValue =
            TestingConstants.TestDoubleValue.ToString(null, CultureInfo.GetCultureInfo("en-us"));

        var actual = printingConfig.PrintToString(input);

        actual.Should().Be($@"{nameof(For_Serialize_FieldOrPropertyThatImplementsIFormattable_Type)}
	{nameof(For_Serialize_FieldOrPropertyThatImplementsIFormattable_Type.Property)} = {expectedTestDoubleValue}
	{nameof(For_Serialize_FieldOrPropertyThatImplementsIFormattable_Type.Field)} = {expectedTestDoubleValue}
");
        CultureInfo.CurrentCulture = tempCultureInfo;
    }

    [Test(Description = "minimar requirements p 4.1")]
    public void For_Serialize_ConcreteField()
    {
        var input = new For_Serialize_ConcreteField_Type();
        var config = ObjectPrinter.For<For_Serialize_ConcreteField_Type>()
            .For(x => x.Serialize)
            .Serialize(x => x.Length.ToString());

        var actual = config.PrintToString(input);

        actual.Should().Be($@"{nameof(For_Serialize_ConcreteField_Type)}
	{nameof(For_Serialize_ConcreteField_Type.Serialize)} = {TestingConstants.TestStringValue.Length}
	{nameof(For_Serialize_ConcreteField_Type.Test)} = {TestingConstants.TestStringValue}
");
    }


    [Test(Description = "minimar requirements p 4.2")]
    public void For_Serialize_ConcreteProperty()
    {
        var input = new For_Serialize_ConcreteProperty_Type();
        var config = ObjectPrinter.For<For_Serialize_ConcreteProperty_Type>()
            .For(x => x.Serialize)
            .Serialize(x => x.Length.ToString());

        var actual = config.PrintToString(input);

        actual.Should().Be($@"{nameof(For_Serialize_ConcreteProperty_Type)}
	{nameof(For_Serialize_ConcreteProperty_Type.Serialize)} = {TestingConstants.TestStringValue.Length}
	{nameof(For_Serialize_ConcreteProperty_Type.Test)} = {TestingConstants.TestStringValue}
");
    }
}