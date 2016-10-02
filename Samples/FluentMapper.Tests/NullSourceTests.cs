using NUnit.Framework;
using System;

namespace FluentMapping.Tests
{
    [TestFixture]
    public sealed class NullSourceTests
    {
        [Test]
        public void DefaultBehavior()
        {
            var mapper = FluentMapper
                    .ThatMaps<Target>()
                    .From<Source>()
                    .Create();

            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                var target = mapper.Map(null);
            });

            Assert.That(ex.Message, Does.StartWith("Cannot map instance of Target from " +
                "null instance of Source."));
        }

        [Test]
        public void ReturnNull()
        {
            var mapper = FluentMapper
                    .ThatMaps<Target>()
                    .From<Source>()
                    .WithNullSource()
                        .ReturnNull()
                    .Create();

            var target = mapper.Map(null);

            Assert.That(target, Is.Null);

            target = mapper.Map(new Source());

            Assert.That(target, Is.Not.Null);
        }

        [Test]
        public void CreateEmptyObject()
        {
            var mapper = FluentMapper
                    .ThatMaps<Target>()
                    .From<Source>()
                    .WithNullSource()
                        .CreateEmpty()
                    .Create();

            var target = mapper.Map(null);

            Assert.That(target, Is.Not.Null);

            target = mapper.Map(new Source { Num = 7 });

            Assert.That(target, Is.Not.Null);
            Assert.That(target.Num, Is.EqualTo(7));
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
