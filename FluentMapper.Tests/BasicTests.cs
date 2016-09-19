using NUnit.Framework;

namespace FluentMapping.Tests
{
    [TestFixture]
    public sealed class BasicTests
    {
        [Test]
        public void BasicMapping()
        {
            var mapper = FluentMapper
                    .ThatMaps<Target>()
                    .From<Source>()
                    .Create();

            Assert.That(mapper, Is.Not.Null);

            var source = new Source { Str = "a value", Num = 123 };

            var target = mapper.Map(source);

            Assert.That(target.Num, Is.EqualTo(source.Num));
            Assert.That(target.Str, Is.EqualTo(source.Str));
        }

        public sealed class Target
        {
            public string Str { get; set; }
            public int Num { get; set; }
        }

        public sealed class Source
        {
            public string Str { get; set; }
            public int Num { get; set; }
        }
    }
}
