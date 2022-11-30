using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using ObjectPrinting.Abstractions.Configs;

namespace ObjectPrinting;

public static class ObjectPrinter
{
    private static readonly IWindsorContainer Container;

    public static IPrintingConfig<T> For<T>() =>
        Container.Resolve<IPrintingConfig<T>>();

    static ObjectPrinter()
    {
        Container = new WindsorContainer();
        var kernel = Container.Kernel;
        kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, true));

        Container.Register(
            Classes.FromAssemblyContaining(typeof(ObjectPrinter))
                .Where(component =>
                    component.Namespace is not null && component.Namespace.Contains(nameof(Implementations)))
                .WithService.AllInterfaces()
                .LifestyleTransient()
        );
    }
}