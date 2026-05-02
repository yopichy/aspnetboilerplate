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
        Configuration.ReplaceService<ObjectMapping.IObjectMapper, MapperlyObjectMapper>();
    }

    public override void PostInitialize()
    {
        RegisterMappers();
    }

    private void RegisterMappers()
    {
        var mapperInterface = typeof(IAbpMapper<,>);
        var reverseMapperInterface = typeof(IAbpReverseMapper<,>);
        var queryableMapperInterface = typeof(IAbpMapperlyQueryableMapper<,>);

        var types = _typeFinder.Find(type =>
            !type.IsAbstract &&
            !type.IsInterface &&
            type.GetInterfaces().Any(i =>
                i.IsGenericType &&
                (i.GetGenericTypeDefinition() == mapperInterface ||
                 i.GetGenericTypeDefinition() == reverseMapperInterface ||
                 i.GetGenericTypeDefinition() == queryableMapperInterface)
            )
        );

        Logger.DebugFormat("Found {0} Mapperly mapper classes", types.Length);

        foreach (var type in types)
        {
            Logger.Debug(type.FullName);

            // Collect all relevant interfaces this type implements that are not yet registered.
            var ifacesToRegister = type.GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == mapperInterface ||
                     i.GetGenericTypeDefinition() == reverseMapperInterface ||
                     i.GetGenericTypeDefinition() == queryableMapperInterface))
                .Where(i => !IocManager.IsRegistered(i))
                .ToArray();

            if (ifacesToRegister.Length == 0)
            {
                continue;
            }

            // Register all relevant interfaces in a single Windsor component so that resolving
            // any of them returns the same transient instance within a single call.
            IocManager.IocContainer.Register(
                Component.For(ifacesToRegister).ImplementedBy(type).LifestyleTransient()
            );
        }
    }
}
