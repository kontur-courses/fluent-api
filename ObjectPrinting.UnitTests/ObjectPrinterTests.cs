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

    [Test(Description = "minimal requirements p 4.1")]
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


    [Test(Description = "minimal requirements p 4.2")]
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

    [Test(Description = "minimal requirements p 5")]
    public void For_Serialize_CutStrings()
    {
        var input = new For_Serialize_CutStrings_Type();
        var config = ObjectPrinter.For<For_Serialize_CutStrings_Type>()
            .For(x => x.CutUpTo50)
            .Cut(50)
            .For(x => x.CutUpTo25)
            .Cut(25)
            .ForType<string>()
            .Cut(75);

        var actual = config.PrintToString(input);

        actual.Should().Be($@"{nameof(For_Serialize_CutStrings_Type)}
	{nameof(For_Serialize_CutStrings_Type.CutUpTo50)} = {TestingConstants.LongTestStringValue[..50]}
	{nameof(For_Serialize_CutStrings_Type.CutUpTo75Other)} = {TestingConstants.LongTestStringValue[..75]}
	{nameof(For_Serialize_CutStrings_Type.CutUpTo25)} = {TestingConstants.LongTestStringValue[..25]}
");
    }

    [Test(Description = "minimal requirements p 6")]
    public void For_Exclude_ConcreteFieldAndConcreteProperty()
    {
        var input = new For_Exclude_ConcreteFieldAndConcreteProperty_Type();
        var printingConfig = ObjectPrinter.For<For_Exclude_ConcreteFieldAndConcreteProperty_Type>()
            .For(x => x.Property)
            .Exclude()
            .For(x => x.Field)
            .Exclude();

        var actual = printingConfig.PrintToString(input);

        actual.Should().Be($@"{nameof(For_Exclude_ConcreteFieldAndConcreteProperty_Type)}
	{nameof(For_Exclude_ConcreteFieldAndConcreteProperty_Type.Test)} = {TestingConstants.TestStringValue}
");
    }

    [Test(Description = "minimal requirements p 7")]
    public void For_IgnoreMembers_CycleInheritance()
    {
        var input = new For_IgnoreMembers_CycleInheritance_Type();
        var config = ObjectPrinter.For<For_IgnoreMembers_CycleInheritance_Type>()
            .WithCyclicInheritanceHandler(CyclicInheritanceHandler.IgnoreMembers);

        var actual = config.PrintToString(input);

        actual.Should().Be(@"For_IgnoreMembers_CycleInheritance_Type
	Children = NestedType
		Parent = For_IgnoreMembers_CycleInheritance_Type
");
    }


    [Test(Description = "full requirements p 2.1")]
    public void For_Serialize_EnumerableOfObjects()
    {
        var input = new For_Serialize_Objects_Type[]
        {
            new(), new(), new()
        };
        var config = ObjectPrinter.For<For_Serialize_Objects_Type[]>();

        var actual = config.PrintToString(input);

        actual.Should().Be(@"For_Serialize_Objects_Type[]
	Items
		[0] = For_Serialize_Objects_Type
			Value = TestValue
		[1] = For_Serialize_Objects_Type
			Value = TestValue
		[2] = For_Serialize_Objects_Type
			Value = TestValue
");
    }

    [Test(Description = "full requirements p 2.2")]
    public void For_Serialize_DictionaryOfObjects()
    {
        var input = new Dictionary<string, For_Serialize_Objects_Type>
        {
            { "first", new() }, { "second", new() }, { "third", new() }
        };
        var config = ObjectPrinter.For<Dictionary<string, For_Serialize_Objects_Type>>();

        var actual = config.PrintToString(input);

        actual.Should().Be(@"Dictionary`2
	Items
		[first] = For_Serialize_Objects_Type
			Value = TestValue
		[second] = For_Serialize_Objects_Type
			Value = TestValue
		[third] = For_Serialize_Objects_Type
			Value = TestValue
");
    }
}