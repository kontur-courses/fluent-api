using NUnit.Framework;
using System;

namespace FluentMapping.Tests
{
    [TestFixture]
    public sealed class SimplePropertyMappingTests
    {
        [Test]
        public void InvalidMapping()
        {
            var spec = FluentMapper
                .ThatMaps<Target>()
                .From<Source>();

            var ex = Assert.Throws<Exception>(() => spec.Create());

            var expectedMessage = "Unmapped properties: " +
                "Target.Num, Target.Str, " +
                "Source.ID, Source.Name";

            Assert.That(ex.Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void PropertyMatching()
        {
            var mapper = FluentMapper.ThatMaps<Target>()
                .From<Source>()
                .ThatSets(tgt => tgt.Str).From(src => src.Name)
                .ThatSets(tgt => tgt.Num).From(src => src.ID)
                .Create();

            var source = new Source {
                ID = 7,
                Name = "Bob"
            };

            var target = mapper.Map(source);

            Assert.That(target.Num, Is.EqualTo(7));
            Assert.That(target.Str, Is.EqualTo("Bob"));
        }

        [Test]
        public void IgnoringProperties()
        {
            var mapper = FluentMapper.ThatMaps<Target>()
                .From<Source>()
                .ThatSets(tgt => tgt.Str).From(src => src.Name)
                .IgnoringTargetProperty(tgt => tgt.Num)
                .IgnoringSourceProperty(src => src.ID)
                .Create();

            var source = new Source
            {
                ID = 7,
                Name = "Bob"
            };

            var target = mapper.Map(source);

            Assert.That(target.Str, Is.EqualTo("Bob"));
        }

        [Test]
        public void IgnoringSourceProperty_NonPropertyExpression()
        {
            var spec = FluentMapper.ThatMaps<Target>().From<Source>();

            var ex = Assert.Throws<Exception>(() => spec.IgnoringSourceProperty(src => 7));

            Assert.That(ex.Message, Is.EqualTo("IgnoringSourceProperty(...) requires an expression "
                + "that is a simple property access of the form 'src => src.Property'."
                ));
        }

        [Test]
        public void IgnoringTargetProperty_MethodCallExpression()
        {
            var spec = FluentMapper.ThatMaps<Target>().From<Source>();

            var ex = Assert.Throws<Exception>(() => spec.IgnoringTargetProperty(tgt => tgt.AField));

            Assert.That(ex.Message, Is.EqualTo("IgnoringTargetProperty(...) requires an expression "
                + "that is a simple property access of the form 'tgt => tgt.Property'."
                ));
        }

        public sealed class Target
        {
            public string Str { get; set; }
            public int Num { get; set; }

            public int AField;
        }

        public sealed class Source
        {
            public string Name { get; set; }
            public int ID { get; set; }
        }
    }
}
