using System.Drawing;
using System.Globalization;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests;

public class ObjectPrintingTests
{
    private Person person;
    private Room room;

    [SetUp]
    public void SetUp()
    {
        person = new Person
        {
            Name = "Frank",
            Surname = "Sinatra",
            Age = 19000000,
            Id = new Guid(),
            Height = 180.5,
            Money = 300,
        };

        room = new Room
        {
            Width = 32.5,
            Length = 48.6,
            Height = 2.2,
        };
    }

    [Test]
    public void Exclude_ExcludesGivenTypes()
    {
        var config = ObjectPrinter.For<Person>()
            .Exclude<long>()
            .Exclude<Guid>()
            .Exclude<string>();

        person.PrintToString(config)
            .Should()
            .Be("Person (\n\tHeight: 180,5;\n\tAge: 19000000;\n)");
    }

    [Test]
    public void Exclude_ExcludesGivenProperties()
    {
        var config = ObjectPrinter.For<Person>()
            .Exclude(p => p.Id)
            .Exclude(p => p.Name)
            .Exclude(p => p.Height);

        person.PrintToString(config)
            .Should()
            .Be("Person (\n\tSurname: Sinatra;\n\tAge: 19000000;\n\tMoney: 300;\n)");
    }

    [Test]
    public void Select_SerializesSpecifiedTypes()
    {
        var config = ObjectPrinter.For<Room>()
            .Serialize<double>().With(num => Math.Ceiling(num).ToString());

        room.PrintToString(config)
            .Should()
            .Be("Room (\n\tWidth: 33;\n\tLength: 49;\n\tHeight: 3;\n)");
    }

    [Test]
    public void Select_SerializesSpecifiedProperties()
    {
        var config = ObjectPrinter.For<Room>()
            .Serialize(r => r.Height).With(num => (num / 2).ToString());

        room.PrintToString(config)
            .Should()
            .Be("Room (\n\tWidth: 32,5;\n\tLength: 48,6;\n\tHeight: 1,1;\n)");
    }

    [Test]
    public void SetCulture_WorksWithGivenType()
    {
        var cultureObject = new CultureObject
        {
            DateTimeProp = new DateTime(2020, 01, 01),
            DoubleProp = 22.22
        };

        var config = ObjectPrinter.For<CultureObject>()
            .SetCulture<DateTime>(CultureInfo.InvariantCulture);

        cultureObject.PrintToString(config)
            .Should()
            .Be("CultureObject (\n\tDateTimeProp: 01/01/2020 00:00:00;\n\tDoubleProp: 22,22;\n)");
    }

    [Test]
    public void SetCulture_WorksWithGivenProperty()
    {
        var cultureObject = new CultureObject
        {
            DateTimeProp = new DateTime(2020, 01, 01),
            DoubleProp = 22.22
        };

        var config = ObjectPrinter.For<CultureObject>()
            .SetCulture(c => c.DoubleProp, CultureInfo.InvariantCulture);

        cultureObject.PrintToString(config)
            .Should()
            .Be("CultureObject (\n\tDateTimeProp: 01.01.2020 00:00:00;\n\tDoubleProp: 22.22;\n)");
    }

    [Test]
    public void SliceStrings_Works()
    {
        var fullName = new FullName
        {
            Name = "SmallName",
            Surname = "VeryBigSurNameWithALotOfWater",
        };

        var config = ObjectPrinter.For<FullName>()
            .SliceStrings(10);

        fullName.PrintToString(config)
            .Should()
            .Be("FullName (\n\tName: SmallName;\n\tSurname: VeryBigSur;\n)");
    }

    [Test]
    public void PrintToString_WorksWithIEnumerable()
    {
        var arr = new[] { 1, 2 };
        
        arr.PrintToString()
            .Should()
            .Be("[\n\t1,\n\t2,\n]");
    }
    
    [Test]
    public void PrintToString_WorksWithNestedIEnumerable()
    {
        var arr = new[] { new []{1, 2}, new []{3, 4} };
        
        arr.PrintToString()
            .Should()
            .Be("[\n\t[\n\t\t1,\n\t\t2,\n\t],\n\t[\n\t\t3,\n\t\t4,\n\t],\n]");
    }

    [Test]
    public void PrintToString_WorksWithIDictionary()
    {
        var dict = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 4,
            [3] = 9
        };

        dict.PrintToString()
            .Should()
            .Be("{\n\t1: 1;\n\t2: 4;\n\t3: 9;\n}");
    }
    
    [Test]
    public void PrintToString_WorksWithNestedIDictionary()
    {
        var dict = new Dictionary<int, Dictionary<int, int>>
        {
            [1] = new ()
            {
                [2] = 3,
                [4] = 5,
            },
            [6] = new ()
            {
                [7] = 8,
                [9] = 10,
            },
        };

        dict.PrintToString()
            .Should()
            .Be("{\n\t1: {\n\t\t2: 3;\n\t\t4: 5;\n\t};\n\t6: {\n\t\t7: 8;\n\t\t9: 10;\n\t};\n}");
    }

    [Test]
    public void PropertyGettingMethod_ThrowsArgumentExceptionOnWrongExpression()
    {
        new Action(() => ObjectPrinter.For<Person>()
                .Exclude(p => 4))
            .Should()
            .ThrowExactly<ArgumentException>()
            .Where(e => e.Message.Contains("MemberExpression"));
        
        new Action(() => ObjectPrinter.For<Person>()
                .Serialize(p => 4))
            .Should()
            .ThrowExactly<ArgumentException>()
            .Where(e => e.Message.Contains("MemberExpression"));
    }

    [Test]
    public void PrintToString_HandlesCyclicLinks()
    {
        var obj1 = new CyclicObject {SomeProp = 30};
        
        var obj2 = new CyclicObject {SomeProp = 50};

        obj1.Another = obj2;
        obj2.Another = obj1;

        obj1.PrintToString()
            .Should()
            .Be("CyclicObject (\n\tSomeProp: 30;\n\tAnother: CyclicObject (\n\t\tSomeProp: 50;\n\t\tAnother: cyclic link;\n\t);\n)");
    }

    [Test]
    public void PrintToString_WorksWithStrings()
    {
        "serialize me".PrintToString()
            .Should()
            .Be("serialize me");
    }

    [Test]
    public void PrintToString_WorksWithValueTypes()
    {
        new Point(1, 1).PrintToString()
            .Should()
            .Be("{X=1,Y=1}");
    }
}