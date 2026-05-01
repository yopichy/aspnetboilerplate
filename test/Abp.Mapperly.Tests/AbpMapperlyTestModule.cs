using Abp.Modules;
using Abp.Reflection.Extensions;

namespace Abp.Mapperly.Tests;

[DependsOn(typeof(AbpMapperlyModule))]
public class AbpMapperlyTestModule : AbpModule
{
    public override void Initialize()
    {
        // Register all ITransientDependency / ISingletonDependency types in the test assembly
        // so that mappers with constructor-injected services can be resolved correctly.
        IocManager.RegisterAssemblyByConvention(typeof(AbpMapperlyTestModule).GetAssembly());
    }
}
