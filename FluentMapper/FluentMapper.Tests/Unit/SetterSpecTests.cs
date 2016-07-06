using NUnit.Framework;
using System.Linq;

namespace FluentMapping.Tests.Unit
{
    [TestFixture]
    public sealed class SetterSpecTests
    {
        [Test]
        public void Ctor()
        {
            var spec = new TypeMappingSpec<Target, Source>();

            var testee = spec.ThatSets(tgt => tgt.TargetProp)
                as ISetterSpecProperties<Target, Source>
                ;

            Assert.That(testee.Spec, Is.SameAs(spec));
            Assert.That(testee.TargetProperty, Is.Not.Null);
            Assert.That(testee.TargetProperty.Name, Is.EqualTo(nameof(Target.TargetProp)));
            Assert.That(testee.TargetProperty.DeclaringType, Is.EqualTo(typeof(Target)));
        }

        [Test]
        public void From_Creates_Mapping()
        {
            var result = new TypeMappingSpec<Target, Source>()
                .ThatSets(tgt => tgt.TargetProp).From(src => src.SourceProp)
                as ITypeMappingSpecProperties<Target, Source>
                ;

            Assert.That(
                result.SourceProperties,
                Is.EqualTo(new[] { typeof(Source).GetProperty(nameof(Source.MatchingProp)) }));
            Assert.That(
                result.TargetProperties,
                Is.EqualTo(new[] { typeof(Target).GetProperty(nameof(Target.MatchingProp)) }));
            Assert.That(result.MappingActions.Count(), Is.EqualTo(1));

            var target = new Target();
            var source = new Source { SourceProp = "value" };

            result.MappingActions.Single()(target, source);
            Assert.That(target.TargetProp, Is.EqualTo(source.SourceProp));
        }

        [Test]
        public void MultipleMappings()
        {
            var result = new TypeMappingSpec<Target, Source>()
                .ThatSets(tgt => tgt.TargetProp).From(src => src.SourceProp)
                .ThatSets(tgt => tgt.MatchingProp).From(src => src.MatchingProp)
                as ITypeMappingSpecProperties<Target, Source>
                ;

            Assert.That(result.SourceProperties, Is.Empty);
            Assert.That(result.TargetProperties, Is.Empty);
            Assert.That(result.MappingActions.Count(), Is.EqualTo(2));

            var source = new Source { MatchingProp = 7 };
            var target = new Target();

            foreach (var action in result.MappingActions)
                action(target, source);

            Assert.That(target.MatchingProp, Is.EqualTo(7));
        }

        public sealed class Source
        {
            public int MatchingProp { get; set; }

            public string SourceProp { get; set; }
        }

        public sealed class Target
        {
            public int MatchingProp { get; set; }

            public string TargetProp { get; set; }
        }
    }
}
