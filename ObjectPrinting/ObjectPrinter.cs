using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using ObjectPrinting.Abstractions.Configs;

namespace ObjectPrinting;

public static class ObjectPrinter
{
    private static IWindsorContainer? _container;

    public static IPrintingConfig<T> For<T>() =>
        CreateConfig<T>();

    private static IPrintingConfig<T> CreateConfig<T>()
    {
        if (_container is null)
        {
            _container = new WindsorContainer();
            var kernel = _container.Kernel;
            kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, true));

            _container.Register(
                Classes.FromAssemblyContaining(typeof(ObjectPrinter))
                    .Where(component =>
                        component.Namespace is not null && component.Namespace.Contains(nameof(Implementations)))
                    .WithService.AllInterfaces()
                    .LifestyleTransient()
            );
        }

        return _container.Resolve<IPrintingConfig<T>>();
    }
}