using FluentMapping.Internal;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace FluentMapping.Tests.Unit
{
    [TestFixture]
    public sealed class TypeMappingSpecTests
    {
        [Test]
        public void GathersPropertiesInCtor()
        {
            var testee = new TypeMappingSpec<Target, Source>()
                as ITypeMappingSpecProperties<Target, Source>
                ;
            
            Assert.That(
                testee.SourceProperties.Count(),
                Is.EqualTo(2),
                "Expected two SourceProperties.");
            Assert.That(
                testee.SourceProperties.All(x => x.DeclaringType == typeof(Source)),
                "Expected SourceProperties to all have DeclaringType of Source."
                );
            Assert.That(
                testee.SourceProperties.Select(x => x.Name),
                Is.EquivalentTo(new[] { "Prop1", "Prop2" })
                );
            Assert.That(
                testee.TargetProperties.Count(),
                Is.EqualTo(2),
                "Expected two TargetProperties.");
            Assert.That(
                testee.TargetProperties.All(x => x.DeclaringType == typeof(Target)),
                "Expected TargetProperties to all have DeclaringType of Target."
                );
            Assert.That(
                testee.TargetProperties.Select(x => x.Name),
                Is.EquivalentTo(new[] { "Prop3", "Prop4" })
                );
        }

        [Test]
        public void PropertiesVisibility()
        {
            var publicProps = typeof(TypeMappingSpec<,>)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Assert.That(publicProps, Is.Empty);
        }

        [Test]
        public void PropertiesSubset_ViaCtor()
        {
            var testee = 
                    new TypeMappingSpec<Target, Source>().Transforms()
                .WithTargetProperties(
                    typeof(Target).GetProperties().Where(x => x.Name != "Prop3")
                    ).Transforms()
                .WithSourceProperties(
                    typeof(Source).GetProperties().Where(x => x.Name != "Prop1")
                ).Properties();

            Assert.That(testee.SourceProperties.Count(), Is.EqualTo(1));
            Assert.That(testee.TargetProperties.Count(), Is.EqualTo(1));
            Assert.That(
                testee.TargetProperties,
                Has.Some.Matches<PropertyInfo>(x => x.Name == "Prop4"));
            Assert.That(
                testee.SourceProperties,
                Has.Some.Matches<PropertyInfo>(x => x.Name == "Prop2"));
        }

        [Test]
        public void MappingAction_Ctor()
        {
            var action = new Action<Target, Source>((tgt, src) => { /* no-op */ });

            var testee = new TypeMappingSpec<Target, Source>()
                .Transforms().WithSourceProperties(new PropertyInfo[0])
                .Transforms().WithTargetProperties(new PropertyInfo[0])
                .Transforms().WithMappingActions(new [] { action })
                .Properties();

            Assert.That(testee.MappingActions, Is.EqualTo(new[] { action }));
            Assert.That(testee.SourceProperties, Is.Empty);
            Assert.That(testee.TargetProperties, Is.Empty);
        }

        [Test]
        public void MappingActionOnly_Usage()
        {
            var called = false;
            var action = new Action<Target, Source>((tgt, src) => called = true);

            var testee = new TypeMappingSpec<Target, Source>()
                .Transforms().WithTargetProperties(new PropertyInfo[0])
                .Transforms().WithSourceProperties(new PropertyInfo[0])
                .Transforms().WithMappingActions(new[] {action})
                ;
            var mapper = testee.Create();

            mapper.Map(new Source());

            Assert.That(called, Is.True);
        }

        //[Test]
        //public void Assembler_InMapping()
        //{
        //    var testee = new TypeMappingSpec<Target, Source>(
        //        new PropertyInfo[0],
        //        new PropertyInfo[0],
        //        new Action<Target, Source>[0],
        //        new ReturnNullAssembler<Target, Source>()
        //        );
        //}

        public sealed class Source
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; set; }
        }

        public sealed class Target
        {
            public string Prop3 { get; set; }
            public int Prop4 { get; set; }
        }
    }
}
