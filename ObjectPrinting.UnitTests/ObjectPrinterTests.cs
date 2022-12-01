using System.Globalization;
using FluentAssertions;

namespace ObjectPrinting.UnitTests;

[TestFixture]
public class ObjectPrinterTests
{
    [Test(Description = "minimal requirements p 1")]
    public void For_Exclude_FieldOfType()
    {
        var input = new For_Exclude_FieldAndPropertyOfType_Type();
        var printingConfig = ObjectPrinter.For<For_Exclude_FieldAndPropertyOfType_Type>()
            .Excluding<TestingVoid>();

        var actual = printingConfig.PrintToString(input);

        actual.Should().Be($@"{nameof(For_Exclude_FieldAndPropertyOfType_Type)}
	{nameof(For_Exclude_FieldAndPropertyOfType_Type.Test)} = {TestingConstants.TestStringValue}
");
    }

    [Test(Description = "minimal requirements p 2")]
    public void For_Serialize_FieldOfType()
    {
        var input = new For_Serialize_FieldAndPropertyOfType_Type();
        var printingConfig = ObjectPrinter.For<For_Serialize_FieldAndPropertyOfType_Type>()
            .Printing<string>()
            .Using(x => x.Length.ToString());

        var actual = printingConfig.PrintToString(input);

        actual.Should().Be($@"{nameof(For_Serialize_FieldAndPropertyOfType_Type)}
	{nameof(For_Serialize_FieldAndPropertyOfType_Type.TestProperty)} = {TestingConstants.TestStringValue.Length.ToString()}
	{nameof(For_Serialize_FieldAndPropertyOfType_Type.Test)} = {TestingConstants.TestStringValue.Length.ToString()}
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
    public void For_Serialize_ConcreteFieldAndProperty()
    {
        var input = new For_Serialize_FieldAndProperty_Type();
        var config = ObjectPrinter.For<For_Serialize_FieldAndProperty_Type>()
            .Printing(x => x.SerializeProperty)
            .Using(x => x.Length.ToString())
            .Printing(x => x.Serialize)
            .Using(x => x.Length.ToString());

        var actual = config.PrintToString(input);

        actual.Should().Be($@"{nameof(For_Serialize_FieldAndProperty_Type)}
	{nameof(For_Serialize_FieldAndProperty_Type.SerializeProperty)} = {TestingConstants.TestStringValue.Length}
	{nameof(For_Serialize_FieldAndProperty_Type.Serialize)} = {TestingConstants.TestStringValue.Length}
	{nameof(For_Serialize_FieldAndProperty_Type.Test)} = {TestingConstants.TestStringValue}
");
    }

    [Test(Description = "minimal requirements p 5")]
    public void For_Serialize_TrimmedToLengthStrings()
    {
        var input = new For_Serialize_CutStrings_Type();
        var config = ObjectPrinter.For<For_Serialize_CutStrings_Type>()
            .Printing(x => x.CutUpTo50)
            .TrimmedToLength(50)
            .Printing(x => x.CutUpTo25)
            .TrimmedToLength(25)
            .Printing<string>()
            .TrimmedToLength(75);

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
            .Excluding(x => x.Property)
            .Excluding(x => x.Field);

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
    public void For_Serialize_ArrayOfObjects()
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

        actual.Should().Be(@"Dictionary<String, For_Serialize_Objects_Type>
	KeyValuePairs
		[first] = For_Serialize_Objects_Type
			Value = TestValue
		[second] = For_Serialize_Objects_Type
			Value = TestValue
		[third] = For_Serialize_Objects_Type
			Value = TestValue
");
    }


    [Test]
    public void For_Serialize_UsingDefaultPrintToStringSyntax()
    {
        var input = new For_Serialize_Objects_Type();

        var actual = input.PrintToString();

        actual.Should().Be($@"{nameof(For_Serialize_Objects_Type)}
	{nameof(For_Serialize_Objects_Type.Value)} = {TestingConstants.TestStringValue}
");
    }


    [Test]
    public void For_Serialize_UsingBoundedPrintToStringSyntax()
    {
        var input = new For_Serialize_Objects_Type();

        var actual = input
            .PrintToString(x => x
                .Printing(o => o.Value)
                .Using(v => v + v));

        actual.Should().Be($@"{nameof(For_Serialize_Objects_Type)}
	{nameof(For_Serialize_Objects_Type.Value)} = {TestingConstants.TestStringValue + TestingConstants.TestStringValue}
");
    }
}