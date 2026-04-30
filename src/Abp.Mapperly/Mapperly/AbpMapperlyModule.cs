using System.Linq;
using Abp.Configuration.Startup;
using Abp.Modules;
using Abp.Reflection;
using Castle.MicroKernel.Registration;

namespace Abp.Mapperly;

[DependsOn(typeof(AbpKernelModule))]
public class AbpMapperlyModule : AbpModule
{
    private readonly ITypeFinder _typeFinder;

    public AbpMapperlyModule(ITypeFinder typeFinder)
    {
        _typeFinder = typeFinder;
    }

    public override void PreInitialize()
    {
        IocManager.Register<IAbpMapperlyConfiguration, AbpMapperlyConfiguration>();

        Configuration.ReplaceService<ObjectMapping.IObjectMapper, MapperlyObjectMapper>();
    }

    public override void PostInitialize()
    {
        RegisterMappers();
    }

    private void RegisterMappers()
    {
        var mapperInterface = typeof(IAbpMapper<,>);
        var queryableMapperInterface = typeof(IAbpMapperlyQueryableMapper<,>);

        var types = _typeFinder.Find(type =>
            !type.IsAbstract &&
            !type.IsInterface &&
            type.GetInterfaces().Any(i =>
                i.IsGenericType &&
                (i.GetGenericTypeDefinition() == mapperInterface ||
                 i.GetGenericTypeDefinition() == queryableMapperInterface)
            )
        );

        Logger.DebugFormat("Found {0} Mapperly mapper classes", types.Length);

        foreach (var type in types)
        {
            Logger.Debug(type.FullName);

            foreach (var iface in type.GetInterfaces().Where(i =>
                i.IsGenericType &&
                (i.GetGenericTypeDefinition() == mapperInterface ||
                 i.GetGenericTypeDefinition() == queryableMapperInterface)))
            {
                if (!IocManager.IsRegistered(iface))
                {
                    IocManager.IocContainer.Register(
                        Component.For(iface).ImplementedBy(type).LifestyleTransient()
                    );
                }
            }
        }
    }
}
