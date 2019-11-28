using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private MusicAlbum musicAlbum;

        [SetUp]
        public void SetUp()
        {
            var songs = new List<Song>
            {
                new Song("Crises"),
                new Song("Moonlight Shadow"),
                new Song("In High Places"),
                new Song("Foreign Affair"),
                new Song("Taurus III"),
                new Song("Shadow On The Wall")
            };

            var genres = new Dictionary<string, string>
            {
                {"progressive rock", "developed in the UK in 1960s"},
                {"art rock", "reflects avant-garde approach to rock"},
            };

            musicAlbum = new MusicAlbum("Crises", "Mike Oldfield", genres, 2009,9.9, songs);
        }

        [Test]
        public void Should_ExcludeTypes()
        {
            var expected = "MusicAlbum\n" +
                           "	Year = 2009\n"+
                           "	Rating = 9.9\n";
            
            var printer = ObjectPrinter
                .For<MusicAlbum>()
                .Excluding<string>()
                .Excluding(m => m.Genres)
                .Excluding(m => m.Songs);
            var actual = printer.PrintToString(musicAlbum);

            actual.ShouldBeEquivalentTo(expected);
        }
        
        [Test]
        public void Should_SetTypeCustomSerialization()
        {
            var expected = "MusicAlbum\n" +
                           "	Name = CRISES\n" +
                           "	Artist = MIKE OLDFIELD\n" +
                           "	Year = 2009\n"+
                           "	Rating = 9.9\n";
            
            var printer = ObjectPrinter
                .For<MusicAlbum>()
                .Printing<string>().Using(s=>s.ToUpper())
                .Excluding(m => m.Genres)
                .Excluding(m => m.Songs);
            var actual = printer.PrintToString(musicAlbum);

            actual.ShouldBeEquivalentTo(expected);
        }
        
        [Test]
        public void Should_SetNumbersCultureInfo()
        {
            var commaCulture = new CultureInfo("en")
            {
                NumberFormat =
                {
                    NumberDecimalSeparator = ","
                }
            };
            
            var expected = "MusicAlbum\n" +
                           "	Name = Crises\n" +
                           "	Artist = Mike Oldfield\n" +
                           "	Year = 2009\n"+
                           "	Rating = 9,9";
            
            var printer = ObjectPrinter
                .For<MusicAlbum>()
                .Printing<double>().Using(commaCulture)
                .Excluding(m => m.Genres)
                .Excluding(m => m.Songs);
            var actual = printer.PrintToString(musicAlbum);

            actual.ShouldBeEquivalentTo(expected);
        }
        
        [Test]
        public void Should_SetPropertyCustomSerialization()
        {
            var expected = "MusicAlbum\n" +
                           "	Name = CRISES\n" +
                           "	Artist = Mike Oldfield\n" +
                           "	Year = 2009\n"+
                           "	Rating = 9.9\n";
            
            var printer = ObjectPrinter
                .For<MusicAlbum>()
                .Printing(m=>m.Name).Using(s=>s.ToUpper())
                .Excluding(m => m.Genres)
                .Excluding(m => m.Songs);
            var actual = printer.PrintToString(musicAlbum);

            actual.ShouldBeEquivalentTo(expected);
        }
        
        [Test]
        public void Should_TrimStrings()
        {
            var expected = "MusicAlbum\n" +
                           "	Name = Crises\n" +
                           "	Artist = Mike Ol\n" +
                           "	Year = 2009\n"+
                           "	Rating = 9.9\n";
            
            var printer = ObjectPrinter
                .For<MusicAlbum>()
                .Printing<string>().TrimmedToLength(7)
                .Excluding(m => m.Genres)
                .Excluding(m => m.Songs);
            var actual = printer.PrintToString(musicAlbum);

            actual.ShouldBeEquivalentTo(expected);
        }
        
        [Test]
        public void Should_ExcludeProperties()
        {
            var expected = "MusicAlbum\n" +
                           "	Name = Crises\n" +
                           "	Artist = Mike Oldfield\n" +
                           "	Year = 2009\n"+
                           "	Rating = 9.9\n";
            
            var printer = ObjectPrinter
                .For<MusicAlbum>()
                .Excluding(m => m.Genres)
                .Excluding(m => m.Songs);
            var actual = printer.PrintToString(musicAlbum);

            actual.ShouldBeEquivalentTo(expected);
        }
        
        [Test]
        public void Should_PrintArrays()
        {
            var selfReflect = new SelfReflect();
            Action act = () => selfReflect.PrintToString();
            act.ShouldNotThrow<StackOverflowException>();
        }

        [Test]
        public void Should_PrintDictionaries()
        {
            var expected = "MusicAlbum\n" +
                           "	Genres = Dictionary`2\n" +
                           "		[\n" +
                           "			KeyValuePair`2\n" +
                           "				Key = progressive rock\n" +
                           "				Value = developed in the UK in 1960s\n" +
                           "			KeyValuePair`2\n" +
                           "				Key = art rock\n" +
                           "				Value = reflects avant-garde approach to rock\n" +
                           "		]\n";

            var printer = ObjectPrinter
                .For<MusicAlbum>()
                .Excluding(m => m.Rating)
                .Excluding(m => m.Year)
                .Excluding(m => m.Name)
                .Excluding(m => m.Artist)
                .Excluding(m => m.Songs);
            var actual = printer.PrintToString(musicAlbum);

            actual.ShouldBeEquivalentTo(expected);
        }
        
        [Test]
        public void Should_PrintLists()
        {
            var expected = "MusicAlbum\n" +
                           "	Songs = List`1\n" +
                           "		[\n" +
                           "			Song\n" +
                           "				Name = Crises\n" +
                           "			Song\n" +
                           "				Name = Moonlight Shadow\n" +
                           "			Song\n" +
                           "				Name = In High Places\n	" +
                           "		Song\n" +
                           "				Name = Foreign Affair\n" +
                           "			Song\n" +
                           "				Name = Taurus III\n" +
                           "			Song\n" +
                           "				Name = Shadow On The Wall\n" +
                           "		]\n";

            var printer = ObjectPrinter
                .For<MusicAlbum>()
                .Excluding(m => m.Rating)
                .Excluding(m => m.Year)
                .Excluding(m => m.Name)
                .Excluding(m => m.Artist)
                .Excluding(m => m.Genres);
            var actual = printer.PrintToString(musicAlbum);

            actual.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void Should_NotThrowStackOverflowException_WhenCyclicReflection()
        {
            var selfReflect = new SelfReflect();
            Action act = () => selfReflect.PrintToString();
            act.ShouldNotThrow<StackOverflowException>();
        }

        [Test]
        public void Demo()
        {
            // musicAlbum.
            var printer = ObjectPrinter.For<MusicAlbum>();
            Console.WriteLine(printer.PrintToString(musicAlbum));
        }

        /*var person = new Person { Name = "Alexxx", Age = 19, Height = 1.50};

        var commaCulture = new CultureInfo("en")
        {
            NumberFormat =
            {
                NumberDecimalSeparator = ","
            }
        };
        
        var printer = ObjectPrinter.For<Person>()
            .Excluding<Guid>()
            .Printing<int>().Using(i => i.ToString("X"))
            .Printing<double>().Using(commaCulture) 
            .Printing(p => p.Name).TrimmedToLength(4)
            .Excluding(p => p.Age); 
        var s1 = printer.PrintToString(person);
        var s2 = person.PrintToString();
        var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
        Console.WriteLine(s1);
        Console.WriteLine(s2);
        Console.WriteLine(s3);
     }*/
    }
}